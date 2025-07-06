using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETPLib.ExcelTools
{
	public class ExcelHandler
	{
		private List<ExcelData> m_ExcelDatas;

		public int TableCount
		{
			get
			{
				return m_ExcelDatas.Count;
			}
		}

		public ExcelHandler(string path)
		{
			using (Workbook workbook = new Workbook(path))
			{
				m_ExcelDatas = new List<ExcelData>();
				for (int i = 0; i < workbook.Worksheets.Count; i++)
				{
					m_ExcelDatas.Add(new ExcelData(workbook.Worksheets[i]));
				}
			}
		}

		public ExcelData GetData(int index)
		{
			return m_ExcelDatas[index];
		}
	}
}
