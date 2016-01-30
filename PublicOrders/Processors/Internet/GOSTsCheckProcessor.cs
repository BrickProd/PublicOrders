using PublicOrders.Models;
using PublicOrders.Processors.Internet.Main;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using PublicOrders.Data;

namespace PublicOrders.Processors.Internet
{
    public delegate void AllGOSTsChecked_delegete(ResultType_enum resultType_enum, string message);
    public delegate void GOSTCheckProgress_delegate(string text, int intValue);

    public class GOSTsCheckProcessor
    {
        MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        private string text = "";
        private HtmlAgilityPack.HtmlDocument doc;
        private InternetRequestEngine internetRequestEngine = null;

        private bool isWork = false;
        private bool isPause = false;

        private AllGOSTsChecked_delegete allGOSTsChecked_delegete = null;
        private GOSTCheckProgress_delegate gostCheckProgress_delegate = null;

        private List<Product> products { get; set; } 

        public GOSTsCheckProcessor(AllGOSTsChecked_delegete _allGOSTsChecked_delegete,
                                   GOSTCheckProgress_delegate _gostCheckProgress_delegate, List<Product> products)

        {
            allGOSTsChecked_delegete = _allGOSTsChecked_delegete;
            gostCheckProgress_delegate = _gostCheckProgress_delegate;
            this.products = products;
        }

        // 0 - ГОСТ актуален
        // 1 - ГОСТ неактуален
        // 2 - ГОСТ не найден
        // 3 - Сервер недоступен
        // 4 - Имя продукта не совпадает с ГОСТ
        private Int16 IsGOSTActual(HtmlAgilityPack.HtmlNode node, string gostNumber, string gostName)
        {
            try
            {
                // Проверим, интернет работает
                Regex regex = new Regex("статус\\s{0,6}\\:", RegexOptions.IgnoreCase);
                Match m = regex.Match(node.InnerText);
                if ((m == null) || (m.Value.Trim() == ""))
                {
                    return 3;
                }


                // Получаем коллекцию найденных ГОСТов
                HtmlAgilityPack.HtmlNodeCollection gosts = node.SelectNodes(".//div[@class=\"resultsInd\"]");
                if (gosts == null)
                {
                    return 3;
                }
                if (gosts.Count == 0)
                {
                    return 2;
                }

                string searchedGostNumber = "";
                string searchedGostName = "";
                string searchedGostStatus = "";
                HtmlAgilityPack.HtmlNodeCollection collTmp = null;
                foreach (HtmlAgilityPack.HtmlNode gost in gosts)
                {
                    // Ищем точное совпадение ГОСТа
                    searchedGostNumber = gost.SelectSingleNode(".//div/h2/a/span").InnerText.Trim();
                    if (searchedGostNumber == gostNumber)
                    {
                        // ПРоверяем статус ГОСТа
                        collTmp = gost.SelectNodes(".//div/strong");
                        searchedGostStatus = collTmp[collTmp.Count - 1].InnerText.ToLower().Trim();

                        if (searchedGostStatus == "действует")
                        {
                            // Проверяем совпадение название товара с ГОСТом
                            /*searchedGostName = gost.SelectSingleNode(".//div/span/a").InnerText.ToLower().Trim();
                            if (searchedGostName.IndexOf(gostName.ToLower()) > -1)
                            {*/
                            return 0;
                            /*}
                            else {
                                return 4;
                            }*/
                        }
                        else
                        {
                            return 1;
                        }
                    }
                }

                return 2;
            }
            catch
            {
                return 2;
            }
        }

        private void CheckGOSTs_proc()
        {
            try
            {
                isWork = true;
                isPause = false;

                InternetRequestEngine internetRequestEngine = new InternetRequestEngine();
                //List<Product> products = DataService.Context.Products.ToList();
                Regex regex = new Regex("гост.*?\\d*-\\d{2,4}", RegexOptions.IgnoreCase);
                MatchCollection mColl = null;
                Int16 checkResult = -1;

                double productInterval = 100 / Convert.ToDouble(products.Count());
                int currentInterval = 0;

                int productNum = 0;
                foreach (Product product in products )
                {
                    productNum++;
                    currentInterval = Convert.ToInt32(productNum * productInterval);
                    gostCheckProgress_delegate("Обработка продукта.. [" + productNum + "\\" + products.Count() + "]", currentInterval);

                    while (isPause)
                    {
                        Thread.Sleep(300);
                    }
                    if (!isWork) break;

                    mColl = regex.Matches(product.Certification);
                    if (mColl != null) {
                        if (mColl.Count > 0)
                        {
                            foreach (Match m in mColl)
                            {
                                doc = internetRequestEngine.GetHtmlDoc(@"http://www.gostinfo.ru/catalog/gostlist?searchString=" + m.Value + @"&searchcatalogbtn=%C8%F1%EA%E0%F2%FC");
                                checkResult = IsGOSTActual(doc.DocumentNode, m.Value.Trim(), product.Name);
                                if (checkResult != 0)
                                {
                                    if (checkResult == 3) break;
                                    product.IsNotActualCert = checkResult;
                                    break;
                                }
                                product.IsNotActualCert = 0;
                            }
                        }
                        else {
                            product.IsNotActualCert = 2;
                        }


                    }

                    DataService.Context.Entry(product).State = System.Data.Entity.EntityState.Modified;
                    DataService.Context.SaveChanges();
                }

                gostCheckProgress_delegate("", 0);
                allGOSTsChecked_delegete(ResultType_enum.Done, "");
            }
            catch (Exception ex)
            {
                gostCheckProgress_delegate("", 0);
                allGOSTsChecked_delegete(ResultType_enum.Error, ex.Message + '\n' + ex.StackTrace);
                return;
            }
        }

        public void Operate()
        {
            Thread searchCustomers_thread = new Thread(CheckGOSTs_proc);
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
