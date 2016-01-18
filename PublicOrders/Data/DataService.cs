using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Models;

namespace PublicOrders.Data
{
    public static class DataService
    {
        private static WinnersDbContext winnersDbContext = new WinnersDbContext();

        public static WinnersDbContext WinnersDbContext
        {
            get { return winnersDbContext; }
        }

        public static int WinnersDbContextSaveChanges()
        {
            return winnersDbContext.SaveChanges();
        }

        public static void UpdateContext()
        {
            
            var context = ((IObjectContextAdapter)winnersDbContext).ObjectContext;
            WinnersDbContext.Winners.ToList().ForEach(m =>
            {
                context.Refresh(System.Data.Entity.Core.Objects.RefreshMode.StoreWins, m);
            });
            

        }

        //public static ObservableCollection<Winner> Winners
        //{
        //    get { return new ObservableCollection<Winner>(winnersDbContext.Winners); }
        //}

        //public static ObservableCollection<Lot> Lots
        //{
        //    get { return new ObservableCollection<Lot>(winnersDbContext.Lots); }
        //}
    }
}
