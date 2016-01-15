using PublicOrders.Models;
using PublicOrders.Processors.Internet.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PublicOrders.Processors.Internet
{
    public delegate void AllWinersSearched_delegete(Customer customer, ResultType_enum resultType_enum, string message);
    public delegate void WinnerSearched_delegate(Winner winner);
    public delegate void WinnerSearchProgress_delegate(Customer customer, string text, int intValue);

    public class WinnersSearchProcessor
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

        private AllWinersSearched_delegete allWinersSearched_delegete = null;
        private WinnerSearched_delegate winnerSearched_delegate = null;
        private WinnerSearchProgress_delegate winnerSearchProgress_delegate = null;

        public WinnersSearchProcessor(Customer _customer, CustomerType_enum _customerType_enum,
                               LawType_enum _lawType_enum, Int64 _lowPrice, Int64 _highPrice,
                               DateTime _lowPublishDate, DateTime _highPublishDate,
                               AllWinersSearched_delegete _allWinersSearched_delegete,
                               WinnerSearched_delegate _winnerSearched_delegate,
                               WinnerSearchProgress_delegate _winnerSearchProgress_delegate)
        {

            customer = _customer;
            customerType_enum = _customerType_enum;
            lawType_enum = _lawType_enum;
            lowPrice = _lowPrice;
            highPrice = _highPrice;
            lowPublishDate = _lowPublishDate;
            highPublishDate = _highPublishDate;
            allWinersSearched_delegete = _allWinersSearched_delegete;
            winnerSearched_delegate = _winnerSearched_delegate;
            winnerSearchProgress_delegate = _winnerSearchProgress_delegate;
        }

        private ResultType_enum AnalizeContract(Customer customer, HtmlAgilityPack.HtmlNode contractNode, InternetRequestEngine internetRequestEngine,
                                                out Winner winner, out string contractMessage)
        {
            contractMessage = "";
            HtmlAgilityPack.HtmlNode nodeTmp = null;
            HtmlAgilityPack.HtmlNodeCollection nodesTmp = null;

            try
            {
                #region Определение номера контракта (возможно победитель есть в БД) и ссылки
                string contractNumber = "";
                text = ".//table";
                text += "/tr";
                text += "/td[@class=\"descriptTenderTd\"]";
                text += "/dl";
                text += "/dt";
                text += "/a";

                nodeTmp = contractNode.SelectSingleNode(text);
                if (nodeTmp.InnerText.IndexOf('№') > -1) {
                    contractNumber = nodeTmp.InnerText.Remove(nodeTmp.InnerText.IndexOf('№'), 1).Trim();
                } else {
                    contractNumber = nodeTmp.InnerText.Trim();
                }

                winner = mvm.wc.Winners.ToList().FirstOrDefault(m => (m.ContractNumber == contractNumber));
                if (winner != null) {
                    return ResultType_enum.Done;
                }

                string contractLink = @"http://new.zakupki.gov.ru" + nodeTmp.Attributes["href"].Value.Trim();
                #endregion

                // Переходим по ссылке контракта
                doc = internetRequestEngine.GetHtmlDoc(contractLink);
                string checkMessage = "";
                ResultType_enum resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);
                if (resultTypeCheck != ResultType_enum.Done)
                {
                    winner = null;
                    return ResultType_enum.Error;
                }

                #region П О Б Е Д И Т Е Л Ь
                winner = new Winner();
                winner.ContractNumber = contractNumber;
                #region Название и ИНН
                text = "//div[@class=\"cardWrapper\"]";
                text += "/div[@class=\"wrapper\"]";
                text += "/div[@class=\"mainBox\"]";
                text += "/div[@class=\"contentTabBoxBlock\"]";
                text += "/div[@class=\"noticeTabBox padBtm20\"]";
                text += "/div[@class=\"noticeTabBoxWrapper\"]";
                text += "/div[@class=\"addingTbl col9Tbl\"]";
                text += "/table";
                text += "/tr";

                HtmlAgilityPack.HtmlNodeCollection winnerTrs = doc.DocumentNode.SelectNodes(text);

                string winnerName = winnerTrs.ElementAt(2).SelectSingleNode(".//td").InnerHtml.Trim();
                if (winnerName.IndexOf("<table") > -1)
                {
                    winner.Name = winnerName.Substring(0, winnerName.IndexOf("<table")).Trim();

                    // ИНН победителя
                    nodesTmp = winnerTrs.ElementAt(2).SelectNodes(".//td/table/tr");
                    nodesTmp = nodesTmp.ElementAt(1).SelectNodes(".//td");
                    winner.Vatin = nodesTmp.ElementAt(1).InnerText.Trim();
                }
                else {
                    winner.Name = winnerName;
                }
                #endregion

                #region Адрес
                nodesTmp = winnerTrs.ElementAt(2).SelectNodes(".//td");
                winner.Address = nodesTmp[6].InnerText.Trim();
                #endregion

                #region Телефон, электронная почта
                nodesTmp = winnerTrs.ElementAt(2).SelectNodes(".//td");
                string telEmail = nodesTmp[8].InnerHtml.Trim();
                string[] telEmailMas = null;
                if (telEmail.IndexOf("<br>") > -1) {
                    telEmailMas = telEmail.Split(new string[] { "<br>" }, StringSplitOptions.None);
                    if (telEmailMas[0].Trim() != "")
                        winner.Phone = telEmailMas[0].Trim();
                    if (telEmailMas[1].Trim() != "")
                        winner.Email = telEmailMas[1].Trim();
                }
                else {
                    winner.Phone = telEmail;
                }

                #endregion

                #endregion

                #region Л О Т (сайт контракта)
                Lot lot = new Lot();
                lot.Winner = winner;
                #region Дата заключения контракта
                text = "//div[@class=\"cardWrapper\"]";
                text += "/div[@class=\"wrapper\"]";
                text += "/div[@class=\"mainBox\"]";
                text += "/div[@class=\"contentTabBoxBlock\"]";
                text += "/div[@class=\"noticeTabBox padBtm20\"]";
                text += "/div[@class=\"noticeTabBoxWrapper\"]";

                nodesTmp = doc.DocumentNode.SelectNodes(text);
                HtmlAgilityPack.HtmlNodeCollection lotTrs = nodesTmp.ElementAt(2).SelectNodes(".//table/tr");

                lot.DocumentDateTime = Convert.ToDateTime(lotTrs.ElementAt(0).ChildNodes.ElementAt(3).InnerText.Trim());

                #endregion

                #region Цена контракта
                string priceStr = lotTrs.ElementAt(3).ChildNodes.ElementAt(3).InnerText.Trim().Replace(" ", "").ToLower();
                if (priceStr.IndexOf(',') > -1)
                {
                    lot.DocumentPrice = Convert.ToInt64(priceStr.Substring(0, priceStr.IndexOf(',')));
                }
                else
                {
                    lot.DocumentPrice = Convert.ToInt64(priceStr);
                }
                #endregion

                #region Валюта
                string priceTypeStr = lotTrs.ElementAt(4).ChildNodes.ElementAt(3).InnerText.Trim();
                LotPriceType lotPriceType = mvm.wc.LotPriceTypes.FirstOrDefault(m => m.Name.ToLower().Trim() == priceTypeStr.ToLower());
                if (lotPriceType == null)
                {
                    lotPriceType = new LotPriceType();
                    lotPriceType.Name = priceTypeStr;
                    mvm.wc.LotPriceTypes.Add(lotPriceType);
                    mvm.wc.SaveChanges();
                }
                lot.LotPriceType = lotPriceType;
                #endregion

                #endregion

                // Переходим по ссылке заказа
                text = "//div[@class=\"cardWrapper\"]";
                text += "/div[@class=\"wrapper\"]";
                text += "/div[@class=\"mainBox\"]";
                text += "/div[@class=\"contentTabBoxBlock\"]";
                text += "/div[@class=\"noticeTabBox padBtm20\"]";
                text += "/div[@class=\"noticeTabBoxWrapper\"]";

                nodesTmp = doc.DocumentNode.SelectNodes(text);
                string orderLink = @"http://new.zakupki.gov.ru" + 
                                     nodesTmp.ElementAt(0).SelectNodes(".//table/tr").ElementAt(2).SelectSingleNode(".//td/a").Attributes["href"].Value.Trim();

                doc = internetRequestEngine.GetHtmlDoc(orderLink);
                checkMessage = "";
                resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);
                if (resultTypeCheck != ResultType_enum.Done)
                {
                    winner = null;
                    return ResultType_enum.Error;
                }

                #region Л О Т (сайт заказа)
                #region Название
                text = "//div[@class=\"cardWrapper\"]";
                text += "/div[@class=\"wrapper\"]";
                text += "/div[@class=\"mainBox\"]";
                text += "/div[@class=\"contentTabBoxBlock\"]";
                text += "/div[@class=\"noticeTabBox padBtm20\"]";
                text += "/div[@class=\"noticeTabBoxWrapper\"]";

                HtmlAgilityPack.HtmlNodeCollection templates2Nodes = doc.DocumentNode.SelectNodes(text);
                lot.Name = templates2Nodes.ElementAt(0).SelectSingleNode(".//table/tr/td/span").InnerText.Trim();
                #endregion

                #region Максимальная цена
                priceStr = templates2Nodes.ElementAt(4).SelectNodes(".//table/tr").ElementAt(0).ChildNodes.ElementAt(3).InnerText.Trim().Replace(" ", "").ToLower();
                if (priceStr.IndexOf(',') > -1)
                {
                    lot.LotPrice = Convert.ToInt64(priceStr.Substring(0, priceStr.IndexOf(',')));
                }
                else
                {
                    lot.LotPrice = Convert.ToInt64(priceStr);
                }
                #endregion

                #region Ссылка на заказ
                lot.OrderHref = orderLink;
                #endregion

                #region Дата создания записи
                lot.CreateDateTime = DateTime.Now;
                #endregion


                #endregion

                #region З А К А З
                Order order = new Order();
                lot.Order = order;
                order.Customer = customer;

                #region Дата публикации
                order.PublishDateTime = lot.DocumentDateTime;
                #endregion

                #region Номер закона
                /*if (lot.OrderHref.IndexOf("zk44") > -1)
                {*/
                    order.LawType = mvm.wc.LawTypes.FirstOrDefault(m => m.Name == "44");
                /*}
                else
                {
                    order.LawType = mvm.wc.LawTypes.FirstOrDefault(m => m.Name == "None");
                }*/
                #endregion

                #region Тип заказа
                string orderTypeStr = templates2Nodes.ElementAt(0).SelectNodes(".//table/tr").ElementAt(0).ChildNodes.ElementAt(3).InnerText.Trim();
                OrderType orderType = mvm.wc.OrderTypes.FirstOrDefault(m => m.Name.ToLower() == orderTypeStr.ToLower());
                if (orderType == null)
                {
                    orderType = new OrderType();
                    orderType.Name = orderTypeStr;
                    mvm.wc.OrderTypes.Add(orderType);
                    mvm.wc.SaveChanges();
                }
                order.OrderType = orderType;

                #endregion

                #region Номер
                order.Number = lot.OrderHref.Substring(lot.OrderHref.IndexOf("?regNumber=") + 11, lot.OrderHref.Length - (lot.OrderHref.IndexOf("?regNumber=") + 11));
                #endregion

                #region Дата создания записи
                order.CreateDateTime = DateTime.Now;
                #endregion

                #endregion

                mvm.wc.Lots.Add(lot);
                mvm.wc.SaveChanges();


                return ResultType_enum.Done;
            }
            catch (Exception ex)
            {
                contractMessage = ex.Message + '\n' + ex.StackTrace;
                winner = null;
                return ResultType_enum.Error;
            }
        }

        private void WinnersSearch_proc()
        {
            try
            {
                isWork = true;
                isPause = false;
                #region Запрос в интернет на получение контрактов заказчика
                InternetRequestEngine internetRequestEngine = new InternetRequestEngine();

                string lawTypeStr = "";
                switch (lawType_enum)
                {
                    case (LawType_enum._44_94_223):
                        lawTypeStr = "fz44=on&";
                        break;
                    case (LawType_enum._44_94):
                        lawTypeStr = "fz44=on&";
                        break;
                    case (LawType_enum._223):
                        lawTypeStr = "fz44=on&";
                        break;
                }

                text =  @"http://new.zakupki.gov.ru/epz/contract/extendedsearch/results.html?searchString=&";
                text += @"pageNumber=1&sortDirection=false&recordsPerPage=_500&sortBy=PO_DATE_OBNOVLENIJA&";
                text += lawTypeStr + @"customerInn=" + customer.Vatin + "&customerCode=&customerFz223id=" + Convert.ToString(customer.Law_223_ID) + "&";
                text += @"customerFz94id=" + Convert.ToString(customer.Law_44_94_ID) + "&customerTitle=" + customer.Name + "&";
                text += @"ec=true&priceFrom=" + lowPrice + "&priceTo=" + highPrice + "&contractDateFrom=" + lowPublishDate.ToString("dd.MM.yyyy") + "&";
                text += @"contractDateTo=" + highPublishDate.ToString("dd.MM.yyyy") + "&budgetaryFunds=on&extraBudgetaryFunds=on&openMode=DEFAULT_SAVED_SETTING";


                winnerSearchProgress_delegate(customer, "Поиск контрактов заказчика..", 0);
                doc = internetRequestEngine.GetHtmlDoc(text);
                string checkMessage = "";
                ResultType_enum resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);
                if (resultTypeCheck != ResultType_enum.Done)
                {
                    #region Если нет подключения к интернету, то берем значения из БД
                    winnerSearchProgress_delegate(customer, "Получение заказов из БД..", 0);

                    bool searchedFromDB = false;
                    List<Order> ordersDB = mvm.wc.Orders.Where(m => ((m.Customer.Vatin == customer.Vatin) &&
                                                                   (m.PublishDateTime < highPublishDate) &&
                                                                   (m.PublishDateTime > lowPublishDate))).ToList();

                    if (ordersDB != null)
                    {
                        switch (lawType_enum)
                        {
                            case (LawType_enum._44_94_223):
                                ordersDB = ordersDB.Where(m => ((m.LawType.Name == "44") || (m.LawType.Name == "94"))).ToList();
                                break;
                            case (LawType_enum._44_94):
                                ordersDB = ordersDB.Where(m => ((m.LawType.Name == "44") || (m.LawType.Name == "94"))).ToList();
                                break;
                            case (LawType_enum._223):
                                ordersDB = ordersDB.Where(m => ((m.LawType.Name == "44") || (m.LawType.Name == "94"))).ToList();
                                break;
                        }

                        List<Lot> searchLots = null;
                        foreach (Order searchOrder in ordersDB)
                        {
                            searchLots = searchOrder.Lots.Where(m => ((m.LotPrice > lowPrice) && (m.LotPrice < highPrice))).ToList();
                            foreach (Lot searchLot in searchLots)
                            {
                                if ((searchLot.Winners != null) && (searchLot.Winners.Count() > 0))
                                {
                                    searchedFromDB = true;
                                    winnerSearched_delegate(searchLot.Winners.ElementAt(0));
                                }
                            }
                        }
                    }
                    #endregion

                    if (searchedFromDB == false)
                    {
                        allWinersSearched_delegete(customer, ResultType_enum.ErrorNetwork, "Соединение с сервером отсутствует!\nПобедители в БД не найдены!");
                    }
                    else
                    {
                        allWinersSearched_delegete(customer, ResultType_enum.Done, "Соединение с сервером отсутствует!");
                    }

                    return;
                }


                text = "//div[@class=\"outerWrapper mainPage mainPage\"]";
                text += "/div[@class=\"wrapper\"]";
                text += "/div[@class=\"mainBox\"]";
                text += "/div[@class=\"parametrs margBtm10\"]";
                text += "/div[@class=\"registerBox margBtm20\"]";

                HtmlAgilityPack.HtmlNodeCollection contractCollection = doc.DocumentNode.SelectNodes(text);
                if ((contractCollection == null) || (contractCollection.Count == 0))
                {
                    allWinersSearched_delegete(customer, ResultType_enum.NotSearch, "");
                    return;
                }
                #endregion


                double contractInterval = 100 / Convert.ToDouble(contractCollection.Count());
                int currentInterval = 0;

                int contractNum = 1;
                Winner winner = null;
                foreach (HtmlAgilityPack.HtmlNode nodeContract in contractCollection)
                {
                    currentInterval = Convert.ToInt32(contractNum * contractInterval);
                    winnerSearchProgress_delegate(customer, "Обработка победителя.. [" + contractNum + "\\" + contractCollection.Count() + "]", currentInterval);

                    while (isPause)
                    {
                        Thread.Sleep(300);
                    }
                    if (!isWork) break;

                    #region Анализ контракта с победителем
                    string contractMessage = "";
                    ResultType_enum contractResult = AnalizeContract(customer, nodeContract, internetRequestEngine,
                                                             out winner, out contractMessage);

                    if (contractResult == ResultType_enum.Error) continue;

                    // Проверить на повтор и записать в БД
                    /*Order repeatOrder = mvm.wc.Orders.ToList().FirstOrDefault(m => (m.Number == order.Number));
                    if (repeatOrder == null)
                    {
                        order.CreateDateTime = DateTime.Now;
                        mvm.wc.Orders.Add(order);
                        mvm.wc.SaveChanges();
                    }
                    else
                    {
                        order = repeatOrder;
                    }

                    //orders.Add(order);
                    contractNum++;
                    #endregion

                    #region Поиск победителей по заказу
                    var lots = mvm.wc.Lots.Where(p => p.Order.Number.Trim().ToLower() == order.Number.Trim().ToLower()).ToList();
                    // В БД уже есть лоты на заказ (лот заполняется вместе с его победителем, поэтому выводим эти лоты)
                    if ((lots != null) && (lots.Count() > 0))
                    {
                        //var winner = null;
                        foreach (var lot in lots)
                        {
                            var winner = mvm.wc.Winners.ToList().FirstOrDefault(m => m.Lot == lot);
                            if ((winner != null) && (winner.Name.Trim() != ""))
                                lotSearched_delegate(winner);
                        }
                    }
                    // Начинаем поиск победителей в интернете
                    else
                    {
                        // Ищем победителей, если у заказа прошла неделя с момента поиска победителей
                        if ((order.WinnersSearchDateTime == null) || ((DateTime.Now - order.WinnersSearchDateTime) > TimeSpan.FromDays(7)))
                        {
                            string winnerEngineMessage = "";
                            ResultType_enum resultSearch = winnerSearchEngine.FillWinners(order, internetRequestEngine, lotSearched_delegate, out winnerEngineMessage);

                            if ((resultSearch != ResultType_enum.Error) &&
                                (resultSearch != ResultType_enum.ErrorNetwork) &&
                                (resultSearch != ResultType_enum.ReglamentaWork))
                            {
                                // Сохраняем дату поиска победителей
                                order.WinnersSearchDateTime = DateTime.Now;
                                mvm.wc.Entry(order).State = System.Data.Entity.EntityState.Modified;
                                mvm.wc.SaveChanges();
                            }
                        }
                    }*/
                    #endregion
                }

                allWinersSearched_delegete(customer, ResultType_enum.Done, "");
            }
            catch (Exception ex)
            {
                isWork = false;
                allWinersSearched_delegete(customer, ResultType_enum.Error, ex.Message + '\n' + ex.StackTrace);
                return;
            }
        }

        public void Operate()
        {
            Thread lotsSearch_thread = new Thread(WinnersSearch_proc);
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
