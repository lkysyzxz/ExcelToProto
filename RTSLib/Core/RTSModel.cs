using System.Reflection;

namespace RTSLib.Core;

public class RTSModel
{
    private static HashSet<string> s_TypesBlackList = new HashSet<string>()
    {
        "<>c"
    };
    private Assembly m_Assembly;
    private Dictionary<string, TypeInfo> m_NameToTypes;
    public RTSModel(Assembly assembly)
    {
        m_Assembly = assembly;
        m_NameToTypes = new Dictionary<string, TypeInfo>();
        
        foreach (var type in assembly.DefinedTypes)
        {
            if (!s_TypesBlackList.Contains(type.Name) && !s_TypesBlackList.Contains(type.Name))
            {
                m_NameToTypes.Add(type.Name, type);
            }
        }
    }

    public TypeInfo GetTypeInfo(string typeName)
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
			TypeInfo type = m_NameToTypes[typeName];
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
			TypeInfo type = m_NameToTypes[typeName];
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
}