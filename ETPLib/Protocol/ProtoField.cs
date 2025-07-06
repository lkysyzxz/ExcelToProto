namespace ETPLib.Protocol;

public class ProtoField
{
    public string FieldProp { get; set; }
    public string FieldType { get; set; }
    public string FieldName { get; set; }

    public ProtoField(string fieldType, string fieldName, string fieldProp = "")
    {
        FieldProp = fieldProp;
        FieldType = fieldType;
        FieldName = fieldName;
    }

    public bool HasProp
    {
        get
        {
            return !string.IsNullOrEmpty(FieldProp);
        }
    }
}

public class RepeatedProtoField : ProtoField
{
    public RepeatedProtoField(string fieldType, string fieldName) : base(fieldType, fieldName,
        ProtoKeywords.FIELD_PROP_REPEATED)
    {
        
    }
}

public class OptionalProtoField : ProtoField
{
    public OptionalProtoField(string fieldType, string fieldName) : base(fieldType, fieldName,
        ProtoKeywords.FIELD_PROP_OPTIONAL)
    {
        
    }
}

public class NormalProtoField : ProtoField
{
    public NormalProtoField(string fieldType, string fieldName) : base(fieldType, fieldName)
    {
        
    }
}