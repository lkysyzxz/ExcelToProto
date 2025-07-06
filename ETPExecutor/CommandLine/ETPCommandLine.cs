using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelToProto.CommandLine
{
	public class ETPCommandLine
	{
		[Option("build-one", Default = false, HelpText = "Process one Excel File.", Required = true )]
		public bool BuildOne {  get; set; }

		[Option("excel-path", HelpText = "Excel File Path.", Required = false)]
		public string ExcelPath { get; set; }
	}
}
