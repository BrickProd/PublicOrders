using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Models;
using System.Threading;
using Word = Microsoft.Office.Interop.Word;

namespace PublicOrders.Processors.Main
{
    public delegate void CreateDocumentDone_delegete(ResultType resultType, string message);
    public class CreateDocumentProcessor
    {
        private bool isWork = false;

        private FreedomProcessor freedomProcessor = null;
        private Form2Processor form2Processor = null;
        private CommitteeProcessor committeeProcessor = null;

        object falseValue = false;
        object trueValue = true;
        Object missingObj = System.Reflection.Missing.Value;
        Object trueObj = true;
        Object falseObj = false;
        Object begin = Type.Missing;
        Object end = Type.Missing;

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
            try
            {
                string message = "";
                Word.Application application = new Word.Application();
                ResultType createResult = ResultType.Done;
                //application.DisplayAlerts = Microsoft.Office.Interop.Word.WdAlertLevel.wdAlertsNone;
                Word._Document doc = null;

                switch (template.Name.ToLower().Trim())
                {
                    case ("свобода"):
                        freedomProcessor = new FreedomProcessor();
                        createResult = freedomProcessor.Create(document, application, out doc, out message);
                        break;
                    case ("форма 2"):
                        form2Processor = new Form2Processor();
                        createResult = form2Processor.Create(document, application, out doc, out message);
                        break;
                    case ("комитет"):
                        committeeProcessor = new CommitteeProcessor();
                        createResult = committeeProcessor.Create(document, application, out doc, out message);
                        break;
                    default:
                        doc = null;
                        message = "Данный движок не обрабатывает шаблоны типа: <" + Convert.ToString(template.Name.Trim()) + ">";
                        isWork = false;
                        done_del(ResultType.Error, message);
                        return;
                }

                switch (createResult)
                {
                    case (ResultType.Done):
                        // Добавление инструкции
                        // Получаем путь инструкции
                        string instText = "";
                        if (document.Instruction != null)
                        {
                            instText = document.Instruction.Text;
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
                    case (ResultType.Error):
                        isWork = false;
                        done_del(ResultType.Error, message);
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
                done_del(ResultType.Error, ex.Message + '\n' + ex.StackTrace);
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
