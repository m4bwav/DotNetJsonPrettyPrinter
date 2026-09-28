using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using GoldenCapture;
using NUnit.Framework;

namespace JsonPrettyPrinterPlus.GoldenTests
{
    /// <summary>
    /// From the package-modernize template tests/GoldenTests.cs.template (CachingServiceWithAOPSupport 2.0.0), adapted for one
    /// recording per runtime. Runs the Phase 0 capture's cases (tests/Golden/Capture/Cases.cs and Json.cs, compiled here by
    /// link, unchanged) against this repository's library and compares every answer with what the published 3.0.1 answered
    /// on the same runtime. Answers must be equal as JSON. The one ruled exception class (plan D1): an exception the capture
    /// marked as worded by System.Text.Json is compared by type, parameter and source only while the running
    /// System.Text.Json is not the version the recording names, because a .NET 10 patch on a runner moves those messages.
    /// </summary>
    [TestFixture]
    [NonParallelizable]
    public class GoldenTests
    {
        private static List<Case>? _actual;
        private static Dictionary<string, JsonElement>? _recorded;
        private static string _recordedSerializer = "";
        private static string _recordedProcess = "";

        private static string Recording =>
#if NETFRAMEWORK
            "3.0.1.net48-windows.json";
#else
            "3.0.1.net10.0-windows.json";
#endif

        [OneTimeSetUp]
        public static void RunCapture()
        {
            // The capture ran with every culture invariant (tests/Golden/Capture/Program.cs); threads it starts inherit the default.
            var saved = (Thread.CurrentThread.CurrentCulture, Thread.CurrentThread.CurrentUICulture, CultureInfo.DefaultThreadCurrentCulture, CultureInfo.DefaultThreadCurrentUICulture);
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            try
            {
                _actual = Cases.Run().ToList();
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = saved.Item1;
                Thread.CurrentThread.CurrentUICulture = saved.Item2;
                CultureInfo.DefaultThreadCurrentCulture = saved.Item3;
                CultureInfo.DefaultThreadCurrentUICulture = saved.Item4;
            }

            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Golden", Recording);
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            _recordedSerializer = doc.RootElement.GetProperty("systemTextJson").GetString()!;
            _recordedProcess = doc.RootElement.GetProperty("process").GetString()!;
            _recorded = doc.RootElement.GetProperty("cases").EnumerateArray()
                .ToDictionary(c => Key(c.GetProperty("group").GetString()!, c.GetProperty("name").GetString()!), c => c.GetProperty("result").Clone(), StringComparer.Ordinal);
        }

        private static string Key(string group, string name)
        {
            return group + " | " + name;
        }

        private static bool SerializerMatchesRecording => Cases.SerializerVersion() == _recordedSerializer;

        [Test]
        public void Runs_in_the_recordings_process_bitness()
        {
            Assert.That(Environment.Is64BitProcess ? "64-bit" : "32-bit", Is.EqualTo(_recordedProcess));
        }

        [Test]
        public void Every_recorded_case_runs()
        {
            Assert.That(_actual!.Select(c => Key(c.Group, c.Name)), Is.EquivalentTo(_recorded!.Keys));
        }

        [Test]
        public void Every_answer_matches_the_recording_apart_from_the_named_exception()
        {
            var failures = new List<string>();
            foreach (var c in _actual!)
            {
                var key = Key(c.Group, c.Name);
                var actualText = Json.Write(c.Result);
                using var actual = JsonDocument.Parse(actualText);
                if (!JsonEqual(_recorded![key], actual.RootElement, SerializerMatchesRecording))
                {
                    failures.Add(key + ": expected " + _recorded[key].GetRawText() + ", got " + actualText.Trim());
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void The_named_exception_is_only_used_when_the_serializer_version_differs()
        {
            TestContext.Out.WriteLine("recorded " + _recordedSerializer + ", running " + Cases.SerializerVersion());
            var serializerWorded = _recorded!.Values.Count(v => v.ValueKind == JsonValueKind.Object && v.TryGetProperty("$from", out var f) && f.GetString() == "System.Text.Json");
            Assert.That(serializerWorded, Is.GreaterThan(0), "the recording marks the answers the exception may cover");
        }

        private static bool JsonEqual(JsonElement expected, JsonElement actual, bool exactMessages)
        {
            if (expected.ValueKind != actual.ValueKind)
            {
                return false;
            }

            switch (expected.ValueKind)
            {
                case JsonValueKind.Object:
                    if (!exactMessages && IsSerializerWorded(expected) && IsSerializerWorded(actual))
                    {
                        return ExceptionType(expected) == ExceptionType(actual)
                            && Property(expected, "$param") == Property(actual, "$param");
                    }

                    var ep = expected.EnumerateObject().ToList();
                    var ap = actual.EnumerateObject().ToList();
                    return ep.Count == ap.Count && ep.Zip(ap, (x, y) => x.Name == y.Name && JsonEqual(x.Value, y.Value, exactMessages)).All(ok => ok);
                case JsonValueKind.Array:
                    var ei = expected.EnumerateArray().ToList();
                    var ai = actual.EnumerateArray().ToList();
                    return ei.Count == ai.Count && ei.Zip(ai, (x, y) => JsonEqual(x, y, exactMessages)).All(ok => ok);
                // Raw text, not GetString(): the recordings hold lone surrogates, which .NET Framework's reader refuses to decode.
                // Both sides come from the capture's ASCII writer, so equal raw text is equal strings.
                default:
                    return expected.GetRawText() == actual.GetRawText();
            }
        }

        private static bool IsSerializerWorded(JsonElement e)
        {
            return Property(e, "$from") == "System.Text.Json" && Property(e, "$throws") != null;
        }

        private static string? Property(JsonElement e, string name)
        {
            return e.TryGetProperty(name, out var p) ? p.GetString() : null;
        }

        private static string ExceptionType(JsonElement e)
        {
            var text = Property(e, "$throws")!;
            var colon = text.IndexOf(':');
            return colon < 0 ? text : text.Substring(0, colon);
        }
    }
}
