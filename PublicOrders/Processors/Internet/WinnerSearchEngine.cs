using PublicOrders.Models;
using PublicOrders.Processors.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions;

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
                            LotPriceType _44lotPriceType = mvm.wc.LotPriceTypes.FirstOrDefault(m => m.Name.ToLower().Trim() == _44lotPriceTypeName.ToLower());
                            if (_44lotPriceType == null)
                            {
                                _44lotPriceType = new LotPriceType();
                                _44lotPriceType.Name = _44lotPriceTypeName;
                                mvm.wc.LotPriceTypes.Add(_44lotPriceType);
                                mvm.wc.SaveChanges();
                            }
                            _44lot.LotPriceType = _44lotPriceType;
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

                                //mvm.wc.SaveChanges();

                                Winner winner = new Winner();
                                winner.Name = winnerName;
                                winner.Phone = winnerPhone;
                                if (winnerEmail.Trim() != "")
                                    winner.Email = winnerEmail;
                                winner.Lot = _44lot;

                                mvm.wc.Lots.Add(_44lot);
                                mvm.wc.Winners.Add(winner);
                                mvm.wc.SaveChanges();

                                lotSearched_delegate(winner);
                            }
                        }


                        break;
                    #endregion
                    #region 94 Закон
                    case ("94"):
                        // Если в заказе больше одного лота (<order.Price == 0>), то не заполняем (в будущем предусмотреть и такую ситуацию)
                        if ((order.Price == null) || (order.Price == 0)) break;
                        // Параметры лота
                        string _94lotName = order.Name;
                        long _94lotPrice = order.Price;
                        string _94lotPriceTypeName = order.OrderPriceType.Name;
                    
                        string _94lotOrderHref = @"http://zakupki.gov.ru/pgz/public/action/orders/info/common_info/show?source=epz&notificationId=" + order.HrefId;
                        long _94lotDocumentPrice = 0;
                        string _94lotDocumentDateTime = "";

                        doc = internetRequestEngine.GetHtmlDoc(@"http://zakupki.gov.ru/pgz/public/action/orders/info/contract_info/show?source=epz&notificationId=" + order.HrefId);
                        checkMessage = "";
                        resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);
                        if (resultTypeCheck != ResultType_enum.Done)
                        {
                            winnerMessage = checkMessage;
                            return resultTypeCheck;
                        }

                        Regex regex = new Regex("html\\?reestrNumber=(\\d*)");
                        Match match = regex.Match(doc.DocumentNode.OuterHtml);
                        Group gr = match.Groups[1];


                        doc = internetRequestEngine.GetHtmlDoc(@"http://zakupki.gov.ru/epz/contract/contractCard/common-info.html?reestrNumber=" + match.Groups[1].Value);
                        checkMessage = "";
                        resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);
                        if (resultTypeCheck != ResultType_enum.Done)
                        {
                            winnerMessage = checkMessage;
                            return resultTypeCheck;
                        }

                        // Ищем информацию о поставщиках
                        text = "//div[@class=\"cardWrapper\"]";
                        text += "/div[@class=\"wrapper\"]";
                        text += "/div[@class=\"mainBox\"]";
                        text += "/div[@class=\"noticeTabBox\"]";
                        text += "/div[@class=\"noticeTabBoxWrapper\"]";
                        text += "/table[@class=\"cTdCenter\"]";
                        text += "/tbody";
                        text += "/tr";
                        text += "/td";

                        HtmlAgilityPack.HtmlNodeCollection winnerColl = doc.DocumentNode.SelectNodes(text);
                        if ((winnerColl == null) || (winnerColl.Count != 10)) return ResultType_enum.NotSearch;

                        Winner _94winner = new Winner();
                        try {
                            // Название победителя
                            _94winner.Name = winnerColl[1].InnerText.Trim();

                            // Телефон, email
                            string telMailStr = winnerColl[6].InnerText.Trim();
                            if (telMailStr.IndexOf("\n") > -1)
                            {
                                string[] telMailMas = winnerColl[6].InnerText.Trim().Split('\n');
                                _94winner.Phone = telMailMas[0].Trim();
                                _94winner.Email = telMailMas[1].Trim();
                            }
                            else
                            {
                                _94winner.Phone = telMailStr.Trim();
                            }
                        } catch {

                        }

                        if ((_94winner.Name == null) || (_94winner.Name == "")) {
                            return ResultType_enum.NotSearch;
                        }

                        // Дозаполняем лот и загружаем в БД
                        text = "//div[@class=\"cardWrapper\"]";
                        text += "/div[@class=\"wrapper\"]";
                        text += "/div[@class=\"mainBox\"]";
                        //text += "/div[@class=\"contentTabs noticeTabs\"]";
                        text += "/div[@class=\"noticeTabBox\"]";
                        text += "/div[@class=\"noticeTabBoxWrapper\"]";

                        HtmlAgilityPack.HtmlNodeCollection lotInfoColl = doc.DocumentNode.SelectNodes(text);
                        if ((lotInfoColl == null) || (lotInfoColl.Count != 5)) return ResultType_enum.NotSearch;

                        HtmlAgilityPack.HtmlNodeCollection tdColl = lotInfoColl[3].SelectNodes(".//table/tr/td");
                        // Дата заключения контракта
                        _94lotDocumentDateTime = tdColl[1].InnerText.Trim();
                        if (_94lotDocumentDateTime.IndexOf('(') > -1)
                        {
                            _94lotDocumentDateTime = _94lotDocumentDateTime.Substring(0, _94lotDocumentDateTime.IndexOf('(')).Trim();
                        }
                        else
                        {
                            _94lotDocumentDateTime = _94lotDocumentDateTime.Trim();
                        }

                        // Цена контракта
                        string _94lotDocumentPriceStr = tdColl[5].InnerText.Trim();
                        if (_94lotDocumentPriceStr.IndexOf(',') > -1) {
                            _94lotDocumentPriceStr = _94lotDocumentPriceStr.Substring(0, _94lotDocumentPriceStr.IndexOf(','));
                        }

                        _94lotDocumentPrice = Convert.ToInt64(_94lotDocumentPriceStr.Replace(" ", ""));

                        if ((_94lotName == "") || (_94lotPrice == 0) ||
                            (_94lotDocumentPrice == 0) || (_94lotDocumentDateTime == ""))
                        return ResultType_enum.NotSearch;

                        Lot _94lot = new Lot();
                        _94winner.Lot = _94lot;
                        _94lot.Name = _94lotName;
                        _94lot.Price = _94lotPrice;
                        // lotPriceType
                        LotPriceType lotPriceType = mvm.wc.LotPriceTypes.FirstOrDefault(m => m.Name.ToLower().Trim() == _94lotPriceTypeName.ToLower());
                        if (lotPriceType == null)
                        {
                            lotPriceType = new LotPriceType();
                            lotPriceType.Name = _94lotPriceTypeName;
                            mvm.wc.LotPriceTypes.Add(lotPriceType);
                            mvm.wc.SaveChanges();
                        }
                        _94lot.LotPriceType = lotPriceType;
                        _94lot.Order = order;
                        _94lot.OrderHref = _94lotOrderHref;
                        _94lot.CreateDateTime = DateTime.Now;
                        _94lot.DocumentDateTime = Convert.ToDateTime(_94lotDocumentDateTime);
                        _94lot.DocumentPrice = _94lotDocumentPrice;

                        mvm.wc.Lots.Add(_94lot);
                        mvm.wc.Winners.Add(_94winner);
                        mvm.wc.SaveChanges();


                        isWinner = true;
                        lotSearched_delegate(_94winner);
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
