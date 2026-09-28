// A fresh consumer of the packed or published JsonPrettyPrinter, run by run.sh on net10.0 and net48 (from the
// package-modernize template, 2026-09-28). Calls the public API only, checks answers from the golden recording of 3.0.1,
// prints the package version it loaded (run.sh checks it as a whole line) and returns 0 only when every answer is right.

using System;
using System.Linq;
using System.Reflection;
using JsonPrettyPrinterPlus;
using JsonPrettyPrinterPlus.JsonSerialization;

public static class Program
{
    public static int Main()
    {
        var nl = new string((char)10, 1);
        var ok = true;

        var pretty = @"{""a"":[1,{}],""b"":""x""}".PrettyPrintJson();
        ok &= pretty == string.Join(nl, "{", @"    ""a"": [", "        1,", "        {}", "    ],", @"    ""b"": ""x""", "}");

        var tabs = "[1]".PrettyPrintJson(new JsonPrettyPrintOptions { UseTabs = true, NewLine = " " });
        ok &= tabs == "[ " + (char)9 + "1 ]";

        try
        {
            "]".PrettyPrintJson();
            ok = false;
        }
        catch (FormatException e)
        {
            ok &= e.Message == "Unexpected ']' at index 0: there is no open object or array to close.";
        }

        ok &= new { A = 1, B = "x" }.ToJson() == @"{""A"":1,""B"":""x""}";
        ok &= "42".DeserializeFromJson<int>() == 42;

        var info = typeof(JsonPrettyPrinter).Assembly.GetCustomAttributes<AssemblyInformationalVersionAttribute>().Single().InformationalVersion;
        Console.WriteLine("JsonPrettyPrinter " + info.Split('+')[0]);
        Console.WriteLine(ok ? "consumer answers as expected" : "wrong answer");
        return ok ? 0 : 1;
    }
}
