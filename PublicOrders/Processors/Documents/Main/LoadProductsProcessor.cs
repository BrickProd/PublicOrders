﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Models;
using System.Threading;
using PublicOrders.Processors;

namespace PublicOrders.Processors.Main
{
    public delegate void LoadProductsDone_delegete(ResultType_enum ResultType_enum, string templateStr, int productsAddedCount, int productsRepeatCount, int productsMergeCount, string message);
    public class LoadProductsProcessor
    {
        private bool isWork = false;

        private FreedomProcessor freedomProcessor = null;
        private Form2Processor form2Processor = null;
        private CommitteeProcessor committeeProcessor = null;

        private string documentPath = "";
        private string templateStr;
        LoadProductsDone_delegete done_del = null;

        public LoadProductsProcessor(string _documentPath, string _templateStr, LoadProductsDone_delegete _done_del)
        {
            documentPath = _documentPath;
            templateStr = _templateStr;
            done_del = _done_del;
        }

        private void Operate_thread()
        {
            string message = "";
            int productsAddedCount = 0;
            int productsRepeatCount = 0;
            int productsMergeCount = 0;
            ResultType_enum ResultType_enum;
            try
            {
                switch (templateStr.Trim().ToLower())
                {
                    case ("свобода"):
                        FreedomProcessor freedomLoadProcessor = new FreedomProcessor();
                        ResultType_enum = freedomLoadProcessor.Learn(documentPath, out productsAddedCount, out productsRepeatCount, out productsMergeCount, out message);
                        isWork = false;
                        done_del(ResultType_enum, templateStr, productsAddedCount, productsRepeatCount, productsMergeCount, message);
                        return;
                    case ("форма 2"):
                        Form2Processor form2LoadProcessor = new Form2Processor();
                        ResultType_enum = form2LoadProcessor.Learn(documentPath, out productsAddedCount, out productsRepeatCount, out productsMergeCount, out message);
                        isWork = false;
                        done_del(ResultType_enum, templateStr, productsAddedCount, productsRepeatCount, productsMergeCount, message);
                        return;
                    case ("комитет"):
                        CommitteeProcessor committeeLoadProcessor = new CommitteeProcessor();
                        ResultType_enum = committeeLoadProcessor.Learn(documentPath, out productsAddedCount, out productsRepeatCount, out productsMergeCount, out message);
                        done_del(ResultType_enum, templateStr, productsAddedCount, productsRepeatCount, productsMergeCount, message);
                        isWork = false;
                        return;
                    default:
                        isWork = false;
                        done_del(ResultType_enum.Error, templateStr, productsAddedCount, productsRepeatCount, productsMergeCount, "Данный движок не обучает шаблоны типа: <" + templateStr.Trim() + ">");
                        return;
                }
                isWork = false;
            }
            catch (Exception ex)
            {
                isWork = false;
                done_del(ResultType_enum.Error, templateStr, productsAddedCount, productsRepeatCount, productsMergeCount, ex.Message + '\n' + ex.StackTrace);
            }
        }

        public void Operate()
        {
            if (isWork) return;
            isWork = true;
            Task operate_task = new Task(Operate_thread);
            operate_task.Start();
            /*Thread operate_thread = new Thread(Operate_thread);
            operate_thread.Start();*/
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
