using System.Collections.Generic;
using System.Text;

/// <summary>
/// Author  gaofan
/// Date    2017.12.29
/// Desc    快速正确而且代码易读的CSV读取器
///         因为奇怪百度出来的代码要么很复杂
///         要么没判断特殊字符串而自己写了一套
/// </summary>
public class CsvReader
{
    public List<List<string>> Read(string csvStr)
    {
        StringBuilder sb = new StringBuilder();
        List<List<string>> excel = new List<List<string>>();
        List<string> row = new List<string>();

        int state = 0;
        //1=新格子，2=新行
        int action = 0;
        int len = csvStr.Length;
        for (int i = 0; i < len; i++)
        {
            char c = csvStr[i];
            switch (state)
            {
                //新的单元格刚开始
                case 0:
                    switch (FindKeyword(StartKeys, csvStr, ref i))
                    {
                        case KeywordType.CellSpec:
                            state = 2;
                            break;
                        //刚开始也是有可能就结束的
                        case KeywordType.CellEnd:
                            action = 1;
                            break;
                        case KeywordType.NewLine:
                            action = 2;
                            break;
                        default:
                            sb.Append(c);
                            state = 1;
                            break;
                    }
                    break;

                //一般字符串第二个开始
                case 1:
                    switch (FindKeyword(GeneralKeys, csvStr, ref i))
                    {
                        case KeywordType.CellEnd:
                            action = 1;
                            break;
                        case KeywordType.NewLine:
                            action = 2;
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                    break;

                //特殊字符串第一个字符开始
                case 2:
                    switch (FindKeyword(SpacKeys, csvStr, ref i))
                    {
                        case KeywordType.CellSpecChar:
                            sb.Append("\"");
                            break;
                        case KeywordType.CellSpecEnd:
                            action = 1;
                            break;
                        case KeywordType.CellSpecNewLine:
                            action = 2;
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                    break;

                default:
                    UnityEngine.Debug.LogError("出现未知状态" + "CsvReader.Read");
                    break;
            }

            switch (action)
            {
                case 1:
                    row.Add(sb.ToString());
                    sb.Remove(0, sb.Length);
                    state = 0;
                    break;
                case 2:
                    row.Add(sb.ToString());
                    sb.Remove(0, sb.Length);
                    excel.Add(row);
                    row = new List<string>();
                    state = 0;
                    break;
            }
            action = 0;
        }

        //文件没有正确的结尾标识
        if (state != 0)
        {
            row.Add(sb.ToString());
            sb.Remove(0, sb.Length);
            excel.Add(row);
            UnityEngine.Debug.LogWarning("Excel没有正确的结尾标识" + "CsvReader.Reqd");
        }
        return excel;
    }

    private KeywordType FindKeyword(Keword[] keywords, string str, ref int index)
    {
        int hasLen = str.Length - index;
        for (int i = 0; i < keywords.Length; i++)
        {
            Keword keyword = keywords[i];
            if (hasLen < keyword.KeyLen)
            {
                continue;
            }
            bool finded = true;
            for (int j = 0; j < keyword.KeyLen; j++)
            {
                if (str[index + j] != keyword.Key[j])
                {
                    finded = false;
                    break;
                }
            }
            if (!finded)
            {
                continue;
            }
            index += keyword.KeyLen - 1;
            return keyword.Type;
        }
        return KeywordType.None;
    }

    static readonly Keword CallSpec = new Keword(KeywordType.CellSpec, "\"");
    static readonly Keword NewLine = new Keword(KeywordType.NewLine, "\r\n");
    static readonly Keword CellEnd = new Keword(KeywordType.CellEnd, ",");
    static readonly Keword CellSpecChar = new Keword(KeywordType.CellSpecChar, "\"\"");
    static readonly Keword CellSpecEnd = new Keword(KeywordType.CellSpecEnd, "\",");
    static readonly Keword CellSpecNewLine = new Keword(KeywordType.CellSpecNewLine, "\"\r\n");

    static readonly Keword[] StartKeys = { CallSpec, CellEnd, NewLine };
    static readonly Keword[] GeneralKeys = { CellEnd, NewLine };
    static readonly Keword[] SpacKeys = { CellSpecChar, CellSpecEnd, CellSpecNewLine };

    enum KeywordType
    {
        None,
        CellEnd,
        NewLine,
        CellSpec,
        CellSpecChar,
        CellSpecEnd,
        CellSpecNewLine,
    }

    class Keword
    {
        public readonly KeywordType Type;
        public readonly string Key;
        public readonly int KeyLen;
        public Keword(KeywordType type, string key)
        {
            Type = type;
            Key = key;
            KeyLen = key.Length;
        }
    }
}
