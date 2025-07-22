using Aspose.Words.LowCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ETPLib.Work
{
	public static class CompilerUtil
	{
		public static string CompileProtoCodeToCSharp(string outFileName, string protoFilePath, string excelToolDir, string generateCodeWorkSpace)
		{
			string protocPath = Path.Combine(excelToolDir, PathDefine.PROTOC);

			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = protocPath;
			startInfo.Arguments = $"--csharp_out={generateCodeWorkSpace} {protoFilePath}";
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = true;
			startInfo.CreateNoWindow = false;
			using (Process compileProcess = new Process())
			{
				compileProcess.StartInfo = startInfo;
				compileProcess.Start();

				using(StreamReader reader = compileProcess.StandardOutput)
				{
					string result = reader.ReadToEnd();
					Console.WriteLine(result);
				}

				compileProcess.WaitForExit();
			}
			if(!File.Exists($"{generateCodeWorkSpace}/{outFileName}.cs"))
			{
				Thread.Sleep(5000);
			}
			if (!File.Exists($"{generateCodeWorkSpace}/{outFileName}.cs"))
			{
				Console.WriteLine($"[ERROR] Compile Proto File ({protoFilePath}) Failed.");
				return "";
			}
			else
			{
				return File.ReadAllText($"{generateCodeWorkSpace}/{outFileName}.cs");
			}
		}
	}
}
