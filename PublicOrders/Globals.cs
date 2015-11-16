using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Web;

namespace PublicOrders.Models
{
    public enum CustomerLevel_enum {
        Federal = 0,
        Subject = 1,
        Municipal = 2,
        Other = 3,
        None = 4
    }

    public enum ResultType_enum
    {
        Done = 0,           // Успешно выполненная задача
        NotSearch = 1,      // Ничего не найдено
        NullSearchText = 2, // Пустая строка поиска
        ReglamentaWork = 3, // На сайте проводятся регламентские работы
        ErrorNetwork = 4,   // Не удается подключиться к сети
        Error = 5           // Ошибка
    };
    public enum CustomerType_enum
    {
        Customer = 0,
        Organization = 1
    }
    public enum LawType_enum
    {
        _44 = 0,
        _94 = 1,
        _223 = 2,
        _44_94 = 3,
        _44_94_223 = 4,
        None = 5
    }
    class Globals
    {
        public static string CutAddress(string text)
        {
            if (text.IndexOf("место нахождения:") > -1)
            {
                text = text.Substring(text.IndexOf("место нахождения:") + 17, text.Length - (text.IndexOf("место нахождения:") + 17)).Trim();
            }
            else
            {
                if (text.IndexOf("место нахождения :") > -1)
                {
                    text = text.Substring(text.IndexOf("место нахождения :") + 18, text.Length - (text.IndexOf("место нахождения :") + 18)).Trim();
                }
            }
            return text;
        }

        public static string CleanSpaces(string text)
        {
            while (true)
            {
                if (text.IndexOf("  ") > -1)
                {
                    text = text.Replace("  ", " ");
                }
                else
                {
                    break;
                }
            }

            return text;
        }

        public static string CutLotName(string text)
        {

            Regex regex = new Regex("лот.*?\\d{1,}(.*)", RegexOptions.IgnoreCase);
            MatchCollection matchColl = regex.Matches(text);
            foreach (Match match in matchColl)
            {
                if (match.Groups.Count == 2)
                {
                    text = match.Groups[1].Value.Trim();
                }
                break;
            }

            return text;
        }

        public static string DecodeInternetSymbs(string text)
        {
            text = HttpUtility.UrlDecode(text);
            text = HttpUtility.HtmlDecode(text);
            return text;
        }

        public static string CleanWordCell(string cellText)
        {
            if ((cellText.Length == 1) && (cellText[0] == '\a'))
            {
                return String.Empty;
            }
            return cellText.Replace("\r\a", String.Empty);
        }

        public static string DeleteNandSpaces(string text)
        {
            try
            {
                text = text.Replace('\n', ' ').Replace('\r', ' ');
                while (text.IndexOf("  ") > -1)
                {
                    text = text.Replace("  ", " ");
                }

                return text;
            }
            catch
            {
                return text;
            }
        }

        public static string ConvertTextExtent(string text)
        {
            try
            {
                string extent = "";

                // Находим цифры степени
                Regex regexExtent = new Regex("\\d\\s{0,6}(x|·|•|×|х)\\s{0,1}10(\\d{1,3})", RegexOptions.Multiline);
                MatchCollection matchCollExtent = regexExtent.Matches(text);
                foreach (Match matchExtent in matchCollExtent)
                {
                    extent = "";
                    if (matchExtent.Groups.Count == 3)
                    {
                        if ((matchExtent.Groups[2].Value.Length > 1) ||
                            (matchExtent.Groups[2].Value[0]) == '0')
                        {
                            continue;
                        }

                        foreach (char ch in matchExtent.Groups[2].Value)
                        {
                            extent += ConvertDigitForExtent(Convert.ToString(ch));
                        }

                        text = text.Remove(matchExtent.Groups[2].Index, extent.Length);
                        text = text.Insert(matchExtent.Groups[2].Index, extent);
                    }
                }

                // Степень системы счисления
                regexExtent = new Regex("(\\s|\\\\|/|x|·|•|×|х)с{0,1}м{0,1}д{0,1}м\\s{0,1}(-{0,1}\\d)(\\s|$|\\)|\\]|\\.|×|:|,)", RegexOptions.Multiline);
                matchCollExtent = regexExtent.Matches(text);
                foreach (Match matchExtent in matchCollExtent)
                {
                    extent = "";
                    if (matchExtent.Groups.Count == 4)
                    {
                        if ((matchExtent.Groups[2].Value.Length > 1) ||
                            (matchExtent.Groups[2].Value[0]) == '0')
                        {
                            continue;
                        }

                        foreach (char ch in matchExtent.Groups[2].Value)
                        {
                            extent += ConvertDigitForExtent(Convert.ToString(ch));
                        }

                        text = text.Remove(matchExtent.Groups[2].Index, extent.Length);
                        text = text.Insert(matchExtent.Groups[2].Index, extent);
                    }
                }

                return text;
            }
            catch
            {
                return text;
            }
        }

        private static string ConvertDigitForExtent(string digit)
        {
            switch (digit)
            {
                case ("1"):
                    return "¹";
                case ("2"):
                    return "²";
                case ("3"):
                    return "³";
                case ("4"):
                    return "⁴";
                case ("5"):
                    return "⁵";
                case ("6"):
                    return "⁶";
                case ("7"):
                    return "⁷";
                case ("8"):
                    return "⁸";
                case ("9"):
                    return "⁹";
                case ("0"):
                    return "⁰";
                case ("-"):
                    return "ˉ";
                default:
                    return "_";
            }
        }

        public static ResultType_enum CheckDocResult(HtmlAgilityPack.HtmlDocument doc, out string message)
        {
            try
            {
                message = "";
                if (doc == null)
                {
                    message = "Не удается подключиться к сети";
                    return ResultType_enum.ErrorNetwork;
                }
                //Не удается отобразить эту страницу
                if (doc.DocumentNode.InnerText.IndexOf("Не удается отобразить эту страницу") > -1)
                {
                    message = "Не удается подключиться к сети";
                    return ResultType_enum.ErrorNetwork;
                }
                // Бан
                if (doc.DocumentNode.InnerText.IndexOf("403 Forbidden") > -1)
                {
                    message = "Бан";
                    return ResultType_enum.ErrorNetwork;
                }
                
                if (doc.DocumentNode.InnerText.IndexOf("Ведутся регламентные работы") > -1)
                {
                    message = "Ведутся регламентские работы";
                    return ResultType_enum.ReglamentaWork;
                }
                return ResultType_enum.Done;
            }
            catch (Exception ex)
            {
                message = ex.Message + '\n' + ex.StackTrace;
                return ResultType_enum.Error;
            }
        }
    }
}
