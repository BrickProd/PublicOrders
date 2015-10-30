using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Models;
using System.IO;
using Word = Microsoft.Office.Interop.Word;
using System.Windows;
using System.Collections.ObjectModel;

namespace PublicOrders.Processors
{
    class FreedomProcessor
    {
        private bool isWork = false;
        Object missingObj = System.Reflection.Missing.Value;

        private MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        public ResultType_enum Learn(string docPath, out int productsAddedCount, out int productsRepeatCount, out int productsMergeCount, out string message)
        {
            productsAddedCount = 0;
            productsRepeatCount = 0;
            productsMergeCount = 0;
            try
            {
                isWork = true;
                message = "";

                // Проверка пути документа
                if (!File.Exists(docPath))
                {
                    message = "Документа по пути <" + docPath + "> не существует";
                    return ResultType_enum.Error;
                }

                //Создаём новый Word.Application
                Word.Application application = new Microsoft.Office.Interop.Word.Application();

                //Загружаем документ
                Microsoft.Office.Interop.Word.Document doc = null;

                object fileName = docPath;
                object falseValue = false;
                object trueValue = true;
                object missing = Type.Missing;

                doc = application.Documents.Open(ref fileName, ref missing, ref trueValue,
                ref missing, ref missing, ref missing, ref missing, ref missing,
                ref missing, ref missing, ref missing, ref missing, ref missing,
                ref missing, ref missing, ref missing);

                // Ищем таблицу с данными
                Microsoft.Office.Interop.Word.Table tbl = null;
                try
                {
                    tbl = application.ActiveDocument.Tables[1];
                }
                catch
                {
                    message = "В документе не найдена таблица для обучения";
                    return ResultType_enum.Error;
                }
                if (tbl == null)
                {
                    message = "В документе не найдена таблица для обучения";
                    return ResultType_enum.Error;
                }
                if (tbl.Columns.Count != 6)
                {
                    message = "Количество столбцов таблицы не совпадает со спецификацией";
                    return ResultType_enum.Error;
                }


                // Заполняем продукты
                var r = mvm.dc.Rubrics.FirstOrDefault(m => m.Name.ToLower() == "--без рубрики--");

                // Новый обход документа
                Product product = null;
                FreedomProperty freedomProperty = null;
                Word.Cell cell = tbl.Cell(4, 1);
                while (cell != null) {
                    try
                    {
                        string cellValue = cell.Range.Text.Trim();

                        switch (cell.ColumnIndex) {
                            case (2):
                                // Название (--ПЕРВОЕ ЗНАЧЕНИЕ--)
                                product = new Product();
                                product.Name = Globals.DeleteNandSpaces(Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue)));
                                freedomProperty = new FreedomProperty();
                                break;
                            case (5):
                                // Торговая марка
                                if (product == null) break;
                                product.TradeMark = Globals.DeleteNandSpaces(Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue)));
                                break;
                            case (3):
                                // Требования заказчика
                                freedomProperty.CustomerParam = Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue));
                                break;
                            case (4):
                                // Требования участника
                                freedomProperty.MemberParam = Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue));
                                break;
                            case (6):
                                // Сертификация (--ПОСЛЕДНЕЕ ЗНАЧЕНИЕ--)
                                if (product == null) break;
                                product.Certification = Globals.DeleteNandSpaces(Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue)));

                                if ((freedomProperty.MemberParam.Trim() != "") ||
                                    (freedomProperty.CustomerParam.Trim() != ""))
                                        product.FreedomProperties.Add(freedomProperty);
                                product.ModifiedDateTime = DateTime.Now;

                                // Проверка на повтор
                                // Если совпали название и товарный знак и значения по всем атрибутам, то это ПОВТОР
                                // Если совпали название и товарный знак и значений по данному шаблону нет (или пусты), то это СЛИЯНИЕ
                                // Если совпали название и товарный знак и значения НЕ совпали, то это НОВЫЙ ПРОДУКТ
                                IEnumerable<Product> repeatProducts = mvm.dc.Products.Where(m => (m.Name == product.Name && m.TradeMark == product.TradeMark && m.Certification == product.Certification)).ToList();
                                if (repeatProducts.Any()) {
                                    // Изначально проверим на повтор
                                    bool isRepeat = false;
                                    foreach (Product repeatProduct in repeatProducts)
                                    {
                                        var freedomProperties = repeatProduct.FreedomProperties;
                                        if (freedomProperties.Count() != 1) continue;
                                        if ((freedomProperties.ElementAt(0).CustomerParam == product.FreedomProperties.ElementAt(0).CustomerParam) &&
                                            (freedomProperties.ElementAt(0).MemberParam == product.FreedomProperties.ElementAt(0).MemberParam)) {
                                            isRepeat = true;
                                            break;
                                        }

                                    }
                                    if (isRepeat) {
                                        productsRepeatCount++;
                                        continue;
                                    }

                                    // Слияние делается в том случае, если найден один повторный документ и нет значений по шпблону
                                    if (repeatProducts.Count() == 1)
                                    {
                                        if ((repeatProducts.ElementAt(0).FreedomProperties == null) ||
                                            (repeatProducts.ElementAt(0).FreedomProperties.Count() == 0))
                                        {
                                            productsMergeCount++;
                                            repeatProducts.ElementAt(0).FreedomProperties = product.FreedomProperties;
                                            product = repeatProducts.ElementAt(0);
                                            mvm.dc.Entry(product).State = System.Data.Entity.EntityState.Modified;
                                            mvm.dc.SaveChanges();
                                            continue;
                                        }
                                    }
                                }


                                product.Rubric = r;
                                mvm.dc.Products.Add(product);

                                mvm.dc.SaveChanges();
                                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    mvm.ProductCollection.Add(product);
                                }));

                                productsAddedCount++;
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                    finally {
                        cell = cell.Next;
                    }
                }

                // Закрываем приложение
                application.Quit(ref missing, ref missing, ref missing);
                application = null;

                return ResultType_enum.Done;
        }

            catch (Exception ex)
            {
                message = ex.Message + '\n' + ex.StackTrace;
                return ResultType_enum.Error;
            }
            finally
            {
                isWork = false;
            }
        }

        public ResultType_enum Create(List<Product> products, Word.Application application, out Word._Document doc, out string message)
        {
            try
            {
                isWork = true;
                message = "";

                Object trueObj = true;
                Object falseObj = false;
                Object begin = Type.Missing;
                Object end = Type.Missing;
                doc = null;

                // Если вылетим не этом этапе, приложение останется открытым
                try
                {
                    doc = application.Documents.Add(ref missingObj, ref missingObj, ref missingObj, ref missingObj);
                    // Стиль
                    object patternstyle = Word.WdStyleType.wdStyleTypeParagraph;
                    Word.Style wordstyle = doc.Styles.Add("myStyle", ref patternstyle);
                    wordstyle.Font.Size = 9;
                    wordstyle.Font.Name = "Times New Roman";
                    Word.Range wordrange = doc.Range(ref begin, ref end);
                    object oWordStyle = wordstyle;
                    wordrange.set_Style(ref oWordStyle);

                    // Вставляем параграфы
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


                    doc.Paragraphs[1].Range.Text = "Приложение № 1";
                    doc.Paragraphs[1].Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    doc.Paragraphs[1].Range.ParagraphFormat.LeftIndent = 300;
                    doc.Paragraphs[1].Range.ParagraphFormat.LineSpacing = 11;
                    //doc.Paragraphs[1].Range.ParagraphFormat.LineUnitBefore = 0;
                    doc.Paragraphs[1].SpaceAfter = 0;
                    doc.Paragraphs[1].SpaceBefore = 0;
                    doc.Paragraphs[2].LineUnitAfter = 0;
                    doc.Paragraphs[2].LineUnitBefore = 0;
                    doc.Paragraphs[3].LineUnitAfter = 0;
                    doc.Paragraphs[3].LineUnitBefore = 0;
                    doc.Paragraphs[4].LineUnitAfter = 0;
                    doc.Paragraphs[4].LineUnitBefore = 0;
                    //doc.Paragraphs[1].LineSpacingRule = Word.WdLineSpacing.wdLineSpaceSingle;

                    doc.Paragraphs[2].Range.Text = "к Техническому заданию";
                    doc.Paragraphs[2].Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    doc.Paragraphs[2].Range.ParagraphFormat.LeftIndent = 300;

                    doc.Paragraphs[4].Range.Text = "ТРЕБОВАНИЯ К ТОВАРАМ, ИСПОЛЬЗУЕМЫМ ПРИ ВЫПОЛНЕНИИ РАБОТ";
                    doc.Paragraphs[4].Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;


                    //Добавляем таблицу (но перед этим узнаем сколько продуктов)
                    string productsMessage = "";

                    Object defaultTableBehavior =
                     Word.WdDefaultTableBehavior.wdWord9TableBehavior;
                    Object autoFitBehavior =
                     Word.WdAutoFitBehavior.wdAutoFitWindow;
                    Word.Table wordtable = doc.Tables.Add(doc.Paragraphs[5].Range, 3 + products.Count, 6,
                      ref defaultTableBehavior, ref autoFitBehavior);

                    // Объединение ячеек
                    object begCell = wordtable.Cell(1, 1).Range.Start;
                    object endCell = wordtable.Cell(2, 1).Range.End;
                    Word.Range wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Cells.Merge();

                    begCell = wordtable.Cell(1, 2).Range.Start;
                    endCell = wordtable.Cell(1, 3).Range.End;
                    wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Cells.Merge();

                    begCell = wordtable.Cell(1, 3).Range.Start;
                    endCell = wordtable.Cell(1, 4).Range.End;
                    wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Cells.Merge();

                    begCell = wordtable.Cell(1, 4).Range.Start;
                    endCell = wordtable.Cell(2, 6).Range.End;
                    wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Cells.Merge();


                    // Окраска строки с номерами
                    begCell = wordtable.Cell(3, 1).Range.Start;
                    endCell = wordtable.Cell(3, 6).Range.End;
                    wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray10;

                    // Заполнение заголовка
                    doc.Tables[1].Cell(1, 1).Range.Text = "№ п/п";
                    doc.Tables[1].Cell(1, 1).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 2).Range.Text = "Требования, установленные заказчиком";
                    doc.Tables[1].Cell(1, 2).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 3).Range.Text = "Значение, предлагаемое участником";
                    doc.Tables[1].Cell(1, 3).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 4).Range.Text = "Сведения о сертификации";
                    doc.Tables[1].Cell(1, 4).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    doc.Tables[1].Cell(2, 2).Range.Text = "Наименование применяемых товаров (материалов)";
                    doc.Tables[1].Cell(2, 2).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(2, 3).Range.Text = "Требуемый параметр и требуемое значение";
                    doc.Tables[1].Cell(2, 3).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(2, 4).Range.Text = "Требуемый параметр и требуемое значение";
                    doc.Tables[1].Cell(2, 4).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(2, 5).Range.Text = "Указание на товарный знак, фирменное наименование, патенты, полезные модели, промышленные образцы, наименование места происхождения товара или наименование производителя товар";
                    doc.Tables[1].Cell(2, 5).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    doc.Tables[1].Cell(3, 1).Range.Text = "1";
                    doc.Tables[1].Cell(3, 1).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(3, 2).Range.Text = "2";
                    doc.Tables[1].Cell(3, 2).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(3, 3).Range.Text = "3";
                    doc.Tables[1].Cell(3, 3).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(3, 4).Range.Text = "4";
                    doc.Tables[1].Cell(3, 4).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(3, 5).Range.Text = "5";
                    doc.Tables[1].Cell(3, 5).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(3, 6).Range.Text = "6";
                    doc.Tables[1].Cell(3, 6).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    // Заполняем продукты
                    for (int i = 0; i < products.Count; i++)
                    {
                        if (!isWork) break;
                        doc.Tables[1].Cell(i + 4, 1).Range.Text = Convert.ToString(i + 1) + '.';
                        doc.Tables[1].Cell(i + 4, 1).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        doc.Tables[1].Cell(i + 4, 2).Range.Text = products.ElementAt(i).Name;
                        doc.Tables[1].Cell(i + 4, 2).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        doc.Tables[1].Cell(i + 4, 5).Range.Text = products.ElementAt(i).TradeMark;
                        doc.Tables[1].Cell(i + 4, 5).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                       // В данном шаблоне одно свойство(строка) у продукта
                        if (products[0].FreedomProperties.Count() != 1)
                        {
                            message = "Ошибка при составлении шаблона";
                            return ResultType_enum.Error;
                        }
                        FreedomProperty freedomProperty = products[i].FreedomProperties.ElementAt(0);

                        // Требования заказчика
                        doc.Tables[1].Cell(i + 4, 3).Range.Text = freedomProperty.CustomerParam;
                        doc.Tables[1].Cell(i + 4, 3).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        // Требования участника
                        doc.Tables[1].Cell(i + 4, 4).Range.Text = freedomProperty.MemberParam;
                        doc.Tables[1].Cell(i + 4, 4).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        // Сертификация
                        doc.Tables[1].Cell(i + 4, 6).Range.Text = products[i].Certification;
                        doc.Tables[1].Cell(i + 4, 6).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                    }

                    application.Visible = true;
                }

                catch (Exception er)
                {
                    if (doc != null)
                        doc.Close(ref falseObj, ref missingObj, ref missingObj);
                    doc = null;
                }


                return ResultType_enum.Done;
            }
            catch (Exception ex)
            {
                doc = null;
                message = ex.Message + '\n' + ex.StackTrace;
                return ResultType_enum.Error;
            }
            finally
            {
                isWork = false;
            }
        }

        public void Stop()
        {
            isWork = false;
        }
    }
}
