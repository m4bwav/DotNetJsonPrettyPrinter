using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace JsonPrettyPrinterPlus.GoldenTests
{
    /// <summary>
    /// Every line of tests/Golden/PublicApi-3.0.1.txt (listed by reflection from the published 3.0.1, tests/Golden/ApiList)
    /// must still be true: type kinds, members, init accessors and parameter names, which callers use in named arguments.
    /// Package validation checks the shape against 3.0.1 but not parameter names. The listing logic is a copy of
    /// tests/Golden/ApiList/Program.cs, which is frozen with the golden files.
    /// </summary>
    [TestFixture]
    public class PublicApiTests
    {
        private const BindingFlags Declared = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        [Test]
        public void Every_3_0_1_api_line_still_exists()
        {
            var current = new HashSet<string>(List(typeof(JsonPrettyPrinter).Assembly), StringComparer.Ordinal);
            Assert.That(Baseline().Where(l => !current.Contains(l)), Is.Empty);
        }

        [Test]
        public void No_public_member_was_added_without_a_plan_decision()
        {
            var baseline = new HashSet<string>(Baseline(), StringComparer.Ordinal);
            Assert.That(List(typeof(JsonPrettyPrinter).Assembly).Where(l => !baseline.Contains(l)), Is.Empty);
        }

        private static List<string> Baseline()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Golden", "PublicApi-3.0.1.txt");
            return File.ReadAllLines(path).Where(l => l.Length > 0 && l[0] != '#').ToList();
        }

        private static List<string> List(Assembly asm)
        {
            var lines = new List<string>();
            foreach (var t in asm.GetExportedTypes())
            {
                var kind = t.IsInterface ? "interface" : t.IsEnum ? "enum" : t.IsValueType ? "struct" : t.IsAbstract && t.IsSealed ? "static class" : t.IsAbstract ? "abstract class" : t.IsSealed ? "sealed class" : "class";
                lines.Add(Name(t) + " type " + kind);
                if (t.BaseType != null && t.BaseType != typeof(object))
                {
                    lines.Add(Name(t) + " base " + Name(t.BaseType));
                }

                foreach (var i in t.GetInterfaces())
                {
                    lines.Add(Name(t) + " implements " + Name(i));
                }

                foreach (var c in t.GetConstructors(Declared | BindingFlags.NonPublic).Where(c => c.IsPublic || c.IsFamily || c.IsFamilyOrAssembly))
                {
                    lines.Add(Name(t) + " constructor " + (c.IsPublic ? "" : "protected ") + ".ctor(" + Params(c) + ")");
                }

                foreach (var m in t.GetMethods(Declared | BindingFlags.NonPublic).Where(m => (m.IsPublic || m.IsFamily || m.IsFamilyOrAssembly) && !m.IsSpecialName))
                {
                    var access = m.IsPublic ? "" : "protected ";
                    var stat = m.IsStatic ? "static " : "";
                    var ext = m.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false) ? "extension " : "";
                    var generic = m.IsGenericMethodDefinition ? "<" + string.Join(", ", m.GetGenericArguments().Select(a => a.Name)) + ">" : "";
                    lines.Add(Name(t) + " method " + access + stat + ext + Name(m.ReturnType) + " " + m.Name + generic + "(" + Params(m) + ")");
                }

                foreach (var p in t.GetProperties(Declared))
                {
                    var setter = p.GetSetMethod();
                    var init = setter != null && setter.ReturnParameter.GetRequiredCustomModifiers().Any(x => x.FullName == "System.Runtime.CompilerServices.IsExternalInit");
                    var acc = (p.GetGetMethod() != null ? "get; " : "") + (setter == null ? "" : init ? "init; " : "set; ");
                    var isStatic = (p.GetGetMethod() ?? setter)!.IsStatic;
                    lines.Add(Name(t) + " property " + (isStatic ? "static " : "") + Name(p.PropertyType) + " " + p.Name + " { " + acc + "}");
                }

                foreach (var f in t.GetFields(Declared | BindingFlags.NonPublic).Where(f => f.IsPublic || f.IsFamily || f.IsFamilyOrAssembly))
                {
                    lines.Add(Name(t) + " field " + (f.IsPublic ? "" : "protected ") + (f.IsStatic ? "static " : "") + (f.IsInitOnly ? "readonly " : "") + (f.IsLiteral ? "const " : "") + Name(f.FieldType) + " " + f.Name);
                }
            }

            return lines;
        }

        private static string Params(MethodBase m)
        {
            return string.Join(", ", m.GetParameters().Select(p =>
                Name(p.ParameterType) + " " + p.Name + (p.HasDefaultValue ? " = " + (p.DefaultValue ?? "null") : "")));
        }

        private static string Name(Type t)
        {
            if (t.IsGenericParameter)
            {
                return t.Name;
            }

            if (t.IsByRef)
            {
                return "ref " + Name(t.GetElementType()!);
            }

            if (t.IsGenericType)
            {
                var def = t.GetGenericTypeDefinition();
                var baseName = (def.Namespace == null ? "" : def.Namespace + ".") + def.Name.Split('`')[0];
                if (def == typeof(Nullable<>))
                {
                    return Name(t.GetGenericArguments()[0]) + "?";
                }

                return baseName + "<" + string.Join(", ", t.GetGenericArguments().Select(Name)) + ">";
            }

            return t.FullName ?? t.Name;
        }
    }
}
