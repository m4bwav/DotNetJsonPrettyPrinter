using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Running;
using JsonPrettyPrinterPlus;

namespace JsonPrettyPrinterPlus.Benchmarks
{
    /// <summary>
    /// Pretty prints a generated document of about 1 MB (minified) so before and after numbers for the
    /// printer can go in CHANGELOG.md. Run with: dotnet run -c Release --project benchmarks/JsonPrettyPrinterPlus.Benchmarks
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.Throughput, warmupCount: 3, iterationCount: 10)]
    public class PrettyPrintBenchmark
    {
        private string _document = string.Empty;

        [GlobalSetup]
        public void Setup()
        {
            _document = GenerateDocument(1024 * 1024);
        }

        [Benchmark]
        public string PrettyPrint1MB()
        {
            return _document.PrettyPrintJson();
        }

        /// <summary>Deterministic, minified, with nested objects, arrays, escapes and numbers.</summary>
        internal static string GenerateDocument(int targetLength)
        {
            var sb = new StringBuilder(targetLength + 1024);
            sb.Append("{\"items\":[");
            var i = 0;
            while (sb.Length < targetLength)
            {
                if (i > 0) sb.Append(',');
                sb.Append("{\"id\":").Append(i)
                  .Append(",\"name\":\"item ").Append(i).Append(" with \\\"quotes\\\" and a \\\\ backslash\"")
                  .Append(",\"tags\":[\"alpha\",\"beta\",\"gamma\"]")
                  .Append(",\"nested\":{\"depth\":{\"value\":").Append(i % 7).Append(",\"empty\":{},\"list\":[]}}")
                  .Append(",\"flag\":").Append(i % 2 == 0 ? "true" : "false")
                  .Append(",\"nothing\":null}");
                i++;
            }
            sb.Append("]}");
            return sb.ToString();
        }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<PrettyPrintBenchmark>(args: args);
        }
    }
}
