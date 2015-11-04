using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using PublicOrders.Models;
using Word = Microsoft.Office.Interop.Word;

namespace PublicOrders.Processors.Documents.Main
{
    public delegate void CreateWinnersDocumentDone_delegete(ResultType_enum ResultType_enum, string message);
    public class CreateWinnersDocProcessor
    {
        object falseValue = false;
        object trueValue = true;
        Object missingObj = System.Reflection.Missing.Value;
        Object trueObj = true;
        Object falseObj = false;
        Object begin = Type.Missing;
        Object end = Type.Missing;

        private bool isWork = false;
        List<Winner> winners = null;
        CreateWinnersDocumentDone_delegete createWinnersDocumentDone_delegete = null;
        public CreateWinnersDocProcessor(List<Winner> _winners, CreateWinnersDocumentDone_delegete _createWinnersDocumentDone_delegete) {
            winners = _winners;
            createWinnersDocumentDone_delegete = _createWinnersDocumentDone_delegete;
        }

        private void Operate_thread() {
            Word._Document doc = null;
            Word.Application application = null;
            try
            {
                if (winners.Count() == 0) {
                    createWinnersDocumentDone_delegete(ResultType_enum.Done, "");
                    return;
                }

                isWork = true;



                    application = new Word.Application();
                    ResultType_enum createResult = ResultType_enum.Done;

                    doc = application.Documents.Add(ref missingObj, ref missingObj, ref missingObj, ref missingObj);
                    // Стиль
                    object patternstyle = Word.WdStyleType.wdStyleTypeParagraph;
                    Word.Style wordstyle = doc.Styles.Add("myStyle", ref patternstyle);
                    wordstyle.Font.Size = 9;
                    wordstyle.Font.Name = "Times New Roman";
                    Word.Range wordrange = doc.Range(ref begin, ref end);
                    object oWordStyle = wordstyle;
                    wordrange.set_Style(ref oWordStyle);

                    doc.Paragraphs.Add(ref missingObj);
                    doc.Paragraphs.Add(ref missingObj);
                    doc.Paragraphs.Add(ref missingObj);
                    doc.Paragraphs.Add(ref missingObj);
                    doc.Paragraphs.Add(ref missingObj);
                    doc.Paragraphs.Add(ref missingObj);
                    doc.Paragraphs.Add(ref missingObj);
                    doc.Paragraphs.Add(ref missingObj);
                    doc.Paragraphs.Add(ref missingObj);
                    doc.Paragraphs.Add(ref missingObj);
                    doc.Paragraphs.Add(ref missingObj);

                    doc.Paragraphs[2].Range.Text = "ЗАКАЗЧИК";
                    doc.Paragraphs[2].Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Paragraphs[2].Range.Font.Bold = 1;


                    doc.Paragraphs[3].Range.Text = winners.ElementAt(0).Lot.Order.Customer.Name.ToUpper();
                    doc.Paragraphs[3].Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Paragraphs[4].Range.Text = "ИНН : " + winners.ElementAt(0).Lot.Order.Customer.Vatin.Trim();
                    doc.Paragraphs[4].Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;


                    doc.Paragraphs[6].Range.Text = "ПОБЕДИТЕЛИ";
                    doc.Paragraphs[6].Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Paragraphs[6].Range.Font.Bold = 1;

                    Object defaultTableBehavior = Word.WdDefaultTableBehavior.wdWord9TableBehavior;
                    Object autoFitBehavior = Word.WdAutoFitBehavior.wdAutoFitWindow;
                    Word.Table wordtable = doc.Tables.Add(doc.Paragraphs[7].Range, winners.Count() + 2, 7,
                      ref defaultTableBehavior, ref autoFitBehavior);

                    // Размеры колонок
                    wordtable.Columns[1].Width = 30;
                    //wordtable.Columns[2].Width = 150;

                    // Объединение ячеек заголовка
                    object begCell = wordtable.Cell(1, 1).Range.Start;
                    object endCell = wordtable.Cell(2, 1).Range.End;
                    Word.Range wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Cells.Merge();

                    begCell = wordtable.Cell(1, 2).Range.Start;
                    endCell = wordtable.Cell(1, 5).Range.End;
                    wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Cells.Merge();

                    begCell = wordtable.Cell(1, 3).Range.Start;
                    endCell = wordtable.Cell(1, 4).Range.End;
                    wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Cells.Merge();



                    // Заполнение заголовка
                    // Первая строка
                    doc.Tables[1].Cell(1, 1).Range.Text = "№ п/п";
                    doc.Tables[1].Cell(1, 1).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 2).Range.Text = "Лот";
                    doc.Tables[1].Cell(1, 2).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 3).Range.Text = "Победитель";
                    doc.Tables[1].Cell(1, 3).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    // Лот
                    doc.Tables[1].Cell(2, 2).Range.Text = "Название";
                    doc.Tables[1].Cell(2, 2).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(2, 3).Range.Text = "Цена";
                    doc.Tables[1].Cell(2, 3).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(2, 4).Range.Text = "Закон";
                    doc.Tables[1].Cell(2, 4).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(2, 5).Range.Text = "Дата публикации";
                    doc.Tables[1].Cell(2, 5).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    // Победитель
                    doc.Tables[1].Cell(2, 6).Range.Text = "Название";
                    doc.Tables[1].Cell(2, 6).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(2, 7).Range.Text = "Контакты";
                    doc.Tables[1].Cell(2, 7).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    // Заполняем лоты
                    for (int i = 0; i < winners.Count(); i++)
                    {
                        if (!isWork) break;

                        doc.Tables[1].Cell(i + 3, 1).Range.Text = Convert.ToString(i + 1) + '.';
                        doc.Tables[1].Cell(i + 3, 1).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        doc.Tables[1].Cell(i + 3, 2).Range.Text = winners[i].Lot.Name.Trim();
                        doc.Tables[1].Cell(i + 3, 2).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        doc.Tables[1].Cell(i + 3, 3).Range.Text = winners[i].Lot.LotPrice + " >> " +
                                                                  winners[i].Lot.DocumentPrice + " (" +
                                                                  winners[i].Lot.LotPriceType.Name.Trim() + ")";
                        doc.Tables[1].Cell(i + 3, 3).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        doc.Tables[1].Cell(i + 3, 4).Range.Text = winners[i].Lot.Order.LawType.Name.Trim();
                        doc.Tables[1].Cell(i + 3, 4).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        doc.Tables[1].Cell(i + 3, 5).Range.Text = winners[i].Lot.Order.PublishDateTime.ToString("dd.MM.yyyy");
                        doc.Tables[1].Cell(i + 3, 5).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        doc.Tables[1].Cell(i + 3, 6).Range.Text = winners[i].Name;
                        doc.Tables[1].Cell(i + 3, 6).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        string telStr = "телефон не указан";
                        string emailStr = "email не указан";
                        if ((winners[i].Email != null) && (winners[i].Email.Trim() != ""))
                        {
                            emailStr = winners[i].Email.Trim();
                        }

                        if ((winners[i].Phone != null) && (winners[i].Phone.Trim() != ""))
                        {
                            telStr = winners[i].Phone.Trim();
                        }

                        doc.Tables[1].Cell(i + 3, 7).Range.Text = telStr + ", " + emailStr;
                        doc.Tables[1].Cell(i + 3, 7).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    }
                    application.Visible = true;


                isWork = false;
                createWinnersDocumentDone_delegete(ResultType_enum.Done, "");
            }
            catch (Exception ex){
                isWork = false;
                if (doc != null)
                    doc.Close(ref falseObj, ref missingObj, ref missingObj);
                doc = null;

                if (application != null)
                    application.Quit();
                application = null;
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
