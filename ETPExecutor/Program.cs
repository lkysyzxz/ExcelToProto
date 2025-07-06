// See https://aka.ms/new-console-template for more information

using CommandLine;
using ETPLib;
using ExcelToProto.CommandLine;
using System.Configuration;
using System.Diagnostics;
using System.Windows;

namespace ETPExe
{

	public class Program
	{
		public static void Main(string[] args)
		{
			Parser.Default.ParseArguments<ETPCommandLine>(args).WithParsed(OnArgsParsed);
		}

		public static void OnArgsParsed(ETPCommandLine commandLine)
		{
			string currentDirectory = Environment.CurrentDirectory;

			string excelWorkSpace = ConfigurationManager.AppSettings["ExcelWorkSpace"].ToString();

			string protobufWorkSpace = ConfigurationManager.AppSettings["ProtobufWorkSpace"].ToString();

			string serializedDataWorkSpace = ConfigurationManager.AppSettings["SerializedDataWorkSpace"].ToString();

			string gameWorkSpace = ConfigurationManager.AppSettings["GameWorkSpace"].ToString();

			Console.WriteLine($"Current Directory: {currentDirectory}.");
			Console.WriteLine($"Excel Work Space: {excelWorkSpace}.");
			Console.WriteLine($"Protobuf Work Space: {protobufWorkSpace}.");
			Console.WriteLine($"Serialized Data Work Space: {serializedDataWorkSpace}.");
			Console.WriteLine($"Game Work Space: {gameWorkSpace}.");

			if (commandLine.BuildOne)
			{
				string excelPath = commandLine.ExcelPath;
				Console.WriteLine($"Excel Path: {excelPath}.");
				
				ETPWorkUnit workUnit = new ETPWorkUnit(excelPath, excelWorkSpace, protobufWorkSpace);

				workUnit.DoWork();
			}
		}
	}
}
