DotNetJsonPrettyPrinter
=======================

Json Pretty Printer/Beautifier Library For .Net

Background article: http://www.markdavidrogers.com/oxitesample/Blog/json-pretty-printerbeautifier-library-for-net  
nuget package: https://www.nuget.org/packages/JsonPrettyPrinter/

This library is a simple and light weight json pretty printer.  There are few other .net json pretty printers, but they are usually heavier and focus on some other aspect of javascript or have only been described in article format.

You can use the pretty printer object or just the extension methods I've provided. I've included json and/or beautifing extension methods, for ease of use.

Example:
```
"{"Lorem":"ipsum","dolor":{ "sit":"amet"},"consectetur":{"adipisicing":{"sed":"do"}}}".PrettyPrintJson()
```
becomes:

```
{
    "Lorem": "ipsum",
    "dolor": {
        "sit": "amet"
    },
    "consectetur": {
        "adipisicing": {
            "sed": "do"
        }
    }
}
```
