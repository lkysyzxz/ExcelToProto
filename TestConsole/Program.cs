// See https://aka.ms/new-console-template for more information
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis.Emit;
using System.Runtime.Loader;
using ActorConfig;
using Google.Protobuf;
using System.Runtime.CompilerServices;
using Google.Protobuf.Reflection;
using RTSLib.Core;
using TypeInfo = System.Reflection.TypeInfo;

Console.WriteLine("Hello, World!");

string actorCS = File.ReadAllText("G:\\GameProjects\\UtilTools\\ExcelToProto\\TestConsole\\Actor.cs");


// 创建语法树
SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(actorCS);

var protobufAssembly = Assembly.GetAssembly(typeof(IMessage));


// 自动加载所有程序集引用
var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(a => MetadataReference.CreateFromFile(a.Location)).ToList();

assemblies.Add(MetadataReference.CreateFromFile(protobufAssembly.Location));

CSharpCompilation compilation = CSharpCompilation.Create(
		"Actor",
		syntaxTrees: new[] { syntaxTree },
		references: assemblies,
		options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

// 编译代码
using (var ms = new MemoryStream())
{
	EmitResult result = compilation.Emit(ms);

	if (!result.Success)
	{
		Console.WriteLine("编译失败：");
		foreach (Diagnostic diagnostic in result.Diagnostics.Where(diag => diag.IsWarningAsError || diag.Severity == DiagnosticSeverity.Error))
		{
			Console.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
		}
	}
	else
	{
		
		ms.Seek(0, SeekOrigin.Begin);
		Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);

		RTSModel model = new RTSModel(assembly);

		RTSObject actorArray = model.CreateInstance("Actor_Array", new Type[] { }, new object[] { });

		RTSObject config = actorArray.InvokePropertyGet("Config");

		object actors = new object[2];

		for (int i = 0; i < 2; i++)
		{

			RTSObject actor = model.CreateInstance("Actor", new Type[] { }, new object[] { });

			actor.InvokePropertySet("Id", new RTSObject(i+1));
			actor.InvokePropertySet("Name", new RTSObject("测试角色"+i));
			actor.InvokePropertySet("SexId", model.CreateEnumInstance("Sex", "Female"));
			actor.InvokePropertySet("SkillType", new RTSObject("Magic"));

			config.InvokeMethod("Add_Actor", actor.Target);
		}

		int size = (int)actorArray.InvokeMethod("CalculateSize", new object[] { });
		byte[] buffer = new byte[size];
		CodedOutputStream codedOutput = new CodedOutputStream(buffer);

		actorArray.InvokeMethod("WriteTo_CodedOutputStream", new object[] { codedOutput });

		Actor_Array arr = Actor_Array.Parser.ParseFrom(buffer);

		Console.Read();
	}
}


Console.Read();