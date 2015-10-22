using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using PublicOrders.Models;

namespace PublicOrders
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public DocumentDbContext dc { get; set; }
        public ObservableCollection<Template> TemplateCollection {get;set;}
        public ObservableCollection<Product> ProductCollection { get; set; }

        //public DocumentDbContext dc { get; set; }
        public MainViewModel()
		{
            // Insert code required on object creation below this point.
            dc = new DocumentDbContext();
            TemplateCollection = new ObservableCollection<Template>(dc.Templates);
            ProductCollection = new ObservableCollection<Product>(dc.Products);
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