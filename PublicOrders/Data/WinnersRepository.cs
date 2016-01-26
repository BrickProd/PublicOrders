using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Models;

namespace PublicOrders.Data
{
    public class WinnersRepository
    {

        public ObservableCollection<Winner> GetWinners()
        {
            using (var context = new PublicOrdersContext())
            {
                return new ObservableCollection<Winner>(context.Winners);
            }
        }
        public ObservableCollection<Customer> GetCustomers()
        {
            using (var context = new PublicOrdersContext())
            {
                return new ObservableCollection<Customer>(context.Customers);
            }
        }

        public Customer GetCustomer(string name, string vatin)
        {
            using (var context = new PublicOrdersContext())
            {
                return context.Customers.Include(m=>m.CustomerLevel).Include(m=>m.CustomerTypes).FirstOrDefault(m =>m.Name == name && m.Vatin == vatin);
            }
        }

        public Winner GetWinner(string name, string vatin)
        {
            using (var context = new PublicOrdersContext())
            {
                return context.Winners.FirstOrDefault(m => m.Name == name && m.Vatin == vatin);
            }
        }

        public CustomerType GetCustomerType(string typeCode)
        {
            using (var context = new PublicOrdersContext())
            {
                return context.CustomerTypes.FirstOrDefault(m => m.CustomerTypeCode.ToLower() == typeCode);
            }
        }

        public ObservableCollection<CustomerLevel> GetCustomerLevels()
        {
            using (var context = new PublicOrdersContext())
            {
                return new ObservableCollection<CustomerLevel>(context.CustomerLevels);
            }
        }

        public ObservableCollection<LotPriceType> GetLotPriceTypes()
        {
            using (var context = new PublicOrdersContext())
            {
                return new ObservableCollection<LotPriceType>(context.LotPriceTypes);
            }
        }


        public Lot GetLot(string contactNumber)
        {
            using (var context = new PublicOrdersContext())
            {
                return context.Lots.FirstOrDefault(m => m.ContractNumber == contactNumber);
            }
        }


        public void AddWinner(Winner winner)
        {
            using (var context = new PublicOrdersContext())
            {
                context.Winners.Add(winner);
                context.SaveChanges();
            }
        }

        public void AddCustomer(Customer customer)
        {
            using (var context = new PublicOrdersContext())
            {
                context.Customers.Add(customer);
                context.SaveChanges();
            }
        }

        public void AddLot(Lot lot)
        {
            using (var context = new PublicOrdersContext())
            {
                context.Lots.Add(lot);
                context.SaveChanges();
            }
        }

        public void SaveWinner(Winner winner)
        {
            using (var context = new PublicOrdersContext())
            {
                context.Entry(winner).State = EntityState.Modified;
                context.SaveChanges();
            }
        }


    }
}
