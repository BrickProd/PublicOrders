using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Models;
using System.Windows;
using System.Threading;
using System.Collections.ObjectModel;
using PublicOrders.Processors;
using PublicOrders.Processors.Internet;
using System.Text.RegularExpressions;
using System.Web;

namespace PublicOrders.Processors.Main
{
    public delegate void AllCustomersSearched_delegete(ResultType_enum resultType_enum, string message);
    public delegate void CustomerSearched_delegate(Customer customer);
    public delegate void CustomerSearchProgress_delegate(string text, int intValue);

    public class CustomersSearchProcessor
    {
        MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        private string text = "";
        private HtmlAgilityPack.HtmlDocument doc;
        private InternetRequestEngine internetRequestEngine  = null;

        private bool isWork = false;
        private bool isPause = false;

        private string customerName = "";
        private decimal priceMin = 0;
        private decimal priceMax = 0;
        private string town = "";
        private DateTime publishDateMin;
        private DateTime publishDateMax;
        private CustomerType_enum customerType_enum;
        private LawType_enum lawType_enum;

        private AllCustomersSearched_delegete allCustomersSearched_delegete = null;
        private CustomerSearched_delegate customerSearched_delegate = null;
        private CustomerSearchProgress_delegate customerSearchProgress_delegate = null;

        public CustomersSearchProcessor(string _customerName, CustomerType_enum _customerType_enum, decimal _priceMin, decimal _priceMax, string _town,
                               DateTime _publishDateMin, DateTime _publishDateMax,
                               LawType_enum _lawType_enum, 
                               AllCustomersSearched_delegete _allCustomersSearched_delegete,
                               CustomerSearched_delegate _customerSearched_delegate,
                               CustomerSearchProgress_delegate _customerSearchProgress_delegate)
        {
            customerName = _customerName;
            customerType_enum = _customerType_enum;
            priceMin = _priceMin;
            priceMax = _priceMax;
            town = _town;
            publishDateMin = _publishDateMin;
            publishDateMax = _publishDateMax;
            lawType_enum = _lawType_enum;
            allCustomersSearched_delegete = _allCustomersSearched_delegete;
            customerSearched_delegate = _customerSearched_delegate;
            customerSearchProgress_delegate = _customerSearchProgress_delegate;
        }

        private void SearchCustomers_proc()
        {
            try
            {
                isWork = true;
                isPause = false;

                InternetRequestEngine internetRequestEngine = new InternetRequestEngine();
                if (customerName == "") {
                    allCustomersSearched_delegete(ResultType_enum.NullSearchText, "");
                    return;
                } 

                string lawTypeStr = "";
                switch (lawType_enum)
                {
                    case (LawType_enum._44_94):
                        lawTypeStr = "fz94=on&registered94=on&";
                        break;
                    case (LawType_enum._223):
                        lawTypeStr = "fz223=on&registered94=on&registered223=on&inn=&ogrn=&kpp=&okvedIds=&ppoIds=&";
                        break;
                    case (LawType_enum._44_94_223):
                        lawTypeStr = "fz94=on&fz223=on&";
                        break;
                }

                text = @"http://new.zakupki.gov.ru/epz/organization/extendedsearch/results.html?searchString=" + HttpUtility.UrlEncode(customerName) + "&pageNumber=1&";
                text += @"sortDirection=false&recordsPerPage=_50&sortBy=PO_NAZVANIYU&" + lawTypeStr + "address=" + town + "&";
                text += @"openMode=DEFAULT_SAVED_SETTING";

                #region Получение заказчиков, заполнение их параметров

                customerSearchProgress_delegate("Поиск заказчиков..", 0);
                doc = internetRequestEngine.GetHtmlDoc(text);
                string checkMessage = "";
                ResultType_enum resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);
                if (resultTypeCheck != ResultType_enum.Done)
                {
                    customerSearchProgress_delegate("Получение заказчиков из БД..", 0);
                    ObservableCollection<Customer> customers = new ObservableCollection<Customer>(mvm.wc.Customers.Where(m => ((m.Name.Contains(customerName)) || (m.Vatin.Contains(customerName)))).ToList());

                    if (customers.Count() > 0)
                    {
                        allCustomersSearched_delegete(ResultType_enum.Done, "Соединение с сервером отсутствует!");
                    }
                    else {
                        allCustomersSearched_delegete(ResultType_enum.ErrorNetwork, "Соединение с сервером отсутствует!\nПобедители в БД не найдены!");
                    }

                    return;
                }

                text = "//div[@class=\"outerWrapper mainPage mainPage\"]";
                text += "/div[@class=\"wrapper\"]";
                text += "/div[@class=\"mainBox\"]";
                text += "/div[@class=\"parametrs margBtm10\"]";
                text += "/div[@class=\"content\"]";
                text += "/div[@id=\"exceedSphinxPageSizeDiv\"]";
                text += "/div[@class=\"registerBox margBtm20\"]";

                HtmlAgilityPack.HtmlNodeCollection customerCollection = doc.DocumentNode.SelectNodes(text);
                if ((customerCollection == null) || (customerCollection.Count == 0)) {
                    allCustomersSearched_delegete(ResultType_enum.NotSearch, "");
                    return;
                }

                // Получение параметров заказчиков
                double customerInterval = 100 / Convert.ToDouble(customerCollection.Count());
                int currentInterval = 0;

                int customerNum = 1;
                Customer customer = null;
                foreach (HtmlAgilityPack.HtmlNode node in customerCollection)
                {

                    currentInterval = Convert.ToInt32(customerNum * customerInterval);
                    customerSearchProgress_delegate("Обработка заказчика.. [" + customerNum + "\\" + customerCollection.Count() + "]", currentInterval);

                    while (isPause)
                    {
                        Thread.Sleep(300);
                    }
                    if (!isWork) break;

                    string customerMessage = "";
                    customer = new Customer();
                    ResultType_enum customerResult = FillCustomer(customer, node, internetRequestEngine,
                                                                      out customerMessage);

                    if (customerResult == ResultType_enum.Error) continue;

                    // Проверить на повтор и записать в БД
                    Customer repeatCustomer = mvm.wc.Customers.FirstOrDefault(m => (m.Name == customer.Name && m.Vatin == customer.Vatin));
                    if (repeatCustomer != null)
                    {
                        if (repeatCustomer.CustomerTypes.FirstOrDefault(m => m.CustomerTypeCode.Trim().ToLower() == customerType_enum.ToString().ToLower()) == null)
                        {
                            repeatCustomer.CustomerTypes.Add(mvm.wc.CustomerTypes.FirstOrDefault(m => m.CustomerTypeCode.ToLower() == customerType_enum.ToString().ToLower()));
                        }
                        customer = repeatCustomer;
                    }
                    else {
                        customer.CustomerTypes.Add(mvm.wc.CustomerTypes.FirstOrDefault(m => m.CustomerTypeCode.ToLower() == customerType_enum.ToString().ToLower()));
                        customer.CreateDateTime = DateTime.Now;
                        mvm.wc.Customers.Add(customer);
                        mvm.wc.SaveChanges();
                    }

                    customerSearched_delegate(customer);

                    customerNum++;
                }
                #endregion
                //return ResultType.Done;
                allCustomersSearched_delegete(ResultType_enum.Done, "");
            }
            catch (Exception ex)
            {
                allCustomersSearched_delegete(ResultType_enum.Error, ex.Message + '\n' + ex.StackTrace);
                return;
            }
        }

        public ResultType_enum FillCustomer(Customer customer, HtmlAgilityPack.HtmlNode customerNode, InternetRequestEngine internetRequestEngine,
                               out string message)
        {
            try
            {
                message = "";
                HtmlAgilityPack.HtmlNode nodeTmp = null;

                #region Определение параметров
                text = ".//table";
                text += "/tr";
                text += "/td[@class=\"descriptTenderTd\"]";
                text += "/dl";
                text += "/dt";
                text += "/a";

                nodeTmp = customerNode.SelectSingleNode(text);

                // Название заказчика
                customer.Name = nodeTmp.InnerText.Trim();

                // Определение ID 94 и 223 законов
                // Если существует атрибут <href>, значит указана одна ссылка на один закон
                LawType_enum lawType = LawType_enum.None;
                Regex regex = new Regex(@"http://.*?Id=\d*");
                MatchCollection matchColl = null;
                if (nodeTmp.Attributes.Contains("href"))
                {
                    matchColl = regex.Matches(nodeTmp.Attributes["href"].Value);
                }
                else
                {
                    matchColl = regex.Matches(nodeTmp.Attributes["onclick"].Value);
                }

                foreach (Match match in matchColl)
                {
                    string lawID = GetIDLaw(match.Value, out lawType);
                    switch (lawType)
                    {
                        case (LawType_enum._44_94):
                            customer.Law_44_94_ID = lawID;
                            if ((customer.Name.Trim().Substring(0, 3) == "...") || 
                                (customer.Name.Trim().Substring(customer.Name.Trim().Length - 3, 3) == "..."))
                            {
                                // Определяем полное наименование организации по 44, 94 закону
                                doc = internetRequestEngine.GetHtmlDoc(match.Value);
                                string checkMessage = "";
                                ResultType_enum resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);

                                if (resultTypeCheck == ResultType_enum.Done)
                                {
                                    regex = new Regex("Полное наименование</span></td><td><span>(.*?)</span>");
                                    Match fullNameMatch = regex.Match(doc.DocumentNode.InnerHtml);
                                    if ((fullNameMatch.Groups.Count > 1) && (fullNameMatch.Groups[1].Value.Trim() != "")) {
                                        customer.Name = Globals.DecodeInternetSymbs(fullNameMatch.Groups[1].Value.Trim());
                                    }

                                }
                            }

                            break;
                        case (LawType_enum._223):
                            customer.Law_223_ID = lawID;
                            if ((customer.Name.Trim().Substring(0, 3) == "...") ||
                                (customer.Name.Trim().Substring(customer.Name.Trim().Length - 3, 3) == "..."))
                            {
                                // Определяем полное наименование организации по 223 закону
                                doc = internetRequestEngine.GetHtmlDoc(match.Value);
                                string checkMessage = "";
                                ResultType_enum resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);

                                if (resultTypeCheck == ResultType_enum.Done)
                                {
                                    text = "//div[@class=\"cardWrapper\"]";
                                    text += "/div[@class=\"wrapper\"]";
                                    text += "/div[@class=\"mainBox\"]";
                                    text += "/div[@class=\"contentTabBoxBlock\"]";
                                    text += "/div[@class=\"noticeTabBox padBtm20\"]";
                                    text += "/div[@class=\"noticeTabBoxWrapper\"]";
                                    text += "/table";
                                    text += "/tr";
                                    text += "/td";

                                    HtmlAgilityPack.HtmlNodeCollection customer223NameTdNodes = doc.DocumentNode.SelectNodes(text);

                                    bool nextNodeIsName = false;
                                    foreach (HtmlAgilityPack.HtmlNode customer223NameTdNode in customer223NameTdNodes) {

                                        if (nextNodeIsName)
                                        {
                                            customer.Name = Globals.DecodeInternetSymbs(customer223NameTdNode.InnerText.Trim());
                                            break;
                                        }

                                        if (customer223NameTdNode.InnerText.Trim().ToLower() == "полное наименование организации") {
                                            nextNodeIsName = true;
                                        }

                                    }
                                }
                            }

                            break;
                        default:
                            break;
                    }
                }

                // Уровень организации
                text = ".//table";
                text += "/tr";
                text += "/td[@class=\"tenderTd\"]";
                text += "/dl";
                text += "/dt";

                nodeTmp = customerNode.SelectSingleNode(text);
                CustomerLevel_enum customerLevel = CustomerLevel_enum.None;
                if (nodeTmp != null) {
                    switch(nodeTmp.InnerText.ToLower().Trim()) {
                        case ("федеральный уровень"):
                            customerLevel = CustomerLevel_enum.Federal;
                            break;
                        case ("уровень субъекта рф"):
                            customerLevel = CustomerLevel_enum.Subject;
                            break;
                        case ("муниципальный уровень"):
                            customerLevel = CustomerLevel_enum.Municipal;
                            break;
                        case ("иное"):
                            customerLevel = CustomerLevel_enum.Other;
                            break;
                    }
                }
                customer.CustomerLevel = mvm.wc.CustomerLevels.FirstOrDefault(m => m.CustomerLevelCode == customerLevel.ToString());

                // ИНН (VATIN)
                text = ".//table";
                text += "/tr";
                text += "/td[@class=\"descriptTenderTd\"]";
                text += "/dl";
                text += "/dd[@class=\"nameOrganization margTop10 grayText\"]";

                HtmlAgilityPack.HtmlNode vatinNode = customerNode.SelectSingleNode(text);
                if (vatinNode != null) {
                    Regex regexVatin = new Regex("ИНН\\:\\s{0,2}(\\d*)", RegexOptions.IgnoreCase);
                    Match match = regexVatin.Match(vatinNode.InnerText);
                    if (match.Groups.Count > 1) {
                        customer.Vatin = match.Groups[1].Value;
                    }
                }

                // Адрес
                text = ".//table";
                text += "/tr";
                text += "/td[@class=\"descriptTenderTd\"]";
                text += "/dl";
                text += "/dd";

                HtmlAgilityPack.HtmlNodeCollection adressDDColl = customerNode.SelectNodes(text);
                if (adressDDColl.Count == 3)
                {
                    customer.Address = Globals.CutAddress(adressDDColl[2].InnerText.Trim());
                }
                #endregion


                return ResultType_enum.Done;
            }
            catch (Exception ex)
            {
                message = ex.Message + "\n\n" + ex.StackTrace;
                return ResultType_enum.Error;
            }
        }


        #region Получение ID закона и его тип
        // 223 закон   http://zakupki.gov.ru/223/ppa/public/organization/organization.html?epz=true&amp;agencyId=108667 
        // 94-44 закон http://zakupki.gov.ru/pgz/public/action/organization/view?source=epz&amp;organizationId=1428866
        private string GetIDLaw(string text, out LawType_enum lawType_enum)
        {
            lawType_enum = LawType_enum.None;
            try
            {
                int IDpos = text.IndexOf("Id=");
                if (IDpos < 0)
                {
                    lawType_enum = LawType_enum.None;
                    return "";
                }

                if (text.IndexOf(@"zakupki.gov.ru/223") > -1)
                {
                    lawType_enum = LawType_enum._223;
                    return text.Substring(IDpos + 3, text.Length - (IDpos + 3));
                }
                else
                {
                    if (text.IndexOf(@"zakupki.gov.ru/pgz") > -1)
                    {
                        lawType_enum = LawType_enum._44_94;
                        return text.Substring(IDpos + 3, text.Length - (IDpos + 3));
                    }
                    else
                    {
                        lawType_enum = LawType_enum.None;
                        return "";
                    }
                }
            }
            catch
            {
                return "";
            }
        }
        #endregion

        public void Operate()
        {
            Thread searchCustomers_thread = new Thread(SearchCustomers_proc);
            searchCustomers_thread.Start();
        }

        public bool isWorking()
        {
            return isWork;
        }

        public void PausePlay()
        {
            if (isPause)
            {
                isPause = false;
            }
            else
            {
                isPause = true;
            }
        }

        public void Stop()
        {
            isWork = false;
            isPause = false;
        }
    }
}
