using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using HtmlAgilityPack;

namespace PublicOrders.Processors.Internet
{
    public class InternetRequestEngine
    {
        private bool isReady = false;
        private Random rand = null;
        private HtmlAgilityPack.HtmlDocument agilityDoc = null;
        private HtmlAgilityPack.HtmlWeb agilityWeb = null;

        public InternetRequestEngine()
        {
            rand = new Random();
            agilityWeb = new HtmlWeb();
        }

        public HtmlAgilityPack.HtmlDocument GetHtmlDoc(string htmlText, int RandIntervalMax = 700)
        {
            try
            {
                int randInt = rand.Next(100, RandIntervalMax);
                // Корреляция времени запроса
                Thread.Sleep(randInt);
                try
                {
                    agilityDoc = agilityWeb.Load(htmlText);
                }
                catch
                {
                    agilityDoc = null;
                }
                return agilityDoc;
            }
            catch
            {
                return null;
            }
        }
    }
}
