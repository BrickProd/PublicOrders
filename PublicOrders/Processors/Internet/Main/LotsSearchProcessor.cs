using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Models;
using System.Windows;
using System.Threading;
using System.Collections.ObjectModel;
using PublicOrders.Processors.Internet;
using System.Web;

namespace PublicOrders.Processors.Main
{
    public delegate void AllLotsSearched_delegete(ResultType_enum ResultType_enum, string message);
    public delegate void LotSearched_delegate(Winner winner);


    public class LotsSearchProcessor
    {
        MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        private string text = "";
        private HtmlAgilityPack.HtmlDocument doc;
        private InternetRequestEngine internetRequestEngine = null;

        private bool isWork = false;
        private bool isPause = false;

        private Customer customer = null;
        private CustomerType_enum customerType_enum;
        private LawType_enum lawType_enum;
        private Int64 lowPrice = 0;
        private Int64 highPrice = 0;
        private DateTime lowPublishDate;
        private DateTime highPublishDate;

        private WinnerSearchEngine winnerSearchEngine = null;
        private AllLotsSearched_delegete allLotsSearched_delegete = null;
        private LotSearched_delegate lotSearched_delegate = null;
        private int searchingProgress = 0;

        private ObservableCollection<Order> orders = null;

        public LotsSearchProcessor(Customer _customer, CustomerType_enum _customerType_enum, 
                                   LawType_enum _lawType_enum, Int64 _lowPrice, Int64 _highPrice,
                                   DateTime _lowPublishDate, DateTime _highPublishDate,
                                   LotSearched_delegate _lotSearched_delegate, 
                                   AllLotsSearched_delegete _allLotsSearched_delegete)
        {

            customer = _customer;
            customerType_enum = _customerType_enum;
            lawType_enum = _lawType_enum;
            lowPrice = _lowPrice;
            highPrice = _highPrice;
            lowPublishDate = _lowPublishDate;
            highPublishDate = _highPublishDate;
            lotSearched_delegate = _lotSearched_delegate;
            allLotsSearched_delegete = _allLotsSearched_delegete;

            winnerSearchEngine = new WinnerSearchEngine();
        }

        private void LotsSearch_proc()
        {
            try
            {
                isWork = true;
                #region Запрос в интернет на получение заказов заказчика
                orders = new ObservableCollection<Order>();
                InternetRequestEngine internetRequestEngine = new InternetRequestEngine();

                string lawTypeStr = "";
                switch (lawType_enum)
                {
                    case (LawType_enum._44_94_223):
                        lawTypeStr = "FZ_44%2CFZ_223%2CFZ_94";
                        break;
                    case (LawType_enum._44_94):
                        lawTypeStr = "FZ_44%2CFZ_94";
                        break;
                    case (LawType_enum._223):
                        lawTypeStr = "FZ_223";
                        break;
                }

                //lawTypeStr = "FZ_94"; // !!! и 50 записей

                /*switch (customerType_enum)
                {
                    case (CustomerType_enum.Customer):*/
                        text = @"http://zakupki.gov.ru/epz/order/extendedsearch/search.html?sortDirection=false&";
                        text += @"sortBy=UPDATE_DATE&recordsPerPage=_500&pageNo=1&placeOfSearch=" + lawTypeStr + "&";
                        text += @"searchType=ORDERS&morphology=false&strictEqual=false&orderPriceFrom=" + lowPrice + "&orderPriceTo=" + highPrice + "&orderPriceCurrencyId=-1&";
                        text += @"deliveryAddress=&orderPublishDateFrom=" + lowPublishDate.ToString("dd.MM.yyyy") + "&orderPublishDateTo=" + highPublishDate.ToString("dd.MM.yyyy") + "&okdpWithSubElements=false&orderStages=PC&";
                        text += @"headAgencyWithSubElements=false&smallBusinessSubject=I&rnpData=I&executionRequirement=I&penalSystemAdvantage=I&disabilityOrganizationsAdvantage=I&";
                        text += @"russianGoodsPreferences=I&orderPriceCurrencyId=-1&okvedWithSubElements=false&jointPurchase=false&byRepresentativeCreated=false&";
                        text += @"selectedMatchingWordPlace223=NOTICE_AND_DOCS&matchingWordPlace94=NOTIFICATIONS&matchingWordPlace44=NOTIFICATIONS&searchAttachedFile=false&";
                        text += @"changeParameters=true&showLotsInfo=false&customer.code=&customer.fz94id=" + Convert.ToString(customer.Law_44_94_ID) + "&customer.fz223id=" + Convert.ToString(customer.Law_223_ID) + "&";
                        text += @"customer.title=" + customer.Name + "&customer.inn=" + customer.Vatin + "&";
                        text += @"extendedAttributeSearchCriteria.searchByAttributes=NOTIFICATION&law44.okpd.withSubElements=false";

                        /*break;
                    case (CustomerType_enum.Organization):
                        // Запрос на организации
                        text += @"http://zakupki.gov.ru/epz/order/extendedsearch/search.html?sortDirection=false&";
                        text += @"sortBy=UPDATE_DATE&recordsPerPage=_500&pageNo=1&placeOfSearch=" + lawTypeStr + "&searchType=ORDERS&";
                        text += @"morphology=false&strictEqual=false&orderPriceFrom=" + lowPrice + "&orderPriceTo=" + highPrice + "&orderPriceCurrencyId=-1&";
                        text += @"deliveryAddress=Москва&orderPublishDateFrom=" + lowPublishDate.ToString("dd.MM.yyyy") + "&orderPublishDateTo=" + highPublishDate.ToString("dd.MM.yyyy") + "&";
                        text += @"okdpWithSubElements=false&orderStages=PC&headAgencyWithSubElements=false&smallBusinessSubject=I&";
                        text += @"rnpData=I&executionRequirement=I&penalSystemAdvantage=I&disabilityOrganizationsAdvantage=I&";
                        text += @"russianGoodsPreferences=I&orderPriceCurrencyId=-1&okvedWithSubElements=false&jointPurchase=false&";
                        text += @"byRepresentativeCreated=false&selectedMatchingWordPlace223=NOTICE_AND_DOCS&matchingWordPlace94=NOTIFICATIONS&";
                        text += @"matchingWordPlace44=NOTIFICATIONS&searchAttachedFile=false&changeParameters=true&showLotsInfo=false&";
                        text += @"agency.code=&agency.fz94id=" + Convert.ToString(customer.Law_44_94_ID) + "&agency.title=" + customer.Name + "&";
                        text += @"agency.inn=&extendedAttributeSearchCriteria.searchByAttributes=NOTIFICATION&law44.okpd.withSubElements=false";
                        break;
                    default:
                        allLotsSearched_delegete(ResultType_enum.Error, "Неизвестный тип заказчика <" + customerType_enum.ToString() + ">");
                        return;
                }*/

                doc = internetRequestEngine.GetHtmlDoc(text);
                string checkMessage = "";
                ResultType_enum resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);
                //resultTypeCheck = ResultType_enum.ErrorNetwork;
                if (resultTypeCheck != ResultType_enum.Done)
                {
                    // Если нет подключения к интернету, то берем значения из БД
                    bool searchedFromDB = false;
                    List<Order> orders = mvm.wc.Orders.Where(m => ((m.Customer.Name == customer.Name) && 
                                                                   (m.PublishDateTime < highPublishDate) && 
                                                                   (m.PublishDateTime > lowPublishDate))).ToList();

                    if (orders != null) {
                        switch (lawType_enum)
                        {
                            case (LawType_enum._44_94_223):
                                orders = orders.Where(m => ((m.LawType.Name == "44") || (m.LawType.Name == "94") || (m.LawType.Name == "223"))).ToList();
                                break;
                            case (LawType_enum._44_94):
                                orders = orders.Where(m => ((m.LawType.Name == "44") || (m.LawType.Name == "94"))).ToList();
                                break;
                            case (LawType_enum._223):
                                orders = orders.Where(m => m.LawType.Name == "223").ToList();
                                break;
                        }

                        List<Lot> searchLots = null;
                        foreach (Order searchOrder in orders)
                        {
                            searchLots = searchOrder.Lots.Where(m => ((m.Price > lowPrice) && (m.Price < highPrice))).ToList();
                            foreach (Lot searchLot in searchLots)
                            {
                                if ((searchLot.Winners != null) && (searchLot.Winners.Count() > 0))
                                {
                                    searchedFromDB = true;
                                    lotSearched_delegate(searchLot.Winners.ElementAt(0));
                                }
                            }
                        }
                    }

                    if (searchedFromDB == false)
                    {
                        allLotsSearched_delegete(ResultType_enum.ErrorNetwork, "Соединение с сервером отсутствует!\nПобедители в БД не найдены!");
                    }
                    else {
                        allLotsSearched_delegete(ResultType_enum.Done, "Соединение с сервером отсутствует!");
                    }

                    return;
                }


                text = "//div[@class=\"outerWrapper mainPage\"]";
                text += "/div[@class=\"wrapper\"]";
                text += "/div[@class=\"mainBox\"]";
                text += "/div[@class=\"mainBox\"]";
                text += "/div[@class=\"rightCol\"]";
                text += "/div[@class=\"content\"]";
                text += "/div[@id=\"exceedSphinxPageSizeDiv\"]";
                text += "/div[@class=\"registerBox\"]";

                HtmlAgilityPack.HtmlNodeCollection orderCollection = doc.DocumentNode.SelectNodes(text);
                if ((orderCollection == null) || (orderCollection.Count == 0)) {
                    allLotsSearched_delegete(ResultType_enum.NotSearch, "");
                    return;
                }
                #endregion


                int orderNum = 1;
                Order order = null;
                foreach (HtmlAgilityPack.HtmlNode nodeOrder in orderCollection)
                {
                    while (isPause) {
                        Thread.Sleep(300);
                    }
                    if (!isWork) break;

                    #region Заполнение заказа
                    string orderMessage = "";
                    order = new Order();
                    order.Customer = customer;
                    ResultType_enum orderResult = FillOrder(order, nodeOrder, internetRequestEngine,
                                                             orderNum, orderCollection.Count,
                                                             out orderMessage);
                    switch (orderResult)
                    {
                        case (ResultType_enum.Error):
                            /*message = orderMessage;
                            return ResultType.Error;*/
                            continue;
                        default:
                            break;
                    }

                    // Проверить на повтор и записать в БД
                    Order repeatOrder = mvm.wc.Orders.ToList().FirstOrDefault(m => (m.Number == order.Number));
                    if (repeatOrder == null)
                    {
                        order.CreateDateTime = DateTime.Now;
                        mvm.wc.Orders.Add(order);
                        mvm.wc.SaveChanges();
                    }
                    else {
                        order = repeatOrder;
                    }

                    orders.Add(order);
                    orderNum++;
                    #endregion

                    #region Поиск победителей по заказу
                    var lots = mvm.wc.Lots.Where(p => p.Order.Number.Trim().ToLower() == order.Number.Trim().ToLower()).ToList();
                    // В БД уже есть лоты на заказ (лот заполняется вместе с его победителем, поэтому выводим эти лоты)
                    if ((lots != null) && (lots.Count() > 0))
                    {
                        //var winner = null;
                        foreach (var lot in lots ) {
                            var winner = mvm.wc.Winners.ToList().FirstOrDefault(m => m.Lot == lot);
                            if ((winner != null) && (winner.Name.Trim() != ""))
                                lotSearched_delegate(winner);
                        }
                    }
                    // Начинаем поиск победителей в интернете
                    else {
                        // Ищем победителей, если у заказа прошла неделя с момента поиска победителей
                        if ((order.WinnersSearchDateTime == null) || ((DateTime.Now - order.WinnersSearchDateTime) > TimeSpan.FromDays(7))) {
                            string winnerEngineMessage = "";
                            winnerSearchEngine.FillWinners(order, internetRequestEngine, lotSearched_delegate, out winnerEngineMessage);

                            // Сохраняем дату поиска победителей
                            order.WinnersSearchDateTime = DateTime.Now;
                            mvm.wc.Entry(order).State = System.Data.Entity.EntityState.Modified;
                            mvm.wc.SaveChanges();
                        }
                    }
                    #endregion
                }

                allLotsSearched_delegete(ResultType_enum.Done, "");
            }
            catch (Exception ex)
            {
                isWork = false;
                orders = null;
                allLotsSearched_delegete(ResultType_enum.Error, ex.Message + '\n' + ex.StackTrace);
                return;
            }
        }
        

        private ResultType_enum FillOrder(Order order, HtmlAgilityPack.HtmlNode orderNode, InternetRequestEngine internetRequestEngine,
                            int orderNum, int orderCount, out string orderMessage)
        {
            orderMessage = "";
            HtmlAgilityPack.HtmlNode nodeTmp = null;

            try
            {
                #region Определение параметров
                // Тип заказа
                text = ".//table";
                text += "/tr";
                text += "/td[@class=\"tenderTd\"]";
                text += "/dl";
                text += "/dt";

                nodeTmp = orderNode.SelectSingleNode(text);
                string orderTypeStr = nodeTmp.InnerText.Trim();
                OrderType orderType = mvm.wc.OrderTypes.FirstOrDefault(m => m.Name.ToLower() == orderTypeStr.ToLower());
                if (orderType == null) {
                    orderType = new OrderType();
                    orderType.Name = orderTypeStr;
                    mvm.wc.OrderTypes.Add(orderType);
                    mvm.wc.SaveChanges();
                }
                order.OrderType = orderType;

                // Номер заказа
                text = ".//table";
                text += "/tr";
                text += "/td[@class=\"descriptTenderTd\"]";
                text += "/dl";
                text += "/dt";
                text += "/a";

                nodeTmp = orderNode.SelectSingleNode(text);
                order.Number = ConvertNum(nodeTmp.InnerText.Trim());

                // Название заказа
                text = ".//table";
                text += "/tr";
                text += "/td[@class=\"descriptTenderTd\"]";
                text += "/dl";
                text += "/dd";

                HtmlAgilityPack.HtmlNodeCollection nodeCol = orderNode.SelectNodes(text);
                foreach (HtmlAgilityPack.HtmlNode node in nodeCol)
                {
                    if (!node.Attributes.Contains("class"))
                    {
                        text = ".//a";
                        string orderNameStr = Globals.DecodeInternetSymbs(node.SelectSingleNode(text).Attributes["title"].Value).Trim();
                        if (orderNameStr.Length > 400)
                        {
                            orderNameStr = orderNameStr.Substring(0, 398) + "..";
                        }

                        order.Name = orderNameStr;
                        break;
                    }
                }

                // Номер закона
                lawType_enum = LawType_enum.None;
                HtmlAgilityPack.HtmlNodeCollection lowTypeColl = null;

                text = ".//table";
                text += "/tr";
                text += "/td[@class=\"amountTenderTd \"]";
                text += "/p";
                text += "/span";

                lowTypeColl = orderNode.SelectNodes(text);
                if (nodeTmp == null)
                {
                    text = ".//table";
                    text += "/tr";
                    text += "/td[@class=\"amountTenderTd\"]";
                    text += "/p";
                    text += "/span";

                    lowTypeColl = orderNode.SelectNodes(text);
                }

                bool searched = false;
                foreach (HtmlAgilityPack.HtmlNode lowTypeNode in lowTypeColl)
                {
                    if (lowTypeNode.Attributes.Contains("class"))
                    {
                        switch (lowTypeNode.Attributes["class"].Value.Trim())
                        {
                            case ("fz94"):
                                searched = true;
                                order.LawType = mvm.wc.LawTypes.FirstOrDefault(m => m.Name == "94");
                                break;
                            case ("fz44"):
                                searched = true;
                                order.LawType = mvm.wc.LawTypes.FirstOrDefault(m => m.Name == "44");
                                break;
                            case ("fz223"):
                                searched = true;
                                order.LawType = mvm.wc.LawTypes.FirstOrDefault(m => m.Name == "223");
                                break;
                            default:
                                order.LawType = mvm.wc.LawTypes.FirstOrDefault(m => m.Name == "None");
                                break;
                        }
                    }
                    if (searched) break;
                }


                // Поиск ссылки на закупку
                switch (order.LawType.Name.Trim().ToLower())
                {
                    case ("44"):
                        //href = @"http://zakupki.gov.ru/epz/order/notice/ea44/view/supplier-results.html?regNumber=" + this.number;
                        order.HrefId = order.Number;
                        break;
                    case ("94"):
                        text = ".//table";
                        text += "/tr";
                        text += "/td[@class=\"descriptTenderTd\"]";
                        text += "/dl";
                        text += "/dt";
                        text += "/a";

                        HtmlAgilityPack.HtmlNode hrefNode = orderNode.SelectSingleNode(text);
                        if (hrefNode.Attributes.Contains("href"))
                        {
                            // Поиск ID для ссылки protocols.html?noticeId
                            order.HrefId = hrefNode.Attributes["href"].Value.Trim();
                            order.HrefId = order.HrefId.Substring(order.HrefId.IndexOf("?source=epz&notificationId=") + 27, order.HrefId.Length - (order.HrefId.IndexOf("?source=epz&notificationId=") + 27));
                            //order.HrefId = order.HrefId.Substring(order.HrefId.IndexOf(".html?noticeId") + 15, order.HrefId.IndexOf("&epz=true") - (order.HrefId.IndexOf(".html?noticeId") + 15));
                        }
                        break;
                    case ("223"):
                        text = ".//table";
                        text += "/tr";
                        text += "/td[@class=\"descriptTenderTd\"]";
                        text += "/dl";
                        text += "/dt";
                        text += "/a";

                        hrefNode = orderNode.SelectSingleNode(text);
                        if (hrefNode.Attributes.Contains("href"))
                        {
                            // Поиск ID для ссылки protocols.html?noticeId
                            order.HrefId = hrefNode.Attributes["href"].Value.Trim().Replace("common-info", "protocols");
                            order.HrefId = order.HrefId.Substring(order.HrefId.IndexOf(".html?noticeId") + 15, order.HrefId.IndexOf("&epz=true") - (order.HrefId.IndexOf(".html?noticeId") + 15));
                        }

                        break;
                    default:
                        order.HrefId = "None";
                        break;
                }

                // Цена и валюта
                text = ".//table";
                text += "/tr";
                text += "/td[@class=\"amountTenderTd \"]";
                text += "/dl";
                HtmlAgilityPack.HtmlNode priceNode = orderNode.SelectSingleNode(text);

                foreach (HtmlAgilityPack.HtmlNode dtddNode in priceNode.ChildNodes)
                {
                    switch (dtddNode.Name.Trim().ToLower())
                    {
                        case ("dt"):
                            // Цена
                            string orderPriceStr = dtddNode.InnerText.Trim().Replace(" ", "").ToLower();
                            if (orderPriceStr != "нескольколотов") {
                                try
                                {
                                    
                                    if (orderPriceStr.IndexOf(',') > -1)
                                    {
                                        order.Price = Convert.ToInt64(orderPriceStr.Substring(0, orderPriceStr.IndexOf(',')));
                                    }
                                    else {
                                        order.Price = Convert.ToInt64(orderPriceStr);
                                    }
                                }
                                catch {
                                    string sss = "ывавы";
                                }
                            }
                            break;
                        case ("dd"):
                            // Валюта
                            string orderPriceTypeStr = dtddNode.InnerText.Trim();

                            OrderPriceType orderPriceType = mvm.wc.OrderPriceTypes.FirstOrDefault(m => m.Name.ToLower().Trim() == orderPriceTypeStr.ToLower());
                            if (orderPriceType == null)
                            {
                                orderPriceType = new OrderPriceType();
                                orderPriceType.Name = orderPriceTypeStr;
                                mvm.wc.OrderPriceTypes.Add(orderPriceType);
                                mvm.wc.SaveChanges();
                            }
                            order.OrderPriceType = orderPriceType;
                            break;
                        default:
                            break;
                    }
                }

                // Дата публикации
                text = ".//table";
                text += "/tr";
                text += "/td[@class=\"publishingTd\"]";
                text += "/ul[@class=\"publishing\"]";
                text += "/li";

                HtmlAgilityPack.HtmlNodeCollection publishingColl = orderNode.SelectNodes(text);
                if (publishingColl.Count > 1)
                {
                    order.PublishDateTime = Convert.ToDateTime(publishingColl[1].InnerText.Trim());
                }
                else
                {
                    orderMessage = "У заказа не указана дата публикации";
                    return ResultType_enum.Error;
                }
                #endregion

                return ResultType_enum.Done;
            }
            catch (Exception ex)
            {
                orderMessage = ex.Message + '\n' + ex.StackTrace;
                return ResultType_enum.Error;
            }
        }

        private string ConvertNum(string text)
        {
            try
            {
                int numInd = text.IndexOf('№');
                if ((numInd >= 0) && (text.Length > 1))
                {
                    text = text.Substring(text.IndexOf('№') + 1, text.Length - 1).Trim();
                }

                return text;
            }
            catch
            {
                return text;
            }
        }

        public void Operate()
        {
            Thread lotsSearch_thread = new Thread(LotsSearch_proc);
            lotsSearch_thread.Start();
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
            else {
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
