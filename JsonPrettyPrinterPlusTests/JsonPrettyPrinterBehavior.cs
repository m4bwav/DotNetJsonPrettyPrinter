using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using JsonPrettyPrinterPlus;
using JsonPrettyPrinterPlus.JsonSerialization;
using NUnit.Framework;

namespace JsonPrettyPrinterPlusTests
{
    [TestFixture]
    public class JsonPrettyPrinterBehavior
    {
        private const string NewLine = "\n"; // the 3.0 default; 2.x wrote Environment.NewLine
        private static readonly string ComplexJsonLintExamplePath = Path.Combine(AppContext.BaseDirectory, "TestFiles", "jsonLintBeautifyExample.json");

        private static readonly string BasicPrettyPrintArrayInObjectExample =
            "{" + NewLine +
            "    \"CreatedDate\": \"2010-01-01T00:00:00\"," + NewLine +
            "    \"Id\": \"7df51e04-ca58-4804-82f6-e0af2f1d5265\"," + NewLine +
            "    \"Names\": [" + NewLine +
            "        \"One\"," + NewLine +
            "        \"Two\"," + NewLine +
            "        \"Three\"" + NewLine +
            "    ]" + NewLine +
            "}";

        private static readonly string JsonLintVersionOfSimpleObject = "{" + NewLine + "    \"Hammer\": \"throw\"" + NewLine + "}";

        /// <summary>Joins lines with the printer's default newline.</summary>
        private static string Lines(params string[] lines)
        {
            return string.Join(NewLine, lines);
        }

        private static TestLeafObject CreateTestLeafObject()
        {
            return new TestLeafObject
            {
                Id = new Guid("7df51e04-ca58-4804-82f6-e0af2f1d5265"),
                Names = new[] { "One", "Two", "Three" },
                CreatedDate = new DateTime(2010, 1, 1)
            };
        }

        private static TestRootObject GenerateComplexTestObject()
        {
            return new TestRootObject
            {
                Leaves = new List<TestLeafObject> { CreateTestLeafObject(), CreateTestLeafObject(), CreateTestLeafObject() },
                Id = new Guid("325116C6-5147-4f09-8B9D-5547359A5153"),
                CreatedDate = new DateTime(2010, 4, 14),
                Friend = CreateTestLeafObject(),
                Titles = new[] { "Crazy horse's weeding", "\"c:\\windows\\home\"", "happy" }
            };
        }

        private static void SerializeAndCompareTheTwoStrings<T>(T testObj, string example)
        {
            var unprettyString = testObj.ToJson();

            ComparePrettyPrintResultWithExample(testObj, unprettyString, example);
        }

        private static void ComparePrettyPrintResultWithExample<T>(T testObj, string unprettyString, string example)
        {
            var prettyString = unprettyString.PrettyPrintJson();
            var deserializedObject = prettyString.DeserializeFromJson<T>();
            Assert.That(deserializedObject, Is.EqualTo(testObj));

            Assert.That(prettyString, Is.EqualTo(example));
        }

        // ----- the original suite -----

        [Test]
        public void should_be_able_to_handle_an_empty_object()
        {
            Assert.That("{}".PrettyPrintJson(), Is.EqualTo("{}"));
        }

        [Test]
        public void should_be_able_to_pretty_print_a_simple_object()
        {
            var testObj = new SimpleObject { Hammer = "throw" };

            var unprettyString = testObj.ToJson().Replace(":", " :" + NewLine + NewLine + NewLine);

            ComparePrettyPrintResultWithExample(testObj, unprettyString, JsonLintVersionOfSimpleObject);
        }

        [Test]
        public void should_beautify_nested_objects_correctly()
        {
            var testComplexObject = GenerateComplexTestObject();

            // The example file is stored with LF, the printer's default newline; the Replace guards against a CRLF checkout.
            var complexTestString = File.ReadAllText(ComplexJsonLintExamplePath).Replace("\r\n", "\n").TrimEnd();

            SerializeAndCompareTheTwoStrings(testComplexObject, complexTestString);
        }

        [Test]
        public void should_pretty_print_json_an_object_with_complex_members()
        {
            var testObj = CreateTestLeafObject();

            SerializeAndCompareTheTwoStrings(testObj, BasicPrettyPrintArrayInObjectExample);
        }

        // ----- stage 1: escapes -----

        [Test]
        public void escaped_backslash_before_a_closing_quote_ends_the_string()
        {
            // Before 2.1.0 the \\" was read as an escaped quote and the rest of the document was swallowed.
            var pretty = "{\"a\":\"x\\\\\",\"b\":1}".PrettyPrintJson();

            Assert.That(pretty, Is.EqualTo(Lines("{", "    \"a\": \"x\\\\\",", "    \"b\": 1", "}")));
        }

        [Test]
        public void escaped_quote_stays_inside_the_string()
        {
            var pretty = "{\"a\":\"say \\\"hi\\\", ok\",\"b\":{}}".PrettyPrintJson();

            Assert.That(pretty, Is.EqualTo(Lines("{", "    \"a\": \"say \\\"hi\\\", ok\",", "    \"b\": {}", "}")));
        }

        [Test]
        public void newline_and_unicode_escapes_pass_through()
        {
            var pretty = "{\"a\":\"line\\nbreak\\t\\u00e9\\\\\\\"\",\"b\":[1]}".PrettyPrintJson();

            Assert.That(pretty, Is.EqualTo(Lines("{", "    \"a\": \"line\\nbreak\\t\\u00e9\\\\\\\"\",", "    \"b\": [", "        1", "    ]", "}")));
        }

        [Test]
        public void brackets_and_commas_inside_strings_are_literal()
        {
            var pretty = "{\"a\":\"{[,]}:\",\"b\":\"x\"}".PrettyPrintJson();

            Assert.That(pretty, Is.EqualTo(Lines("{", "    \"a\": \"{[,]}:\",", "    \"b\": \"x\"", "}")));
        }

        [Test]
        public void whitespace_inside_strings_is_preserved()
        {
            var pretty = "{\"a\":\"two  spaces\\tand a tab\"}".PrettyPrintJson();

            Assert.That(pretty, Is.EqualTo(Lines("{", "    \"a\": \"two  spaces\\tand a tab\"", "}")));
        }

        // ----- stage 1: empty scopes -----

        [TestCase("{}", "{}")]
        [TestCase("[]", "[]")]
        [TestCase("{ }", "{}")]
        [TestCase("[ ]", "[]")]
        [TestCase("{\n\t}", "{}")]
        [TestCase("  [ \r\n ]  ", "[]")]
        public void empty_scopes_print_on_one_line(string input, string expected)
        {
            Assert.That(input.PrettyPrintJson(), Is.EqualTo(expected));
        }

        [Test]
        public void nested_empty_object_stays_on_its_line()
        {
            Assert.That("{\"a\":{}}".PrettyPrintJson(), Is.EqualTo(Lines("{", "    \"a\": {}", "}")));
        }

        [Test]
        public void nested_empty_scopes_at_any_depth()
        {
            var pretty = "{\"a\":{\"b\":[],\"c\":{ }},\"d\":[[],{},[ { } ]]}".PrettyPrintJson();

            Assert.That(pretty, Is.EqualTo(Lines(
                "{",
                "    \"a\": {",
                "        \"b\": [],",
                "        \"c\": {}",
                "    },",
                "    \"d\": [",
                "        [],",
                "        {},",
                "        [",
                "            {}",
                "        ]",
                "    ]",
                "}")));
        }

        [Test]
        public void nested_arrays_indent_per_level()
        {
            var pretty = "[[1,2],[3,[4]]]".PrettyPrintJson();

            Assert.That(pretty, Is.EqualTo(Lines(
                "[",
                "    [",
                "        1,",
                "        2",
                "    ],",
                "    [",
                "        3,",
                "        [",
                "            4",
                "        ]",
                "    ]",
                "]")));
        }

        // ----- stage 1: exceptions and reuse -----

        [Test]
        public void null_input_throws_argument_null_exception()
        {
            Assert.That(() => ((string)null!).PrettyPrintJson(), Throws.ArgumentNullException);
            Assert.That(() => new JsonPrettyPrinter().PrettyPrint((string)null!), Throws.ArgumentNullException);
            Assert.That(() => new JsonPrettyPrinter().PrettyPrint("{}", (TextWriter)null!), Throws.ArgumentNullException);
            Assert.That(() => "{}".PrettyPrintJson(null!), Throws.ArgumentNullException);
        }

        [Test]
        public void stray_closing_bracket_throws_format_exception_with_the_index()
        {
            Assert.That(() => "{\"a\":1}}".PrettyPrintJson(),
                Throws.TypeOf<FormatException>().With.Message.Contains("index 7"));
            Assert.That(() => "]".PrettyPrintJson(), Throws.TypeOf<FormatException>().With.Message.Contains("index 0"));
        }

        [Test]
        public void mismatched_closing_bracket_throws_format_exception()
        {
            Assert.That(() => "[1}".PrettyPrintJson(), Throws.TypeOf<FormatException>().With.Message.Contains("expected ']'"));
            Assert.That(() => "{\"a\":1]".PrettyPrintJson(), Throws.TypeOf<FormatException>().With.Message.Contains("expected '}'"));
        }

        [Test]
        public void unclosed_input_is_printed_as_far_as_it_goes()
        {
            // Not a validator: a missing closing bracket is not an error.
            Assert.That("{\"a\":1".PrettyPrintJson(), Is.EqualTo(Lines("{", "    \"a\": 1")));
        }

        [Test]
        public void printer_reused_after_malformed_input_starts_clean()
        {
            var printer = new JsonPrettyPrinter();

            Assert.That(() => printer.PrettyPrint("]"), Throws.TypeOf<FormatException>());
            Assert.That(() => printer.PrettyPrint("{\"unterminated"), Throws.Nothing);

            Assert.That(printer.PrettyPrint("{\"a\":[1]}"), Is.EqualTo(Lines("{", "    \"a\": [", "        1", "    ]", "}")));
        }

        [TestCase("")]
        [TestCase("   ")]
        [TestCase("\r\n\t")]
        public void blank_input_prints_nothing(string input)
        {
            Assert.That(input.PrettyPrintJson(), Is.EqualTo(string.Empty));

            var writer = new StringWriter();
            new JsonPrettyPrinter().PrettyPrint(input, writer);
            Assert.That(writer.ToString(), Is.EqualTo(string.Empty));
        }

        // ----- stage 1: idempotence and round trip -----

        [Test]
        public void pretty_printing_twice_gives_the_same_text()
        {
            var once = File.ReadAllText(ComplexJsonLintExamplePath).PrettyPrintJson();
            var twice = once.PrettyPrintJson();

            Assert.That(twice, Is.EqualTo(once));
        }

        [Test]
        public void pretty_printed_text_is_the_same_document()
        {
            const string original = "{\"a\":\"x\\\\\",\"b\":[1,2.5,-3e2,true,false,null,{}],\"c\":{\"d\":\"\\u00e9\\\"\",\"e\":[]}}";

            var pretty = original.PrettyPrintJson();

            Assert.That(JsonNode.DeepEquals(JsonNode.Parse(original), JsonNode.Parse(pretty)), Is.True);
        }

        // ----- stage 4: options and overloads -----

        [Test]
        public void options_control_indent_size_and_newline()
        {
            var options = new JsonPrettyPrintOptions { IndentSize = 2, NewLine = "\n" };

            Assert.That("{\"a\":[1]}".PrettyPrintJson(options), Is.EqualTo("{\n  \"a\": [\n    1\n  ]\n}"));
        }

        [Test]
        public void environment_newline_can_be_requested()
        {
            var options = new JsonPrettyPrintOptions { NewLine = Environment.NewLine };

            Assert.That("{\"a\":1}".PrettyPrintJson(options), Is.EqualTo("{" + Environment.NewLine + "    \"a\": 1" + Environment.NewLine + "}"));
        }

        [Test]
        public void options_can_use_tabs()
        {
            var options = new JsonPrettyPrintOptions { UseTabs = true, NewLine = "\n" };

            Assert.That("{\"a\":[1]}".PrettyPrintJson(options), Is.EqualTo("{\n\t\"a\": [\n\t\t1\n\t]\n}"));
        }

        [Test]
        public void zero_indent_puts_each_value_on_its_own_line_without_indentation()
        {
            var options = new JsonPrettyPrintOptions { IndentSize = 0, NewLine = "\n" };

            Assert.That("{\"a\":[1]}".PrettyPrintJson(options), Is.EqualTo("{\n\"a\": [\n1\n]\n}"));
        }

        [Test]
        public void default_options_match_the_historical_output()
        {
            Assert.That(JsonPrettyPrintOptions.Default.IndentSize, Is.EqualTo(4));
            Assert.That(JsonPrettyPrintOptions.Default.UseTabs, Is.False);
            Assert.That(JsonPrettyPrintOptions.Default.NewLine, Is.EqualTo("\n"));
            Assert.That(new JsonPrettyPrinter().Options, Is.EqualTo(JsonPrettyPrintOptions.Default));
        }

        [Test]
        public void invalid_options_are_rejected()
        {
            Assert.That(() => new JsonPrettyPrintOptions { IndentSize = -1 }, Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(() => new JsonPrettyPrintOptions { NewLine = null! }, Throws.ArgumentNullException);
            Assert.That(() => new JsonPrettyPrinter((JsonPrettyPrintOptions)null!), Throws.ArgumentNullException);
        }

        [Test]
        public void text_writer_overload_writes_the_same_output()
        {
            const string json = "{\"a\":[1,{\"b\":\"c\"}]}";
            var writer = new StringWriter();

            new JsonPrettyPrinter().PrettyPrint(json, writer);

            Assert.That(writer.ToString(), Is.EqualTo(json.PrettyPrintJson()));
        }

        [Test]
        public void span_overload_writes_the_same_output()
        {
            const string json = "  {\"a\":[1,{\"b\":\"c\"}]}  ";

            Assert.That(new JsonPrettyPrinter().PrettyPrint(json.AsSpan()), Is.EqualTo(json.PrettyPrintJson()));
        }

        [Test]
        public void serializer_helpers_have_option_overloads()
        {
            var obj = new SimpleObject { Hammer = "throw" };

            Assert.That(obj.ToJson(), Is.EqualTo("{\"Hammer\":\"throw\"}"));
            Assert.That(obj.ToJson(prettyPrint: true), Is.EqualTo(JsonLintVersionOfSimpleObject));
            Assert.That(obj.ToJson(new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase }),
                Is.EqualTo("{\"hammer\":\"throw\"}"));
            Assert.That("{\"hammer\":\"throw\"}".DeserializeFromJson<SimpleObject>(new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }),
                Is.EqualTo(obj));
        }
    }
}
