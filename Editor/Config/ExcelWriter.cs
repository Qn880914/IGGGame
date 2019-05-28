#region Namespace

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

#endregion

namespace IGG.Game.Data.Config
{
    /// <summary>
    /// Author mingzhang02
    /// Date 2019.2.16
    /// Desc 将配置文件的数据读取出来修改后再写回到csv文件中。
    /// 注意1\path不能为空，2\path所指向的文件必须和给定的Config文件一致。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ExcelWriter<T>
    {
        public static void Save(string path, List<T> objs)
        {
            ExcelWriter<T> writer = new ExcelWriter<T>();
            writer.SaveToExcel(path, objs);
        }

        public void SaveToExcel(string path, List<T> objs)
        {
            Encoding encoding = Encoding.GetEncoding("gb2312");
            string csvStr = File.ReadAllText(path, encoding);
            if (string.IsNullOrEmpty(csvStr))
            {
                return;
            }

            List<List<string>> excel = new CsvReader().Read(csvStr);

            string result = "";
            //原封不动拷贝csv的头三行数据，分别为注释、类型、字段
            for (int i = 0; i < 3 && i < excel.Count; i++)
            {
                if (i == 0)
                {
                    FormatString(excel[i]);
                }

                for (int j = 0; j < excel[i].Count; j++)
                {
                    result += excel[i][j];
                    if (j != excel[i].Count - 1)
                    {
                        result += ",";
                    }
                }

                result += "\r\n";
            }

            //根据第三行的字段名称获取对应字段的值并转换为文本。（不能通过反射的GetFields，因为无法保证顺序一致）
            List<string> fieldNames = excel.Count > 2 ? excel[2] : new List<string>();
            for (int i = 0; i < objs.Count; i++)
            {
                string rowStr = ConfigToRowStr(objs[i], fieldNames);
                result += rowStr + "\r\n";
            }

            File.WriteAllText(path, result, encoding);
        }

        //处理转义字符
        private void FormatString(List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = list[i].Replace("\"", "\"\""); //替换英文冒号 英文冒号需要换成两个冒号
                if (list[i].Contains(",") || list[i].Contains("\"") || list[i].Contains("\r") || list[i].Contains("\n")
                ) //含逗号 冒号 换行符的需要放到引号中
                {
                    list[i] = string.Format("\"{0}\"", list[i]);
                }
            }
        }

        private string ConfigToRowStr(T cfg, List<string> fieldNames)
        {
            string rowStr = "";
            for (int i = 0; i < fieldNames.Count; i++)
            {
                FieldInfo field = cfg.GetType().GetField(FormatName(fieldNames[i]));

                rowStr += TypeToString(field, cfg);
                if (i != fieldNames.Count - 1)
                {
                    rowStr += ",";
                }
            }

            return rowStr;
        }

        private string TypeToString(FieldInfo field, T cfg)
        {
            string str = "";

            if (field.FieldType == typeof(string))
            {
                str = (string) field.GetValue(cfg);
            }
            else if (field.FieldType == typeof(int))
            {
                str = field.GetValue(cfg).ToString();
            }
            else if (field.FieldType == typeof(uint))
            {
                str = field.GetValue(cfg).ToString();
            }
            else if (field.FieldType == typeof(float))
            {
                str = field.GetValue(cfg).ToString();
            }
            else if (field.FieldType == typeof(string[]))
            {
                string[] values = (string[]) field.GetValue(cfg);
                if (values != null)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        str += values[i];
                        if (i != values.Length - 1)
                        {
                            str += "|";
                        }
                    }
                }
            }
            else if (field.FieldType == typeof(int[]))
            {
                int[] values = (int[]) field.GetValue(cfg);
                if (values != null)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        str += values[i].ToString();
                        if (i != values.Length - 1)
                        {
                            str += "|";
                        }
                    }
                }
            }
            else if (field.FieldType == typeof(uint[]))
            {
                uint[] values = (uint[]) field.GetValue(cfg);
                if (values != null)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        str += values[i].ToString();
                        if (i != values.Length - 1)
                        {
                            str += "|";
                        }
                    }
                }
            }
            else if (field.FieldType == typeof(float[]))
            {
                float[] values = (float[]) field.GetValue(cfg);
                if (values != null)
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        str += string.Format("{0:F2}", values[i]);
                        if (i != values.Length - 1)
                        {
                            str += "|";
                        }
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogError("Can not deal type:" + field.GetType());
            }

            return str;
        }

        /// <summary>
        /// 按头字母大写来格式化名字
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected string FormatName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }

            StringWriter sw = new StringWriter();
            bool isFrist = true;
            for (int index = 0; index < name.Length; index++)
            {
                char c = name[index];
                if (c == '_')
                {
                    isFrist = true;
                    continue;
                }
                else if (c == '#' || c == '$')
                {
                    continue;
                }

                if (isFrist)
                {
                    if (c >= 'a' && c <= 'z')
                    {
                        c = char.ToUpper(c);
                    }

                    isFrist = false;
                }

                sw.Write(c);
            }

            sw.Close();
            string newName = sw.ToString();
            if (newName.EndsWith("config"))
            {
                newName = newName.Replace("config", "Config");
            }

            return newName;
        }
    }
}