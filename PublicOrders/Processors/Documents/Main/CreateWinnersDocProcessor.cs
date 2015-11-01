using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using PublicOrders.Models;

namespace PublicOrders.Processors.Documents.Main
{
    public delegate void CreateWinnersDocumentDone_delegete(ResultType_enum ResultType_enum, string message);
    public class CreateWinnersDocProcessor
    {
        private bool isWork = false;
        List<Winner> winners = null;
        CreateWinnersDocumentDone_delegete createWinnersDocumentDone_delegete = null;
        public CreateWinnersDocProcessor(List<Winner> _winners, CreateWinnersDocumentDone_delegete _createWinnersDocumentDone_delegete) {
            winners = _winners;
            createWinnersDocumentDone_delegete = _createWinnersDocumentDone_delegete;
        }

        private void Operate_thread() {
            try
            {
                isWork = true;



                isWork = false;
                createWinnersDocumentDone_delegete(ResultType_enum.Done, "");
            }
            catch (Exception ex){
                createWinnersDocumentDone_delegete(ResultType_enum.Error, ex.Message + '\n' + ex.StackTrace);
            }
        }

        public void Operate()
        {
            if (isWork) return;
            Thread operate_thread = new Thread(Operate_thread);
            operate_thread.Start();
        }

        public bool isWorking()
        {
            return isWork;
        }

        public void Stop()
        {
            isWork = false;
        }
    }
}
