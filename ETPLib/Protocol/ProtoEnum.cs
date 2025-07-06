namespace ETPLib.Protocol;

public class ProtoEnum:ProtoStruct
{
    public List<string> EnumFields { get; set; }
    public ProtoEnum(string name) : base(name)
    {
        EnumFields = new List<string>();
    }

    public void AddEnumField(string name)
    {
        EnumFields.Add(name);
    }
}