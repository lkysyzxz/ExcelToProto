using ETPLib.ExcelTools;
using ETPLib.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETPLib
{
	public static class TypeMapping
	{
		private static Dictionary<string, string> s_ExcelTypeToProtoTypes = new Dictionary<string, string>()
		{
			{ExcelTypes.STRING  , ProtoTypes.STRING   },
			{ExcelTypes.INT32   , ProtoTypes.INT32    },
			{ExcelTypes.INT64   , ProtoTypes.INT64    },
			{ExcelTypes.DOUBLE  , ProtoTypes.DOUBLE   },
			{ExcelTypes.BOOLEAN , ProtoTypes.BOOLEAN  },
			{ExcelTypes.BYTES   , ProtoTypes.BYTES    },
			{ExcelTypes.UINT32  , ProtoTypes.UINT32   },
			{ExcelTypes.UINT64  , ProtoTypes.UINT64   },
			{ExcelTypes.SINT32  , ProtoTypes.SINT32   },
			{ExcelTypes.SINT64  , ProtoTypes.SINT64   },
			{ExcelTypes.FIXED32 , ProtoTypes.FIXED32  },
			{ExcelTypes.FIXED64 , ProtoTypes.FIXED64  },
			{ExcelTypes.SFIXED32, ProtoTypes.SFIXED32 },
			{ExcelTypes.SFIXED64, ProtoTypes.SFIXED64 },
		};

		public static string MappingType(string type)
		{
			if(s_ExcelTypeToProtoTypes.TryGetValue(type, out string result))
			{
				return result;
			}
			return "";
		}
	}
}
