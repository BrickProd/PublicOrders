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
            context = new PublicOrdersContext();
            context.Configuration.LazyLoadingEnabled = false;

            Winners.Clear();
            context.Winners.ToList().ForEach(m => Winners.Add(m));

            WinnerStatuses.Clear();
            context.WinnerStatuses.ToList().ForEach(m => WinnerStatuses.Add(m));

            ClientUsers.Clear();
            context.Users.Where(m => m.UserStatusId == 2).ToList().ForEach(m => ClientUsers.Add(m));

            context.Configuration.LazyLoadingEnabled = true;
        }

        public static void UpdateNotesContext()
        {
            //var context = ((IObjectContextAdapter)Context).ObjectContext;
            //context.Refresh(System.Data.Entity.Core.Objects.RefreshMode.StoreWins, Context.WinnerNotes);
        }

        public static void UpdateProductsContext()
        {
            context = new PublicOrdersContext();
            context.Configuration.LazyLoadingEnabled = false;
            Products.Clear();
            context.Products.ToList().ForEach(m=> Products.Add(m));

            Rubrics.Clear();
            context.Rubrics.ToList().ForEach(m => Rubrics.Add(m));

            Instructions.Clear();
            context.Instructions.ToList().ForEach(m => Instructions.Add(m));


            context.Configuration.LazyLoadingEnabled = true;

        }

        public static ObservableCollection<Winner> Winners { get; set; } = new ObservableCollection<Winner>(context.Winners);
        public static ObservableCollection<Customer> Customers { get; set; } = new ObservableCollection<Customer>(/*context.Customers*/);
        public static ObservableCollection<Lot> Lots { get; set; } = new ObservableCollection<Lot>(/*context.Lots*/);
        public static ObservableCollection<WinnerStatus> WinnerStatuses { get; set; } = new ObservableCollection<WinnerStatus>(context.WinnerStatuses);
        public static ObservableCollection<User> ClientUsers { get; set; } = new ObservableCollection<User>(context.Users.Where(m => m.UserStatusId == 2));


        public static ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>(context.Products);
        public static ObservableCollection<Rubric> Rubrics { get; set; } = new ObservableCollection<Rubric>(context.Rubrics);
        public static ObservableCollection<Instruction> Instructions { get; set; } = new ObservableCollection<Instruction>(context.Instructions);
        public static ObservableCollection<string> Templates { get; set; } = new ObservableCollection<string>(new List<string> { "Комитет", "Свобода", "Форма 2"});
    }
}
