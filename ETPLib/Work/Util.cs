using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETPLib.Work
{
	public static class Util
	{
		public static string ToFirstUpper(string value)
		{
			return value.Substring(0, 1).ToUpper() + value.Substring(1);
		}
	}
}
