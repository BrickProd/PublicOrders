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
    class CommitteeProcessor
    {
        private bool isWork = false;
        Object missingObj = System.Reflection.Missing.Value;

        private MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        private void SaveProduct(Product product, Rubric rubric, ref int productsAddedCount, ref int productsRepeatCount, ref int productsMergeCount)
        {
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
                        if (repeatProduct.CommitteeProperties.Count() > 0)
                        {
                            foreach (CommitteeProperty repeatCommitteeProperty in repeatProduct.CommitteeProperties)
                            {
                                CommitteeProperty newCommitteeProperty = product.CommitteeProperties.FirstOrDefault(m => (m.MaxValue == repeatCommitteeProperty.MaxValue &&
                                                                                                              m.MinValue == repeatCommitteeProperty.MinValue &&
                                                                                                              m.ParamName == repeatCommitteeProperty.ParamName &&
                                                                                                              m.SpecificParam == repeatCommitteeProperty.SpecificParam &&
                                                                                                              m.VariableParam == repeatCommitteeProperty.VariableParam &&
                                                                                                              m.Measure == repeatCommitteeProperty.Measure));
                                if (newCommitteeProperty != null)
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
                    if ((repeatProducts.Count() == 1) && ((repeatProducts.ElementAt(0).CommitteeProperties == null) ||
                            (repeatProducts.ElementAt(0).CommitteeProperties.Count() == 0)))
                    {

                        repeatProducts.ElementAt(0).CommitteeProperties = product.CommitteeProperties;
                        //repeatProducts.ElementAt(0).Certification = product.Certification;
                        mvm.dc.Entry(repeatProducts.ElementAt(0)).State = System.Data.Entity.EntityState.Modified;
                        mvm.dc.SaveChanges();
                        productsMergeCount++;
                    }

                    else
                    {
                        product.Rubric = rubric;
                        mvm.dc.Products.Add(product);

                        mvm.dc.SaveChanges();
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            mvm.ProductCollection.Add(product);
                        }));

                        productsAddedCount++;
                    }
                }
            }
        }

        private void SaveProperty(Product product, CommitteeProperty committeeProperty)
        {
            if (committeeProperty != null)
            {
                if ((committeeProperty.MaxValue != "") ||
                    (committeeProperty.MinValue != "") ||
                    (committeeProperty.ParamName != "") ||
                    (committeeProperty.SpecificParam != "") ||
                    (committeeProperty.VariableParam != "") ||
                    (committeeProperty.Measure != ""))
                {
                    product.CommitteeProperties.Add(committeeProperty);
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
                if (tbl.Columns.Count != 9)
                {
                    message = "Количество столбцов таблицы не совпадает со спецификацией";
                    return ResultType_enum.Error;
                }

                // Заполняем
                var r = mvm.dc.Rubrics.FirstOrDefault(m => m.Name.ToLower() == "--без рубрики--");

                // Новый обход документа
                Product product = null;
                CommitteeProperty committeeProperty = null;

                Word.Cell cell = tbl.Cell(2, 1);
                int rowIndex = 2;
                while (cell != null)
                {
                    try
                    {
                        string cellValue = cell.Range.Text.Trim();
                        if (rowIndex != cell.RowIndex)
                        {
                            SaveProperty(product, committeeProperty);
                            rowIndex = cell.RowIndex;
                            committeeProperty = new CommitteeProperty();
                        }
                        else
                        {
                            if (committeeProperty == null) committeeProperty = new CommitteeProperty();
                        }

                        switch (cell.ColumnIndex)
                        {
                            case (2):
                                // Название (--ПЕРВОЕ ЗНАЧЕНИЕ--)
                                if (product != null)
                                {
                                    SaveProduct(product, r, ref productsAddedCount, ref productsRepeatCount, ref productsMergeCount);
                                }

                                product = new Product();
                                product.Name = Globals.DeleteNandSpaces(Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue)));

                                break;
                            case (3):
                                // Значение параметра
                                committeeProperty.ParamName = Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue));
                                break;
                            case (9):
                                // Торговая марка
                                if (product == null) break;
                                product.TradeMark = Globals.DeleteNandSpaces(Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue)));
                                break;
                            case (4):
                                // Минимальное значение
                                committeeProperty.MinValue = Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue));
                                break;
                            case (5):
                                // Максимальное значение
                                committeeProperty.MaxValue = Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue));
                                break;
                            case (6):
                                // Значение, которые не могут изменяться
                                committeeProperty.VariableParam = Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue));
                                break;
                            case (7):
                                // Конкретные показатели
                                committeeProperty.SpecificParam = Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue));
                                break;
                            case (8):
                                // Единица измерения
                                committeeProperty.Measure = Globals.ConvertTextExtent(Globals.CleanWordCell(cellValue));
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
                SaveProperty(product, committeeProperty);
                SaveProduct(product, r, ref productsAddedCount, ref productsRepeatCount, ref productsMergeCount);

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


                    doc.Paragraphs[1].Range.Text = "Приложение № 3";
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

                    doc.Paragraphs[2].Range.Text = "к Техническому заданию";
                    doc.Paragraphs[2].Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;
                    doc.Paragraphs[2].Range.ParagraphFormat.LeftIndent = 300;

                    doc.Paragraphs[4].Range.Text = "СВЕДЕНИЯ О КАЧЕСТВЕ, ТЕХНИЧЕСКИХ ХАРАКТЕРИСТИКАХ ТОВАРА, ЕГО БЕЗОПАСНОСТИ, ФУНКЦИОНАЛЬНЫХ ХАРАКТЕРИСТИКАХ (ПОТРЕБИТЕЛЬСКИХ СВОЙСТВАХ) ТОВАРА, РАЗМЕРЕ, УПАКОВКЕ, ОТГРУЗКЕ ТОВАРА И ИНЫЕ СВЕДЕНИЯ О ТОВАРЕ, ПРЕДСТАВЛЕНИЕ КОТОРЫХ ПРЕДУСМОТРЕНО ДОКУМЕНТАЦИЕЙ ОБ АУКЦИОНЕ В ЭЛЕКТРОННОЙ ФОРМЕ";
                    doc.Paragraphs[4].Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;


                    // Подсчитываем количество строк у продуктов (потому что одно свойство продукта занимает одну строку)
                    int propertiesCount = 0;
                    foreach (Product product in products)
                    {
                        if (product.CommitteeProperties == null) continue;
                        propertiesCount += product.CommitteeProperties.Count();
                    }

                    Object defaultTableBehavior =
                     Word.WdDefaultTableBehavior.wdWord9TableBehavior;
                    Object autoFitBehavior =
                     Word.WdAutoFitBehavior.wdAutoFitWindow;
                    Word.Table wordtable = doc.Tables.Add(doc.Paragraphs[5].Range, 1 + propertiesCount, 9,
                      ref defaultTableBehavior, ref autoFitBehavior);

                    // Заполнение заголовка
                    doc.Tables[1].Cell(1, 1).Range.Text = "№ п/п";
                    doc.Tables[1].Cell(1, 1).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 2).Range.Text = "Наименование товара";
                    doc.Tables[1].Cell(1, 2).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 3).Range.Text = "Наименование показателя";
                    doc.Tables[1].Cell(1, 3).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 4).Range.Text = "Минимальные значения показателей";
                    doc.Tables[1].Cell(1, 4).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 5).Range.Text = "Максимальные значения показателей";
                    doc.Tables[1].Cell(1, 5).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 6).Range.Text = "Значения показателей, которые не могут изменяться";
                    doc.Tables[1].Cell(1, 6).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 7).Range.Text = "Конкретные показатели используемого товара, соответствующие значениям, установленным документацией, предлагаемые участником закупки";
                    doc.Tables[1].Cell(1, 7).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 8).Range.Text = "Единица измерения";
                    doc.Tables[1].Cell(1, 8).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    doc.Tables[1].Cell(1, 9).Range.Text = "Товарный знак";
                    doc.Tables[1].Cell(1, 9).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;


                    // Заполнение продуктами
                    int productIndexCompilator = 0;
                    int propertyIndexCompilator = 0;
                    for (int i = 0; i < propertiesCount; i++)
                    {
                        if (!isWork) break;
                        if ((products[i].CommitteeProperties == null) || (products[i].CommitteeProperties.Count == 0)) continue;

                        if (propertyIndexCompilator == 0)
                        {
                            // Объединяем ячейки по продукту
                            // Номер
                            object begCell = wordtable.Cell(i + 2, 1).Range.Start;
                            object endCell = wordtable.Cell(i + 2 + products[productIndexCompilator].CommitteeProperties.Count() - 1, 1).Range.End;
                            Word.Range wordcellrange = doc.Range(ref begCell, ref endCell);
                            wordcellrange.Select();
                            try
                            {
                                application.Selection.Cells.Merge();
                            }
                            catch
                            {

                            }

                            // Название продукта
                            begCell = wordtable.Cell(i + 2, 2).Range.Start;
                            endCell = wordtable.Cell(i + 2 + products[productIndexCompilator].CommitteeProperties.Count() - 1, 2).Range.End;
                            wordcellrange = doc.Range(ref begCell, ref endCell);
                            wordcellrange.Select();
                            try
                            {
                                application.Selection.Cells.Merge();
                            }
                            catch
                            {

                            }

                            // Товарный знак
                            begCell = wordtable.Cell(i + 2, 9).Range.Start;
                            endCell = wordtable.Cell(i + 2 + products[productIndexCompilator].CommitteeProperties.Count() - 1, 9).Range.End;
                            wordcellrange = doc.Range(ref begCell, ref endCell);
                            wordcellrange.Select();
                            try
                            {
                                application.Selection.Cells.Merge();
                            }
                            catch
                            {

                            }

                            doc.Tables[1].Cell(i + 2, 1).Range.Text = Convert.ToString(productIndexCompilator + 1) + '.';
                            doc.Tables[1].Cell(i + 2, 1).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                            doc.Tables[1].Cell(i + 2, 2).Range.Text = products.ElementAt(productIndexCompilator).Name;
                            doc.Tables[1].Cell(i + 2, 2).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                            doc.Tables[1].Cell(i + 2, 9).Range.Text = products.ElementAt(productIndexCompilator).TradeMark;
                            doc.Tables[1].Cell(i + 2, 9).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                        }

                        // Наименование показателя
                        doc.Tables[1].Cell(i + 2, 3).Range.Text = products[productIndexCompilator].CommitteeProperties.ElementAt(propertyIndexCompilator).ParamName;
                        doc.Tables[1].Cell(i + 2, 3).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        // Минимальные значения показателей
                        doc.Tables[1].Cell(i + 2, 4).Range.Text = products[productIndexCompilator].CommitteeProperties.ElementAt(propertyIndexCompilator).MinValue;
                        doc.Tables[1].Cell(i + 2, 4).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        // Максимальные значения показателей
                        doc.Tables[1].Cell(i + 2, 5).Range.Text = products[productIndexCompilator].CommitteeProperties.ElementAt(propertyIndexCompilator).MaxValue;
                        doc.Tables[1].Cell(i + 2, 5).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        // Значения показателей, которые не могут изменяться
                        doc.Tables[1].Cell(i + 2, 6).Range.Text = products[productIndexCompilator].CommitteeProperties.ElementAt(propertyIndexCompilator).VariableParam;
                        doc.Tables[1].Cell(i + 2, 6).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        // Конкретные показатели
                        doc.Tables[1].Cell(i + 2, 7).Range.Text = products[productIndexCompilator].CommitteeProperties.ElementAt(propertyIndexCompilator).SpecificParam;
                        doc.Tables[1].Cell(i + 2, 7).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        // Единица измерения
                        doc.Tables[1].Cell(i + 2, 8).Range.Text = products[productIndexCompilator].CommitteeProperties.ElementAt(propertyIndexCompilator).Measure;
                        doc.Tables[1].Cell(i + 2, 8).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        propertyIndexCompilator++;
                        if (products[productIndexCompilator].CommitteeProperties.Count() == propertyIndexCompilator)
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
