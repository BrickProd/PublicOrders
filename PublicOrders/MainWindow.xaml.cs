using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace PublicOrders
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
	    private DispatcherTimer dt = null;

		public MainWindow()
		{
			this.InitializeComponent();

            dt = new DispatcherTimer();
		    dt.Tick += new EventHandler(GetWether);
            dt.Interval = new TimeSpan(0, 0, 1, 0, 0);
            dt.Start();
        }

        private void GetWether(object sender, EventArgs e)
        {
            XmlDocument xmlConditions = new XmlDocument();
            try
            {
                xmlConditions.Load(string.Format("http://informer.gismeteo.ru/xml/27612.xml"));
                this.TempInfo.Text = xmlConditions.SelectSingleNode("/MMWEATHER/REPORT/TOWN/FORECAST/TEMPERATURE").Attributes["min"].InnerText + "˚";
                this.TempInfo.ToolTip = string.Format("Сейчас в Москве температура {0}˚... {1}˚, ветер {2}-{3} м/с",
                    xmlConditions.SelectSingleNode("/MMWEATHER/REPORT/TOWN/FORECAST/TEMPERATURE").Attributes["min"].InnerText,
                    xmlConditions.SelectSingleNode("/MMWEATHER/REPORT/TOWN/FORECAST/TEMPERATURE").Attributes["max"].InnerText,
                    xmlConditions.SelectSingleNode("/MMWEATHER/REPORT/TOWN/FORECAST/WIND").Attributes["min"].InnerText,
                    xmlConditions.SelectSingleNode("/MMWEATHER/REPORT/TOWN/FORECAST/WIND").Attributes["min"].InnerText
                    );


                // Ξbrick
                xmlConditions.Load(string.Format("http://www.cbr.ru/scripts/XML_daily.asp"));
                var bax = xmlConditions.SelectSingleNode("/ValCurs/Valute[@ID='R01235']/Value").InnerText;
                this.BaxInfo.Text = "$ "+ bax.Substring(0, bax.Length - 2);

                var euro = xmlConditions.SelectSingleNode("/ValCurs/Valute[@ID='R01239']/Value").InnerText;
                this.EuroInfo.Text = "€ " + euro.Substring(0, euro.Length - 2);


            }
            catch (Exception ex)
            {
                this.TempInfo.Text = "";
                this.TempInfo.ToolTip = "Погода не доступна Х(";
            }
        }
    }
}