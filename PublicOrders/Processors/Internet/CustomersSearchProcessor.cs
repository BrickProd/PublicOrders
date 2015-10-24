using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Models;
using System.Windows;
using System.Threading;

namespace PublicOrders.Processors.Main
{
    public delegate void CustomersSearchDone_delegate(ResultType_enum ResultType_enum, string message);


    public class CustomersSearchProcessor
    {
        private bool isWork = false;

        private string customerName = "";
        private decimal priceMin = 0;
        private decimal priceMax = 0;
        private string town = "";
        private DateTime publishDateMin;
        private DateTime publishDateMax;
        private CustomerType customerType;
        private LawType lawType;
        private CustomersSearchDone_delegate customersSearchDone_delegate = null;
        private int searchingProgress = 0;


        public CustomersSearchProcessor(string _customerName, CustomerType _customerType, decimal _priceMin, decimal _priceMax, string _town,
                               DateTime _publishDateMin, DateTime _publishDateMax,
                               LawType _lawType, CustomersSearchDone_delegate _customersSearchDone_delegate,
                               int _searchingProgress )
        {
            customerName = _customerName;
            customerType = _customerType;
            priceMin = _priceMin;
            priceMax = _priceMax;
            town = _town;
            publishDateMin = _publishDateMin;
            publishDateMax = _publishDateMax;
            lawType = _lawType;
            customersSearchDone_delegate = _customersSearchDone_delegate;
            searchingProgress = _searchingProgress;
        }

        private void SearchCustomers_proc()
        {
            
        }

        public void SearchCustomers()
        {
            Thread searchCustomers_thread = new Thread(SearchCustomers_proc);
            searchCustomers_thread.Start();
        }

        public void Stop()
        {
            isWork = false;
        }
    }
}
