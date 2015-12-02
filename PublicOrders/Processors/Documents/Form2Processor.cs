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
using System.Windows.Documents;

namespace PublicOrders.Processors
{
    class Form2Processor
    {
        object falseValue = false;
        object trueValue = true;
        object missing = Type.Missing;

        private bool isWork = false;
        Object missingObj = System.Reflection.Missing.Value;
        private MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        private void SaveProduct(DocumentDbContext ddc, Product product, Rubric rubric, ref int productsAddedCount, ref int productsRepeatCount, ref int productsMergeCount) {
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
                        if (repeatProduct.Form2Properties.Count() > 0)
                        {
                            foreach (Form2Property repeatForm2Property in repeatProduct.Form2Properties)
                            {
                                Form2Property newForm2Property = product.Form2Properties.FirstOrDefault(m => (m.RequiredParam == repeatForm2Property.RequiredParam &&
                                                                                                              m.RequiredValue == repeatForm2Property.RequiredValue &&
                                                                                                              m.OfferValue == repeatForm2Property.OfferValue &&
                                                                                                              m.Measure == repeatForm2Property.Measure));
                                if (newForm2Property != null)
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
                        else {
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
                    if ((repeatProducts.Count() == 1) && ((repeatProducts.ElementAt(0).Form2Properties == null) ||
                            (repeatProducts.ElementAt(0).Form2Properties.Count() == 0)))
                    {

                        repeatProducts.ElementAt(0).Form2Properties = product.Form2Properties;
                        //repeatProducts.ElementAt(0).Certification = product.Certification;
                        ddc.Entry(repeatProducts.ElementAt(0)).State = System.Data.Entity.EntityState.Modified;
                        ddc.SaveChanges();
                        productsMergeCount++;
                    }

                    else
                    {
                        try
                        {
                            ddc.Products.Add(product);
                            ddc.SaveChanges();

                            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            {
                                mvm.ProductCollection.Add(product);
                            }));

                            productsAddedCount++;
                        }
                        catch (Exception ex) {
                            string sss = "авыаыва";
                        }

                    }
                }
            }
        }

        private void SaveProperty(Product product, Form2Property form2Property ) {
            if (form2Property != null)
            {
                if ((form2Property.OfferValue != "") ||
                    (form2Property.RequiredParam != "") ||
                    (form2Property.RequiredValue != "") ||
                    (form2Property.Measure != ""))
                {
                    //form2Property.Product = product;
                    product.Form2Properties.Add(form2Property);

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
                if (tbl.Columns.Count != 8)
                {
                    message = "Количество столбцов таблицы не совпадает со спецификацией";
                    return ResultType_enum.Error;
                }

                // Заполняем
                // Новый обход документа
                Product product = null;
                Form2Property form2Property = null;
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
                            SaveProperty(product, form2Property);

                            rowIndex = cell.RowIndex;
                            form2Property = new Form2Property();
                        }
                        else {
                            if (form2Property == null) form2Property = new Form2Property();
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
                                //product.ModifiedDateTime = DateTime.Now;
                                //mvm.dc.Products.Add(product);
                                //mvm.dc.SaveChanges();

                                break;
                            case (3):
                                // Торговая марка
                                if (product == null) break;
                                product.TradeMark = Globals.DeleteNandSpaces(Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue)));
                                break;
                            case (4):
                                // Требуемый параметр
                                form2Property.RequiredParam = Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue));
                                break;
                            case (5):
                                // Требуемое значение
                                form2Property.RequiredValue = Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue));
                                break;
                            case (6):
                                // Значение, предлагаемое участником
                                form2Property.OfferValue = Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue));
                                break;
                            case (7):
                                // Единица измерения
                                form2Property.Measure = Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue));
                                break;
                            case (8):
                                // Сертификация (--ПОСЛЕДНЕЕ ЗНАЧЕНИЕ--)
                                if (product == null) break;
                                if ((product.Certification == null) || (product.Certification.Trim() == ""))
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
                SaveProperty(product, form2Property);
                SaveProduct(ddc, product, rubric, ref productsAddedCount, ref productsRepeatCount, ref productsMergeCount);


                ddc.Dispose();

                return ResultType_enum.Done;

                // Заносим товар в БД
                //return dbEngineDocs.SetProducts(DocTemplate.Template_2, products, out productAddedCount, out productRepeatCount, out message);
            }
            catch (Exception ex)
            {
                message = ex.Message + '\n' + ex.StackTrace;
                return ResultType_enum.Error;
            }
            finally {
                if (application != null) {
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

                // если вылетим не этом этапе, приложение останется открытым
                try
                {
                    doc = application.Documents.Add(ref missingObj, ref missingObj, ref missingObj, ref missingObj);
                    // Общий Стиль
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


                    doc.Paragraphs[1].Range.Text = "«Утверждаю»";
                    doc.Paragraphs[1].Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    doc.Paragraphs[1].Range.ParagraphFormat.LeftIndent = 300;
                    doc.Paragraphs[1].Range.ParagraphFormat.LineSpacing = 11;
                    doc.Paragraphs[1].SpaceAfter = 0;
                    doc.Paragraphs[1].SpaceBefore = 0;
                    doc.Paragraphs[2].LineUnitAfter = 0;
                    doc.Paragraphs[2].LineUnitBefore = 0;
                    doc.Paragraphs[3].LineUnitAfter = 0;
                    doc.Paragraphs[3].LineUnitBefore = 0;
                    doc.Paragraphs[4].LineUnitAfter = 0;
                    doc.Paragraphs[4].LineUnitBefore = 0;

                    doc.Paragraphs[2].Range.Text = "Руководитель";
                    doc.Paragraphs[2].Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    doc.Paragraphs[2].Range.ParagraphFormat.LeftIndent = 300;

                    doc.Paragraphs[4].Range.Text = "Форма 2. Сведения о качестве, технических характеристиках товара, его безопасности, функциональных характеристиках (потребительских свойствах) товара, размере и иные сведения о товаре, представление которых предусмотрено документацией об аукционе в электронной форме";
                    doc.Paragraphs[4].Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    string dataString = "";
                    // Заголовок
                    dataString += "№ п/п\t";
                    dataString += "Наименование товара\t";
                    dataString += "Указание на товарный знак (модель), производителя\t";
                    dataString += "Технические характеристики\t";
                    dataString += "\t";
                    dataString += "\t";
                    dataString += "Единица измерения\t";
                    dataString += "Сведения о сертификации\n";
                    dataString += "\t";
                    dataString += "\t";
                    dataString += "\t";
                    dataString += "Требуемый параметр\t";
                    dataString += "Требуемое значение\t";
                    dataString += "Значение, предлагаемое участником\t";
                    dataString += "\t";
                    dataString += "\n";
                    dataString += "1\t";
                    dataString += "2\t";
                    dataString += "3\t";
                    dataString += "4\t";
                    dataString += "5\t";
                    dataString += "6\t";
                    dataString += "7\t";
                    dataString += "8\n";

                    // Строки
                    int a = 0;
                    foreach (Product product in products) {
                        if (!isWork) break;
                        if ((product == null) ||
                            (product.Form2Properties == null) ||
                            (product.Form2Properties.Count == 0))
                            continue;

                        a++;
                        // <nnn> - новый параграф
                        // <rrr> - /r
                        // <nnn> - /n
                        dataString += a + ".\t";
                        dataString += product.Name.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                        dataString += product.TradeMark.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                        // Атрибуты продукта
                        // Первый атрибут
                        dataString += product.Form2Properties[0].RequiredParam.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                        dataString += product.Form2Properties[0].RequiredValue.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                        dataString += product.Form2Properties[0].OfferValue.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                        dataString += product.Form2Properties[0].Measure.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";

                        // Сертификация (конец строки)
                        dataString += product.Certification + "\n";
                        // Остальные атрибуты
                        int b = 0;
                        for (int i = 1; i < product.Form2Properties.Count; i++) {
                            dataString += "\t";
                            dataString += "\t";
                            dataString += "\t";
                            dataString += product.Form2Properties[i].RequiredParam.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                            dataString += product.Form2Properties[i].RequiredValue.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                            dataString += product.Form2Properties[i].OfferValue.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                            dataString += product.Form2Properties[i].Measure.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                            dataString += "\n";
                            b++;
                        }
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

                    // Объединение ячеек заголовка
                    myTable.Cell(1, 1).Merge(myTable.Cell(2, 1));
                    myTable.Cell(1, 2).Merge(myTable.Cell(2, 2));
                    myTable.Cell(1, 3).Merge(myTable.Cell(2, 3));
                    myTable.Cell(1, 4).Merge(myTable.Cell(1, 6));
                    myTable.Cell(1, 5).Merge(myTable.Cell(2, 7));
                    myTable.Cell(1, 6).Merge(myTable.Cell(2, 8));

                    // Окраска строки
                    object begCell = myTable.Cell(3, 1).Range.Start;
                    object endCell = myTable.Cell(3, 8).Range.End;
                    Word.Range wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray10;

                    // Объединение ячеек по продуктам
                    int beginRowNum = 4;
                    foreach (Product product in products)
                    {
                        if (!isWork) break;
                        if ((product == null) ||
                            (product.Form2Properties == null) ||
                            (product.Form2Properties.Count == 0))
                            continue;

                        myTable.Cell(beginRowNum, 1).Merge(myTable.Cell(beginRowNum + product.Form2Properties.Count - 1, 1)); // Номер
                        myTable.Cell(beginRowNum, 2).Merge(myTable.Cell(beginRowNum + product.Form2Properties.Count - 1, 2)); // Наименование
                        myTable.Cell(beginRowNum, 3).Merge(myTable.Cell(beginRowNum + product.Form2Properties.Count - 1, 3)); // Товарный знак
                        myTable.Cell(beginRowNum, 8).Merge(myTable.Cell(beginRowNum + product.Form2Properties.Count - 1, 8)); // Сертификация

                        beginRowNum = beginRowNum + product.Form2Properties.Count;
                    }

                    application.Visible = true;
                }
                catch (Exception er)
                {
                    if (doc != null)
                        doc.Close(ref falseObj, ref missingObj, ref missingObj);
                    doc = null;
                    application.Quit();
                    application = null;
                }

                application.Visible = true;
                return ResultType_enum.Done;
            }
            catch (Exception ex)
            {
                doc = null;
                message = ex.Message + '\n' + ex.StackTrace;
                return ResultType_enum.Error;
            }
            finally {
                isWork = false;
            }
        }

        public void Stop()
        {
            isWork = false;
        }
    }
}
