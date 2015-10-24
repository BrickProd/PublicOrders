using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Configuration;

namespace PublicOrders.ViewModels
{
    public class OrderFilterViewModel
    {
        public ulong MinPrice
        {
            get
            {
                return Properties.Settings.Default.MinPrice;
            }
            set
            {
                Properties.Settings.Default.MinPrice = value;
                Properties.Settings.Default.Save();
            }
        }

        public ulong MaxPrice
        {
            get
            {
                return Properties.Settings.Default.MaxPrice;
            }
            set
            {
                Properties.Settings.Default.MaxPrice = value;
                Properties.Settings.Default.Save();
            }
        }

        public string CustomerCity
        {
            get
            {
                return Properties.Settings.Default.CustomerCity;
            }
            set
            {
                Properties.Settings.Default.CustomerCity = value;
                Properties.Settings.Default.Save();
            }
        }

        public DateTime MinPublicDate
        {
            get
            {
                return Properties.Settings.Default.MinPublicDate;
            }
            set
            {
                Properties.Settings.Default.MinPublicDate = value;
                Properties.Settings.Default.Save();
            }
        }

        public DateTime MaxPublicDate
        {
            get
            {
                return Properties.Settings.Default.MaxPublicDate;
            }
            set
            {
                Properties.Settings.Default.MaxPublicDate = value;
                Properties.Settings.Default.Save();
            }
        }

        public string LawType
        {
            get
            {
                return Properties.Settings.Default.LawType;
            }
            set
            {
                Properties.Settings.Default.LawType = value;
                Properties.Settings.Default.Save();
            }
        }

        public string CustomerType
        {
            get
            {
                return Properties.Settings.Default.CustomerType;
            }
            set
            {
                Properties.Settings.Default.CustomerType = value;
                Properties.Settings.Default.Save();
            }
        }


        public OrderFilterViewModel()
        {

        }
    }
}
