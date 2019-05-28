#region Namespace

using System.Collections.Generic;
using System.IO;
using System.Text;

#endregion

namespace IGG.Game.Utils
{
    public static class StrUtil
    {
        private static Stack<StringBuilder> g_cacheSb = new Stack<StringBuilder>(5);

        public static StringBuilder GetSb()
        {
            if (g_cacheSb.Count == 0)
            {
                return new StringBuilder();
            }
            else
            {
                return g_cacheSb.Pop().Clear();
            }
        }

        public static void PutSb(StringBuilder sb)
        {
            sb.Clear();
            g_cacheSb.Push(sb);
        }


        /// <summary>
        /// 按头字母大写来格式化名字
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FormatName(string name)
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