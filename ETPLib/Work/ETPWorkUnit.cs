using ETPLib.ExcelTools;
using ETPLib.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETPLib
{
    public class ETPWorkUnit
    {
		private static int s_FieldNameRowIndex = 0;

		private static int s_FieldTypeRowIndex = 1;

		private static int s_CommentRowIndex = 2;

		private static int s_ContentRowStartIndex = 4;

        private ExcelHandler m_ExcelHandler;

		private ProtoFileHandler m_ProtoFileHandler;

		private string m_ExcelPath;

		private string m_ProtobufWorkSpace;

		private string m_ExcelWorkSpace;

		private string m_ProtobufPath;

        public ETPWorkUnit(string excelPath,string excelWorkSpace, string protobufWorkSpace)
        {
			if (!Directory.Exists(excelWorkSpace))
			{
				Console.WriteLine($"[ERROR] Excel Work Space: {excelWorkSpace} not Exist.");
				return;
			}
			if (!excelPath.StartsWith(excelWorkSpace))
			{
				Console.WriteLine($"[ERROR] Excel Path is Invalid: {excelPath}");
				return;
			}
			if(!File.Exists(excelPath))
			{
				Console.WriteLine($"[ERROR] Excel File: {excelPath} not Exist.");
				return;
			}
			if (!Directory.Exists(protobufWorkSpace))
			{
				Console.WriteLine($"[ERROR] Protobuf Works Space: {protobufWorkSpace} not Exist.");
				return;
			}
			m_ExcelPath = excelPath;
			m_ProtobufWorkSpace = protobufWorkSpace;
			m_ExcelWorkSpace = excelWorkSpace;

			m_ProtobufPath = GetProtobufOutputPath(excelWorkSpace, protobufWorkSpace, excelPath);

			string protobufDirectory = Path.GetDirectoryName(m_ProtobufPath);
			if (!Directory.Exists(protobufDirectory))
			{
				Directory.CreateDirectory(protobufDirectory);
			}
		}

		public void DoWork()
		{
			m_ExcelHandler = new ExcelHandler(m_ExcelPath);

			string packageName = GetExcelFileNameWithoutExtension(m_ExcelPath);
			m_ProtoFileHandler = new ProtoFileHandler(packageName, m_ProtobufPath);

			for(int i = 0;i<m_ExcelHandler.TableCount;i++)
			{
				ProcessTable(m_ProtoFileHandler, m_ExcelHandler.GetData(i));
			}

			m_ProtoFileHandler.GenerateContent();

			m_ProtoFileHandler.Close();
		}

		private void ProcessTable(ProtoFileHandler protoFileHandler, ExcelData data)
		{
			Dictionary<string, string> fieldNameToType = new Dictionary<string, string>();

			ProtoMessage baseMessage = protoFileHandler.CreateMessage(data.Name);

			int columns = data.Columns;
			for(int col = 0;col < columns;col++)
			{
				string fieldName = data.Get<string>(s_FieldNameRowIndex, col);
				string fieldType = data.Get<string>(s_FieldTypeRowIndex, col);
				fieldNameToType.Add(fieldName, fieldType);

				if (fieldType.StartsWith("Enum_"))
				{
					string enumType = fieldType.Substring("Enum_".Length);
					enumType = enumType.Substring(0, 1).ToUpper() + enumType.Substring(1);
					ProtoEnum protoEnum = baseMessage.CreateEnum(enumType);
					HashSet<string> enumValues = new HashSet<string>();
					for (int row = s_ContentRowStartIndex; row < data.Rows; row++)
					{
						string value = data.Get<string>(row, col);
						if (!enumType.Contains(value))
						{
							protoEnum.AddEnumField(value);
							enumValues.Add(value);
						}
					}
				}
			}

			foreach(KeyValuePair<string, string> fields in fieldNameToType)
			{
				string fieldName = fields.Key;
				string excelType = fields.Value;
				string fieldType = "";
				if (excelType.StartsWith("Enum_"))
				{
					string enumType = excelType.Substring("Enum_".Length);
					
					fieldType = enumType.Substring(0, 1).ToUpper() + enumType.Substring(1);
				}
				else
				{
					fieldType = TypeMapping.MappingType(fields.Value);
				}

				baseMessage.CreateField(fieldType, fieldName);
			}

			ProtoMessage arrayMessage = protoFileHandler.CreateMessage(data.Name + "_Array");

			arrayMessage.CreateField(baseMessage.Name, "Content", ProtoKeywords.FIELD_PROP_REPEATED);
		}

		public static string GetExcelFileNameWithoutExtension(string excelPath)
		{
			return Path.GetFileNameWithoutExtension(excelPath);
		}

		public static string GetProtobufOutputPath(string excelWorkSpace, string protobufWorkSpace, string excelPath)
		{
			string protobufDirecotry = GetProtoOutputDirectory(excelWorkSpace, protobufWorkSpace, excelPath);
			string excelFileName = GetExcelFileNameWithoutExtension(excelPath);
			return Path.Combine(protobufDirecotry, excelFileName + ".proto");
		}

		public static string GetProtoOutputDirectory(string excelWorkSpace, string protobufWorkSpace, string excelPath)
		{
			string excelFileName = Path.GetFileName(excelPath);
			string excelDirectory = excelPath.Remove(excelPath.IndexOf(excelFileName) - 1);
			return excelDirectory.Replace(excelWorkSpace, protobufWorkSpace);
		}
    }
}
