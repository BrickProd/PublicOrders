using System;
using System.Collections.Generic;
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
    }
}
