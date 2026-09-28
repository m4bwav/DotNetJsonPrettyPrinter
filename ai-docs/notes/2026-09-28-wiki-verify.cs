#:package JsonPrettyPrinter@3.0.1
#:property PublishAot=false
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JsonPrettyPrinterPlus;
using JsonPrettyPrinterPlus.JsonSerialization;

Console.OutputEncoding = Encoding.UTF8;

var readme = """{"Lorem":"ipsum","dolor":{ "sit":"amet"},"consectetur":{"adipisicing":{"sed":"do"}},"empty":{}}""";
Show("readme default", readme.PrettyPrintJson());

// ----- options -----
Show("indent 2", """{"a":[1,{"b":"c"}]}""".PrettyPrintJson(new JsonPrettyPrintOptions { IndentSize = 2 }));
Show("tabs", """{"a":[1,{"b":"c"}]}""".PrettyPrintJson(new JsonPrettyPrintOptions { UseTabs = true }));
Show("indent 0", """{"a":[1,{"b":"c"}]}""".PrettyPrintJson(new JsonPrettyPrintOptions { IndentSize = 0 }));
Show("env newline", """{"a":1}""".PrettyPrintJson(new JsonPrettyPrintOptions { NewLine = Environment.NewLine }));
Show("crlf newline", """{"a":1}""".PrettyPrintJson(new JsonPrettyPrintOptions { NewLine = "\r\n" }));
Show("with expression", """{"a":1}""".PrettyPrintJson(JsonPrettyPrintOptions.Default with { IndentSize = 3 }));
Show("record equality", (new JsonPrettyPrintOptions() == JsonPrettyPrintOptions.Default).ToString() + " / " + JsonPrettyPrintOptions.Default.ToString().Replace("\n", "<LF>"));
Show("tabs ignore indent size", """{"a":1}""".PrettyPrintJson(new JsonPrettyPrintOptions { UseTabs = true, IndentSize = 8 }));

// ----- whitespace normalisation -----
Show("odd spacing", """{ "a" : 1 , "b" : [ 1 , 2 ] }""".PrettyPrintJson());
Show("crlf input", ("{\"a\":1," + "\r\n" + "\"b\":2}").PrettyPrintJson());
var once = readme.PrettyPrintJson();
Show("idempotent", (once.PrettyPrintJson() == once).ToString());
Show("whitespace in strings", """{"a":"two  spaces and	a tab"}""".PrettyPrintJson());
Show("blank input", "'" + "   ".PrettyPrintJson() + "'");

// ----- not a validator -----
Show("trailing comma", """{"a":1,}""".PrettyPrintJson());
var commented = """
    {"a":1, // a note about a
    "b":2}
    """;
Show("comment", commented.PrettyPrintJson());
Show("single quotes", """{'a':'b c'}""".PrettyPrintJson());
Show("bare words", """{a:1,b:[x,y]}""".PrettyPrintJson());
Show("top-level number", "42".PrettyPrintJson());
Show("top-level string", "\"hi there\"".PrettyPrintJson());
Show("two documents", ("""{"a":1}""" + "\n" + """{"b":2}""").PrettyPrintJson());
Show("scalars", """[1,2.5,-3e2,true,false,null,{}]""".PrettyPrintJson());
Show("unclosed", """{"a":1""".PrettyPrintJson());
Show("unterminated string", """{"a":"oops""".PrettyPrintJson());
Show("nested empties", """{"a":{"b":[],"c":{ }},"d":[[],{},[ { } ]]}""".PrettyPrintJson());

// ----- errors -----
Show("stray close", Catch(() => """{"a":1}}""".PrettyPrintJson()));
Show("mismatch", Catch(() => "[1}".PrettyPrintJson()));
Show("mismatch 2", Catch(() => """{"a":1]""".PrettyPrintJson()));
Show("close first", Catch(() => "]".PrettyPrintJson()));
Show("null input", Catch(() => ((string)null!).PrettyPrintJson()));
Show("null options", Catch(() => "{}".PrettyPrintJson(null!)));
Show("negative indent", Catch(() => new JsonPrettyPrintOptions { IndentSize = -1 }.ToString()));
Show("null newline", Catch(() => new JsonPrettyPrintOptions { NewLine = null! }.ToString()));
var reused = new JsonPrettyPrinter();
Show("reuse after error 1", Catch(() => reused.PrettyPrint("]")));
Show("reuse after error 2", reused.PrettyPrint("""{"a":[1]}"""));

// ----- escapes and unicode -----
Show("escaped backslash", """{"a":"x\\","b":1}""".PrettyPrintJson());
Show("escaped quote", """{"a":"say \"hi\", ok","b":{}}""".PrettyPrintJson());
var uni = """{"a":"line\nbreak\t""" + "\\" + """u00e9\\\"","b":[1]}""";
Show("unicode escape input", uni);
Show("unicode escape", uni.PrettyPrintJson());
Show("brackets in strings", """{"a":"{[,]}:","url":"http://example.com/a?b=c"}""".PrettyPrintJson());
Show("non-ascii", """{"café":"naïve ☕ 😀 日本"}""".PrettyPrintJson());
var bom = ((char)0xFEFF) + "{}";
var bomOut = bom.PrettyPrintJson();
Show("bom", "length=" + bomOut.Length + " startsWithBom=" + (bomOut[0] == (char)0xFEFF));

// ----- overloads -----
var printer = new JsonPrettyPrinter(new JsonPrettyPrintOptions { IndentSize = 2 });
var sw = new StringWriter();
printer.PrettyPrint(readme, sw);
Show("textwriter", sw.ToString());
var path = Path.Combine(Path.GetTempPath(), "pretty-verify.json");
using (var file = File.CreateText(path))
{
    printer.PrettyPrint("""{"to":"disk"}""", file);
}
Show("file", File.ReadAllText(path));
var bigger = "xxxxx" + """{"s":[1]}""" + "yyyy";
Show("span slice", printer.PrettyPrint(bigger.AsSpan(5, 9)));
var sb = new StringBuilder();
using (var w = new StringWriter(sb))
{
    printer.PrettyPrint("""{"into":"builder"}""".AsSpan(), w);
}
Show("span+writer", sb.ToString());
Show("printer options", printer.Options.IndentSize.ToString());

// ----- deep nesting and threads -----
var deep = new string('[', 1000) + new string(']', 1000);
var deepOut = deep.PrettyPrintJson();
Show("deep nesting", "lines=" + deepOut.Split('\n').Length + " last=" + deepOut[^1]);
var expected = readme.PrettyPrintJson();
var ok = true;
Parallel.For(0, 20000, i => { if (readme.PrettyPrintJson() != expected) ok = false; });
Show("parallel extension", ok ? "20000 identical" : "MISMATCH");

// ----- serialisation -----
var thing = new { Name = "Mark", Tags = new[] { "a", "b" }, When = new DateTime(2010, 1, 1), Nothing = (string?)null, Price = 9.5m };
Show("ToJson", thing.ToJson());
Show("ToJson pretty", thing.ToJson(prettyPrint: true));
Show("ToJson camel", thing.ToJson(new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }, prettyPrint: true));
Show("ToJson null", ((object?)null).ToJson());
Show("ToJson string", "he said \"hi\"".ToJson());
var person = new Person { Name = "Ada", Age = 36, Born = new DateTime(1815, 12, 10), Tags = new[] { "math" } };
Show("ToJson typeinfo", person.ToJson(PersonContext.Default.Person, prettyPrint: true));
var back = """{"Name":"Ada","Age":36,"Born":"1815-12-10T00:00:00","Tags":["math"]}""".DeserializeFromJson<Person>();
Show("Deserialize", back!.Name + " " + back.Age + " " + back.Born.Year + " " + back.Tags.Length);
var back2 = """{"name":"Ada","age":36}""".DeserializeFromJson<Person>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
Show("Deserialize case-insensitive", back2!.Name + " " + back2.Age);
var back3 = """{"Name":"Ada","Age":36}""".DeserializeFromJson(PersonContext.Default.Person);
Show("Deserialize typeinfo", back3!.Name + " " + back3.Age);
Show("Deserialize bad json", Catch(() => "{not json".DeserializeFromJson<Person>()!.Name));
Show("Deserialize null", Catch(() => ((string)null!).DeserializeFromJson<Person>()!.Name));
Show("Deserialize literal null", ("null".DeserializeFromJson<Person>() == null).ToString());
Show("STJ indented vs ours", JsonSerializer.Serialize(new { a = new[] { 1 }, e = new { } }, new JsonSerializerOptions { WriteIndented = true }));
Show("ours same input", JsonSerializer.Serialize(new { a = new[] { 1 }, e = new { } }).PrettyPrintJson());

// ----- page outputs the first run left out (added 2026-09-28 when the wiki was brought under
// wikiwright's saved-output rule; every block below is on a page exactly as printed here) -----
Show("home and getting started: the Ada example", """{"name":"Ada","tags":["math","engines"],"born":1815,"empty":{}}""".PrettyPrintJson());
Show("getting started: serialising an object", new { Name = "Mark", Tags = new[] { "a", "b" }, When = new DateTime(2010, 1, 1), Price = 9.5m }.ToJson(prettyPrint: true));
Show("not a validator: block comment", """{"a":1 /* note */,"b":2}""".PrettyPrintJson());
Show("recipes: one line", """{"a":1,"b":[1,2],"c":{}}""".PrettyPrintJson(new JsonPrettyPrintOptions { IndentSize = 0, NewLine = " " }));
Show("recipes: minify", System.Text.Json.Nodes.JsonNode.Parse("""{ "a" : [ 1 , 2 ] }""")!.ToJsonString());
Show("serialisation helpers: default escaping", new { s = """<b>&'é"</b>""" }.ToJson());
Show("serialisation helpers: relaxed escaping", new { s = """<b>&'é"</b>""" }.ToJson(new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
Show("not a validator: strip comments first, then pretty", System.Text.Json.Nodes.JsonNode.Parse(commented, null,
    new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true })!.ToJsonString().PrettyPrintJson());

// The pages' PowerShell and F# snippets, run as written against the package in the NuGet cache.
Show("getting started: PowerShell", Run("pwsh", "-NoProfile", "-NonInteractive", "-Command", """
    Add-Type -Path "$env:USERPROFILE/.nuget/packages/jsonprettyprinter/3.0.1/lib/netstandard2.0/JsonPrettyPrinterPlus.dll"
    [JsonPrettyPrinterPlus.PrettyPrinterExtensions]::PrettyPrintJson('{"a":[1,2],"b":{}}')

    $options = [JsonPrettyPrinterPlus.JsonPrettyPrintOptions]::new()
    $options.IndentSize = 2
    [JsonPrettyPrinterPlus.PrettyPrinterExtensions]::PrettyPrintJson('{"a":[1]}', $options)

    $compact = [ordered]@{ name = 'Ada'; tags = @('math') } | ConvertTo-Json -Compress
    [JsonPrettyPrinterPlus.PrettyPrinterExtensions]::PrettyPrintJson($compact)
    """));
var fsx = Path.Combine(Path.GetTempPath(), "jpp-wiki-verify.fsx");
File.WriteAllText(fsx, """"
    #r "nuget: JsonPrettyPrinter, 3.0.1"
    open JsonPrettyPrinterPlus
    open JsonPrettyPrinterPlus.JsonSerialization

    let json = """{"a":[1,{"b":"c"}],"e":{}}"""
    printfn "%s" (json.PrettyPrintJson())
    printfn "%s" (json.PrettyPrintJson(JsonPrettyPrintOptions(IndentSize = 2)))
    printfn "%s" ({| Name = "Ada"; Tags = [| "math" |] |}.ToJson(prettyPrint = true))
    """");
Show("getting started: F#", Run("dotnet", "fsi", "--quiet", fsx));

static void Show(string title, string text)
{
    Console.WriteLine("=== " + title + " ===");
    Console.WriteLine(text.Replace("\r", "<CR>").Replace("\t", "<TAB>"));
    Console.WriteLine("--- end (" + text.Length + " chars)");
}

// Runs a page's snippet in another host and returns what it printed, LF line endings.
static string Run(string file, params string[] args)
{
    var info = new System.Diagnostics.ProcessStartInfo(file) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
    foreach (var a in args)
    {
        info.ArgumentList.Add(a);
    }

    using var p = System.Diagnostics.Process.Start(info)!;
    var stdout = p.StandardOutput.ReadToEnd();
    var stderr = p.StandardError.ReadToEnd();
    p.WaitForExit();
    var text = stdout + (stderr.Length > 0 ? "--- stderr\n" + stderr : "") + (p.ExitCode != 0 ? "--- exit " + p.ExitCode : "");
    return text.Replace("\r\n", "\n").TrimEnd();
}

static string Catch(Func<string> f)
{
    try
    {
        return f();
    }
    catch (Exception e)
    {
        return e.GetType().Name + ": " + e.Message;
    }
}

public sealed class Person
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public DateTime Born { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
}

[JsonSerializable(typeof(Person))]
internal partial class PersonContext : JsonSerializerContext
{
}
