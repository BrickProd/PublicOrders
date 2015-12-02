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
        object falseValue = false;
        object trueValue = true;
        object missing = Type.Missing;

        private bool isWork = false;
        Object missingObj = System.Reflection.Missing.Value;

        private MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;


        private void SaveProduct(DocumentDbContext ddc, Product product, Rubric rubric, ref int productsAddedCount, ref int productsRepeatCount, ref int productsMergeCount)
        {
            if (product != null)
            {
                product.ModifiedDateTime = DateTime.Now;
                product.Rubric = ddc.Rubrics.Find(rubric.RubricId);
                // Проверка на повтор
                // Если совпали название и товарный знак и значения по всем атрибутам, то это ПОВТОР
                // Если совпали название и товарный знак и значений по данному шаблону нет (или пусты), то это СЛИЯНИЕ
                // Если совпали название и товарный знак и значения НЕ совпали, то это НОВЫЙ ТОВАР
                bool isRepeat = false;
                IEnumerable<Product> repeatProducts = ddc.Products.Where(m => (m.Name == product.Name && 
                                                                               m.TradeMark == product.TradeMark &&
                                                                               m.Rubric.Name.Trim().ToLower() == product.Rubric.Name.Trim().ToLower())).ToList();
                if (repeatProducts.Any())
                {
                    // Изначально проверим на повтор
                    foreach (Product repeatProduct in repeatProducts)
                    {
                        isRepeat = true;
                        if (repeatProduct.FreedomProperties.Count() > 0)
                        {
                            foreach (FreedomProperty repeatFreedomProperty in repeatProduct.FreedomProperties)
                            {
                                FreedomProperty newFreedomProperty = product.FreedomProperties.FirstOrDefault(m => (m.CustomerParam == repeatFreedomProperty.CustomerParam &&
                                                                                                              m.MemberParam == repeatFreedomProperty.MemberParam));
                                if (newFreedomProperty != null)
                                {
                                    isRepeat = true;
                                }
                                else
                                {
                                    isRepeat = false;
                                }
                                if (!isRepeat)
                                {
                                    break;
                                }
                            }
                            if (isRepeat)
                            {
                                break;
                            }
                        }
                        else
                        {
                            isRepeat = false;
                        }
                    }
                }
                if (isRepeat)
                {
                    productsRepeatCount++;
                }
                else
                {
                    // Слияние делается в том случае, если найден один повторный документ и нет значений по шпблону
                    if ((repeatProducts.Count() == 1) && ((repeatProducts.ElementAt(0).FreedomProperties == null) ||
                            (repeatProducts.ElementAt(0).FreedomProperties.Count() == 0)))
                    {
                        productsMergeCount++;
                        repeatProducts.ElementAt(0).FreedomProperties = product.FreedomProperties;
                        //repeatProducts.ElementAt(0).Certification = product.Certification;
                        ddc.Entry(repeatProducts.ElementAt(0)).State = System.Data.Entity.EntityState.Modified;
                        ddc.SaveChanges();
                    }

                    else
                    {
                        ddc.Products.Add(product);

                        ddc.SaveChanges();
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            mvm.ProductCollection.Add(product);
                        }));

                        productsAddedCount++;
                    }
                }
            }
        }

        private void SaveProperty(Product product, FreedomProperty freedomProperty)
        {
            if (freedomProperty != null)
            {
                if ((freedomProperty.CustomerParam != "") ||
                    (freedomProperty.MemberParam != ""))
                {
                    product.FreedomProperties.Add(freedomProperty);
                }

            }
        }

        public ResultType_enum Learn(string docPath, Rubric rubric, out int productsAddedCount, out int productsRepeatCount, out int productsMergeCount, out string message)
        {
            productsAddedCount = 0;
            productsRepeatCount = 0;
            productsMergeCount = 0;
            Word.Application application = null;
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
                application = new Microsoft.Office.Interop.Word.Application();

                //Загружаем документ
                Microsoft.Office.Interop.Word.Document doc = null;

                object fileName = docPath;
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


                // Заполняем товар
                // Новый обход документа
                Product product = null;
                FreedomProperty freedomProperty = null;
                DocumentDbContext ddc = new DocumentDbContext();

                Word.Cell cell = tbl.Cell(4, 1);
                int rowIndex = 4;
                while (cell != null)
                {
                    try
                    {
                        string cellValue = cell.Range.Text.Trim();
                        if (rowIndex != cell.RowIndex)
                        {
                            SaveProperty(product, freedomProperty);
                            rowIndex = cell.RowIndex;
                            freedomProperty = new FreedomProperty();
                        }
                        else
                        {
                            if (freedomProperty == null) freedomProperty = new FreedomProperty();
                        }

                        switch (cell.ColumnIndex)
                        {
                            case (2):
                                // Название (--ПЕРВОЕ ЗНАЧЕНИЕ--)
                                if (Globals.CleanWordCell(cellValue) == "") break;
                                if (product != null)
                                {
                                    SaveProduct(ddc, product, rubric, ref productsAddedCount, ref productsRepeatCount, ref productsMergeCount);
                                }

                                product = new Product();
                                product.Name = Globals.DeleteNandSpaces(Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue)));

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
                                product.Certification = Globals.DeleteNandSpaces(Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue)));
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        break;
                    }
                    finally
                    {
                        cell = cell.Next;
                    }
                }
                SaveProperty(product, freedomProperty);
                SaveProduct(ddc, product, rubric, ref productsAddedCount, ref productsRepeatCount, ref productsMergeCount);



                ddc.Dispose();

                return ResultType_enum.Done;
            }
            catch (Exception ex)
            {
                message = ex.Message + '\n' + ex.StackTrace;
                return ResultType_enum.Error;
            }
            finally
            {
                if (application != null)
                {
                    // Закрываем приложение
                    application.Quit(ref missing, ref missing, ref missing);
                    application = null;
                }
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

                    string dataString = "";
                    // Заголовок
                    dataString += "№ п/п\t";
                    dataString += "Требования, установленные заказчиком\t";
                    dataString += "\t";
                    dataString += "Значение, предлагаемое участником\t";
                    dataString += "\t";
                    dataString += "Сведения о сертификации\n";
                    dataString += "\t";
                    dataString += "Наименование товара\t";
                    dataString += "Требуемый параметр и требуемое значение\t";
                    dataString += "Требуемый параметр и требуемое значение\t";
                    dataString += "Указание на товарный знак (модель), производителя\t";
                    dataString += "\n";
                    dataString += "1\t";
                    dataString += "2\t";
                    dataString += "3\t";
                    dataString += "4\t";
                    dataString += "5\t";
                    dataString += "6\n";

                    // Строки
                    int a = 0;
                    foreach (Product product in products)
                    {
                        if (!isWork) break;
                        if ((product == null) ||
                            (product.FreedomProperties == null) ||
                            (product.FreedomProperties.Count == 0))
                            continue;

                        a++;
                        // <nnn> - новый параграф
                        // <rrr> - /r
                        // <nnn> - /n
                        dataString += a + ".\t";
                        dataString += product.Name.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                        dataString += product.FreedomProperties[0].CustomerParam.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                        dataString += product.FreedomProperties[0].MemberParam.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                        dataString += product.TradeMark.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                        dataString += product.Certification.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\n";
                    }



                    Word.Range wdRng = doc.Paragraphs[5].Range;
                    wdRng.Text = dataString;

                    object Separator = Word.WdTableFieldSeparator.wdSeparateByTabs;
                    object Format = Word.WdTableFormat.wdTableFormatSimple1;
                    object ApplyBorders = true;
                    //object AutoFit = true;

                    Object defaultTableBehavior =
                        Word.WdDefaultTableBehavior.wdWord8TableBehavior;
                    Object autoFitBehavior =
                        Word.WdAutoFitBehavior.wdAutoFitFixed;

                    Word.Table myTable = wdRng.ConvertToTable(ref Separator,
                    Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                    ref ApplyBorders, Type.Missing, Type.Missing, Format,
                     Type.Missing, Type.Missing, Type.Missing,
                     Type.Missing, Type.Missing, ref autoFitBehavior,
                     defaultTableBehavior);

                    // Замена <nnn> на параграф
                    wdRng.Select();
                    Word.Find findObject = wdRng.Find;
                    findObject.ClearFormatting();
                    findObject.Text = "<nnn>";
                    //findObject.Replacement.ClearFormatting();
                    findObject.Replacement.Text = "^p";//strB.Append("<w:br/>");

                    object replaceAll = Word.WdReplace.wdReplaceAll;
                    findObject.Execute(ref missing, ref missing, ref missing, ref missing, ref missing,
                        ref missing, ref missing, ref missing, ref missing, ref missing,
                        ref replaceAll, ref missing, ref missing, ref missing, ref missing);

                    // Выравнивание
                    myTable.Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                    myTable.Rows[1].Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    myTable.Rows[2].Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    myTable.Rows[3].Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    // Стиль таблицы
                    object styleTypeTable = Word.WdStyleType.wdStyleTypeTable;
                    Word.Style styl = doc.Styles.Add
                         ("New Table Style", ref styleTypeTable);

                    styl.Font.Name = "Arial";
                    styl.Font.Size = 11;
                    Word.TableStyle stylTbl = styl.Table;
                    stylTbl.Borders.Enable = 1;


                    object objStyle = styl;
                    myTable.Range.set_Style(ref objStyle);

                    // Окраска строки
                    object begCell = myTable.Cell(3, 1).Range.Start;
                    object endCell = myTable.Cell(3, 6).Range.End;
                    Word.Range wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray10;

                    // Объединение ячеек заголовка
                    myTable.Cell(1, 1).Merge(myTable.Cell(2, 1));
                    myTable.Cell(1, 6).Merge(myTable.Cell(2, 6));
                    myTable.Cell(1, 2).Merge(myTable.Cell(1, 3));
                    myTable.Cell(1, 3).Merge(myTable.Cell(1, 4));

                    application.Visible = true;

                    // Инициализируем количество заполняемого товара
                    /*int linesCount = 0;
                    foreach (Product product in products ) {
                        if ((product == null) || (product.FreedomProperties == null) || (product.FreedomProperties.Count != 1)) continue;
                        linesCount++;
                    }


                    //Добавляем таблицу (но перед этим узнаем сколько товара)
                    string productsMessage = "";

                    Object defaultTableBehavior =
                     Word.WdDefaultTableBehavior.wdWord9TableBehavior;
                    Object autoFitBehavior =
                     Word.WdAutoFitBehavior.wdAutoFitWindow;
                    Word.Table wordtable = doc.Tables.Add(doc.Paragraphs[5].Range, 3 + linesCount, 6,
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

                    // Заполняем товар
                    for (int i = 0; i < products.Count; i++)
                    {
                        if (!isWork) break;
                        if ((products[i] == null) ||
                            (products[i].FreedomProperties == null) ||
                            (products[i].FreedomProperties.Count != 1)) continue;

                        doc.Tables[1].Cell(i + 4, 1).Range.Text = Convert.ToString(i + 1) + '.';
                        doc.Tables[1].Cell(i + 4, 1).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        doc.Tables[1].Cell(i + 4, 2).Range.Text = products.ElementAt(i).Name;
                        doc.Tables[1].Cell(i + 4, 2).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        doc.Tables[1].Cell(i + 4, 5).Range.Text = products.ElementAt(i).TradeMark;
                        doc.Tables[1].Cell(i + 4, 5).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

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

                    application.Visible = true;*/
                }

                catch (Exception er)
                {
                    if (doc != null)
                        doc.Close(ref falseObj, ref missingObj, ref missingObj);
                    doc = null;
                    application.Quit();
                    application = null;
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
