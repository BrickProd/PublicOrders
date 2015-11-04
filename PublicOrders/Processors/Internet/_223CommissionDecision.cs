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
    class _223CommissionDecision
    {
        MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        private string text = "";
        private HtmlAgilityPack.HtmlDocument doc;

        public ResultType_enum Work(Order order, InternetRequestEngine internetRequestEngine,
                               HtmlAgilityPack.HtmlNode winnerProtocolNode, LotSearched_delegate lotSearched_delegate,
                               out string commissionDecisionMessage, out bool isWinner)
        {
            try
            {
                isWinner = false;
                commissionDecisionMessage = "";

                List<Lot> lotsTemp = new List<Lot>();
                List<Winner> winnersTemp = new List<Winner>();

                // Получаем ID протокола
                string protocolHref = winnerProtocolNode.Attributes["onclick"].Value;
                string protocolID = protocolHref.Substring(protocolHref.IndexOf("protocolInfoId=") + 15,
                    protocolHref.IndexOf("&purchaseId=") - (protocolHref.IndexOf("protocolInfoId=") + 15));

                doc = internetRequestEngine.GetHtmlDoc(@"http://zakupki.gov.ru/223/purchase/public/purchase/protocol/ip/application/list.html?noticeInfoId=&protocolInfoId=3299829&mode=view");


                text = @"http://zakupki.gov.ru/223/purchase/public/purchase/protocol/ip/application/comission-decision.html?noticeInfoId=&protocolInfoId=" + protocolID + "&mode=view";
                //text = @"http://zakupki.gov.ru/223/purchase/public/purchase/protocol/ip/application/comission-decision.html?noticeInfoId=716491&protocolInfoId=692914&mode=view";
                doc = internetRequestEngine.GetHtmlDoc(text);
                string checkMessage = "";
                ResultType_enum resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);
                if (resultTypeCheck != ResultType_enum.Done)
                {
                    commissionDecisionMessage = checkMessage;
                    return resultTypeCheck;
                }

                // Поиск лотов
                text = "//div[@class=\"cardWrapper\"]";
                text += "/div[@class=\"wrapper\"]";
                text += "/div[@class=\"mainbox\"]";
                text += "/div[@class=\"contentTabBoxBlock\"]";
                text += "/div[@class=\"noticeTabBox\"]";
                text += "/div[@class=\"noticeTabBoxWrapper\"]";
                text += "/div[@class=\"blue-border rootOfLot\"]";

                HtmlAgilityPack.HtmlNodeCollection lotsColl = doc.DocumentNode.SelectNodes(text);
                if (lotsColl == null) return ResultType_enum.NotSearch;

                string _223lotName = "";
                long _223lotPrice = 0;
                string _223lotPriceTypeName = "";
                long _223lotDocumentPrice = 0;
                string _223lotDocumentDate = "";

                Lot _223lot = null;
                Winner _223winner = null;
                int num = 1;

                // Обработка лотов
                int lotNum = 0;
                foreach (HtmlAgilityPack.HtmlNode lotNode in lotsColl)
                {
                    // Заполняем цену и валюту лота
                    _223lotName = "";
                    _223lotPrice = 0;
                    _223lotPriceTypeName = "";
                    _223lotDocumentPrice = 0;
                    _223lotDocumentDate = "";

                    _223lot = new Lot();
                    _223winner = new Winner();
                    _223winner.Lot = _223lot;

                    _223lot.Order = order;
                    _223lot.CreateDateTime = DateTime.Now;
                    _223lot.OrderHref = @"http://zakupki.gov.ru/223/purchase/public/purchase/info/common-info.html?noticeId=" + order.HrefId + "&epz=true";
                    lotsTemp.Add(_223lot);

                    // Получаем цену и валюту лота
                    text = ".//table";
                    text += "/tr[@class=\"mainExpand\"]";
                    text += "/td";
                    text += "/table[@class=\"outertable\"]";
                    text += "/tr";
                    HtmlAgilityPack.HtmlNodeCollection lotParamsColl = lotNode.SelectNodes(text);

                    if (lotParamsColl != null)
                    {
                        HtmlAgilityPack.HtmlNode lotParamPriceNode = lotParamsColl[1];
                        num = 1;
                        foreach (HtmlAgilityPack.HtmlNode tdNode in lotParamPriceNode.ChildNodes)
                        {
                            if (tdNode.Name != "td") continue;
                            if (num == 2)
                            {
                                string priceTypePriceLot = tdNode.InnerText.Trim();
                                if (priceTypePriceLot.IndexOf(' ') > -1)
                                {
                                    string _223lotPriceStr = Globals.DecodeInternetSymbs(priceTypePriceLot.Substring(0, priceTypePriceLot.IndexOf(' ')).Trim());
                                    if (_223lotPriceStr.IndexOf(',') > -1)
                                    {
                                        _223lotPrice = Convert.ToInt64(_223lotPriceStr.Substring(0, _223lotPriceStr.IndexOf(',')).Replace(" ", ""));
                                    }
                                    else
                                    {
                                        _223lotPrice = Convert.ToInt64(_223lotPriceStr.Replace(" ", ""));
                                    }
                                    //_223lotPrice = Convert.ToInt64(Globals.DecodeInternetSymbs(priceTypePriceLot.Substring(0, priceTypePriceLot.IndexOf(' ')).Trim()));
                                    _223lotPriceTypeName = Globals.DecodeInternetSymbs(priceTypePriceLot.Substring(priceTypePriceLot.IndexOf(' '), priceTypePriceLot.Length - priceTypePriceLot.IndexOf(' ')).Trim());
                                }

                                break;
                            }

                            num++;
                        }
                    }
                    else
                    {
                        text = ".//table[@class=\"outertable\"]";
                        text += "/tr";
                        lotParamsColl = lotNode.SelectNodes(text);


                        if (lotParamsColl != null)
                        {
                            int numTr = 1;
                            foreach (HtmlAgilityPack.HtmlNode trNode in lotParamsColl)
                            {
                                if (trNode.Name != "tr") continue;

                                int numTdParam = 1;
                                switch (numTr)
                                {
                                    case (1):
                                        numTdParam = 1;
                                        foreach (HtmlAgilityPack.HtmlNode tdNode in trNode.ChildNodes)
                                        {
                                            if (tdNode.Name != "td") continue;
                                            if (numTdParam == 1)
                                            {
                                                _223lotName = Globals.CutLotName(tdNode.InnerText.Trim());
                                                break;
                                            }
                                            numTdParam++;
                                        }
                                        break;
                                    case (2):
                                        numTdParam = 1;
                                        foreach (HtmlAgilityPack.HtmlNode tdNode in trNode.ChildNodes)
                                        {
                                            if (tdNode.Name != "td") continue;
                                            if (numTdParam == 2)
                                            {
                                                string priceTypePriceLot = tdNode.InnerText.Trim();
                                                if (priceTypePriceLot.IndexOf(' ') > -1)
                                                {
                                                    string _223lotPriceStr = Globals.DecodeInternetSymbs(priceTypePriceLot.Substring(0, priceTypePriceLot.IndexOf(' ')).Trim());
                                                    if (_223lotPriceStr.IndexOf(',') > -1)
                                                    {
                                                        _223lotPrice = Convert.ToInt64(_223lotPriceStr.Substring(0, _223lotPriceStr.IndexOf(',')));
                                                    }
                                                    else
                                                    {
                                                        _223lotPrice = Convert.ToInt64(_223lotPriceStr);
                                                    }
                                                    //_223lotPrice = Convert.ToInt64(Globals.DecodeInternetSymbs(priceTypePriceLot.Substring(0, priceTypePriceLot.IndexOf(' ')).Trim()));
                                                    _223lotPriceTypeName = Globals.DecodeInternetSymbs(priceTypePriceLot.Substring(priceTypePriceLot.IndexOf(' '), priceTypePriceLot.Length - priceTypePriceLot.IndexOf(' ')).Trim());
                                                    break;
                                                }
                                            }
                                            numTdParam++;
                                        }

                                        break;
                                    default:
                                        break;
                                }

                                numTr++;
                            }
                        }
                        else
                        {
                            return ResultType_enum.NotSearch;
                        }
                    }

                    if (_223lotName != "")
                    {
                        _223lot.Name = _223lotName;
                        if (_223lot.Name.Length > 400)
                        {
                            _223lot.Name = _223lot.Name.Substring(0, 398) + "..";
                        }
                    }

                    if ((_223lotPrice != 0) && (_223lotPriceTypeName != ""))
                    {
                        _223lot.LotPrice = _223lotPrice;
                        LotPriceType lotPriceType = mvm.wc.LotPriceTypes.FirstOrDefault(m => m.Name.ToLower().Trim() == _223lotPriceTypeName.ToLower());
                        if (lotPriceType == null)
                        {
                            lotPriceType = new LotPriceType();
                            lotPriceType.Name = _223lotPriceTypeName;
                            mvm.wc.LotPriceTypes.Add(lotPriceType);
                            mvm.wc.SaveChanges();
                        }
                        _223lot.LotPriceType = lotPriceType;
                    }
                    else
                    {
                        // Не указана цена, берем следующий лот
                        continue;
                    }

                    // Получаем список участников
                    text = ".//table";
                    text += "/tr[@class=\"expandable-row\"]";
                    text += "/td";
                    text += "/div[@class=\"clientDemandWrapper\"]";
                    text += "/table[@class=\"outertable clientDemandTbl\"]";
                    text += "/tr";

                    HtmlAgilityPack.HtmlNodeCollection partnerWithHeaderColl = lotNode.SelectNodes(text);

                    // Поиск победителя среди участников
                    int tdnum = 1;
                    isWinner = false;
                    foreach (HtmlAgilityPack.HtmlNode partnerNode in partnerWithHeaderColl)
                    {
                        if (partnerNode.Attributes.Count > 0) continue;

                        tdnum = 1;
                        string nameTmp = "";
                        foreach (HtmlAgilityPack.HtmlNode tdNode in partnerNode.ChildNodes)
                        {
                            if (tdNode.Name != "td") continue;
                            if (tdnum == 2)
                            {
                                nameTmp = Globals.DecodeInternetSymbs(tdNode.InnerText.Trim());
                                nameTmp = Globals.CleanSpaces(nameTmp.Replace('\n', ' ').Replace('\t', ' '));
                            }

                            if (tdnum == 4)
                            {
                                if (tdNode.InnerText.Trim().ToLower() == "победитель")
                                {
                                    _223winner.Name = nameTmp;
                                    winnersTemp.Add(_223winner);
                                    isWinner = true;
                                }
                                break;
                            }
                            tdnum++;
                        }
                        if (isWinner) break;
                    }
                    lotNum++;
                }

                // Если лоты уже с заполненным именем, то не ищем имена
                bool allHasName = true;
                foreach (Lot lot in lotsTemp)
                {
                    if ((lot.Name == null) || (lot.Name == ""))
                    {
                        allHasName = false;
                        break;
                    }
                }

                bool hasWinners = false;
                foreach (Winner winner in winnersTemp)
                {
                    if ((winner.Name != null) && (winner.Name != "")) {
                        hasWinners = true;
                        break;
                    }
                }

                // Заполняем имена лотов
                if ((!allHasName) && (hasWinners))
                {
                    if (lotsTemp.Count > 0)
                    {
                        text = @"http://zakupki.gov.ru/223/purchase/public/purchase/protocol/ip/application/list.html?noticeInfoId=&protocolInfoId=" + protocolID + @"&mode=view";
                        //text = @"http://zakupki.gov.ru/223/purchase/public/purchase/protocol/rkz/application/list.html?noticeInfoId=&protocolInfoId=1698215&mode=view";
                        doc = internetRequestEngine.GetHtmlDoc(text);
                        if (doc.DocumentNode.InnerText.IndexOf("Запрашиваемая страница временно недоступна") > -1)
                        {
                            //text = @"http://zakupki.gov.ru/223/purchase/public/purchase/protocol/ip/application/list.html?noticeInfoId=&protocolInfoId=" + protocolID + @"&mode=view";
                            text = @"http://zakupki.gov.ru/223/purchase/public/purchase/protocol/rkz/application/list.html?noticeInfoId=&protocolInfoId=1698215&mode=view";
                            doc = internetRequestEngine.GetHtmlDoc(text);
                        }

                        resultTypeCheck = Globals.CheckDocResult(doc, out checkMessage);
                        if (resultTypeCheck != ResultType_enum.Done)
                        {
                            commissionDecisionMessage = checkMessage;
                            return resultTypeCheck;
                        }

                        // Поиск имен лотов
                        text = "//div[@class=\"cardWrapper\"]";
                        text += "/div[@class=\"wrapper\"]";
                        text += "/div[@class=\"mainbox\"]";
                        text += "/div[@class=\"contentTabBoxBlock\"]";
                        text += "/div[@class=\"noticeTabBox\"]";
                        text += "/div[@class=\"clientDemandWrapper\"]";
                        text += "/table";
                        text += "/tr[@class=\"mainExpand\"]";
                        text += "/td[@class=\"descriptTenderTd\"]";
                        text += "/dt";

                        HtmlAgilityPack.HtmlNodeCollection lotNamesColl = doc.DocumentNode.SelectNodes(text);
                        if (lotNamesColl == null)
                        {
                            // Поиск имен лотов
                            text = "//div[@class=\"cardWrapper\"]";
                            text += "/div[@class=\"wrapper\"]";
                            text += "/div[@class=\"mainbox\"]";
                            text += "/div[@class=\"contentTabBox\"]";
                            text += "/div[@class=\"contentTabBoxBlock\"]";
                            text += "/div[@class=\"noticeTabBox\"]";
                            text += "/div[@class=\"noticeTabBoxWrapper\"]";
                            text += "/div[@class=\"clientDemandWrapper\"]";
                            text += "/table";
                            text += "/tr";
                            text += "/td";
                            text += "/table";
                            text += "/tr[@class=\"mainExpand\"]";
                            text += "/td[@class=\"descriptTenderTd\"]";

                            lotNamesColl = doc.DocumentNode.SelectNodes(text);
                            if (lotNamesColl == null) return ResultType_enum.NotSearch;
                        }

                        // Перебираем имена лотов
                        int lotNameNum = 0;
                        foreach (HtmlAgilityPack.HtmlNode lotNameNode in lotNamesColl)
                        {
                            try
                            {
                                lotsTemp[lotNameNum].Name = Globals.DecodeInternetSymbs(lotNameNode.InnerText.Trim());
                            }
                            catch
                            {
                                continue;
                            }
                            lotNameNum++;
                        }

                        // Поиск цены контракта и даты
                        // Перебираем лоты
                        text = "//div[@class=\"cardWrapper\"]";
                        text += "/div[@class=\"wrapper\"]";
                        text += "/div[@class=\"mainbox\"]";
                        text += "/div[@class=\"contentTabBoxBlock\"]";
                        text += "/div[@class=\"noticeTabBox\"]";
                        text += "/div[@class=\"clientDemandWrapper\"]";
                        text += "/table";
                        text += "/tr[@class=\"expandRow\"]";

                        HtmlAgilityPack.HtmlNodeCollection lotsPriceDateColl = doc.DocumentNode.SelectNodes(text);

                        if ((lotsPriceDateColl != null) && (lotsPriceDateColl.Count > 0))
                        {
                            int lotPriceDateNum = 0;
                            foreach (HtmlAgilityPack.HtmlNode lotPriceDateNode in lotsPriceDateColl)
                            {
                                text = ".//td";
                                text += "/div";
                                text += "/table";
                                text += "/tr";

                                HtmlAgilityPack.HtmlNodeCollection customersColl = lotPriceDateNode.SelectNodes(text);
                                if (customersColl != null)
                                {
                                    // Перебираем участников и ищем победителя
                                    Winner mainWinner = null;
                                    foreach (HtmlAgilityPack.HtmlNode customerNode in customersColl)
                                    {
                                        if (customerNode.Attributes.Count > 0) continue;

                                        HtmlAgilityPack.HtmlNodeCollection customerAttributesColl = customerNode.SelectNodes(".//td");
                                        int attrNum = 1;
                                        foreach (HtmlAgilityPack.HtmlNode customerAttributeNode in customerAttributesColl)
                                        {
                                            if (customerAttributeNode.Name != "td") continue;
                                            switch (attrNum)
                                            {
                                                case (2):
                                                    string nameTmp = Globals.DecodeInternetSymbs(customerAttributeNode.InnerText.Trim());
                                                    if (nameTmp.IndexOf("Просмотреть заявку") > -1)
                                                    {
                                                        nameTmp = nameTmp.Substring(0, nameTmp.IndexOf("Просмотреть заявку")).Trim();
                                                    }

                                                    foreach (Winner winner in winnersTemp) {
                                                        if (nameTmp.Trim().ToLower() == winner.Name.Trim().ToLower()) {
                                                            mainWinner = winner;
                                                            break;
                                                        }
                                                    }

                                                    break;
                                                case (3):
                                                    if (customerAttributeNode.InnerText.Trim() != "")
                                                    {
                                                        _223lotDocumentDate = customerAttributeNode.InnerText.Trim();
                                                        break;
                                                    }
                                                    break;

                                                case (4):
                                                    if (customerAttributeNode.InnerText.Trim() != "")
                                                    {
                                                        // Цена
                                                        string priceTypePriceLot = Globals.DecodeInternetSymbs(customerAttributeNode.InnerText.Trim());

                                                        string[] priceTypePriceLotMas = priceTypePriceLot.Split(' ');
                                                        if (priceTypePriceLotMas[0].IndexOf('.') > -1) {
                                                            string str = priceTypePriceLotMas[0].Substring(0, priceTypePriceLotMas[0].IndexOf('.'));

                                                            _223lotDocumentPrice = Convert.ToInt64(str);
                                                        } else {
                                                            _223lotDocumentPrice = Convert.ToInt64(priceTypePriceLotMas[0]);
                                                        }

                                                        // Валюта
                                                        _223lotPriceTypeName = priceTypePriceLotMas[1].Trim();
                                                        /*string lotPriceTypeStr = priceTypePriceLotMas[1].Trim();

                                                        LotPriceType lotPriceType = mvm.wc.LotPriceTypes.FirstOrDefault(m => m.Name.ToLower().Trim() == lotPriceTypeStr.ToLower());
                                                        if (lotPriceType == null)
                                                        {
                                                            lotPriceType = new LotPriceType();
                                                            lotPriceType.Name = lotPriceTypeStr;
                                                            mvm.wc.LotPriceTypes.Add(lotPriceType);
                                                            mvm.wc.SaveChanges();
                                                        }
                                                        order.OrderPriceType = lotPriceType;*/
                                                    }
                                                    break;
                                            }

                                            attrNum++;
                                        }
                                        if (mainWinner != null)
                                        {
                                            mainWinner.Lot.Name = lotsTemp[lotPriceDateNum].Name;

                                            // Цена
                                            mainWinner.Lot.DocumentPrice = _223lotDocumentPrice;

                                            // Валюта
                                            LotPriceType lotPriceType = mvm.wc.LotPriceTypes.FirstOrDefault(m => m.Name.ToLower().Trim() == _223lotPriceTypeName.ToLower());
                                            if (lotPriceType == null)
                                            {
                                                lotPriceType = new LotPriceType();
                                                lotPriceType.Name = _223lotPriceTypeName;
                                                mvm.wc.LotPriceTypes.Add(lotPriceType);
                                                mvm.wc.SaveChanges();
                                            }
                                            mainWinner.Lot.LotPriceType = lotPriceType; 

                                            if (_223lotDocumentDate.IndexOf('(') > -1)
                                            {
                                                mainWinner.Lot.DocumentDateTime = Convert.ToDateTime(_223lotDocumentDate.Substring(0, _223lotDocumentDate.IndexOf('(')).Trim());
                                            }
                                            else
                                            {
                                                mainWinner.Lot.DocumentDateTime = Convert.ToDateTime(_223lotDocumentDate.Trim());
                                            }

                                            // Сохраняем победителя
                                            mvm.wc.Lots.Add(mainWinner.Lot);
                                            mvm.wc.SaveChanges();

                                            mvm.wc.Winners.Add(mainWinner);
                                            mvm.wc.SaveChanges();

                                            lotSearched_delegate(mainWinner);

                                            break;
                                        }
                                        else
                                        {
                                            _223lotDocumentPrice = 0;
                                            _223lotDocumentDate = "";
                                        }
                                    }
                                }

                                lotPriceDateNum++;
                            }
                        }
                    }
                }
                else
                {
                    if (hasWinners)
                    {
                        foreach (Winner winnerTmp in winnersTemp)
                        {
                            if ((winnerTmp.Name != null) && (winnerTmp.Name != "") && 
                                (winnerTmp.Lot.Name != null) && (winnerTmp.Lot.Name != ""))
                            {
                                mvm.wc.Lots.Add(winnerTmp.Lot);
                                mvm.wc.SaveChanges();

                                mvm.wc.Winners.Add(winnerTmp);
                                mvm.wc.SaveChanges();

                                lotSearched_delegate(winnerTmp);
                            }
                        }
                    }
                }

                return ResultType_enum.Done;
            }
            catch (Exception ex)
            {
                isWinner = false;
                commissionDecisionMessage = ex.Message + '\n' + ex.StackTrace;
                return ResultType_enum.Error;
            }
        }
    }
}
