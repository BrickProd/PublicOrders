using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Configuration;
using System.Collections.ObjectModel;

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

        public class LT
        {
            public string name { get; set; }
            public string value { get; set; }
            public LT() { }
        }

        public LT LawType
        {
            get
            {
                if (LawTypes.First(m => m.value == Properties.Settings.Default.LawType) != null)
                    return LawTypes.First(m => m.value == Properties.Settings.Default.LawType);
                else return null;
            }
            set
            {
                //var a = value;
                Properties.Settings.Default.LawType = value.value;
                Properties.Settings.Default.Save();
            }
        }

        public class CT
        {
            public string name { get; set; }
            public string value { get; set; }
            public CT() { }
        }
        public CT CustomerType
        {
            get
            {
                if (CustomerTypes.First(m => m.value == Properties.Settings.Default.CustomerType) != null)
                    return CustomerTypes.First(m => m.value == Properties.Settings.Default.CustomerType);
                else return null;
            }
            set
            {
                Properties.Settings.Default.CustomerType = value.value;
                Properties.Settings.Default.Save();
            }
        }

        public ObservableCollection<LT> LawTypes { get; set; }
        public ObservableCollection<CT> CustomerTypes { get; set; }

        public OrderFilterViewModel()
        {
            LawTypes = new ObservableCollection<LT>( new List<LT> {
                new LT (){ name = "№44(№94), №223", value="_44_94_223"  },
                new LT (){ name = "№44(№94)", value="_44_94"  },
                new LT (){ name = "№223", value="_223"  }
            });

            CustomerTypes = new ObservableCollection<CT>(new List<CT> {
                new CT (){ name = "Заказчик", value="Customer"  },
                new CT (){ name = "Организация, размещающая заказ", value="Organization"  }
            });
        }
    }
}
