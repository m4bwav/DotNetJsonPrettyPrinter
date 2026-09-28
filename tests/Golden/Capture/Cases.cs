#nullable disable

// Every case of the golden capture of JsonPrettyPrinter (package-modernize retrofit, Phase 0, 2026-09-28). Touches only
// public names, so the golden test compiles this file unchanged against the new library and compares its answers.
// The contract is 3.0.1. The same file records 2.1.1 (same names; only the NewLine default differs) and, with JPP_V1
// defined by Capture.csproj, 1.0.1.1, whose API allows the cases outside the "#if !JPP_V1" blocks (printing with the
// defaults, the 1.x printer object, ToJSON and DeserializeFromJson). Case names are the same in every version, so the
// recordings can be compared case by case for the upgrade story.
// Inputs are written with two placeholders so no editor or shell can rewrite them: a backtick stands for a backslash,
// and {U+XXXX} for that UTF-16 code unit (see J). Outputs longer than 4000 characters are recorded as their length,
// SHA-256 of the UTF-16LE bytes, and the first and last 200 characters.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using JsonPrettyPrinterPlus;
using JsonPrettyPrinterPlus.JsonSerialization;
#if JPP_V1
using JsonPrettyPrinterPlus.JsonPrettyPrinterInternals;
#else
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
#endif

namespace GoldenCapture
{
    public sealed class Case
    {
        public string Group;
        public string Name;
        public object Result;
    }

    // Types for the serializer cases: public, with public setters, so every serializer in every version can use them.
    public class Poco
    {
        public string Name { get; set; }

        public int Count { get; set; }

        public string[] Tags { get; set; }

        public DateTime When { get; set; }

        public Inner Nested { get; set; }

        public string Field;
    }

    public class Inner
    {
        public double Value { get; set; }

        public bool? Flag { get; set; }
    }

    public class Node
    {
        public string Name { get; set; }

        public Node Next { get; set; }
    }

    public static class Cases
    {
        private const char BS = (char)92;
        private const int Summarize = 4000;
        private static readonly string LF = new string((char)10, 1);
        private static readonly string CRLF = new string(new[] { (char)13, (char)10 });
        private static readonly List<Case> All = new List<Case>();
        private static readonly Dictionary<string, object> Printed = new Dictionary<string, object>();
        private static readonly List<KeyValuePair<string, string>> Corpus = BuildCorpus();

        public static List<Case> Run()
        {
            All.Clear();
            Printed.Clear();
            PrintCases();
            SurfaceCases();
            IdempotenceCases();
            OptionCases();
            OptionErrorCases();
            OptionRecordCases();
            NullCases();
            ReuseCases();
            WriterCases();
            BigCases();
            ThreadCases();
            CultureCases();
            ToJsonCases();
            FromJsonCases();
            return All;
        }

        public static string PackageVersion(Assembly asm)
        {
            var info = asm.GetCustomAttributes(false).OfType<AssemblyInformationalVersionAttribute>().Select(a => a.InformationalVersion).FirstOrDefault();
            if (string.IsNullOrEmpty(info))
            {
                return asm.GetName().Version.ToString();
            }

            var plus = info.IndexOf('+');
            return plus < 0 ? info : info.Substring(0, plus);
        }

        public static string SerializerVersion()
        {
#if JPP_V1
            return "not used (1.x serializes with JavaScriptSerializer from System.Web.Extensions)";
#else
            return "System.Text.Json " + PackageVersion(typeof(JsonSerializer).Assembly);
#endif
        }

        // ---------- the input corpus ----------

        // A backtick stands for a backslash; {U+XXXX} for one UTF-16 code unit.
        public static string J(string s)
        {
            var sb = new StringBuilder(s.Length);
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c == '`')
                {
                    sb.Append(BS);
                }
                else if (c == '{' && i + 3 < s.Length && s[i + 1] == 'U' && s[i + 2] == '+')
                {
                    var end = s.IndexOf('}', i);
                    sb.Append((char)int.Parse(s.Substring(i + 3, end - i - 3), NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                    i = end;
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        private static List<KeyValuePair<string, string>> BuildCorpus()
        {
            var c = new List<KeyValuePair<string, string>>();
            void Add(string name, string input) => c.Add(new KeyValuePair<string, string>(name, J(input)));

            // Well-formed documents.
            Add("readme", @"{""Lorem"":""ipsum"",""dolor"":{ ""sit"":""amet""},""consectetur"":{""adipisicing"":{""sed"":""do""}},""empty"":{}}");
            Add("empty-object", "{}");
            Add("empty-array", "[]");
            Add("empty-object-space", "{ }");
            Add("empty-array-space", "[ ]");
            Add("empty-object-newline", "{{U+000A}}");
            Add("nested-empty-object", @"{""a"":{}}");
            Add("nested-empty-array", @"{""a"":[]}");
            Add("empties-in-array", "[{},[],{ },[ ]]");
            Add("empties-deep", "[[[{}]]]");
            Add("empty-then-value", @"{""a"":{},""b"":1}");
            Add("flat-object", @"{""a"":1,""b"":""two"",""c"":true,""d"":null}");
            Add("flat-array", "[1,2,3]");
            Add("array-of-objects", @"[{""a"":1},{""b"":2}]");
            Add("object-of-arrays", @"{""a"":[1,2],""b"":[[3],[4,5]]}");
            Add("mixed-deep", @"{""a"":{""b"":{""c"":{""d"":[1,{""e"":[]}]}}}}");

            // Whitespace outside strings, and input that is already pretty.
            Add("spaced", @" { ""a"" : 1 , ""b"" : [ 1 , 2 ] } ");
            Add("tabs-crlf", @"{{U+0009}""a""{U+0009}:{U+000D}{U+000A}[1,{U+000D}{U+000A}2]}");
            Add("already-pretty", @"{{U+000A}    ""a"": 1,{U+000A}    ""b"": [{U+000A}        2{U+000A}    ]{U+000A}}");
            Add("already-pretty-crlf", @"{{U+000D}{U+000A}    ""a"": 1,{U+000D}{U+000A}    ""b"": [{U+000D}{U+000A}        2{U+000D}{U+000A}    ]{U+000D}{U+000A}}");
            Add("pretty-two-space", @"{{U+000A}  ""a"": {{U+000A}    ""b"": 1{U+000A}  }{U+000A}}");

            // Strings and escapes.
            Add("string-spaces", @"{""a"":""b c  d""}");
            Add("string-structure", @"{""a"":""{[,:]}""}");
            Add("escaped-quote", @"{""a"":""x`""y""}");
            Add("escaped-backslash-end", @"{""a"":""x``"",""b"":1}");
            Add("escaped-backslash-only", @"{""a"":""``""}");
            Add("escaped-backslash-then-quote", @"{""a"":""```"""",""b"":1}");
            Add("escape-sequences", @"{""a"":""`n`t`r`b`f`/""}");
            Add("unicode-escapes", @"{""a"":""`u00e9`u4e2d`uD83D`uDE00""}");
            Add("single-quoted", "{'a':'b'}");
            Add("single-in-double", @"{""a"":""it's""}");
            Add("double-in-single", @"{'a':'say ""hi""'}");
            Add("escaped-single", "{'a':'it`'s'}");
            Add("single-with-structure", "{'a':'{[,]}'}");

            // Top-level scalars and numbers.
            Add("number", "1");
            Add("negative-zero", "-0");
            Add("decimal", "1.0");
            Add("exponent", "1e3");
            Add("leading-zeros", "007");
            Add("string-scalar", @"""text""");
            Add("true", "true");
            Add("null", "null");
            Add("padded-number", "  42  ");
            Add("numbers", "[1.5e-10,-0,0.0,1E+2]");

            // Text outside ASCII, control characters, and whitespace the printer does not drop.
            Add("non-ascii", @"{""{U+00E9}"":""{U+4E2D}{U+6587}""}");
            Add("emoji", @"{""a"":""{U+D83D}{U+DE00}""}");
            Add("lone-high-in-string", @"{""a"":""{U+D83D}""}");
            Add("lone-low-in-string", @"[""{U+DE00}""]");
            Add("lone-high-outside", "[{U+D83D}]");
            Add("nul-in-string", @"{""a"":""{U+0000}""}");
            Add("nul-outside", "[{U+0000}]");
            Add("control-in-string", @"{""a"":""{U+0001}""}");
            Add("line-separator-in-string", @"{""a"":""x{U+2028}y""}");
            Add("raw-newline-in-string", @"{""a"":""line1{U+000A}line2""}");
            Add("raw-crlf-in-string", @"{""a"":""x{U+000D}{U+000A}y""}");
            Add("bom-start", @"{U+FEFF}{""a"":1}");
            Add("nbsp-between", @"{""a"":{U+00A0}1}");
            Add("zero-width-between", @"{""a"":{U+200B}1}");
            Add("ideographic-space-between", @"{""a"":{U+3000}1}");
            Add("rtl", @"{""{U+05D0}"":""{U+202E}abc""}");

            // Blank input of every kind.
            Add("blank-empty", "");
            Add("blank-space", " ");
            Add("blank-mixed", "{U+0009}{U+000D}{U+000A} ");
            Add("blank-nbsp", "{U+00A0}");
            Add("blank-bom", "{U+FEFF}");
            Add("blank-zero-width", "{U+200B}");
            Add("blank-line-separator", "{U+2028}");
            Add("blank-ideographic", "{U+3000}");
            Add("blank-vertical-tab", "{U+000B}");
            Add("blank-form-feed", "{U+000C}");
            Add("blank-next-line", "{U+0085}");

            // Not JSON, or not balanced.
            Add("stray-close-brace", "}");
            Add("stray-close-bracket", "]");
            Add("extra-close", @"{""a"":1}}");
            Add("wrong-close-object", @"{""a"":[1}");
            Add("wrong-close-array", "[1,2}");
            Add("close-first", "}{");
            Add("unclosed-object", @"{""a"":1");
            Add("unclosed-nested", "{{{");
            Add("unclosed-array", "[1,[2");
            Add("unterminated-string", @"{""a"":""b");
            Add("unbalanced-quotes", @"{""a"":""b,""c"":1}");
            Add("trailing-comma-object", @"{""a"":1,}");
            Add("trailing-comma-array", "[1,2,]");
            Add("line-comment", @"{""a"":1, // a note about a{U+000A}""b"":2}");
            Add("block-comment", @"{""a"":1/*note*/,""b"":2}");
            Add("bare-words", "{a:b,c:[d]}");
            Add("two-documents", "{}{}");
            Add("two-documents-values", @"{""a"":1}{""b"":2}");
            Add("colon-in-array", "[1:2]");
            Add("commas-only", ",,,");
            Add("colons-only", "::");
            Add("lone-quote", @"""");
            Add("backslash-outside", "[1,`2]");
            Add("backslash-at-end", @"""abc`");
            Add("leading-text", @"xyz{""a"":1}");
            Add("js-literals", "[NaN,Infinity,-Infinity,undefined]");
            Add("hex-number", "[0x1F]");
            Add("stray-close-after-document", @"{""a"":1}]");
            Add("close-in-string-then-stray", @"{""a"":""}""}}");
            return c;
        }

        private static string Input(string name)
        {
            return Corpus.First(kv => kv.Key == name).Value;
        }

        // ---------- helpers ----------

        private static void Add(string group, string name, Func<object> call)
        {
            All.Add(new Case { Group = group, Name = name, Result = Try(call) });
        }

        private static object Try(Func<object> step)
        {
            try
            {
                return step();
            }
            catch (Exception e)
            {
                return Describe(e);
            }
        }

        public static JsonObject Describe(Exception e)
        {
            var o = new JsonObject();
            AddException(o, e);
            return o;
        }

        private static void AddException(JsonObject o, Exception e)
        {
            o.Add("$throws", e.GetType().FullName + ": " + FirstLine(e.Message));
            if (e is ArgumentException ae && ae.ParamName != null)
            {
                o.Add("$param", ae.ParamName);
            }

            // Messages worded by a dependency change with its version, not with this library; mark them.
            var from = e.TargetSite?.DeclaringType?.Assembly.GetName().Name;
            if (from == "System.Text.Json" || from == "System.Web.Extensions")
            {
                o.Add("$from", from);
            }

            var inner = new List<object>();
            for (var x = e.InnerException; x != null; x = x.InnerException)
            {
                inner.Add(x.GetType().FullName + ": " + FirstLine(x.Message));
            }

            if (inner.Count > 0)
            {
                o.Add("$inner", inner);
            }
        }

        private static string FirstLine(string message)
        {
            if (message == null)
            {
                return "";
            }

            var i = message.IndexOfAny(new[] { (char)13, (char)10 });
            return i < 0 ? message : message.Substring(0, i);
        }

        // A string as recorded: itself, or a summary when it is long.
        public static object Text(string s)
        {
            if (s == null || s.Length <= Summarize)
            {
                return s;
            }

            var o = new JsonObject();
            o.Add("length", s.Length);
            o.Add("sha256", Sha256(s));
            o.Add("head", s.Substring(0, 200));
            o.Add("tail", s.Substring(s.Length - 200));
            return o;
        }

        private static string Sha256(string s)
        {
            using (var h = SHA256.Create())
            {
                return string.Concat(h.ComputeHash(Encoding.Unicode.GetBytes(s)).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }

        // What a writer received before the call returned or threw.
        private static object Written(StringWriter w, Exception e)
        {
            if (e == null)
            {
                return Text(w.ToString());
            }

            var o = new JsonObject();
            o.Add("written", Text(w.ToString()));
            AddException(o, e);
            return o;
        }

        private static object Same(object result, string name)
        {
            return Json.Write(result) == Json.Write(Printed[name]) ? "=print" : result;
        }

        private static string Print(string s)
        {
            return s.PrettyPrintJson();
        }

        private static JsonPrettyPrinter NewPrinter()
        {
#if JPP_V1
            return new JsonPrettyPrinter(new JsonPPStrategyContext());
#else
            return new JsonPrettyPrinter();
#endif
        }

        private static string ToJ(object o)
        {
#if JPP_V1
            return o.ToJSON();
#else
            return o.ToJson();
#endif
        }

        private static string ToJP(object o)
        {
#if JPP_V1
            return o.ToJSON(true);
#else
            return o.ToJson(true);
#endif
        }

        // ---------- printing: every input through PrettyPrintJson() ----------

        private static void PrintCases()
        {
            foreach (var kv in Corpus)
            {
                var result = Try(() => Text(Print(kv.Value)));
                Printed[kv.Key] = result;
                All.Add(new Case { Group = "print", Name = kv.Key, Result = result });
            }
        }

        // ---------- the other entry points agree with PrettyPrintJson(), or record what they did instead ----------

        private static void SurfaceCases()
        {
            foreach (var kv in Corpus)
            {
                var s = kv.Value;
                Add("printer.string", kv.Key, () => Same(Try(() => Text(NewPrinter().PrettyPrint(s))), kv.Key));
#if !JPP_V1
                Add("printer.span", kv.Key, () => Same(Try(() => Text(new JsonPrettyPrinter().PrettyPrint(s.AsSpan()))), kv.Key));
                Add("printer.string.writer", kv.Key, () =>
                {
                    var w = new StringWriter(CultureInfo.InvariantCulture);
                    try
                    {
                        new JsonPrettyPrinter().PrettyPrint(s, w);
                    }
                    catch (Exception e)
                    {
                        return Same(Written(w, e), kv.Key);
                    }

                    return Same(Written(w, null), kv.Key);
                });
                Add("printer.span.writer", kv.Key, () =>
                {
                    var w = new StringWriter(CultureInfo.InvariantCulture);
                    try
                    {
                        new JsonPrettyPrinter().PrettyPrint(s.AsSpan(), w);
                    }
                    catch (Exception e)
                    {
                        return Same(Written(w, e), kv.Key);
                    }

                    return Same(Written(w, null), kv.Key);
                });
                Add("extension.default-options", kv.Key, () => Same(Try(() => Text(s.PrettyPrintJson(JsonPrettyPrintOptions.Default))), kv.Key));
                Add("printer.default-options", kv.Key, () => Same(Try(() => Text(new JsonPrettyPrinter(JsonPrettyPrintOptions.Default).PrettyPrint(s))), kv.Key));
#endif
            }
        }

        // ---------- printing the output again gives the same text ----------

        private static void IdempotenceCases()
        {
            foreach (var kv in Corpus)
            {
                if (!(Printed[kv.Key] is string first))
                {
                    continue;
                }

                Add("idempotent", kv.Key, () =>
                {
                    var second = Print(first);
                    return second == first ? (object)true : Text(second);
                });
            }
        }

        // ---------- layout options ----------

        private static readonly string[] OptionInputs =
        {
            "readme", "empty-object", "nested-empty-object", "empties-in-array", "flat-array", "object-of-arrays",
            "string-structure", "raw-newline-in-string", "number", "unclosed-object",
        };

        private static void OptionCases()
        {
#if !JPP_V1
            var sets = new List<KeyValuePair<string, JsonPrettyPrintOptions>>
            {
                new KeyValuePair<string, JsonPrettyPrintOptions>("indent-0", new JsonPrettyPrintOptions { IndentSize = 0 }),
                new KeyValuePair<string, JsonPrettyPrintOptions>("indent-1", new JsonPrettyPrintOptions { IndentSize = 1 }),
                new KeyValuePair<string, JsonPrettyPrintOptions>("indent-2", new JsonPrettyPrintOptions { IndentSize = 2 }),
                new KeyValuePair<string, JsonPrettyPrintOptions>("indent-8", new JsonPrettyPrintOptions { IndentSize = 8 }),
                new KeyValuePair<string, JsonPrettyPrintOptions>("tabs", new JsonPrettyPrintOptions { UseTabs = true }),
                new KeyValuePair<string, JsonPrettyPrintOptions>("tabs-indent-2", new JsonPrettyPrintOptions { UseTabs = true, IndentSize = 2 }),
                new KeyValuePair<string, JsonPrettyPrintOptions>("tabs-indent-0", new JsonPrettyPrintOptions { UseTabs = true, IndentSize = 0 }),
                new KeyValuePair<string, JsonPrettyPrintOptions>("newline-lf", new JsonPrettyPrintOptions { NewLine = LF }),
                new KeyValuePair<string, JsonPrettyPrintOptions>("newline-crlf", new JsonPrettyPrintOptions { NewLine = CRLF }),
                new KeyValuePair<string, JsonPrettyPrintOptions>("newline-cr", new JsonPrettyPrintOptions { NewLine = new string((char)13, 1) }),
                new KeyValuePair<string, JsonPrettyPrintOptions>("newline-empty", new JsonPrettyPrintOptions { NewLine = "" }),
                new KeyValuePair<string, JsonPrettyPrintOptions>("newline-space-indent-0", new JsonPrettyPrintOptions { NewLine = " ", IndentSize = 0 }),
                new KeyValuePair<string, JsonPrettyPrintOptions>("newline-text", new JsonPrettyPrintOptions { NewLine = "<br>" }),
                new KeyValuePair<string, JsonPrettyPrintOptions>("indent-2-crlf", new JsonPrettyPrintOptions { IndentSize = 2, NewLine = CRLF }),
                new KeyValuePair<string, JsonPrettyPrintOptions>("default-instance", JsonPrettyPrintOptions.Default),
                new KeyValuePair<string, JsonPrettyPrintOptions>("new-instance", new JsonPrettyPrintOptions()),
            };
            foreach (var set in sets)
            {
                foreach (var name in OptionInputs)
                {
                    var s = Input(name);
                    var o = set.Value;
                    Add("options." + set.Key, name, () => Text(s.PrettyPrintJson(o)));
                }
            }
#endif
        }

        private static void OptionErrorCases()
        {
#if !JPP_V1
            Add("options.errors", "indent-negative", () => new JsonPrettyPrintOptions { IndentSize = -1 }.IndentSize);
            Add("options.errors", "indent-min-value", () => new JsonPrettyPrintOptions { IndentSize = int.MinValue }.IndentSize);
            Add("options.errors", "newline-null", () => new JsonPrettyPrintOptions { NewLine = null }.NewLine);
            Add("options.errors", "indent-max-value-options", () => new JsonPrettyPrintOptions { IndentSize = int.MaxValue }.IndentSize);
            Add("options.errors", "indent-max-value-printer", () => new JsonPrettyPrinter(new JsonPrettyPrintOptions { IndentSize = int.MaxValue }).Options.IndentSize);
            Add("options.errors", "indent-max-value-extension", () => Text(Input("flat-array").PrettyPrintJson(new JsonPrettyPrintOptions { IndentSize = int.MaxValue })));
            Add("options.errors", "indent-max-value-with-tabs", () => Text(Input("flat-array").PrettyPrintJson(new JsonPrettyPrintOptions { IndentSize = int.MaxValue, UseTabs = true })));
            Add("options.errors", "indent-100000", () => Text(Input("object-of-arrays").PrettyPrintJson(new JsonPrettyPrintOptions { IndentSize = 100000 })));
            Add("options.errors", "newline-10000-chars", () => Text(Input("flat-array").PrettyPrintJson(new JsonPrettyPrintOptions { NewLine = new string('x', 10000) })));
#endif
        }

        private static void OptionRecordCases()
        {
#if !JPP_V1
            var d = JsonPrettyPrintOptions.Default;
            Add("options.record", "default-values", () =>
            {
                var o = new JsonObject();
                o.Add("IndentSize", d.IndentSize);
                o.Add("UseTabs", d.UseTabs);
                o.Add("NewLine", d.NewLine);
                return o;
            });
            Add("options.record", "default-to-string", () => d.ToString());
            Add("options.record", "custom-to-string", () => new JsonPrettyPrintOptions { IndentSize = 2, UseTabs = true, NewLine = CRLF }.ToString());
            Add("options.record", "default-same-instance", () => ReferenceEquals(JsonPrettyPrintOptions.Default, JsonPrettyPrintOptions.Default));
            Add("options.record", "new-equals-default", () => new JsonPrettyPrintOptions().Equals(d));
            Add("options.record", "new-operator-equals-default", () => new JsonPrettyPrintOptions() == d);
            Add("options.record", "new-hash-equals-default-hash", () => new JsonPrettyPrintOptions().GetHashCode() == d.GetHashCode());
            Add("options.record", "different-newline-not-equal", () => new JsonPrettyPrintOptions { NewLine = CRLF }.Equals(d));
            Add("options.record", "with-indent", () => (d with { IndentSize = 2 }).ToString());
            Add("options.record", "with-leaves-default", () => (d with { IndentSize = 2 }) != null && d.IndentSize == 4);
            Add("options.record", "with-nothing-equal-not-same", () =>
            {
                var copy = d with { };
                return copy.Equals(d) + " " + ReferenceEquals(copy, d);
            });
            Add("options.record", "printer-default-options-is-default", () => ReferenceEquals(new JsonPrettyPrinter().Options, d));
            Add("options.record", "printer-keeps-options-instance", () =>
            {
                var o = new JsonPrettyPrintOptions { IndentSize = 3 };
                return ReferenceEquals(new JsonPrettyPrinter(o).Options, o);
            });
            Add("options.record", "equals-null", () => d.Equals(null));
#endif
        }

        // ---------- null arguments ----------

        private static void NullCases()
        {
            Add("nulls", "extension", () => ((string)null).PrettyPrintJson());
            Add("nulls", "printer-string", () => NewPrinter().PrettyPrint((string)null));
#if JPP_V1
            Add("nulls", "printer-null-context", () => new JsonPrettyPrinter(null).PrettyPrint("{}"));
#else
            Add("nulls", "extension-options-null-input", () => ((string)null).PrettyPrintJson(JsonPrettyPrintOptions.Default));
            Add("nulls", "extension-null-options", () => "{}".PrettyPrintJson(null));
            Add("nulls", "extension-both-null", () => ((string)null).PrettyPrintJson(null));
            Add("nulls", "printer-null-options", () => new JsonPrettyPrinter((JsonPrettyPrintOptions)null).PrettyPrint("{}"));
            Add("nulls", "printer-string-writer-null-input", () =>
            {
                var w = new StringWriter(CultureInfo.InvariantCulture);
                try
                {
                    new JsonPrettyPrinter().PrettyPrint((string)null, w);
                }
                catch (Exception e)
                {
                    return Written(w, e);
                }

                return Written(w, null);
            });
            Add("nulls", "printer-string-null-writer", () =>
            {
                new JsonPrettyPrinter().PrettyPrint("{}", null);
                return "returned";
            });
            Add("nulls", "printer-blank-string-null-writer", () =>
            {
                new JsonPrettyPrinter().PrettyPrint("  ", null);
                return "returned";
            });
            Add("nulls", "printer-span-null-writer", () =>
            {
                new JsonPrettyPrinter().PrettyPrint("{}".AsSpan(), null);
                return "returned";
            });
            Add("nulls", "printer-null-string-null-writer", () =>
            {
                new JsonPrettyPrinter().PrettyPrint((string)null, null);
                return "returned";
            });
            Add("nulls", "printer-empty-span", () => new JsonPrettyPrinter().PrettyPrint(ReadOnlySpan<char>.Empty));
            Add("nulls", "printer-default-span", () => new JsonPrettyPrinter().PrettyPrint(default(ReadOnlySpan<char>)));
            Add("nulls", "printer-default-span-writer", () =>
            {
                var w = new StringWriter(CultureInfo.InvariantCulture);
                new JsonPrettyPrinter().PrettyPrint(default(ReadOnlySpan<char>), w);
                return Written(w, null);
            });
            Add("nulls", "tojson-null-options", () => new Poco { Name = "x" }.ToJson((JsonSerializerOptions)null, true));
            Add("nulls", "tojson-null-typeinfo", () => 1.ToJson((JsonTypeInfo<int>)null));
            Add("nulls", "fromjson-null-options", () => Show("42".DeserializeFromJson<int>((JsonSerializerOptions)null)));
            Add("nulls", "fromjson-null-typeinfo", () => Show("42".DeserializeFromJson((JsonTypeInfo<int>)null)));
            Add("nulls", "fromjson-null-json-typeinfo", () => Show(((string)null).DeserializeFromJson(IntInfo())));
            Add("nulls", "fromjson-null-json-null-typeinfo", () => Show(((string)null).DeserializeFromJson((JsonTypeInfo<int>)null)));
            Add("nulls", "fromjson-null-json-options", () => Show(((string)null).DeserializeFromJson<int>(new JsonSerializerOptions())));
#endif
            Add("nulls", "tojson-null", () => ToJ(null));
            Add("nulls", "tojson-null-pretty", () => ToJP(null));
            Add("nulls", "fromjson-null-json", () => Show(((string)null).DeserializeFromJson<int>()));
        }

        // ---------- one printer, several documents: no state leaks from a malformed one ----------

        private static void ReuseCases()
        {
            Sequence("printer-unterminated-string", J(@"{""a"":""b"), J(@"{""c"":1}"));
            Sequence("printer-stray-close", "}", J(@"{""c"":[1]}"));
            Sequence("printer-unclosed", "{{{", "[1]");
            Sequence("printer-pending-escape", J(@"{""a"":""x`"), J(@"{""b"":""y""}"));
            Sequence("printer-single-quote", "{'a':'b", "{'c':1}");
            Sequence("printer-wrong-close", "[1}", "[{}]");
            Add("reuse", "extension-after-unterminated", () => new List<object> { Try(() => Print(J(@"{""a"":""b"))), Try(() => Print(J(@"{""c"":1}"))) });
            Add("reuse", "extension-after-stray-close", () => new List<object> { Try(() => Print("]")), Try(() => Print("[1,{}]")) });
            Add("reuse", "extension-after-pending-escape", () => new List<object> { Try(() => Print(J(@"[""`"))), Try(() => Print(J(@"[""a""]"))) });
        }

        private static void Sequence(string name, params string[] inputs)
        {
            Add("reuse", name, () =>
            {
                var p = NewPrinter();
                return inputs.Select(s => Try(() => Text(p.PrettyPrint(s)))).ToList();
            });
        }

        // ---------- writers ----------

        private static void WriterCases()
        {
#if !JPP_V1
            Add("writer", "appends-after-existing-text", () =>
            {
                var w = new StringWriter(CultureInfo.InvariantCulture);
                w.Write("prefix:");
                new JsonPrettyPrinter().PrettyPrint(Input("flat-array"), w);
                w.Write("|after");
                return w.ToString();
            });
            Add("writer", "blank-writes-nothing", () =>
            {
                var w = new StringWriter(CultureInfo.InvariantCulture);
                new JsonPrettyPrinter().PrettyPrint("  ", w);
                return w.ToString().Length;
            });
            Add("writer", "writer-newline-ignored", () =>
            {
                var w = new StringWriter(CultureInfo.InvariantCulture) { NewLine = "<NL>" };
                new JsonPrettyPrinter().PrettyPrint(Input("flat-array"), w);
                return w.ToString();
            });
            Add("writer", "partial-before-wrong-close", () =>
            {
                var w = new StringWriter(CultureInfo.InvariantCulture);
                try
                {
                    new JsonPrettyPrinter().PrettyPrint(J(@"{""a"":[1,2},""b"":3}"), w);
                }
                catch (Exception e)
                {
                    return Written(w, e);
                }

                return Written(w, null);
            });
            Add("writer", "two-documents-one-writer", () =>
            {
                var w = new StringWriter(CultureInfo.InvariantCulture);
                var p = new JsonPrettyPrinter();
                p.PrettyPrint("[1]", w);
                p.PrettyPrint("{}", w);
                return w.ToString();
            });
#endif
        }

        // ---------- big and deep input ----------

        private static string Document(int minLength)
        {
            var sb = new StringBuilder(minLength + 1000);
            sb.Append('[');
            for (var i = 0; sb.Length < minLength; i++)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }

                var n = i.ToString(CultureInfo.InvariantCulture);
                sb.Append(@"{""id"":").Append(n)
                  .Append(@",""name"":""item ").Append(n)
                  .Append(@""",""tags"":[""a"",""b"",""c""],""nested"":{""x"":").Append(n)
                  .Append(@",""y"":[1,2,{""z"":null}],""empty"":{}},""text"":""lorem ipsum, dolor: {sit} [amet] ")
                  .Append(BS).Append('"').Append(n).Append(BS).Append('"').Append(@"""}");
            }

            sb.Append(']');
            return sb.ToString();
        }

        private static void BigCases()
        {
            Add("big", "document-1mb", () => Text(Print(Document(1000000))));
            Add("big", "document-4mb", () => Text(Print(Document(4000000))));
            Add("big", "string-1m-chars", () => Text(Print(@"{""a"":""" + new string('x', 1000000) + @"""}")));
            Add("big", "whitespace-1m", () => Text(Print("{" + new string(' ', 1000000) + "}")));
            Add("big", "array-1m-zeros", () => Text(Print("[" + string.Join(",", Enumerable.Repeat("0", 1000000)) + "]")));
            Add("big", "deep-arrays-1000", () => Text(Print(new string('[', 1000) + new string(']', 1000))));
            Add("big", "deep-objects-1000", () => Text(Print(string.Concat(Enumerable.Repeat(@"{""a"":", 1000)) + "1" + new string('}', 1000))));
            Add("big", "unclosed-2000", () => Text(Print(new string('[', 2000))));
#if !JPP_V1
            Add("big", "unclosed-20000-indent-0", () => Text(new string('[', 20000).PrettyPrintJson(new JsonPrettyPrintOptions { IndentSize = 0 })));
            Add("big", "deep-balanced-20000-indent-0", () => Text((new string('[', 20000) + new string(']', 20000)).PrettyPrintJson(new JsonPrettyPrintOptions { IndentSize = 0 })));
            Add("big", "document-1mb-writer-equals-string", () =>
            {
                var doc = Document(1000000);
                var w = new StringWriter(CultureInfo.InvariantCulture);
                new JsonPrettyPrinter().PrettyPrint(doc, w);
                return w.ToString() == Print(doc);
            });
#endif
        }

        // ---------- threads: PrettyPrintJson() from many threads at once ----------

        private static void ThreadCases()
        {
            Add("threads", "eight-threads-same-answers", () =>
            {
                var inputs = Corpus.Where(kv => Printed[kv.Key] is string).ToList();
                var expected = inputs.Select(kv => (string)Printed[kv.Key]).ToList();
                var mismatches = 0;
                var failures = 0;
                var threads = Enumerable.Range(0, 8).Select(t => new Thread(() =>
                {
                    for (var r = 0; r < 20; r++)
                    {
                        for (var i = 0; i < inputs.Count; i++)
                        {
                            try
                            {
                                if (Print(inputs[(i + t) % inputs.Count].Value) != expected[(i + t) % inputs.Count])
                                {
                                    Interlocked.Increment(ref mismatches);
                                }
                            }
                            catch (Exception)
                            {
                                Interlocked.Increment(ref failures);
                            }
                        }
                    }
                })).ToList();
                threads.ForEach(x => x.Start());
                threads.ForEach(x => x.Join());
                return mismatches == 0 && failures == 0;
            });
        }

        // ---------- cultures: nothing depends on the thread's culture ----------

        private static object OnThread(string culture, Func<object> f)
        {
            object result = null;
            var t = new Thread(() =>
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
                result = Try(f);
            });
            t.Start();
            t.Join();
            return result;
        }

        private static void CultureCases()
        {
            foreach (var culture in new[] { "tr-TR", "de-DE", "ar-SA" })
            {
                Add("culture", culture + ".print-readme", () => OnThread(culture, () => Print(Input("readme")) == (string)Printed["readme"]));
                Add("culture", culture + ".print-numbers", () => OnThread(culture, () => Print(Input("numbers"))));
                Add("culture", culture + ".tojson-double", () => OnThread(culture, () => ToJ(1.5)));
                Add("culture", culture + ".tojson-date", () => OnThread(culture, () => ToJ(new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc))));
                Add("culture", culture + ".fromjson-double", () => OnThread(culture, () => Show("1.5".DeserializeFromJson<double>())));
            }
        }

        // ---------- ToJson (ToJSON in 1.x) ----------

        private static List<KeyValuePair<string, object>> Values()
        {
            var v = new List<KeyValuePair<string, object>>();
            void Add(string name, object value) => v.Add(new KeyValuePair<string, object>(name, value));
            var cycle = new Node { Name = "a" };
            cycle.Next = new Node { Name = "b", Next = cycle };

            Add("int", 42);
            Add("long-max", long.MaxValue);
            Add("uint-max", uint.MaxValue);
            Add("double-tenth", 0.1);
            Add("double-huge", 1e300);
            Add("double-negative-zero", -0.0);
            Add("double-nan", double.NaN);
            Add("double-infinity", double.PositiveInfinity);
            Add("float-tenth", 0.1f);
            Add("decimal", 1.10m);
            Add("bool", true);
            Add("char", 'x');
            Add("string", "hello");
            Add("string-html", J(@"<b>&'{U+00E9}""</b>"));
            Add("string-controls", J("a{U+0009}b{U+000A}c{U+0001}d{U+2028}e`f"));
            Add("string-emoji", J("{U+D83D}{U+DE00}"));
            Add("string-lone-surrogate", J("x{U+D83D}y"));
            Add("date-utc", new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            Add("date-offset", new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.FromHours(2)));
            Add("guid", new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"));
            Add("timespan", new TimeSpan(1, 2, 3));
            Add("enum", DayOfWeek.Monday);
            Add("int-array", new[] { 1, 2, 3 });
            Add("string-list", new List<string> { "a", "b" });
            Add("dictionary", new Dictionary<string, int> { { "b", 2 }, { "a", 1 } });
            Add("dictionary-int-keys", new Dictionary<int, string> { { 1, "one" } });
            Add("anonymous", new { Name = "Mark", Tags = new[] { "a", "b" } });
            Add("anonymous-nested", new { A = new { B = new object[] { 1, "x", null, true } }, Empty = new { } });
            Add("poco", new Poco { Name = "n", Count = 2, Tags = new[] { "t" }, When = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc), Nested = new Inner { Value = 1.5, Flag = true }, Field = "f" });
            Add("poco-defaults", new Poco());
            Add("cycle", cycle);
            Add("bytes", new byte[] { 1, 2, 3 });
            Add("object-array", new object[] { 1, "a", null, true });
            Add("uri", new Uri("https://example.com/a?b=c"));
            Add("version", new Version(1, 2, 3, 4));
#if !JPP_V1
            Add("date-unspecified", new DateTime(2010, 1, 1));
#endif
            return v;
        }

        private static void ToJsonCases()
        {
            foreach (var kv in Values())
            {
                var value = kv.Value;
                Add("tojson", kv.Key, () => Text(ToJ(value)));
                Add("tojson.pretty", kv.Key, () => Text(ToJP(value)));
            }

#if !JPP_V1
            var poco = new Poco { Name = "n", Count = 2, Tags = new[] { "t" }, When = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc), Nested = new Inner { Value = 1.5 } };
            Add("tojson.options", "camel-case-pretty", () => poco.ToJson(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }, true));
            Add("tojson.options", "null-options-pretty", () => poco.ToJson((JsonSerializerOptions)null, true));
            Add("tojson.options", "null-options-compact", () => poco.ToJson((JsonSerializerOptions)null));
            Add("tojson.options", "write-indented-not-pretty", () => poco.ToJson(new JsonSerializerOptions { WriteIndented = true }, false));
            Add("tojson.options", "write-indented-and-pretty", () => poco.ToJson(new JsonSerializerOptions { WriteIndented = true }, true));
            Add("tojson.options", "relaxed-escaping", () => J(@"<b>&'{U+00E9}""</b>").ToJson(new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
            Add("tojson.options", "include-fields", () => poco.ToJson(new JsonSerializerOptions { IncludeFields = true }));
            Add("tojson.options", "nan-allowed", () => double.NaN.ToJson(new JsonSerializerOptions { NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals }));
            Add("tojson.options", "cycle-preserved", () => Values().First(x => x.Key == "cycle").Value.ToJson(new JsonSerializerOptions { ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve }));
            Add("tojson.typeinfo", "int", () => 42.ToJson(IntInfo()));
            Add("tojson.typeinfo", "int-pretty", () => 42.ToJson(IntInfo(), true));
            Add("tojson.typeinfo", "int-array-pretty", () => new[] { 1, 2 }.ToJson((JsonTypeInfo<int[]>)JsonSerializerOptions.Default.GetTypeInfo(typeof(int[])), true));
            Add("tojson.typeinfo", "string", () => J(@"a""b").ToJson((JsonTypeInfo<string>)JsonSerializerOptions.Default.GetTypeInfo(typeof(string))));
            Add("tojson.typeinfo", "null-string", () => ((string)null).ToJson((JsonTypeInfo<string>)JsonSerializerOptions.Default.GetTypeInfo(typeof(string))));
#endif
        }

#if !JPP_V1
        private static JsonTypeInfo<int> IntInfo()
        {
            return (JsonTypeInfo<int>)JsonSerializerOptions.Default.GetTypeInfo(typeof(int));
        }
#endif

        // ---------- DeserializeFromJson ----------

        private static void FromJsonCases()
        {
            From("int", () => Show(J("42").DeserializeFromJson<int>()));
            From("int-negative-zero", () => Show("-0".DeserializeFromJson<int>()));
            From("int-with-fraction", () => Show("42.0".DeserializeFromJson<int>()));
            From("int-quoted", () => Show(J(@"""42""").DeserializeFromJson<int>()));
            From("int-null", () => Show("null".DeserializeFromJson<int>()));
            From("int-word", () => Show("x".DeserializeFromJson<int>()));
            From("int-empty", () => Show("".DeserializeFromJson<int>()));
            From("int-blank", () => Show("  ".DeserializeFromJson<int>()));
            From("int-exponent", () => Show("1e3".DeserializeFromJson<int>()));
            From("int-overflow", () => Show("2147483648".DeserializeFromJson<int>()));
            From("long-max", () => Show("9223372036854775807".DeserializeFromJson<long>()));
            From("double", () => Show("1.5".DeserializeFromJson<double>()));
            From("double-overflow", () => Show("1e400".DeserializeFromJson<double>()));
            From("decimal", () => Show("1.10".DeserializeFromJson<decimal>()));
            From("bool", () => Show("true".DeserializeFromJson<bool>()));
            From("string", () => Show(J(@"""a`nb""").DeserializeFromJson<string>()));
            From("string-null", () => Show("null".DeserializeFromJson<string>()));
            From("string-from-number", () => Show("42".DeserializeFromJson<string>()));
            From("string-single-quoted", () => Show("'a'".DeserializeFromJson<string>()));
            From("object-from-object", () => Show(J(@"{""a"":1,""b"":[true,null]}").DeserializeFromJson<object>()));
            From("object-from-array", () => Show("[1,2]".DeserializeFromJson<object>()));
            From("object-from-number", () => Show("1".DeserializeFromJson<object>()));
            From("dictionary", () => Show(J(@"{""a"":1,""b"":""x"",""c"":[1],""d"":null,""e"":{""f"":2.5}}").DeserializeFromJson<Dictionary<string, object>>()));
            From("int-array", () => Show("[1,2]".DeserializeFromJson<int[]>()));
            From("string-list", () => Show(J(@"[""a"",""b""]").DeserializeFromJson<List<string>>()));
            From("poco", () => Show(J(@"{""Name"":""n"",""Count"":2,""Tags"":[""t""],""When"":""2010-01-01T00:00:00Z"",""Nested"":{""Value"":1.5,""Flag"":true},""Field"":""f""}").DeserializeFromJson<Poco>()));
            From("poco-lowercase-keys", () => Show(J(@"{""name"":""n"",""count"":2}").DeserializeFromJson<Poco>()));
            From("poco-extra-property", () => Show(J(@"{""Name"":""n"",""Other"":1}").DeserializeFromJson<Poco>()));
            From("poco-wrong-type", () => Show(J(@"{""Count"":""two""}").DeserializeFromJson<Poco>()));
            From("poco-comment", () => Show(J(@"{""Name"":""n"" /* c */}").DeserializeFromJson<Poco>()));
            From("poco-trailing-comma", () => Show(J(@"{""Name"":""n"",}").DeserializeFromJson<Poco>()));
            From("poco-single-quotes", () => Show("{'Name':'n'}".DeserializeFromJson<Poco>()));
            From("poco-unquoted-keys", () => Show("{Name:1}".DeserializeFromJson<Poco>()));
            From("poco-truncated", () => Show(J(@"{""Name"":""n""").DeserializeFromJson<Poco>()));
            From("poco-empty-object", () => Show("{}".DeserializeFromJson<Poco>()));
            From("date-iso", () => Show(J(@"""2010-01-01T00:00:00""").DeserializeFromJson<DateTime>()));
            From("date-iso-utc", () => Show(J(@"""2010-01-01T00:00:00Z""").DeserializeFromJson<DateTime>()));
            From("date-microsoft-format", () => Show(J(@"""`/Date(1262304000000)`/""").DeserializeFromJson<DateTime>()));
            From("guid", () => Show(J(@"""0f8fad5b-d9cb-469f-a165-70867728950e""").DeserializeFromJson<Guid>()));
            From("enum-number", () => Show("1".DeserializeFromJson<DayOfWeek>()));
            From("enum-name", () => Show(J(@"""Monday""").DeserializeFromJson<DayOfWeek>()));
            From("two-documents", () => Show("1 2".DeserializeFromJson<int>()));
            From("pretty-printed-poco", () => Show(J(@"{""Name"":""n"",""Tags"":[""t""]}").PrettyPrintJson().DeserializeFromJson<Poco>()));
#if !JPP_V1
            From("options.case-insensitive", () => Show(J(@"{""name"":""n"",""count"":2}").DeserializeFromJson<Poco>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true })));
            From("options.allow-comments-and-commas", () => Show(J(@"{""Name"":""n"" /* c */,}").DeserializeFromJson<Poco>(new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true })));
            From("options.null-options", () => Show(J(@"{""Name"":""n""}").DeserializeFromJson<Poco>((JsonSerializerOptions)null)));
            From("typeinfo.int", () => Show("42".DeserializeFromJson(IntInfo())));
            From("typeinfo.int-word", () => Show("x".DeserializeFromJson(IntInfo())));
#endif
        }

        private static void From(string name, Func<object> call)
        {
            Add("fromjson", name, call);
        }

        // A deserialized value as recorded: its type and value, without assembly names, the same on every runtime.
        public static object Show(object v)
        {
            if (v == null)
            {
                return null;
            }

            var type = TypeName(v.GetType());
            switch (v)
            {
                case string s:
                    return "string:" + s;
                case bool b:
                    return "bool:" + (b ? "true" : "false");
                case DateTime d:
                    return "DateTime:" + d.ToString("o", CultureInfo.InvariantCulture) + " " + d.Kind;
                case DateTimeOffset d:
                    return "DateTimeOffset:" + d.ToString("o", CultureInfo.InvariantCulture);
                case double d:
                    return "Double:" + d.ToString("R", CultureInfo.InvariantCulture);
                case float f:
                    return "Single:" + f.ToString("R", CultureInfo.InvariantCulture);
#if !JPP_V1
                case JsonElement e:
                    return "JsonElement:" + e.ValueKind + ":" + e.GetRawText();
#endif
                case IDictionary dict:
                {
                    var o = new JsonObject();
                    o.Add("$type", type);
                    foreach (DictionaryEntry kv in dict)
                    {
                        o.Add(Convert.ToString(kv.Key, CultureInfo.InvariantCulture), Show(kv.Value));
                    }

                    return o;
                }

                case IEnumerable list:
                {
                    var o = new JsonObject();
                    o.Add("$type", type);
                    o.Add("items", list.Cast<object>().Select(Show).ToList());
                    return o;
                }
            }

            var t = v.GetType();
            if (t.IsPrimitive || t.IsEnum || v is decimal || v is Guid || v is TimeSpan)
            {
                return type + ":" + Convert.ToString(v, CultureInfo.InvariantCulture);
            }

            if (t.Namespace == typeof(Cases).Namespace)
            {
                var o = new JsonObject();
                o.Add("$type", type);
                foreach (var p in t.GetProperties())
                {
                    o.Add(p.Name, Show(p.GetValue(v, null)));
                }

                foreach (var f in t.GetFields())
                {
                    o.Add(f.Name, Show(f.GetValue(v)));
                }

                return o;
            }

            return type + ":" + v;
        }

        private static string TypeName(Type t)
        {
            if (!t.IsGenericType)
            {
                return t.IsArray ? TypeName(t.GetElementType()) + "[]" : t.Name;
            }

            var name = t.Name.Substring(0, t.Name.IndexOf('`'));
            return name + "<" + string.Join(",", t.GetGenericArguments().Select(TypeName)) + ">";
        }
    }
}
