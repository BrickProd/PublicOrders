using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Models;
using System.Threading;

namespace PublicOrders.Processors
{
    delegate void CreateDocumentDone_delegete(string message);
    class CreateDocumentProcessor
    {
        private Document document = null;
        private Template template;
        CreateDocumentDone_delegete done_del = null;

        public CreateDocumentProcessor(Document _document, Template _template, CreateDocumentDone_delegete _done_del)
        {
            document = _document;
            template = _template;
            done_del = _done_del;
        }

        private void Operate_thread()
        {
            /*PublicOrderEngine publicOrderEngine = new PublicOrderEngine();
            string message = "";
            publicOrderEngine.CreateDocFromTemplate(document.base_ID, docTemplate, connectionString, out message);
            publicOrderEngine.Close();*/

            done_del("");
        }

        public void Operate()
        {
            Thread operate_thread = new Thread(Operate_thread);
            operate_thread.Start();
        }
    }
}
