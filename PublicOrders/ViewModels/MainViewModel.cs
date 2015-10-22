using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using PublicOrders.Models;

namespace PublicOrders
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // Документы
        public DocumentDbContext dc { get; set; }
        public ObservableCollection<Template> TemplateCollection {get;set;}
        public ObservableCollection<Product> ProductCollection { get; set; }

        // Победители
        public WinnersDbContext wc { get; set; }
        public ObservableCollection<Customer> CustomerCollection { get; set; }

        public MainViewModel()
		{
            // Документы
            dc = new DocumentDbContext();
            TemplateCollection = new ObservableCollection<Template>(dc.Templates);
            ProductCollection = new ObservableCollection<Product>(dc.Products);

            // Победители
            wc = new WinnersDbContext();
            CustomerCollection = new ObservableCollection<Customer>(wc.Customers);
        }
		
		
		
		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(String info)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(info));
			}
		}
		#endregion

	}
}