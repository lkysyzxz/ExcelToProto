using System.Reflection;

namespace RTSLib.Core;

public class MethodObject
{
    private RTSObject m_RTSObject;
    private MethodInfo m_MethodInfo;

    public string Name
    {
        get
        {
            return m_MethodInfo.Name;
        }
    }
    
    public MethodObject(RTSObject rtsObject, MethodInfo methodInfo)
    {
        m_RTSObject = rtsObject;
        m_MethodInfo = methodInfo;
    }

    public object? Invoke(params object[] args)
    {
        return m_MethodInfo.Invoke(m_RTSObject, args);
    }
}