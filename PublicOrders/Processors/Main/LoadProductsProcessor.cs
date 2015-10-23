using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Models;
using System.Threading;
using PublicOrders.Processors;

namespace PublicOrders.Processors.Main
{
    public delegate void LoadProductsDone_delegete(ResultType resultType, Template template, int productsAddedCount, int productsRepeatCount, string message);
    public class LoadProductsProcessor
    {
        private bool isWork = false;

        private FreedomProcessor freedomProcessor = null;
        private Form2Processor form2Processor = null;
        private CommitteeProcessor committeeProcessor = null;

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
                        FreedomProcessor freedomLoadProcessor = new FreedomProcessor();
                        resultType = freedomLoadProcessor.Learn(documentPath, out productsAddedCount, out productsRepeatCount, out message);
                        isWork = false;
                        done_del(resultType, template, productsAddedCount, productsRepeatCount, message);
                        return;
                    case ("форма 2"):
                        Form2Processor form2LoadProcessor = new Form2Processor();
                        resultType = form2LoadProcessor.Learn(documentPath, out productsAddedCount, out productsRepeatCount, out message);
                        isWork = false;
                        done_del(resultType, template, productsAddedCount, productsRepeatCount, message);
                        return;
                    case ("комитет"):
                        CommitteeProcessor committeeLoadProcessor = new CommitteeProcessor();
                        resultType = committeeLoadProcessor.Learn(documentPath, out productsAddedCount, out productsRepeatCount, out message);
                        done_del(resultType, template, productsAddedCount, productsRepeatCount, message);
                        isWork = false;
                        return;
                    default:
                        isWork = false;
                        done_del(ResultType.Error, template, productsAddedCount, productsRepeatCount, "Данный движок не обучает шаблоны типа: <" + template.Name + ">");
                        return;
                }
                isWork = false;
            }
            catch (Exception ex)
            {
                isWork = false;
                done_del(ResultType.Error, template, productsAddedCount, productsRepeatCount, ex.Message + '\n' + ex.StackTrace);
            }
        }

        public void Operate()
        {
            if (isWork) return;
            isWork = true;
            Thread operate_thread = new Thread(Operate_thread);
            operate_thread.Start();
        }

        public bool isWorking()
        {
            return isWork;
        }

        public void Stop()
        {
            if (freedomProcessor != null) freedomProcessor.Stop();
            if (form2Processor != null) form2Processor.Stop();
            if (committeeProcessor != null) committeeProcessor.Stop();
        }
    }
}
