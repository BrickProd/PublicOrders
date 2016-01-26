using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Models;

namespace PublicOrders.Data
{
    public class ProductsRepository
    {
        
        public ObservableCollection<Product> GetProducts()
        {
            using (var context = new PublicOrdersContext())
            {
                return new ObservableCollection<Product>(context.Products);
            }
        }

        public void AddProduct(Product product)
        {
            using (var context = new PublicOrdersContext())
            {
                context.Products.Add(product);
                context.SaveChanges();
            }
        }
    }
}
