using PublicOrders.Models;
using PublicOrders.Processors.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PublicOrders.Processors.Internet
{
    class WinnerSearchEngine
    {
        MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;
        // Поиск победителей на 223 закон
        _223CommissionDecision _223commissionDecision = null;
        _223WinnerChoice _223winnerChoice = null;

        // переменные
        string text = "";
        private HtmlAgilityPack.HtmlDocument doc;
        private bool isWinner = false;

        public WinnerSearchEngine()
        {
            _223commissionDecision = new _223CommissionDecision();
            _223winnerChoice = new _223WinnerChoice();
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

        public ResultType_enum FillWinners(Order order, InternetRequestEngine internetRequestEngine, 
                                           LotSearched_delegate lotSearched_delegate, out string winnerMessage)
        {
            isWinner = false;
            winnerMessage = "";
            HtmlAgilityPack.HtmlNode nodeTmp = null;
            try
            {
                switch (order.LawType.Name.Trim().ToLower())
                {
                    #region 44 Закон
                    case ("44"):
                        // Параметры лота
                        string _44lotName = order.Name;
                        long _44lotPrice = order.Price;
                        string _44lotPriceTypeName = order.OrderPriceType.Name;
                        string _44lotOrderHref = @"http://zakupki.gov.ru/epz/order/notice/zp44/view/common-info.html?regNumber=" + order.HrefId;
                        long _44lotDocumentPrice = 0;
                        string _44lotDocumentDateTime = "";

                        // Параметры победителя
                        string winnerName = "";
                        string winnerPhone = "";
                        string winnerEmail = "";

                        // Закупка
                        string numRegistr = "";

                        doc = internetRequestEngine.GetHtmlDoc(@"http://zakupki.gov.ru/epz/order/notice/ea44/view/supplier-results.html?regNumber=" + order.HrefId);
                        string checkMessage = "";
                        ResultType_enum resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);
                        if (resultTypeCheck != ResultType_enum.Done)
                        {
                            winnerMessage = checkMessage;
                            return resultTypeCheck;
                        }

                        text = "//div[@class=\"cardWrapper\"]";
                        text += "/div[@class=\"wrapper\"]";
                        text += "/div[@class=\"mainBox\"]";
                        text += "/div[@class=\"noticeTabBox\"]";
                        text += "/div[@class=\"noticeTabBoxWrapper\"]";
                        text += "/table";
                        text += "/tr";
                        text += "/td";
                        text += "/table[@class=\"noticeCardTableInBlock contractTable\"]";
                        text += "/tr";
                        text += "/td";
                        text += "/table[@class=\"contractTable displaytagTable\"]";
                        text += "/tbody";
                        text += "/tr";
                        text += "/td";

                        HtmlAgilityPack.HtmlNodeCollection atrCollection = doc.DocumentNode.SelectNodes(text);
                        // Возможно : Сведения о контракте из реестра контрактов отсутствуют
                        if ((atrCollection == null)/* &&
                            (doc.DocumentNode.InnerText.IndexOf("Сведения о контракте из реестра контрактов отсутствуют") > 0)*/)
                        {
                            return ResultType_enum.NotSearch;
                        }

                        int num = 1;
                        foreach (HtmlAgilityPack.HtmlNode atrNode in atrCollection)
                        {
                            switch (num)
                            {
                                case (1):
                                    numRegistr = ConvertNum(atrNode.InnerText.Trim());
                                    break;
                                case (4):
                                    if (atrNode.InnerText.Trim() != "")
                                    {
                                        string lotPriceStr = atrNode.InnerText.Trim().Replace(" ", "").ToLower();
                                        try
                                        {

                                            if (lotPriceStr.IndexOf(',') > -1)
                                            {
                                                _44lotDocumentPrice = Convert.ToInt64(lotPriceStr.Substring(0, lotPriceStr.IndexOf(',')));
                                            }
                                            else
                                            {
                                                if (lotPriceStr.IndexOf(' ') > -1)
                                                {
                                                    _44lotDocumentPrice = Convert.ToInt64(lotPriceStr.Substring(0, lotPriceStr.IndexOf(' ')));
                                                }
                                                else
                                                {
                                                    _44lotDocumentPrice = Convert.ToInt64(lotPriceStr);
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            string sss = "ывавы";
                                        }
                                    }
                                    break;
                                case (5):
                                    if (atrNode.InnerText.Trim() != "")
                                    {
                                        _44lotDocumentDateTime = atrNode.InnerText.Trim();
                                        break;
                                    }
                                    break;
                                default:
                                    break;
                            }

                            num++;
                        }

                        // Номер реестровой записи
                        text = @"http://zakupki.gov.ru/epz/contract/contractCard/common-info.html?reestrNumber=" + numRegistr;
                        doc = internetRequestEngine.GetHtmlDoc(text);
                        resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);
                        if (resultTypeCheck != ResultType_enum.Done)
                        {
                            winnerMessage = checkMessage;
                            return resultTypeCheck;
                        }

                        text = "//div[@class=\"cardWrapper\"]";
                        text += "/div[@class=\"wrapper\"]";
                        text += "/div[@class=\"mainBox\"]";
                        text += "/div[@class=\"noticeTabBox\"]";
                        text += "/div[@class=\"noticeTabBoxWrapper\"]";
                        text += "/table[@class=\"cTdCenter\"]";
                        text += "/tbody";
                        text += "/tr";

                        HtmlAgilityPack.HtmlNodeCollection trCollection = doc.DocumentNode.SelectNodes(text);
                        if (trCollection == null)
                        {
                            return ResultType_enum.NotSearch;
                        }

                        num = 1;
                        int numTd = 1;

                        foreach (HtmlAgilityPack.HtmlNode trNode in trCollection)
                        {
                            if (num == 2)
                            {
                                foreach (HtmlAgilityPack.HtmlNode tdNode in trNode.ChildNodes)
                                {
                                    if (tdNode.Name == "td")
                                    {
                                        switch (numTd)
                                        {
                                            case (1):
                                                // Наименование организации
                                                winnerName = Globals.DecodeInternetSymbs(tdNode.InnerHtml.Trim().Replace("<br>", " "));
                                                isWinner = true;
                                                break;
                                            case (6):
                                                // Телефон, электронная почта
                                                string emailPhone = tdNode.InnerText.Trim();
                                                if (emailPhone.IndexOf('\n') > -1)
                                                {
                                                    winnerPhone = Globals.DecodeInternetSymbs(emailPhone.Substring(0, emailPhone.IndexOf('\n')).Trim());
                                                    winnerEmail = Globals.DecodeInternetSymbs(emailPhone.Substring(emailPhone.IndexOf('\n'), emailPhone.Length - emailPhone.IndexOf('\n')).Trim());
                                                }
                                                else
                                                {
                                                    winnerPhone = emailPhone;
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                        numTd++;
                                    }
                                }
                            }
                            num++;
                        }
                        if ((_44lotName != "") && (_44lotPrice != 0) && (_44lotPriceTypeName != ""))
                        {
                            Lot _44lot = new Lot();
                            _44lot.Order = order;
                            _44lot.Name = _44lotName;
                            if (_44lot.Name.Length > 400)
                            {
                                _44lot.Name = _44lot.Name.Substring(0, 398) + "..";
                            }

                            _44lot.Price = _44lotPrice;
                            // LotPriceType
                            LotPriceType lotPriceType = mvm.wc.LotPriceTypes.FirstOrDefault(m => m.Name.ToLower().Trim() == _44lotPriceTypeName.ToLower());
                            if (lotPriceType == null)
                            {
                                lotPriceType = new LotPriceType();
                                lotPriceType.Name = _44lotPriceTypeName;
                                mvm.wc.LotPriceTypes.Add(lotPriceType);
                                mvm.wc.SaveChanges();
                            }
                            _44lot.LotPriceType = lotPriceType;
                            _44lot.OrderHref = _44lotOrderHref;
                            _44lot.DocumentPrice = _44lotDocumentPrice;
                            _44lot.CreateDateTime = DateTime.Now;
                            try
                            {
                                if (_44lotDocumentDateTime.IndexOf('(') > -1)
                                {
                                    _44lot.DocumentDateTime = Convert.ToDateTime(_44lotDocumentDateTime.Substring(0, _44lotDocumentDateTime.IndexOf('(')).Trim());
                                }
                                else {
                                    _44lot.DocumentDateTime = Convert.ToDateTime(_44lotDocumentDateTime.Trim());
                                }

                            }
                            catch {
                                string sss = "аыв";
                            }


                            if (winnerName != "")
                            {
                                mvm.wc.Lots.Add(_44lot);
                                mvm.wc.SaveChanges();

                                Winner winner = new Winner();
                                winner.Name = winnerName;
                                winner.Phone = winnerPhone;
                                winner.Email = winnerEmail;
                                winner.Lot = _44lot;

                                mvm.wc.Winners.Add(winner);
                                mvm.wc.SaveChanges();

                                lotSearched_delegate(winner);
                            }
                        }


                        break;
                    #endregion
                    #region 94 Закон
                    case ("94"):
                        doc = internetRequestEngine.GetHtmlDoc(@"http://zakupki.gov.ru/pgz/public/action/orders/info/contract_info/show?source=epz&notificationId=" + order.HrefId;
                        resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);
                        if (resultTypeCheck != ResultType_enum.Done)
                        {
                            winnerMessage = checkMessage;
                            return resultTypeCheck;
                        }


                        break;
                    #endregion

                    #region 223 Закон
                    case ("223"):
                        doc = internetRequestEngine.GetHtmlDoc(@"http://zakupki.gov.ru/223/purchase/public/purchase/info/protocols.html?noticeId=" + order.HrefId + @"&amp;epz=true");
                        resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);
                        if (resultTypeCheck != ResultType_enum.Done)
                        {
                            winnerMessage = checkMessage;
                            return resultTypeCheck;
                        }

                        // Поиск протоколов
                        text = "//div[@class=\"cardWrapper\"]";
                        text += "/div[@class=\"wrapper\"]";
                        text += "/div[@class=\"mainbox\"]";
                        text += "/div[@class=\"contentTabBox\"]";
                        text += "/div[@class=\"contentTabBoxBlock\"]";
                        text += "/div[@class=\"noticeTabBox \"]";
                        text += "/div[@class=\"noticeTabBoxWrapper\"]";
                        text += "/table";
                        text += "/tbody";
                        text += "/tr";

                        HtmlAgilityPack.HtmlNodeCollection trColl = doc.DocumentNode.SelectNodes(text);
                        // Страница не найдена
                        if (trColl == null) return ResultType_enum.NotSearch;

                        // Параметры победителя
                        winnerName = "";
                        winnerPhone = "";
                        winnerEmail = "";
                        // Поиск протоколов с победителем
                        foreach (HtmlAgilityPack.HtmlNode protocolNode in trColl)
                        {
                            text = ".//td[@class=\"descriptTenderTd\"]";
                            text += "/dl";
                            text += "/dt";
                            text += "/a";

                            HtmlAgilityPack.HtmlNode winnerProtocolNode = protocolNode.SelectSingleNode(text);
                            if (winnerProtocolNode == null) continue;

                            #region Решение комиссии
                            if ((winnerProtocolNode.InnerText.Trim().ToLower().IndexOf("протокол подведения итогов") > -1) ||
                                (winnerProtocolNode.InnerText.Trim().ToLower().IndexOf("подведению итогов") > -1) ||
                                (winnerProtocolNode.InnerText.Trim().ToLower().IndexOf("оценки и сопоставления заявок") > -1) ||
                                (winnerProtocolNode.InnerText.Trim().ToLower().IndexOf("победител") > -1) ||
                                (winnerProtocolNode.InnerText.Trim().ToLower().IndexOf("протокол основного этапа закупки") > -1) ||
                                (winnerProtocolNode.InnerText.Trim().ToLower().IndexOf("запрос предложений о цене договора") > -1))
                            {
                                string commissionDecisionMessage = "";
                                _223commissionDecision.Work(order, internetRequestEngine, winnerProtocolNode, lotSearched_delegate, out commissionDecisionMessage, out isWinner);

                            }
                            #endregion
                            #region Выбор победителя
                            if (winnerProtocolNode.InnerText.Trim().ToLower().IndexOf("протокол запроса котировок") > -1)
                            {
                                string winnerChoiceMessage = "";
                                // Доделать
                                //_223winnerChoice.Work(order, internetRequestEngine, winnerProtocolNode, lotSearched_delegate, out winnerChoiceMessage, out isWinner);
                            }
                            #endregion
                        }

                        break;

                    #endregion
                    default:
                        return ResultType_enum.Done; // !!! Необходимо добавить 94 закон
                }


                if (!isWinner) return ResultType_enum.NotSearch; else return ResultType_enum.Done;
            }
            catch (Exception ex)
            {
                winnerMessage = ex.Message + '\n' + ex.StackTrace;
                return ResultType_enum.Error;
            }
        }
    }
}
