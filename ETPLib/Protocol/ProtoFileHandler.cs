using ETPLib.FileUtil;

namespace ETPLib.Protocol;

public class ProtoFileHandler:FileHandler
{
    private string m_Version;
    private string m_Package;
    private List<ProtoEnum> m_Enums;
    private List<ProtoMessage> m_Messages;

    private Dictionary<string, ProtoEnum> m_NameToEnum;
    private Dictionary<string, ProtoMessage> m_NameToMessage;
    
    public ProtoFileHandler(string package, string path) : base(path)
    {
		if (!path.EndsWith(".proto"))
		{
			path += ".proto";
		}
        m_Package = package;
        m_Version = ProtoVersions.VERSION_3;
        m_Enums = new List<ProtoEnum>();
        m_Messages = new List<ProtoMessage>();
        m_NameToMessage = new Dictionary<string, ProtoMessage>();
        m_NameToEnum = new Dictionary<string, ProtoEnum>();
    }

    public void AddMessage(ProtoMessage message)
    {
        if (!m_NameToMessage.ContainsKey(message.Name))
        {
            m_Messages.Add(message);
            m_NameToMessage.Add(message.Name, message);
        }
    }

    public ProtoMessage CreateMessage(string name)
    {
        if (!m_NameToMessage.ContainsKey(name))
        {
            ProtoMessage message = new ProtoMessage(name);
            AddMessage(message);
        }
        else
        {
            Console.WriteLine($"[ERROR] Repeate message: {name}");
        }
        return m_NameToMessage[name];
    }

    public ProtoMessage GetMessage(string name)
    {
        if (m_NameToMessage.ContainsKey(name))
        {
            return m_NameToMessage[name];
        }
        return null;
    }
    
    public void AddEnum(ProtoEnum protoEnum)
    {
        if (!m_NameToEnum.ContainsKey(protoEnum.Name))
        {
            m_Enums.Add(protoEnum);
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

    public override void GenerateContent()
    {
        WriteSyntaxVersion();
        
        WritePackage();

        for (int i = 0; i < m_Enums.Count; i++)
        {
            WriteEnum(m_Enums[i]);
        }

        for (int i = 0; i < m_Messages.Count; i++)
        {
            WriteMessage(m_Messages[i]);
        }
    }

    private void WriteSyntaxVersion()
    {
        WriteLine($"{ProtoKeywords.SYNTAX} {m_Version};");
    }

    private void WritePackage()
    {
        WriteLine($"{ProtoKeywords.PACKAGE} {m_Package};");
    }

    private void WriteEnum(ProtoEnum protoEnum)
    {
        WriteLine($"{ProtoKeywords.ENUM}  {protoEnum.Name}", true);
        WriteLine("{",true);
        AddIntent();
        for (int i = 0; i < protoEnum.EnumFields.Count; i++)
        {
            WriteLine($"{protoEnum.EnumFields[i]} = {i},", true);
        }
        SubIntent();
        WriteLine("};", true);
    }

    private void WriteMessage(ProtoMessage message)
    {
        WriteLine($"{ProtoKeywords.MESSAGE} {message.Name}", true);
        WriteLine("{",true);
        AddIntent();
        for (int i = 0; i < message.Enums.Count; i++)
        {
            WriteEnum(message.Enums[i]);
        }

        for (int i = 0; i < message.Fields.Count; i++)
        {
            ProtoField field = message.Fields[i];
            if (field.HasProp)
            {
                WriteLine($"{field.FieldProp} {field.FieldType} {field.FieldName} = {i + 1};", true);
            }
            else
            {
                WriteLine($"{field.FieldType} {field.FieldName} = {i + 1};", true);
            }
        }
        SubIntent();
        WriteLine("};", true);
    }
}