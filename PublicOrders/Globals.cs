using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace PublicOrders.Models
{
    public enum ResultType
    {
        Done = 0,
        DoneNetwork = 1,
        DoneDB = 2,
        NotSearch = 3,
        NullSearchText = 4,
        ReglamentaWork = 5,
        ErrorNetwork = 6,
        ErrorDB = 7,
        Error = 8
    }
    class Globals
    {
        public static ObservableCollection<Product> ProductsGlobal { get; set; }
        public static DocumentDbContext dcGlobal { get; set; }

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

                regexExtent = new Regex("(\\s|\\\\|/|x|·|•|×|х)с{0,1}м{0,1}д{0,1}м\\s{0,1}(-{0,1}\\d)(\\s|$|\\)|\\]|\\.|×|:|,)", RegexOptions.Multiline);

                //regexExtent = new Regex("(\\s|\\|/|x|·|•|×|х)с{0,1}м{0,1}д{0,1}м\\s{0,1}(-{0,1}\\d)(\\s|$|\\)|\\]|\\.|×|:|,)", RegexOptions.Multiline);
                matchCollExtent = regexExtent.Matches(text);
                foreach (Match matchExtent in matchCollExtent)
                {
                    extent = "";
                    if (matchExtent.Groups.Count == 4)
                    {
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
    }
}
