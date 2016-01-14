using PublicOrders.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PublicOrders.Processors.Internet
{
    public delegate void AllWinersSearched_delegete(ResultType_enum resultType_enum, string message);
    public delegate void WinnerSearched_delegate(Customer customer);
    public delegate void WinnerSearchProgress_delegate(string text, int intValue);

    class WinnersSearchProcessor
    {
    }
}
