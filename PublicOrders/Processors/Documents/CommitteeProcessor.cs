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
                // Заполняем
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
                    if ((repeatProducts.Count() == 1) && ((repeatProducts.ElementAt(0).CommitteeProperties == null) ||
                            (repeatProducts.ElementAt(0).CommitteeProperties.Count() == 0)))
                    {

                        repeatProducts.ElementAt(0).CommitteeProperties = product.CommitteeProperties;
                        //repeatProducts.ElementAt(0).Certification = product.Certification;
                        ddc.Entry(repeatProducts.ElementAt(0)).State = System.Data.Entity.EntityState.Modified;
                        ddc.SaveChanges();
                        productsMergeCount++;
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
                if (tbl.Columns.Count != 9)
                {
                    message = "Количество столбцов таблицы не совпадает со спецификацией";
                    return ResultType_enum.Error;
                }

                // Новый обход документа
                DocumentDbContext ddc = new DocumentDbContext();
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
                                if (Globals.CleanWordCell(cellValue) == "") break;
                                if (product != null)
                                {
                                    SaveProduct(ddc, product, rubric, ref productsAddedCount, ref productsRepeatCount, ref productsMergeCount);
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
                SaveProduct(ddc, product, rubric, ref productsAddedCount, ref productsRepeatCount, ref productsMergeCount);

                ddc.Dispose();

                return ResultType_enum.Done;
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

                    string dataString = "";
                    // Заголовок
                    dataString += "№ п/п\t";
                    dataString += "Наименование товара\t";
                    dataString += "Наименование показателя\t";
                    dataString += "Минимальные значения показателей\t";
                    dataString += "Максимальные значения показателей\t";
                    dataString += "Значения показателей, которые не могут изменяться\t";
                    dataString += "Значение, предлагаемое участником\t";
                    dataString += "Единица измерения\t";
                    dataString += "Указание на товарный знак (модель), производителя\n";

                    // Строки
                    int a = 0;
                    foreach (Product product in products)
                    {
                        if (!isWork) break;
                        if ((product == null) ||
                            (product.CommitteeProperties == null) ||
                            (product.CommitteeProperties.Count == 0))
                            continue;

                        a++;
                        // <nnn> - новый параграф
                        // <rrr> - /r
                        // <nnn> - /n
                        dataString += a + ".\t";
                        dataString += product.Name.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                        // Атрибуты продукта
                        // Первый атрибут
                        dataString += product.CommitteeProperties[0].ParamName.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                        dataString += product.CommitteeProperties[0].MinValue.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                        dataString += product.CommitteeProperties[0].MaxValue.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                        dataString += product.CommitteeProperties[0].VariableParam.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                        dataString += product.CommitteeProperties[0].SpecificParam.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                        dataString += product.CommitteeProperties[0].Measure.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";

                        // Товарный знак
                        dataString += product.TradeMark.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\n";
                        // Остальные атрибуты
                        int b = 0;
                        for (int i = 1; i < product.CommitteeProperties.Count; i++)
                        {
                            dataString += "\t";
                            dataString += "\t";
                            dataString += product.CommitteeProperties[i].ParamName.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                            dataString += product.CommitteeProperties[i].MinValue.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                            dataString += product.CommitteeProperties[i].MaxValue.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                            dataString += product.CommitteeProperties[i].VariableParam.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                            dataString += product.CommitteeProperties[i].SpecificParam.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
                            dataString += product.CommitteeProperties[i].Measure.Replace("\r\n", "<nnn>").Replace("\t", " ").Replace("\n", " ").Replace("\r", " ") + "\t";
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

                    // Объединение ячеек по продуктам
                    int beginRowNum = 2;
                    foreach (Product product in products)
                    {
                        if (!isWork) break;
                        if ((product == null) ||
                            (product.CommitteeProperties == null) ||
                            (product.CommitteeProperties.Count == 0))
                            continue;

                        myTable.Cell(beginRowNum, 1).Merge(myTable.Cell(beginRowNum + product.CommitteeProperties.Count - 1, 1)); // Номер
                        myTable.Cell(beginRowNum, 2).Merge(myTable.Cell(beginRowNum + product.CommitteeProperties.Count - 1, 2)); // Наименование
                        myTable.Cell(beginRowNum, 9).Merge(myTable.Cell(beginRowNum + product.CommitteeProperties.Count - 1, 9)); // Товарный знак

                        beginRowNum = beginRowNum + product.CommitteeProperties.Count;
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
