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
    public delegate void AllLotsSearched_delegete(ResultType_enum ResultType_enum, string message);
    public delegate void LotSearched_delegate(Lot lot, Order order, Customer customer);


    public class LotsSearchProcessor
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


        public LotsSearchProcessor(string _customerName, CustomerType _customerType, decimal _priceMin, decimal _priceMax, string _town,
                               DateTime _publishDateMin, DateTime _publishDateMax,
                               LawType _lawType, CustomersSearchDone_delegate _customersSearchDone_delegate,
                               int _searchingProgress)
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

        private void LotsSearch_proc()
        {

        }

        public void Operate()
        {
            Thread lotsSearch_thread = new Thread(LotsSearch_proc);
            lotsSearch_thread.Start();
        }

        public void Stop()
        {
            isWork = false;
        }
    }
}
