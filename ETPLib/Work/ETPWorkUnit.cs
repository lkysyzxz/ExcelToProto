using Aspose.Email;
using ETPLib.ExcelTools;
using ETPLib.Protocol;
using ETPLib.Work;
using Google.Protobuf;
using RTSLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Aspose.CAD.FileFormats.Cgm.Commands.ColourModel;

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

		private string m_ExcelToolDir;

		private string m_SerializedDataWorkSpace;

		private string m_GameWorkSpace;

		private string m_ConfigFilePath;

        public ETPWorkUnit(string excelToolDir, string excelPath,string excelWorkSpace, string protobufWorkSpace, 
	        string serializedDataWorkSpace, string gameWorkSpace, string configFilePath)
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
			m_ExcelToolDir = excelToolDir;
			m_SerializedDataWorkSpace = serializedDataWorkSpace;
			m_GameWorkSpace = gameWorkSpace;
			m_ConfigFilePath = configFilePath;

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
			m_ProtoFileHandler = new ProtoFileHandler(packageName + "Config", m_ProtobufPath);

			for(int i = 0;i<m_ExcelHandler.TableCount;i++)
			{
				ProcessTable(m_ProtoFileHandler, m_ExcelHandler.GetData(i));
			}

			m_ProtoFileHandler.GenerateContent();

			m_ProtoFileHandler.Close();

			string protoFilePath = m_ProtoFileHandler.FilePath;

			if (!Directory.Exists("./Temp"))
			{
				Directory.CreateDirectory("./Temp");
			}

			string csharpCode = CompilerUtil.CompileProtoCodeToCSharp(m_ProtoFileHandler.FileNameWithoutExtension, protoFilePath, m_ExcelToolDir);

			string serializedOutputDirectory = GetSerializedDataOutputDirectory(m_ExcelWorkSpace, m_SerializedDataWorkSpace, m_ExcelPath);

			if(!Directory.Exists(serializedOutputDirectory))
			{
				Directory.CreateDirectory(serializedOutputDirectory);
			}


			RTSModel protoModel = RTSModel.CompileCSharpCode(csharpCode, m_ProtoFileHandler.FileNameWithoutExtension, new Assembly[]
			{
				typeof(Google.Protobuf.IMessage).Assembly
			});
			
			List<string> bytesDataConfigFieldNames = new List<string>();
			List<string> bytesDataPaths = new List<string>();
			for(int i = 0; i < m_ExcelHandler.TableCount; i++)
			{
				ExcelData excelData = m_ExcelHandler.GetData(i);

				byte[] serializeResult = SerializeExcelData(excelData, protoModel, m_ProtoFileHandler);

				string bytesDataPath = Path.Combine(serializedOutputDirectory, $"{excelData.Name}_Config_Array.bytes");
				File.WriteAllBytes(bytesDataPath, serializeResult);
				bytesDataPaths.Add(bytesDataPath);
				bytesDataConfigFieldNames.Add($"{excelData.Name.ToUpper()}_CONFIG_BYTES_DATA_PATH");
			}

			List<string> bytesDataPathsInGame = CopySerializedExcelDataToGameWorkSpace(bytesDataPaths, m_GameWorkSpace);
			
			AppendConfigPaths(bytesDataConfigFieldNames, bytesDataPathsInGame, m_ConfigFilePath);
		}

		private void AppendConfigPaths(List<string> appendFieldNames, List<string> appendPaths, string configFilePath)
		{
			if (!File.Exists(configFilePath))
			{
				File.Create(configFilePath).Close();
			}

			string configCode = File.ReadAllText(configFilePath);
			Match match = Regex.Match(configCode, "public static string (.*) = \\\"(.*)\\\";");
			List<string> fieldNames = new List<string>();
			List<string> bytesDataPaths = new List<string>();
			while (match.Success)
			{
				string field = match.Groups[1].Value;
				string path = match.Groups[2].Value;
				if (File.Exists(path))
				{
					fieldNames.Add(field);
					bytesDataPaths.Add(path);
				}
				match = match.NextMatch();
			}

			List<int> repeatedIndex = new List<int>();
			HashSet<string> originFieldNames = new HashSet<string>();
			for(int i = 0; i < fieldNames.Count; i++)
			{
				originFieldNames.Add(fieldNames[i]);
			}
			for (int i = 0; i < appendFieldNames.Count; i++)
			{
				if (originFieldNames.Contains(appendFieldNames[i]))
				{
					repeatedIndex.Add(i - repeatedIndex.Count);
				}
			}

			for(int i = 0;i < repeatedIndex.Count; i++)
			{
				appendFieldNames.RemoveAt(repeatedIndex[i]);
				appendPaths.RemoveAt(repeatedIndex[i]);
			}

			for (int i = 0; i < appendPaths.Count; i++)
			{
				string path = appendPaths[i];
				if (path.StartsWith("./") || path.StartsWith(".\\"))
				{
					path = path.Substring(2);
				}
				if (path.StartsWith("../") || path.StartsWith("..\\"))
				{
					path = path.Substring(3);
				}
				bytesDataPaths.Add(path);
				fieldNames.Add(appendFieldNames[i]);
			}
			


			WriteConfigFilePath(configFilePath, fieldNames, bytesDataPaths);
			
		}

		private void WriteConfigFilePath(string configFilePath, List<string> fieldNames, List<string> bytesDataPaths)
		{
			string configFileDir = Path.GetDirectoryName(configFilePath);
			if(!Directory.Exists(configFileDir))
			{
				Directory.CreateDirectory(configFileDir);
			}
			using (FileStream fs = File.Open(configFilePath, FileMode.Open))
			{
				using(StreamWriter sw = new StreamWriter(fs))
				{
					string intent = "";
					sw.WriteLine("public static class ConfigPath");
					sw.WriteLine("{");
					intent += "\t";
					
					for(int i =  0; i < fieldNames.Count; i++)
					{
						string path = bytesDataPaths[i];
						path = path.Replace('\\','/');
						sw.WriteLine($"{intent}public static string {fieldNames[i]} = \"{path}\";");
					}

					sw.WriteLine("}");
					sw.Close();
				}
				fs.Close();
			}
		}

		private List<string> CopySerializedExcelDataToGameWorkSpace(List<string> bytesDataPaths, string gameWorkSpace)
		{
			if(!Directory.Exists(gameWorkSpace))
			{
				Directory.CreateDirectory(gameWorkSpace);
			}
			List<string> bytesDataPathsInGame = new List<string>();
			foreach (string bytesDataPath in bytesDataPaths)
			{
				string fileName = Path.GetFileName(bytesDataPath);
				string bytesDataPathInGame = Path.Combine(gameWorkSpace, fileName);
				File.Copy(bytesDataPath, bytesDataPathInGame, true);
				bytesDataPathsInGame.Add(bytesDataPathInGame);
			}

			return bytesDataPathsInGame;
		}

		private byte[] SerializeExcelData(ExcelData excelData, RTSModel protoModel, ProtoFileHandler protoFileMetaData)
		{
			RTSObject configArrayMessage = protoModel.CreateInstance(excelData.Name + "ConfigArray", new Type[] { }, new object[] { });

			RTSObject configArray = configArrayMessage.InvokePropertyGet("Config");

			ProtoMessage elementMessageMeta = protoFileMetaData.GetMessage(excelData.Name);

			HashSet<string> enumTypes = new HashSet<string>();
			for(int i = 0; i < elementMessageMeta.Enums.Count; i++)
			{
				enumTypes.Add(elementMessageMeta.Enums[i].Name);
			}

			for (int row = s_ContentRowStartIndex; row < excelData.Rows; row++)
			{
				RTSObject elementInstnace = protoModel.CreateInstance(excelData.Name, new Type[] { }, new object[] { });
				for(int col = 0;col < elementMessageMeta.Fields.Count;col++)
				{
					ProtoField field = elementMessageMeta.Fields[col];
					string fieldType = field.FieldType;
					string fieldName = field.FieldName;

					string propertyName = Util.ToFirstUpper(fieldName);

					if(fieldType == ProtoTypes.STRING)
					{
						string data = excelData.Get<string>(row, col);
						elementInstnace.InvokePropertySet(propertyName, new RTSObject(data));
					}
					else if(fieldType == ProtoTypes.BOOLEAN)
					{
						if(bool.TryParse(excelData.Get<string>(row,col), out bool data))
						{
							elementInstnace.InvokePropertySet(propertyName, new RTSObject(data));
						}
						else
						{
							Console.WriteLine($"[ERROR] Parse {excelData.Get<string>(row, col)} To Bool Failed.");
						}
					}
					else if(fieldType == ProtoTypes.INT32)
					{
						if(Int32.TryParse(excelData.Get<string>(row, col), out int data))
						{
							elementInstnace.InvokePropertySet(propertyName, new RTSObject(data));
						}
						else
						{
							Console.WriteLine($"[ERROR] Parse {excelData.Get<string>(row, col)} To Int32 Failed.");
						}
						
					}
					else if(fieldType == ProtoTypes.INT64)
					{
						if (Int64.TryParse(excelData.Get<string>(row, col), out Int64 data))
						{
							elementInstnace.InvokePropertySet(propertyName, new RTSObject(data));
						}
						else
						{
							Console.WriteLine($"[ERROR] Parse {excelData.Get<string>(row, col)} To Int64 Failed.");
						}
					}
					else if(fieldType == ProtoTypes.DOUBLE)
					{
						if (Double.TryParse(excelData.Get<string>(row, col), out Double data))
						{
							elementInstnace.InvokePropertySet(propertyName, new RTSObject(data));
						}
						else
						{
							Console.WriteLine($"[ERROR] Parse {excelData.Get<string>(row, col)} To Double Failed.");
						}
					}
					else if(fieldType == ProtoTypes.UINT32)
					{
						if (UInt32.TryParse(excelData.Get<string>(row, col), out UInt32 data))
						{
							elementInstnace.InvokePropertySet(propertyName, new RTSObject(data));
						}
						else
						{
							Console.WriteLine($"[ERROR] Parse {excelData.Get<string>(row, col)} To UInt32 Failed.");
						}
					}
					else if(fieldType == ProtoTypes.UINT64)
					{
						if (UInt64.TryParse(excelData.Get<string>(row, col), out UInt64 data))
						{
							elementInstnace.InvokePropertySet(propertyName, new RTSObject(data));
						}
						else
						{
							Console.WriteLine($"[ERROR] Parse {excelData.Get<string>(row, col)} To UInt64 Failed.");
						}
					}
					else if(fieldType == ProtoTypes.FLOAT)
					{
						if(float.TryParse(excelData.Get<string>(row, col), out float data))
						{
							elementInstnace.InvokePropertySet(propertyName, new RTSObject(data));
						}
						else
						{
							Console.WriteLine($"[ERROR] Parse {excelData.Get<string>(row, col)} To float Failed.");
						}
					}
					else if(enumTypes.Contains(fieldType))
					{
						string enumValue = excelData.Get<string>(row, col);
						elementInstnace.InvokePropertySet(propertyName, protoModel.CreateEnumInstance(fieldType, enumValue));
					}
				}

				configArray.InvokeMethod($"Add_{excelData.Name}", elementInstnace.Target);
			}

			int size = (int)configArrayMessage.InvokeMethod("CalculateSize", new object[] { });
			if (size > 0)
			{
				byte[] buffer = new byte[size];
				CodedOutputStream codedOutput = new CodedOutputStream(buffer);

				configArrayMessage.InvokeMethod("WriteTo_CodedOutputStream", new object[] { codedOutput });
				return buffer;
			}
			return null;
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
					enumType = Util.ToFirstUpper(enumType);

					ProtoEnum protoEnum = baseMessage.CreateEnum(enumType);
					HashSet<string> enumValues = new HashSet<string>();
					for (int row = s_ContentRowStartIndex; row < data.Rows; row++)
					{
						string value = data.Get<string>(row, col);
						if (!enumValues.Contains(value))
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
					
					fieldType = Util.ToFirstUpper(enumType);
				}
				else
				{
					fieldType = TypeMapping.MappingType(fields.Value);
				}

				baseMessage.CreateField(fieldType, fieldName);
			}

			ProtoMessage arrayMessage = protoFileHandler.CreateMessage(data.Name + "ConfigArray");

			arrayMessage.CreateField(baseMessage.Name, "Config", ProtoKeywords.FIELD_PROP_REPEATED);
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

		public static string GetSerializedDataOutputDirectory(string excelWorkSpace, string serializedDataWorkSpace, string excelPath)
		{
			string excelFileName = Path.GetFileName(excelPath);
			string excelDirectory = excelPath.Remove(excelPath.IndexOf(excelFileName) - 1);
			return excelDirectory.Replace(excelWorkSpace, serializedDataWorkSpace);
		}
    }
}
