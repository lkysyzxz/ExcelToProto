using Microsoft.CodeAnalysis;
using System.Reflection;
using Microsoft.CodeAnalysis.Emit;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis.CSharp;

namespace RTSLib.Core;

public class RTSModel
{
    private static HashSet<string> s_TypesBlackList = new HashSet<string>()
    {
        "<>c"
    };
    private Assembly m_Assembly;
    private Dictionary<string, System.Reflection.TypeInfo> m_NameToTypes;
    
    public RTSModel(Assembly assembly)
    {
        m_Assembly = assembly;
        m_NameToTypes = new Dictionary<string, System.Reflection.TypeInfo>();
        
        foreach (var type in assembly.DefinedTypes)
        {
            if (!s_TypesBlackList.Contains(type.Name) && !s_TypesBlackList.Contains(type.Name))
            {
                m_NameToTypes.Add(type.Name, type);
            }
        }
    }

    public System.Reflection.TypeInfo GetTypeInfo(string typeName)
    {
        if (m_NameToTypes.ContainsKey(typeName))
        {
            return m_NameToTypes[typeName];
        }

        return null;
    }

	public RTSObject CreateInstance(string typeName, Type[] types, object[] parameters)
	{
		if (m_NameToTypes.ContainsKey(typeName))
		{
			System.Reflection.TypeInfo type = m_NameToTypes[typeName];
			ConstructorInfo constructor = type.GetConstructor(types);

			if(constructor != null)
			{
				return new RTSObject(constructor.Invoke(parameters));
			}
		}
		return null;
	}

	public RTSObject CreateEnumInstance(string typeName, string enumValue)
	{
		if(m_NameToTypes.ContainsKey(typeName))
		{
            System.Reflection.TypeInfo type = m_NameToTypes[typeName];
			if(type == null || !type.IsEnum)
			{
				return null;
			}
			FieldInfo[] fields = type.GetFields();
			foreach (FieldInfo field in fields)
			{
				if(field.Name == enumValue)
				{
					return new RTSObject(Convert.ChangeType(field.GetValue(null), type));
				}
			}
		}
		return null;
	}

	public static RTSModel CompileCSharpCode(string code, string assemblyName, Assembly[] depencies)
	{
		SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

		var currentDomainAssemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic).Select(a => MetadataReference.CreateFromFile(a.Location)).ToList();

		if (depencies != null)
		{
			for (int i = 0; i < depencies.Length; i++)
			{
				currentDomainAssemblies.Add(MetadataReference.CreateFromFile(depencies[i].Location));
			}
		}

		CSharpCompilation compilation = CSharpCompilation.Create(
			assemblyName,
		syntaxTrees: new[] { syntaxTree },
		references: currentDomainAssemblies,
		options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		using (var ms = new MemoryStream())
		{
			EmitResult result = compilation.Emit(ms);

			if (!result.Success)
			{
				Console.WriteLine("±àÒëÊ§°Ü£º");
				foreach (Diagnostic diagnostic in result.Diagnostics.Where(diag => diag.IsWarningAsError || diag.Severity == DiagnosticSeverity.Error))
				{
					Console.WriteLine($"{diagnostic.Id}: {diagnostic.GetMessage()}");
				}
			}
			else
			{
				ms.Seek(0, SeekOrigin.Begin);
				Assembly assembly = AssemblyLoadContext.Default.LoadFromStream(ms);
				return new RTSModel(assembly);
			}
		}
		return null;
	}
}