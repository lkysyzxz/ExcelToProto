// See https://aka.ms/new-console-template for more information

using System.Runtime.Loader;
using ActorConfig;
using Google.Protobuf;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Google.Protobuf.Reflection;
using TypeInfo = System.Reflection.TypeInfo;

Console.WriteLine("Hello, World!");

string code = File.ReadAllText("/Users/admin/Desktop/repo/csharp_projects/ExcelToProto/TestConsole/ConfigPath.cs");

Regex reg = new Regex("public static string (.*) = \\\"(.*)\\\";");
Match result = reg.Match(code);
if (result.Success)
{
    while (result.Success)
    {
        string name = result.Groups[1].Value;
        string path = result.Groups[2].Value;
        Console.WriteLine(name + "  " + path);
        result = result.NextMatch();
    }
    
}

Console.Read();