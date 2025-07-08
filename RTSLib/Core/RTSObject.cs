using System.Reflection;

namespace RTSLib.Core;

public class RTSObject
{
    private Type m_Type;

    private object m_Target;

    private Dictionary<string, PropertyObject> m_Properties;
    private Dictionary<string, MethodObject> m_Methods;

    public Type Type
    {
        get
        {
            return m_Type;
        }
    }

    public object Target
    {
        get
        {
            return m_Target;
        }
    }

    public RTSObject(object target)
    {
        m_Type = target.GetType();
        m_Target = target;
        m_Properties = new Dictionary<string, PropertyObject>();
        m_Methods = new Dictionary<string, MethodObject>();
        PropertyInfo[] propertyInfos = m_Type.GetProperties();
        foreach (var propertyInfo in propertyInfos)
        {
            m_Properties.Add(propertyInfo.Name, new PropertyObject(this, propertyInfo));
        }
        
        MethodInfo[] methodInfos = m_Type.GetMethods(BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public);
        foreach (var methodInfo in methodInfos)
        {
            string name = methodInfo.Name;
            
            for (int i = 0; i < methodInfo.GetParameters().Length; i++)
            {
                name += "_" + methodInfo.GetParameters()[i].ParameterType.Name;
            }
            
            m_Methods.Add(name, new MethodObject(this, methodInfo));
        }
    }

    public object? InvokeMethod(string name, params object[] args)
    {
        if (m_Methods.ContainsKey(name))
        {
            return m_Methods[name].Invoke(m_Target, args);
        }
        return null;
    }

    public RTSObject InvokePropertyGet(string name)
    {
        if (m_Properties.ContainsKey(name))
        {
            return m_Properties[name].Get();
        }
        return null;
    }
}