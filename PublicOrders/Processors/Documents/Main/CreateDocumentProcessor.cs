using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Models;
using System.Threading;
using Word = Microsoft.Office.Interop.Word;
using System.Windows;

namespace PublicOrders.Processors.Main
{
    public delegate void CreateDocumentDone_delegete(ResultType_enum ResultType_enum, string message);
    public class CreateDocumentProcessor
    {
        //MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;
        private bool isWork = false;

        private FreedomProcessor freedomProcessor = null;
        private Form2Processor form2Processor = null;
        private CommitteeProcessor committeeProcessor = null;

        private object falseValue = false;
        private object trueValue = true;
        private Object missingObj = System.Reflection.Missing.Value;
        private Object trueObj = true;
        private Object falseObj = false;
        private Object begin = Type.Missing;
        private Object end = Type.Missing;
        private Instruction instruction = null;
        private List<Product> products = null;
        private string templateStr;
        CreateDocumentDone_delegete done_del = null;

        public CreateDocumentProcessor(List<Product> _products, Instruction _instruction, string _templateStr, CreateDocumentDone_delegete _done_del)
        {
            products = _products;
            templateStr = _templateStr;
            instruction = _instruction;
            done_del = _done_del;
        }

        private void Operate_thread()
        {
            try
            {
                string message = "";
                Word.Application application = new Word.Application();
                ResultType_enum createResult = ResultType_enum.Done;
                //application.DisplayAlerts = Microsoft.Office.Interop.Word.WdAlertLevel.wdAlertsNone;
                Word._Document doc = null;

                switch (templateStr.ToLower().Trim())
                {
                    case ("свобода"):
                        freedomProcessor = new FreedomProcessor();
                        createResult = freedomProcessor.Create(products, application, out doc, out message);
                        break;
                    case ("форма 2"):
                        form2Processor = new Form2Processor();
                        createResult = form2Processor.Create(products, application, out doc, out message);
                        break;
                    case ("комитет"):
                        committeeProcessor = new CommitteeProcessor();
                        createResult = committeeProcessor.Create(products, application, out doc, out message);
                        break;
                    default:
                        doc = null;
                        message = "Данный движок не обрабатывает шаблоны типа: <" + templateStr.Trim() + ">";
                        isWork = false;
                        done_del(ResultType_enum.Error, message);
                        return;
                }

                switch (createResult)
                {
                    case (ResultType_enum.Done):
                        // Добавление инструкции
                        // Получаем путь инструкции
                        string instText = "";
                        if (instruction != null)
                        {
                            instText = instruction.Text;
                        }
                        else {
                            instText = "";
                        }
                        
                        if (instText != "")
                        {
                            application.Selection.EndKey(Word.WdUnits.wdStory, Word.WdMovementType.wdMove);
                            Word.Paragraph lastPar = doc.Paragraphs.Add(ref missingObj);
                            lastPar.Range.Text = instText;
                            lastPar.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                        }

                        break;
                    case (ResultType_enum.Error):
                        isWork = false;
                        done_del(ResultType_enum.Error, message);
                        return;
                    default:
                        break;
                }

                isWork = false;
                done_del(createResult, message);
            }
            catch (Exception ex)
            {
                isWork = false;
                done_del(ResultType_enum.Error, ex.Message + '\n' + ex.StackTrace);
            }
        }

        public void Operate()
        {
            if (isWork) return;
            isWork = true;
            Thread operate_thread = new Thread(Operate_thread);
            operate_thread.Start();
        }

        public bool isWorking() {
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
