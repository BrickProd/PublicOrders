using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Models;

namespace PublicOrders.Data
{
    public static class DataService
    {
        private static PublicOrdersContext context = new PublicOrdersContext();

        public static PublicOrdersContext Context
        {
            get { return context; }
        }

        public static User CurrentUser { get; set; }

        public static void UpdateWinnerContext()
        {
            //var context = ((IObjectContextAdapter)Context).ObjectContext;

            //Context.Winners.ToList().ForEach(m =>
            //{
            //    context.Refresh(System.Data.Entity.Core.Objects.RefreshMode.ClientWins, m);
            //});
        }

        public static void UpdateNotesContext()
        {
            var context = ((IObjectContextAdapter)Context).ObjectContext;

            Context.WinnerNotes.ToList().ForEach(m =>
            {
                context.Refresh(System.Data.Entity.Core.Objects.RefreshMode.ClientWins, m);
            });
        }

        public static void UpdateProductsContext()
        {
            var context = ((IObjectContextAdapter)Context).ObjectContext;

            Context.Products.ToList().ForEach(m =>
            {
                context.Refresh(System.Data.Entity.Core.Objects.RefreshMode.ClientWins, m);
            });
        }

        public static ObservableCollection<Winner> Winners { get; set; } = new ObservableCollection<Winner>(context.Winners);
        public static ObservableCollection<Customer> Customers { get; set; } = new ObservableCollection<Customer>(/*context.Customers*/);
        public static ObservableCollection<Lot> Lots { get; set; } = new ObservableCollection<Lot>(/*context.Lots*/);


        public static ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>(context.Products);
        public static ObservableCollection<Rubric> Rubrics { get; set; } = new ObservableCollection<Rubric>(context.Rubrics);
        public static ObservableCollection<Instruction> Instructions { get; set; } = new ObservableCollection<Instruction>(context.Instructions);
        public static ObservableCollection<string> Templates { get; set; } = new ObservableCollection<string>(new List<string> { "Комитет", "Свобода", "Форма 2"});
    }
}
