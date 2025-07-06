namespace ETPLib.Protocol;

public class ProtoMessage:ProtoStruct
{
    public List<ProtoEnum> Enums { get; set; }
    public List<ProtoField> Fields { get; set; }

    private Dictionary<string, ProtoField> m_NameToField;

    private Dictionary<string, ProtoEnum> m_NameToEnum;

    public ProtoMessage(string name):base(name)
    {
        Fields = new List<ProtoField>();
        Enums = new List<ProtoEnum>();
        m_NameToField = new Dictionary<string, ProtoField>();
        m_NameToEnum = new Dictionary<string, ProtoEnum>();
    }

    public void AddField(ProtoField field)
    {
        if (!m_NameToField.ContainsKey(field.FieldName))
        {
            Fields.Add(field);
            m_NameToField.Add(field.FieldName, field);
        }
    }

    public ProtoField GetField(string name)
    {
        if (m_NameToField.ContainsKey(name))
        {
            return m_NameToField[name];
        }
        return null;
    }

    public ProtoField CreateField(string type, string name, string fieldProp = "")
    {
        if (!m_NameToField.ContainsKey(name))
        {
            ProtoField field = new ProtoField(type, name, fieldProp);
            AddField(field);
        }
        else
        {
            Console.WriteLine($"[ERROR] Repeated field {name} in {Name}.");
        }
        return m_NameToField[name];
    }
    
    public void AddEnum(ProtoEnum protoEnum)
    {
        if (!m_NameToEnum.ContainsKey(protoEnum.Name))
        {
            Enums.Add(protoEnum);
            m_NameToEnum.Add(protoEnum.Name, protoEnum);
        }
    }

    public ProtoEnum CreateEnum(string name)
    {
        if (!m_NameToEnum.ContainsKey(name))
        {
            ProtoEnum protoEnum = new ProtoEnum(name);
            AddEnum(protoEnum);
        }
        else
        {
            Console.WriteLine($"[ERROR] Repeate Enum: {name}");
        }
        return m_NameToEnum[name];
    }

    public ProtoEnum GeteEnum(string name)
    {
        if (m_NameToEnum.ContainsKey(name))
        {
            return m_NameToEnum[name];
        }
        return null;
    }
}