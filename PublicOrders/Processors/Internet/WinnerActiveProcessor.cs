using PublicOrders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace PublicOrders.Processors.Internet
{
    public delegate void WinnerDatesSearched_delegete(List<DateTime> winDates, ResultType_enum resultType_enum, string message);

    class WinnerActiveProcessor
    {
        private string text = "";
        private string winnerName = "";

        WinnerDatesSearched_delegete winnerDatesSearched_delegete = null;
        public WinnerActiveProcessor(WinnerDatesSearched_delegete _winnerDatesSearched_delegete) {
            winnerDatesSearched_delegete = _winnerDatesSearched_delegete;
        }

        private void OperateWinDates_proc()
        {
            List<DateTime> winDates = new List<DateTime>();
            try
            {
                XmlDocument xmlConditions = null;
                XmlNodeList nodes = null;

                    text = @"http://zakupki.gov.ru/epz/contract/contract/extended/search/rss?placeOfSearch=FZ_44&";
                    text += @"_placeOfSearch=on&customer.title=&customer.code=&customer.fz94id=&customer.fz223id=&";
                    text += @"customer.inn=&_showChangedCustomer=on&contractNumber=&_contractStageList=on&";
                    text += @"contractStageList=ISPOLNENIE_ZAVERSHENO&priceStart=&priceEnd=&contractDateStart=&";
                    text += @"contractDateEnd=&publishDateStart=&publishDateEnd=&registryDateStart=&registryDateEnd=&";
                    text += @"executionDateStart=&executionDateEnd=&selectedContractDataChanges=ANY&";
                    text += @"fundsList=BudgetaryFunds&_fundsList=on&fundsList=ExtraBudgetaryFunds&budgetName=&";
                    text += @"_budgetLevelCodes=on&_nonBudgetCodesList=on&contractCurrencyID=-1&registryNumber=&";
                    text += @"_custLev=on&_headAgencyWithSubElements=on&headAgencyId=&_placingWayForContractList=on&";
                    text += @"summingUpDateStart=&summingUpDateEnd=&orderNumber=&documentRequisites=&budgetCode%5B0%5D=&";
                    text += @"budgetCode%5B1%5D=&budgetCode%5B2%5D=&budgetCode%5B3%5D=&budgetCode%5B4%5D=&";
                    text += @"contractYearPriceStart=&contractYearPriceEnd=&contractTotalYearPriceStart=&";
                    text += @"contractTotalYearPriceEnd=&goodsDescription=&_okdpWithSubElements=on&okdpIds=&";
                    text += @"_okpdWithSubElements=on&okpdIds=&_okpd2WithSubElements=on&okpd2Ids=&goodsCountStart=&";
                    text += @"goodsCountEnd=&unitPriceStart=&unitPriceEnd=&totalProductsPriceByCodeStart=&";
                    text += @"totalProductsPriceByCodeEnd=&supplierTitle=" + winnerName + "&supplierKPP=&_supplierStatusCodes=on&";
                    text += @"supplierAddress=&supplierPhone=&pageNumber=0&searchText=&strictEqual=false&";
                    text += @"morphology=false&recordsPerPage=_10&contractSimpleSorting=PO_DATE_OBNOVLENIJA&kladrRegionCode=";

                    xmlConditions = new XmlDocument();
                    xmlConditions.Load(text);
                    nodes = xmlConditions.SelectNodes("/rss/channel/item/description");

                    Regex regex = new Regex("размещено.*?(\\d{2}\\.\\d{2}\\.\\d{4})", RegexOptions.IgnoreCase);
                    Match m = null;
                    DateTime dt;
                    foreach (XmlNode description in nodes) {
                        m = regex.Match(description.InnerText);
                        if ((m != null) && (m.Groups.Count > 1) && (m.Groups[1].Value.Trim() != "")) {
                            try
                            {
                                dt = Convert.ToDateTime(m.Groups[1].Value.Trim());
                                winDates.Add(dt);
                            }
                            catch {
                                continue;
                            }
                        }
                    }

                    //Размещено: </ strong > 26.08.2014

                winnerDatesSearched_delegete(winDates, ResultType_enum.Done, "");
            }
            catch (Exception ex)
            {
                winnerDatesSearched_delegete(winDates, ResultType_enum.Error, ex.Message + '\n' + ex.StackTrace);
                return;
            }
        }

        public void OperateWinDates(string _winnerName)
        {
            winnerName = _winnerName;
            Thread lotsSearch_thread = new Thread(OperateWinDates_proc);
            lotsSearch_thread.Start();
        }
    }
}
