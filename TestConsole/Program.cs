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

string actorCS = File.ReadAllText("/Users/admin/Desktop/repo/csharp_projects/ExcelToProto/TestConsole/Actor.cs");


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

		RTSModel model = new RTSModel("Actor",assembly);
		
		TypeInfo sexType = model.GetTypeInfo("Sex");
		var fields = sexType.GetFields();
		object r = Convert.ChangeType(fields[1].GetValue(null), sexType);
		
		Type actorType = assembly.GetType("ActorConfig.Actor");
		Type actorArrayType = assembly.GetType("ActorConfig.Actor_Array");
		var actorArrayCreator = actorArrayType.GetConstructor(new Type[] { });
		object actorArray = actorArrayCreator.Invoke(new object[] { });

		PropertyInfo configProperty = actorArrayType.GetProperty("Config");

		object config = configProperty.GetValue(actorArray);

		Type configType = config.GetType();

		MethodInfo addMethod = configType.GetMethod("Add", new Type[] {actorType});

		var actorCreator = actorType.GetConstructor(new Type[] { });

		object actor = actorCreator.Invoke(new object[] { });

		PropertyInfo nameInfo = actorType.GetProperty("Name");

		nameInfo.GetSetMethod().Invoke(actor, new object[] { "WWT" });

		addMethod.Invoke(config, new object[] { actor });

		MethodInfo calculateSizeMethod = actorArrayType.GetMethod("CalculateSize", new Type[] { });
		MethodInfo writeToMethod = actorArrayType.GetMethod("WriteTo", new Type[] {typeof(CodedOutputStream)});

		int size = (int)calculateSizeMethod.Invoke(actorArray, new object[] { });

		byte[] buffer = new byte[size];
		var codedOutput = new CodedOutputStream(buffer);

		writeToMethod.Invoke(actorArray, new object[] { codedOutput });

		Actor_Array arr = Actor_Array.Parser.ParseFrom(buffer);

		Console.Read();
	}
}


Console.Read();