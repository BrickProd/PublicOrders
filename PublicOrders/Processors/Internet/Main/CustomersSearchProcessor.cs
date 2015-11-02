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
    public delegate void CustomersSearchDone_delegate(ObservableCollection<Customer> customers, ResultType_enum ResultType_enum, string message);


    public class CustomersSearchProcessor
    {
        MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        private string text = "";
        private HtmlAgilityPack.HtmlDocument doc;
        private InternetRequestEngine internetRequestEngine  = null;

        private bool isWork = false;

        private string customerName = "";
        private decimal priceMin = 0;
        private decimal priceMax = 0;
        private string town = "";
        private DateTime publishDateMin;
        private DateTime publishDateMax;
        private CustomerType_enum customerType_enum;
        private LawType_enum lawType_enum;
        private CustomersSearchDone_delegate customersSearchDone_delegate = null;

        private ObservableCollection<Customer> customers;

        public CustomersSearchProcessor(string _customerName, CustomerType_enum _customerType_enum, decimal _priceMin, decimal _priceMax, string _town,
                               DateTime _publishDateMin, DateTime _publishDateMax,
                               LawType_enum _lawType_enum, CustomersSearchDone_delegate _customersSearchDone_delegate)
        {
            customerName = _customerName;
            customerType_enum = _customerType_enum;
            priceMin = _priceMin;
            priceMax = _priceMax;
            town = _town;
            publishDateMin = _publishDateMin;
            publishDateMax = _publishDateMax;
            lawType_enum = _lawType_enum;
            customersSearchDone_delegate = _customersSearchDone_delegate;
        }

        private void SearchCustomers_proc()
        {
            try
            {
                customers = new ObservableCollection<Customer>();
                InternetRequestEngine internetRequestEngine = new InternetRequestEngine();
                if (customerName == "") {
                    customersSearchDone_delegate(customers, ResultType_enum.NullSearchText, "");
                    return;
                } 

                string lawTypeStr = "";
                switch (lawType_enum)
                {
                    case (LawType_enum._44_94):
                        lawTypeStr = "FZ_44";
                        break;
                    case (LawType_enum._223):
                        lawTypeStr = "FZ_223";
                        break;
                    case (LawType_enum._44_94_223):
                        lawTypeStr = "EVERYWHERE";
                        break;
                }

                string customerTypeStr = "";
                switch (customerType_enum)
                {
                    case (CustomerType_enum.Customer):
                        customerTypeStr = "CUSTOMER";
                        break;
                    case (CustomerType_enum.Organization):
                        customerTypeStr = "REPRESENTATIVE_ORGANIZATION";
                        break;
                }

                text = @"http://zakupki.gov.ru/epz/organization/organization/extended/search/result.html?sortDirection=true&";
                text += @"organizationSimpleSorting=PO_NAZVANIYU&recordsPerPage=_100&pageNumber=1&searchText=" + HttpUtility.UrlEncode(customerName) + "&";
                text += @"strictEqual=false&morphology=false&placeOfSearch=" + lawTypeStr + "&registrationStatusType=ANY&";
                text += @"kpp=&custLev=F%2CS%2CM%2CNOT_FSM&organizationRoleList=" + customerTypeStr + "&";
                text += @"okvedCode=&okvedWithSubElements=false&districtIds=&regionIds=&cityIds=&organizationTypeList=&";
                text += @"spz=&withBlocked=false&customerIdentifyCode=&headAgencyCode=&headAgencyWithSubElements=false&";
                text += @"organizationsWithBranches=false&legalEntitiesTypeList=&ppoWithSubElements=false&ppoCode=&";
                text += @"address=" + town + "&town=&publishedOrderClause=true&unpublishedOrderClause=true&bik=&bankRegNum=&";
                text += @"bankIdCode=";

                #region Получение заказчиков, заполнение их параметров
                doc = internetRequestEngine.GetHtmlDoc(text);
                string checkMessage = "";
                ResultType_enum resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);
                //resultTypeCheck = ResultType_enum.ErrorNetwork;
                if (resultTypeCheck != ResultType_enum.Done)
                {
                    customers = new ObservableCollection<Customer>(mvm.wc.Customers.Where(m => ((m.Name.Contains(customerName)) || (m.Vatin.Contains(customerName)))).ToList());

                    if (customers.Count() > 0)
                    {
                        customersSearchDone_delegate(customers, ResultType_enum.Done, "Соединение с сервером отсутствует!");
                    }
                    else {
                        customersSearchDone_delegate(customers, ResultType_enum.ErrorNetwork, "Соединение с сервером отсутствует!\nПобедители в БД не найдены!");
                    }

                    return;
                }

                // Если заказчики не найдены
                text = "//div[@class=\"outerWrapper mainPage\"]";
                text += "/div[@class=\"wrapper\"]";
                text += "/div[@class=\"mainBox\"]";
                text += "/div[@class=\"rightCol\"]";
                text += "/div[@class=\"content\"]";
                text += "/div[@class=\"paginator\"]";

                HtmlAgilityPack.HtmlNode htmlNode = doc.DocumentNode.SelectSingleNode(text);
                if (htmlNode == null)
                {
                    customersSearchDone_delegate(customers, ResultType_enum.ErrorNetwork, "");
                    return;
                }
                if (htmlNode.InnerText.Trim() == "Поиск не дал результатов")
                {
                    customersSearchDone_delegate(customers, ResultType_enum.NotSearch, "");
                    return;
                }

                // Определение количества страниц заказчиков
                int pageCustomersCount = 0;
                text = "//div[@class=\"outerWrapper mainPage\"]";
                text += "/div[@class=\"wrapper\"]";
                text += "/div[@class=\"mainBox\"]";
                text += "/div[@class=\"rightCol\"]";
                text += "/div[@class=\"content\"]";
                text += "/div[@class=\"paginator\"]";
                text += "/ul[@class=\"paging\"]/li";

                HtmlAgilityPack.HtmlNodeCollection liCollection = doc.DocumentNode.SelectNodes(text);
                if (liCollection == null)
                {
                    pageCustomersCount = 1;
                }
                else
                {
                    bool nextTagIsCount = false;
                    foreach (HtmlAgilityPack.HtmlNode node in liCollection)
                    {
                        if (nextTagIsCount)
                        {
                            pageCustomersCount = Convert.ToInt32(node.InnerText);
                            break;
                        }

                        if (node.InnerText.ToLower() == "из")
                        {
                            nextTagIsCount = true;
                        }
                    }
                }



                text = "//div[@class=\"outerWrapper mainPage\"]";
                text += "/div[@class=\"wrapper\"]";
                text += "/div[@class=\"mainBox\"]";
                text += "/div[@class=\"rightCol\"]";
                text += "/div[@class=\"content\"]";
                text += "/div[@id=\"exceedSphinxPageSizeDiv\"]";
                text += "/div[@class=\"registerBox\"]";

                HtmlAgilityPack.HtmlNodeCollection customerCollection = doc.DocumentNode.SelectNodes(text);
                if ((customerCollection == null) || (customerCollection.Count == 0)) {
                    customersSearchDone_delegate(customers, ResultType_enum.NotSearch, "");
                    return;
                }

                // Получение параметров заказчиков
                int customerNum = 1;
                Customer customer = null;
                foreach (HtmlAgilityPack.HtmlNode node in customerCollection)
                {
                    string customerMessage = "";
                    customer = new Customer();
                    ResultType_enum customerResult = FillCustomer(customer, node, internetRequestEngine,
                                                                      out customerMessage);
                    switch (customerResult)
                    {
                        case (ResultType_enum.Error):
                            customersSearchDone_delegate(customers, ResultType_enum.Error, customerMessage);
                            return;
                        default:
                            break;
                    }
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

                    customers.Add(customer);

                    customerNum++;
                }
                #endregion
                //return ResultType.Done;
                customersSearchDone_delegate(customers, ResultType_enum.Done, "");
            }
            catch (Exception ex)
            {
                customersSearchDone_delegate(customers, ResultType_enum.Error, ex.Message + '\n' + ex.StackTrace);
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
                //text += "/tbody"; !!!newBrowser
                text += "/tr";
                text += "/td[@class=\"descriptTenderTd\"]";
                text += "/dl";
                text += "/dt";
                text += "/a";

                nodeTmp = customerNode.SelectSingleNode(text);

                // Название заказчика
                if (nodeTmp.Attributes.Contains("title"))
                    customer.Name = Globals.DecodeInternetSymbs(nodeTmp.Attributes["title"].Value.Trim());

                // Определение ID 94 и 223 законов
                // Есди существует атрибут <href>, значит указана одна ссылка на один закон
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
                            break;
                        case (LawType_enum._223):
                            customer.Law_223_ID = lawID;
                            break;
                        default:
                            break;
                    }

                    //break;
                }

                // Уровень организации
                text = ".//table";
                //text += "/tbody"; !!!newBrowser
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
                //text += "/tbody"; !!!newBrowser
                text += "/tr";
                text += "/td[@class=\"descriptTenderTd\"]";
                text += "/dl";
                text += "/dd[@class=\"nameOrganization\"]";
                text += "/span";

                HtmlAgilityPack.HtmlNodeCollection collectionVatin = customerNode.SelectNodes(text);
                foreach (HtmlAgilityPack.HtmlNode nodeVatin in collectionVatin)
                {
                    int vatinIntPos = nodeVatin.InnerText.IndexOf("ИНН:");
                    if (vatinIntPos > -1)
                    {
                        customer.Vatin = nodeVatin.InnerText.Substring(vatinIntPos + 4, nodeVatin.InnerText.Length - (vatinIntPos + 4)).Trim();
                        break;
                    }
                }

                // Адрес
                text = ".//table";
                //text += "/tbody"; !!!newBrowser
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

        public void Stop()
        {
            isWork = false;
        }
    }
}
