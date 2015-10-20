using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Models;
using System.Threading;
using PublicOrders.Processors;

namespace PublicOrders.Processors
{
    delegate void LoadProductsDone_delegete(ResultType resultType, Template template, int productsAddedCount, int productsRepeatCount, string message);
    class LoadProductsProcessor
    {
        private string documentPath = "";
        private Template template;
        LoadProductsDone_delegete done_del = null;

        public LoadProductsProcessor(string _documentPath, Template _template, LoadProductsDone_delegete _done_del)
        {
            documentPath = _documentPath;
            template = _template;
            done_del = _done_del;
        }

        private void Operate_thread()
        {
            string message = "";
            int productsAddedCount = 0;
            int productsRepeatCount = 0;
            ResultType resultType;
            try
            {
                switch (template.Name.Trim().ToLower())
                {
                    case ("свобода"):
                        FreedomLoadProcessor freedomLoadProcessor = new FreedomLoadProcessor();
                        resultType = freedomLoadProcessor.Learn(documentPath, out productsAddedCount, out productsRepeatCount, out message);
                        done_del(resultType, template, productsAddedCount, productsRepeatCount, message);
                        return;
                    case ("форма 2"):
                        Form2LoadProcessor form2LoadProcessor = new Form2LoadProcessor();
                        resultType = form2LoadProcessor.Learn(documentPath, out productsAddedCount, out productsRepeatCount, out message);
                        done_del(resultType, template, productsAddedCount, productsRepeatCount, message);
                        return;
                    case ("комитет"):
                        CommitteeLoadProcessor committeeLoadProcessor = new CommitteeLoadProcessor();
                        resultType = committeeLoadProcessor.Learn(documentPath, out productsAddedCount, out productsRepeatCount, out message);
                        done_del(resultType, template, productsAddedCount, productsRepeatCount, message);
                        return;
                    default:
                        done_del(ResultType.Error, template, productsAddedCount, productsRepeatCount, "Данный движок не обучает шаблоны типа: <" + template.Name + ">");
                        return;
                }
            }
            catch (Exception ex)
            {
                done_del(ResultType.Error, template, productsAddedCount, productsRepeatCount, ex.Message + '\n' + ex.StackTrace);
            }
        }

        public void Operate()
        {
            Thread operate_thread = new Thread(Operate_thread);
            operate_thread.Start();
        }
    }
}
