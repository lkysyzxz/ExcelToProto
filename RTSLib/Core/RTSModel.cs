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
    public RTSModel(string name, Assembly assembly)
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
}