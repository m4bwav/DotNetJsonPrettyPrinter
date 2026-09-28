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
        // The second named exception (found by the first Linux CI run, 2026-09-28): System.Text.Json's WriteIndented writes
        // Environment.NewLine, so this answer holds the newline of the OS it ran on, and the recordings were made on Windows.
        // Off Windows it is compared with each recorded CRLF read as LF. Keyed exactly; nothing else may use it.
        private static readonly HashSet<string> OsNewLineCases = new(StringComparer.Ordinal)
        {
            "tojson.options | write-indented-not-pretty",
        };

        private static readonly string EscapedCrLf = new string((char)92, 1) + "u000d" + new string((char)92, 1) + "u000a";
        private static readonly string EscapedLf = new string((char)92, 1) + "u000a";

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
                var expected = _recorded![key];
                if (OsNewLineCases.Contains(key) && Environment.NewLine.Length == 1)
                {
                    using var lf = JsonDocument.Parse(expected.GetRawText().Replace(EscapedCrLf, EscapedLf));
                    expected = lf.RootElement.Clone();
                }

                if (!JsonEqual(expected, actual.RootElement, SerializerMatchesRecording))
                {
                    failures.Add(key + ": expected " + _recorded[key].GetRawText() + ", got " + actualText.Trim());
                }
            }

            Assert.That(failures, Is.Empty, string.Join(Environment.NewLine, failures));
        }

        [Test]
        public void The_os_newline_exception_names_recorded_cases_that_hold_a_crlf()
        {
            foreach (var key in OsNewLineCases)
            {
                Assert.That(_recorded!.ContainsKey(key), key);
                Assert.That(_recorded[key].GetRawText(), Does.Contain(EscapedCrLf), key);
            }
        }

        [Test]
        public void The_recording_marks_the_answers_worded_by_System_Text_Json()
        {
            var serializerWorded = _recorded!.Values.Count(v => v.ValueKind == JsonValueKind.Object && v.TryGetProperty("$from", out var f) && f.GetString() == "System.Text.Json");
            Assert.That(serializerWorded, Is.GreaterThan(0), "the recording marks the answers the exception may cover");
        }

        [Test]
        public void Warns_while_the_message_exception_is_active()
        {
            if (!SerializerMatchesRecording)
            {
                Assert.Warn("System.Text.Json messages are compared by type, parameter, inner types and path only: recorded " + _recordedSerializer + ", running " + Cases.SerializerVersion());
            }
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
                            && Property(expected, "$param") == Property(actual, "$param")
                            && JsonPath(Property(expected, "$throws")!) == JsonPath(Property(actual, "$throws")!)
                            && InnerTypes(expected) == InnerTypes(actual);
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
            return TypeOf(Property(e, "$throws")!);
        }

        private static string TypeOf(string text)
        {
            var colon = text.IndexOf(':');
            return colon < 0 ? text : text.Substring(0, colon);
        }

        // "Path: $.Count | LineNumber: 0 | BytePositionInLine: 14." is where the reader stopped, not wording: keep it exact.
        private static string JsonPath(string text)
        {
            var at = text.IndexOf("Path: ", StringComparison.Ordinal);
            return at < 0 ? "" : text.Substring(at);
        }

        private static string InnerTypes(JsonElement e)
        {
            return e.TryGetProperty("$inner", out var inner) ? string.Join("|", inner.EnumerateArray().Select(x => TypeOf(x.GetString()!))) : "";
        }
    }
}
