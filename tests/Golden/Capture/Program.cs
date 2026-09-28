#nullable disable

// Golden capture of the PUBLISHED JsonPrettyPrinter (package-modernize retrofit, Phase 0, 2026-09-28).
// Usage: dotnet run -c Release -f net48 -p:OldVersion=3.0.1, then the output path as the one argument (or -f net10.0).
// Writes ASCII JSON with LF line endings. The header proves which DLL answered (SHA-256 of the loaded file, comparable
// with the lib/ folders of the nupkg on nuget.org) and records the process bitness and the System.Text.Json version,
// because allocation failures and serializer messages are worded by those, not by the library.
// Never edit a recording; never regenerate it from new code.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using JsonPrettyPrinterPlus;

namespace GoldenCapture
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("usage: Capture <output.json>");
                return 2;
            }

            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            var cases = Cases.Run();

            var lib = typeof(PrettyPrinterExtensions).Assembly;
            var root = new JsonObject();
            root.Add("package", "JsonPrettyPrinter@" + Cases.PackageVersion(lib));
            root.Add("assembly", lib.GetName().Name + " " + lib.GetName().Version);
            root.Add("assemblySha256", Sha256File(lib.Location));
            root.Add("runtime", RuntimeInformation.FrameworkDescription);
            root.Add("os", RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "windows" : RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macos" : "linux");
            root.Add("process", Environment.Is64BitProcess ? "64-bit" : "32-bit");
            root.Add("systemTextJson", Cases.SerializerVersion());
            root.Add("culture", "invariant");
            // 1.x's JavaScriptSerializer reads and writes dates in local time; the 3.x cases depend on no time zone.
            root.Add("timeZone", TimeZoneInfo.Local.Id + " (UTC" + (TimeZoneInfo.Local.BaseUtcOffset < TimeSpan.Zero ? "-" : "+") + TimeZoneInfo.Local.BaseUtcOffset.ToString(@"hh\:mm", CultureInfo.InvariantCulture) + ")");
            root.Add("captured", DateTime.UtcNow.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            root.Add("note", "Golden outputs of the published JsonPrettyPrinter, recorded by tests/Golden/Capture. Never edit; never regenerate from new code.");
            root.Add("caseCount", cases.Count);
            var list = new List<object>();
            foreach (var c in cases)
            {
                var o = new JsonObject();
                o.Add("group", c.Group);
                o.Add("name", c.Name);
                o.Add("result", c.Result);
                list.Add(o);
            }

            root.Add("cases", list);

            File.WriteAllText(args[0], Json.Write(root), new UTF8Encoding(false));
            Console.Error.WriteLine("wrote " + cases.Count + " cases to " + args[0]);
            return 0;
        }

        private static string Sha256File(string path)
        {
            using (var h = SHA256.Create())
            using (var f = File.OpenRead(path))
            {
                return string.Concat(h.ComputeHash(f).Select(b => b.ToString("x2", CultureInfo.InvariantCulture)));
            }
        }
    }
}
