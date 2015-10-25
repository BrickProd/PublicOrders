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

namespace PublicOrders.Processors.Main
{
    public delegate void CustomersSearchDone_delegate(ResultType_enum ResultType_enum, string message);


    public class CustomersSearchProcessor
    {
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
        private int searchingProgress = 0;
        private ObservableCollection<Customer> customers;

        public CustomersSearchProcessor(string _customerName, CustomerType_enum _customerType_enum, decimal _priceMin, decimal _priceMax, string _town,
                               DateTime _publishDateMin, DateTime _publishDateMax,
                               LawType_enum _lawType_enum, CustomersSearchDone_delegate _customersSearchDone_delegate,
                               int _searchingProgress, ObservableCollection<Customer> _customers)
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
            searchingProgress = _searchingProgress;
            customers = _customers;
        }

        private void SearchCustomers_proc()
        {
            /*try
            {
                message = "";
                customers = new List<Customer>();

                if (customerName == "") return ResultType.NullSearchText;

                string lawTypeStr = "";
                switch (lawType)
                {
                    case (LawType._44_94):
                        lawTypeStr = "FZ_44";
                        break;
                    case (LawType._223):
                        lawTypeStr = "FZ_223";
                        break;
                    case (LawType._44_94_223):
                        lawTypeStr = "EVERYWHERE";
                        break;
                }

                string customerTypeStr = "";
                switch (customerType)
                {
                    case (CustomerType.Customer):
                        customerTypeStr = "CUSTOMER";
                        break;
                    case (CustomerType.Organization):
                        customerTypeStr = "REPRESENTATIVE_ORGANIZATION";
                        break;
                }

                text = @"http://zakupki.gov.ru/epz/organization/organization/extended/search/result.html?sortDirection=true&";
                text += @"organizationSimpleSorting=PO_NAZVANIYU&recordsPerPage=_100&pageNumber=1&searchText=" + customerName + "&";
                text += @"strictEqual=false&morphology=false&placeOfSearch=" + lawTypeStr + "&registrationStatusType=ANY&";
                text += @"kpp=&custLev=F%2CS%2CM%2CNOT_FSM&organizationRoleList=" + customerTypeStr + "&";
                text += @"okvedCode=&okvedWithSubElements=false&districtIds=&regionIds=&cityIds=&organizationTypeList=&";
                text += @"spz=&withBlocked=false&customerIdentifyCode=&headAgencyCode=&headAgencyWithSubElements=false&";
                text += @"organizationsWithBranches=false&legalEntitiesTypeList=&ppoWithSubElements=false&ppoCode=&";
                text += @"address=" + customerAddress + "&town=&publishedOrderClause=true&unpublishedOrderClause=true&bik=&bankRegNum=&";
                text += @"bankIdCode=";

                #region Получение заказчиков, заполнение их параметров
                doc = explorerEngine.GetHtmlDoc(text);
                string checkMessage = "";
                ResultType resultTypeCheck = Globals_DLL.CheckDocResult(doc, out checkMessage);
                if (resultTypeCheck != ResultType.Done)
                {
                    message = checkMessage;
                    return resultTypeCheck;
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
                    message = "Поиск не дал результатов";
                    return ResultType.ErrorNetwork;
                }
                if (htmlNode.InnerText.Trim() == "Поиск не дал результатов")
                {
                    return ResultType.NotSearch;
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
                if ((customerCollection == null) || (customerCollection.Count == 0))
                    return ResultType.NotSearch;

                // Получение параметров заказчиков
                int customerNum = 1;
                Customer customer = null;
                foreach (HtmlAgilityPack.HtmlNode node in customerCollection)
                {
                    string customerMessage = "";
                    customer = new Customer();
                    ResultType customerResult = customer.FillCustomer(node, explorerEngine,
                                                                      out customerMessage);
                    switch (customerResult)
                    {
                        case (ResultType.Error):
                            message = customerMessage;
                            return ResultType.Error;
                        case (ResultType.NotSearch):
                            break;
                        default:
                            break;
                    }
                    customer.customerType = customerType;

                    customers.Add(customer);
                    customerNum++;
                }
                #endregion
                //return ResultType.Done;
                return ResultType.DoneNetwork;
            }
            catch (Exception ex)
            {
                message = ex.Message + '\n' + ex.StackTrace;
                customers = null;
                return ResultType.Error;
            }*/
        }

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
