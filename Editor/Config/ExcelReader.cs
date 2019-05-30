
#region Namespace

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#endregion

namespace IGG.Game.Data.Config
{
    /// <summary>
    /// Author  gaofan
    /// Date    2018.1.2
    /// Desc    excel表
    /// </summary>
    public class Excel : ICfgReader
    {
        private uint m_colCount;
        private int m_headIndex;
        private Dictionary<string, uint> m_headMap;
        private string[] m_heads;
        private uint m_rowCount;
        private ExcelRow[] m_rows;

        public Excel(int headIndex = -1)
        {
            m_headIndex = headIndex;
        }

        /// <summary>
        /// 配置读取路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Load(string path)
        {
            if (!File.Exists(path))
            {
                UnityEngine.Debug.LogError("read csv error, path = " + path + ", e = 文件不存在" +  "Excel.Load");
                return false;
            }

            try
            {
                Encoding encoding = Encoding.GetEncoding("gb2312");
                string csvStr = File.ReadAllText(path, encoding);
                List<List<string>> excel = new CsvReader().Read(csvStr);
                m_rows = Conv2RowArr(excel, m_headIndex, Path.GetFileName(path));
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError("read csv error, path = " + path + ", e = " + e + "Excel.Load");
            }

            return false;
        }

        public uint RowCount
        {
            get { return m_rowCount; }
        }

        public uint ColCount
        {
            get { return m_colCount; }
        }

        public string[] Heads
        {
            get { return m_heads; }
        }

        public IRowReader GetRow(uint rowIndex)
        {
            if (rowIndex >= m_rowCount)
            {
                return null;
            }

            return m_rows[rowIndex];
        }

        public string GetValue(uint rowIndex, uint colIndex)
        {
            IRowReader row = GetRow(rowIndex);
            if (row == null)
            {
                return null;
            }

            return row.Get(colIndex);
        }

        public string GetValue(uint rowIndex, string colName)
        {
            IRowReader row = GetRow(rowIndex);
            if (row == null)
            {
                return null;
            }

            return row.Get(colName);
        }

        /// <summary>
        /// 转为表数据
        /// </summary>
        /// <param name="excel"></param>
        /// <param name="headIndex">head从第几个开始</param>
        /// <param name="fileName">文件名，主要用来打LOG</param>
        /// <returns></returns>
        private ExcelRow[] Conv2RowArr(List<List<string>> excel, int headIndex, string fileName)
        {
            if (excel.Count < 1)
            {
                return new ExcelRow[0];
            }

            int col = 0;
            if (headIndex < 0)
            {
                for (int i = 0; i < excel.Count; i++)
                {
                    List<string> row = excel[i];
                    if (row.Count > col)
                    {
                        col = row.Count;
                    }
                }
            }
            else
            {
                if (headIndex >= excel.Count)
                {
                    UnityEngine.Debug.LogError("file=" + fileName + "，headIndex <= excel.Count" + "Excel.Conv2RowArr");
                    return new ExcelRow[0];
                }

                m_heads = excel[headIndex].ToArray();
                col = m_heads.Length;
                m_headMap = new Dictionary<string, uint>(col);
                for (int i = 0; i < col; i++)
                {
                    string key = m_heads[i];
                    if (string.IsNullOrEmpty(key))
                    {
                        UnityEngine.Debug.LogError("file=" + fileName + "，head key is null, index = " + i + "Excel.Conv2RowArr");
                        key = "";
                    }

                    m_headMap[key] = (uint) i;
                }

                excel.RemoveRange(0, headIndex + 1);
            }

            m_rowCount = (uint) excel.Count;
            m_colCount = (uint) col;
            ExcelRow[] rows = new ExcelRow[m_rowCount];

            //检查每行的长度,并使每行一样
            for (int i = 0; i < m_rowCount; i++)
            {
                List<string> row = excel[i];
                if (row.Count != col)
                {
                    UnityEngine.Debug.LogError("file=" + fileName + "，row.Count != m_colCount, index = " + i + "Excel.Conv2RowArr");
                    int d = col - row.Count;
                    if (d < 0)
                    {
                        d = -d;
                        row.RemoveRange(col, d);
                    }
                    else
                    {
                        for (int j = 0; j < d; j++)
                        {
                            row.Add("");
                        }
                    }
                }

                rows[i] = new ExcelRow((uint) i, row.ToArray(), m_headMap);
            }

            return rows;
        }
    }

    /// <summary>
    /// Author  gaofan
    /// Date    2018.1.2
    /// Desc    excel行
    /// </summary>
    public class ExcelRow : IRowReader
    {
        private readonly string[] m_data;
        private readonly Dictionary<string, uint> m_head;
        private readonly uint m_index;

        public ExcelRow(uint index, string[] row, Dictionary<string, uint> head)
        {
            m_index = index;
            m_data = row;
            m_head = head;
            Count = (uint) row.Length;
        }

        public uint Index
        {
            get { return m_index; }
        }

        public uint Count { get; private set; }

        public string Get(uint colIndex)
        {
            if (m_data == null)
            {
                return null;
            }

            if (colIndex >= m_data.Length)
            {
                return null;
            }

            return m_data[colIndex];
        }

        public string Get(string colName)
        {
            if (m_head == null)
            {
                return null;
            }

            if (!m_head.TryGetValue(colName, out uint index))
            {
                return null;
            }

            return Get(index);
        }
    }
}