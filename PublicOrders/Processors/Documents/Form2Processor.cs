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
    class Form2Processor
    {
        private bool isWork = false;
        Object missingObj = System.Reflection.Missing.Value;
        private MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        private void SaveProduct(Product product, Rubric rubric, ref int productsAddedCount, ref int productsRepeatCount, ref int productsMergeCount) {
            if (product != null)
            {
                product.ModifiedDateTime = DateTime.Now;
                // Проверка на повтор
                // Если совпали название и товарный знак и значения по всем атрибутам, то это ПОВТОР
                // Если совпали название и товарный знак и значений по данному шаблону нет (или пусты), то это СЛИЯНИЕ
                // Если совпали название и товарный знак и значения НЕ совпали, то это НОВЫЙ ПРОДУКТ
                bool isRepeat = false;
                IEnumerable<Product> repeatProducts = mvm.dc.Products.Where(m => (m.Name == product.Name && m.TradeMark == product.TradeMark /*&&
                                                                                 (m.Certification == product.Certification || m.Certification == null || m.Certification == "")*/)).ToList();
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
                        mvm.dc.Entry(repeatProducts.ElementAt(0)).State = System.Data.Entity.EntityState.Modified;
                        mvm.dc.SaveChanges();
                        productsMergeCount++;
                    }

                    else
                    {
                        try
                        {
                            product.Rubric = rubric;

                            /*foreach (Form2Property fp in product.Form2Properties) {
                                mvm.dc.Form2Properties.Add(fp);
                                mvm.dc.SaveChanges();
                            }*/

                            //mvm.dc.Entry(product).State = System.Data.Entity.EntityState.Modified;
                            //mvm.dc.SaveChanges();

                            mvm.dc.Products.Add(product);
                            mvm.dc.SaveChanges();

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
                if (tbl.Columns.Count != 8)
                {
                    message = "Количество столбцов таблицы не совпадает со спецификацией";
                    return ResultType_enum.Error;
                }

                // Заполняем
                var r = mvm.dc.Rubrics.FirstOrDefault(m => m.Name.ToLower() == "--без рубрики--");

                // Новый обход документа
                Product product = null;
                Form2Property form2Property = null;

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
                                    SaveProduct(product, r, ref productsAddedCount, ref productsRepeatCount, ref productsMergeCount);
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
                SaveProduct(product, r, ref productsAddedCount, ref productsRepeatCount, ref productsMergeCount);


                // Закрываем приложение
                application.Quit(ref missing, ref missing, ref missing);
                application = null;

                return ResultType_enum.Done;

                // Заносим продукты в БД
                //return dbEngineDocs.SetProducts(DocTemplate.Template_2, products, out productAddedCount, out productRepeatCount, out message);
            }
            catch (Exception ex)
            {
                message = ex.Message + '\n' + ex.StackTrace;
                return ResultType_enum.Error;
            }
            finally {
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

                    // Подсчитываем количество строк у продуктов (потому что одно свойство продукта занимает одну строку)
                    int propertiesCount = 0;
                    foreach (Product product in products)
                    {
                        if (product.Form2Properties == null) continue;
                        propertiesCount += product.Form2Properties.Count();
                    }

                    Object defaultTableBehavior =
                     Word.WdDefaultTableBehavior.wdWord9TableBehavior;
                    Object autoFitBehavior =
                     Word.WdAutoFitBehavior.wdAutoFitWindow;
                    Word.Table wordtable = doc.Tables.Add(doc.Paragraphs[5].Range, 3 + propertiesCount, 8,
                      ref defaultTableBehavior, ref autoFitBehavior);

                    // Объединение ячеек
                    object begCell = wordtable.Cell(1, 1).Range.Start;
                    object endCell = wordtable.Cell(2, 1).Range.End;
                    Word.Range wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Cells.Merge();

                    begCell = wordtable.Cell(1, 2).Range.Start;
                    endCell = wordtable.Cell(2, 2).Range.End;
                    wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Cells.Merge();

                    begCell = wordtable.Cell(1, 3).Range.Start;
                    endCell = wordtable.Cell(2, 3).Range.End;
                    wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Cells.Merge();

                    begCell = wordtable.Cell(1, 4).Range.Start;
                    endCell = wordtable.Cell(1, 6).Range.End;
                    wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Cells.Merge();

                    begCell = wordtable.Cell(1, 5).Range.Start;
                    endCell = wordtable.Cell(2, 7).Range.End;
                    wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Cells.Merge();

                    begCell = wordtable.Cell(1, 6).Range.Start;
                    endCell = wordtable.Cell(2, 8).Range.End;
                    wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Cells.Merge();

                    // Окраска строки с номерами
                    begCell = wordtable.Cell(3, 1).Range.Start;
                    endCell = wordtable.Cell(3, 8).Range.End;
                    wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray10;

                    // Заполнение заголовка
                    doc.Tables[1].Cell(1, 1).Range.Text = "№ п/п";
                    doc.Tables[1].Cell(1, 1).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 2).Range.Text = "Наименование материала";
                    doc.Tables[1].Cell(1, 2).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 3).Range.Text = "Указание на товарный знак (его словесное обозначение) (при наличии), знак обслуживания (при наличии), фирменное наименование (при наличии), патенты (при наличии), полезные модели (при наличии), промышленные образцы (при наличии), наименование страны происхождения товара";
                    doc.Tables[1].Cell(1, 3).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 4).Range.Text = "Технические характеристики";
                    doc.Tables[1].Cell(1, 4).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    doc.Tables[1].Cell(2, 4).Range.Text = "Требуемый параметр";
                    doc.Tables[1].Cell(2, 4).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(2, 5).Range.Text = "Требуемое значение";
                    doc.Tables[1].Cell(2, 5).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(2, 6).Range.Text = "Значение, предлагаемое участником";
                    doc.Tables[1].Cell(2, 6).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    doc.Tables[1].Cell(1, 5).Range.Text = "Единица измерения";
                    doc.Tables[1].Cell(1, 5).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 6).Range.Text = "Сведения о сертификации";
                    doc.Tables[1].Cell(1, 6).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

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
                    doc.Tables[1].Cell(3, 7).Range.Text = "7";
                    doc.Tables[1].Cell(3, 7).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(3, 8).Range.Text = "8";
                    doc.Tables[1].Cell(3, 8).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;



                    // Заполняем продукты
                    int productIndexCompilator = 0;
                    int propertyIndexCompilator = 0;
                    for (int i = 0; i < propertiesCount; i++)
                    {
                        if (!isWork) break;
                        if ((products[i].Form2Properties == null) || (products[i].Form2Properties.Count == 0)) continue;

                        if (propertyIndexCompilator == 0)
                        {
                            // Объединяем ячейки по продукту (т.к. свойство занимает строку)
                            begCell = wordtable.Cell(i + 4, 1).Range.Start;
                            endCell = wordtable.Cell(i + 4 + products[productIndexCompilator].Form2Properties.Count() - 1, 1).Range.End;
                            wordcellrange = doc.Range(ref begCell, ref endCell);
                            wordcellrange.Select();
                            try
                            {
                                application.Selection.Cells.Merge();
                            }
                            catch
                            {

                            }

                            begCell = wordtable.Cell(i + 4, 2).Range.Start;
                            endCell = wordtable.Cell(i + 4 + products[productIndexCompilator].Form2Properties.Count() - 1, 2).Range.End;
                            wordcellrange = doc.Range(ref begCell, ref endCell);
                            wordcellrange.Select();
                            try
                            {
                                application.Selection.Cells.Merge();
                            }
                            catch
                            {

                            }

                            begCell = wordtable.Cell(i + 4, 3).Range.Start;
                            endCell = wordtable.Cell(i + 4 + products[productIndexCompilator].Form2Properties.Count() - 1, 3).Range.End;
                            wordcellrange = doc.Range(ref begCell, ref endCell);
                            wordcellrange.Select();
                            try
                            {
                                application.Selection.Cells.Merge();
                            }
                            catch
                            {

                            }

                            begCell = wordtable.Cell(i + 4, 8).Range.Start;
                            endCell = wordtable.Cell(i + 4 + products[productIndexCompilator].Form2Properties.Count() - 1, 8).Range.End;
                            wordcellrange = doc.Range(ref begCell, ref endCell);
                            wordcellrange.Select();
                            try
                            {
                                application.Selection.Cells.Merge();
                            }
                            catch
                            {

                            }

                            doc.Tables[1].Cell(i + 4, 1).Range.Text = Convert.ToString(productIndexCompilator + 1) + '.';
                            doc.Tables[1].Cell(i + 4, 1).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                            doc.Tables[1].Cell(i + 4, 2).Range.Text = products.ElementAt(productIndexCompilator).Name;
                            doc.Tables[1].Cell(i + 4, 2).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                            doc.Tables[1].Cell(i + 4, 3).Range.Text = products.ElementAt(productIndexCompilator).TradeMark;
                            doc.Tables[1].Cell(i + 4, 3).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                            doc.Tables[1].Cell(i + 4, 8).Range.Text = products.ElementAt(productIndexCompilator).Certification;
                            doc.Tables[1].Cell(i + 4, 8).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                        }

                        // Требуемый параметр
                        doc.Tables[1].Cell(i + 4, 4).Range.Text = products[productIndexCompilator].Form2Properties.ElementAt(propertyIndexCompilator).RequiredParam;
                        doc.Tables[1].Cell(i + 4, 4).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        // Требуемое значение
                        doc.Tables[1].Cell(i + 4, 5).Range.Text = products[productIndexCompilator].Form2Properties.ElementAt(propertyIndexCompilator).RequiredValue;
                        doc.Tables[1].Cell(i + 4, 5).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        // Значение, предлагаемое участником
                        doc.Tables[1].Cell(i + 4, 6).Range.Text = products[productIndexCompilator].Form2Properties.ElementAt(propertyIndexCompilator).OfferValue;
                        doc.Tables[1].Cell(i + 4, 6).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        // Единица измерения
                        doc.Tables[1].Cell(i + 4, 7).Range.Text = products[productIndexCompilator].Form2Properties.ElementAt(propertyIndexCompilator).Measure;
                        doc.Tables[1].Cell(i + 4, 7).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        propertyIndexCompilator++;
                        if (products[productIndexCompilator].Form2Properties.Count() == propertyIndexCompilator)
                        {
                            propertyIndexCompilator = 0;
                            productIndexCompilator++;
                        }

                    }
                }

                catch (Exception er)
                {
                    if (doc != null)
                        doc.Close(ref falseObj, ref missingObj, ref missingObj);
                    doc = null;
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
