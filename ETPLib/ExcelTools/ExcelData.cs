using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETPLib.ExcelTools
{
	public class ExcelData
	{
		private int m_Rows;
		private int m_Columns;

		private DataTable m_RawData;

		private string m_Name;

		public string Name
		{
			get
			{
				return m_Name;
			}
		}

		public int Rows
		{
			get { return m_Rows; }
		}

		public int Columns
		{
			get
			{
				return m_Columns;
			}
		}

		public ExcelData(Worksheet worksheet)
		{
			m_Rows = worksheet.Cells.MaxDataRow + 1;
			m_Columns = worksheet.Cells.MaxDataColumn + 1;

			m_RawData = new DataTable(worksheet.Name);
			for(int i = 0;i<m_Columns;i++)
			{
				m_RawData.Columns.Add(new DataColumn());
			}
			for(int row = 0; row < m_Rows; row++)
			{
				DataRow dataRow = m_RawData.NewRow();
				for (int col = 0; col < m_Columns; col++)
				{
					dataRow[col] = worksheet.Cells[row, col].Value;
				}
				m_RawData.Rows.Add(dataRow);
			}
			m_Name = worksheet.Name;
		}

		public T Get<T>(int row, int col)
		{
			return (T)m_RawData.Rows[row][col];
		}

		public object Get(int row, int col)
		{
			return m_RawData.Rows[row][col];
		}
	}
}
