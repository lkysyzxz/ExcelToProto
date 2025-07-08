using System.Reflection;

namespace RTSLib.Core;

public class PropertyObject
{
    private RTSObject m_RTSObject;
    private PropertyInfo m_PropertyInfo;
    private MethodInfo m_GetMethod;
    private MethodInfo m_SetMethod;
    
    public PropertyObject(RTSObject rtsObject, PropertyInfo property)
    {
        m_RTSObject = rtsObject;
        m_PropertyInfo = property;
        m_GetMethod = property.GetGetMethod();
        m_SetMethod = property.GetSetMethod();
    }

    public RTSObject Get()
    {
        return new RTSObject(m_GetMethod.Invoke(m_RTSObject.Target, null));
    }

    public void Set(RTSObject value)
    {
        m_SetMethod.Invoke(m_RTSObject.Target, new object[] { value.Target });
    }
}