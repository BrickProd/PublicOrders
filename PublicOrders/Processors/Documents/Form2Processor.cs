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

        public ResultType_enum Learn(string docPath, out int productAddedCount, out int productRepeatCount, out string message)
        {
            productAddedCount = 0;
            productRepeatCount = 0;
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

                // Заполняем продукты
                Product product = null;
                Property property = null;

                bool isNewProduct = false;
                string productName = "";
                //DocumentDbContext dc = new DocumentDbContext();
                for (int i = 4; i <= tbl.Rows.Count; i++)
                {
                    // Название продукта
                    try
                    {
                        if ((Globals.CleanWordCell(tbl.Cell(i, 2).Range.Text.Trim()) == productName) ||
                            (Globals.CleanWordCell(tbl.Cell(i, 2).Range.Text.Trim()) == ""))
                        {
                            isNewProduct = false;
                        }
                        else
                        {
                            isNewProduct = true;
                            productName = Globals.CleanWordCell(tbl.Cell(i, 2).Range.Text.Trim());
                        }

                    }
                    catch
                    {
                        isNewProduct = false;
                    }

                    if (isNewProduct)
                    {
                        if (!isWork) break;
                        product = new Product();
                        product.Name = productName;
                        try
                        {
                            product.TradeMark = Globals.CleanWordCell(tbl.Cell(i, 3).Range.Text.Trim());
                        }
                        catch
                        {
                            product.TradeMark = "";
                        }

                        // Проверить на повтор
                        Product repeatProduct = mvm.dc.Products.FirstOrDefault(m => (m.Name == product.Name && m.TradeMark == product.TradeMark));
                        if (repeatProduct != null)
                        {
                            if (repeatProduct.Templates.FirstOrDefault(m => m.Name.Trim().ToLower() == "форма 2") == null)
                            {
                                product = repeatProduct;
                            }
                            else
                            {
                                productRepeatCount++;
                                continue;
                            }
                        }
                        else
                        {
                            product.Rubric = mvm.dc.Rubrics.FirstOrDefault(m => m.Name.ToLower() == "--без рубрики--");

                            mvm.dc.Products.Add(product);
                            try
                            {
                                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    mvm.ProductCollection.Add(product);
                                }));
                            }
                            catch { }

                        }



                        product.Templates.Add(mvm.dc.Templates.FirstOrDefault(m => m.Name.ToLower() == "форма 2"));
                        mvm.TemplateCollection.FirstOrDefault(m => m.Name.ToLower() == "форма 2").Products.Add(product);
                        mvm.dc.SaveChanges();
                        //mvm.TemplateCollection = new ObservableCollection<Template>(mvm.dc.Templates);

                        productAddedCount++;
                    }

                    // Добавляем свойство
                    property = new Property();
                    product.Properties.Add(property);

                    // Требуемый параметр
                    ParamValue pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = mvm.dc.Params.FirstOrDefault(m => m.Name == "Требуемый параметр" && m.Template.Name.ToLower() == "форма 2");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 4).Range.Text.Trim()));

                    // Требуемое значение
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = mvm.dc.Params.FirstOrDefault(m => m.Name == "Требуемое значение" && m.Template.Name.ToLower() == "форма 2");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 5).Range.Text.Trim()));

                    // Значение, предлагаемое участником
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = mvm.dc.Params.FirstOrDefault(m => m.Name == "Значение, предлагаемое участником" && m.Template.Name.ToLower() == "форма 2");
                    pv.Property = property;
                    try
                    {
                        pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 6).Range.Text.Trim()));
                    }
                    catch
                    {
                        pv.Value = "";
                    }

                    // Единица измерения
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = mvm.dc.Params.FirstOrDefault(m => m.Name == "Единица измерения" && m.Template.Name.ToLower() == "форма 2");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 7).Range.Text.Trim()));

                    // Сертификация
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = mvm.dc.Params.FirstOrDefault(m => m.Name == "Сертификация" && m.Template.Name.ToLower() == "форма 2");
                    pv.Property = property;
                    try
                    {
                        pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 8).Range.Text.Trim()));
                    }
                    catch
                    {
                        pv.Value = "";
                    }

                }

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

        public ResultType_enum Create(Document document, Word.Application application, out Word._Document doc, out string message)
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
                    foreach (Product product in document.Products)
                    {
                        var myTemplate = product.Templates.FirstOrDefault(m => m.Name.Trim().ToLower() == "форма 2");
                        IEnumerable<Property> productProperties = product.Properties.SelectMany(m => m.ParamValues.Where(p => myTemplate.Param.Contains(p.Param))).Select(f => f.Property).Distinct();

                        propertiesCount += productProperties.Count();
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

                    /*begCell = wordtable.Cell(4, 6).Range.Start;
                    endCell = wordtable.Cell(propertiesCount + 4, 6).Range.End;
                    wordcellrange = doc.Range(ref begCell, ref endCell);
                    wordcellrange.Select();
                    application.Selection.Cells.Merge();*/

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
                    ParamValue paramValue = null;
                    int productIndexCompilator = 0;
                    int propertyIndexCompilator = 0;
                    for (int i = 0; i < propertiesCount; i++)
                    {
                        if (!isWork) break;
                        // Получаем свойства продукта на шаблон
                        var myTemplate = document.Products.ElementAt(productIndexCompilator).Templates.FirstOrDefault(m => m.Name.Trim().ToLower() == "форма 2");
                        IEnumerable<Property> productProperties = document.Products.ElementAt(productIndexCompilator).Properties.SelectMany(m => m.ParamValues.Where(p => myTemplate.Param.Contains(p.Param))).Select(f => f.Property).Distinct();

                        if (propertyIndexCompilator == 0)
                        {
                            // Объединяем ячейки по продукту (т.к. свойство занимает строку)
                            begCell = wordtable.Cell(i + 4, 1).Range.Start;
                            endCell = wordtable.Cell(i + 4 + productProperties.Count() - 1, 1).Range.End;
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
                            endCell = wordtable.Cell(i + 4 + productProperties.Count() - 1, 2).Range.End;
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
                            endCell = wordtable.Cell(i + 4 + productProperties.Count() - 1, 3).Range.End;
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
                            endCell = wordtable.Cell(i + 4 + productProperties.Count() - 1, 8).Range.End;
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

                            doc.Tables[1].Cell(i + 4, 2).Range.Text = document.Products.ElementAt(productIndexCompilator).Name;
                            doc.Tables[1].Cell(i + 4, 2).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                            doc.Tables[1].Cell(i + 4, 3).Range.Text = document.Products.ElementAt(productIndexCompilator).TradeMark;
                            doc.Tables[1].Cell(i + 4, 3).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                            // Сертификация
                            paramValue = productProperties.ElementAt(propertyIndexCompilator).ParamValues.FirstOrDefault(m => m.Param.Name == "Сертификация");
                            if (paramValue != null)
                            {
                                doc.Tables[1].Cell(i + 4, 8).Range.Text = paramValue.Value;
                            }
                            else
                            {
                                doc.Tables[1].Cell(i + 4, 8).Range.Text = "";
                            }

                            doc.Tables[1].Cell(i + 4, 8).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        }

                        // Требуемый параметр
                        paramValue = productProperties.ElementAt(propertyIndexCompilator).ParamValues.FirstOrDefault(m => m.Param.Name == "Требуемый параметр");
                        if (paramValue != null)
                        {
                            doc.Tables[1].Cell(i + 4, 4).Range.Text = paramValue.Value;
                        }
                        else
                        {
                            doc.Tables[1].Cell(i + 4, 4).Range.Text = "";
                        }
                        doc.Tables[1].Cell(i + 4, 4).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        // Требуемое значение
                        paramValue = productProperties.ElementAt(propertyIndexCompilator).ParamValues.FirstOrDefault(m => m.Param.Name == "Требуемое значение");
                        if (paramValue != null)
                        {
                            doc.Tables[1].Cell(i + 4, 5).Range.Text = paramValue.Value;
                        }
                        else
                        {
                            doc.Tables[1].Cell(i + 4, 5).Range.Text = "";
                        }
                        doc.Tables[1].Cell(i + 4, 5).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        // Значение, предлагаемое участником
                        paramValue = productProperties.ElementAt(propertyIndexCompilator).ParamValues.FirstOrDefault(m => m.Param.Name == "Значение, предлагаемое участником");
                        if (paramValue != null)
                        {
                            doc.Tables[1].Cell(i + 4, 6).Range.Text = paramValue.Value;
                        }
                        else
                        {
                            doc.Tables[1].Cell(i + 4, 6).Range.Text = "";
                        }
                        doc.Tables[1].Cell(i + 4, 6).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        // Единица измерения
                        paramValue = productProperties.ElementAt(propertyIndexCompilator).ParamValues.FirstOrDefault(m => m.Param.Name == "Единица измерения");
                        if (paramValue != null)
                        {
                            doc.Tables[1].Cell(i + 4, 7).Range.Text = paramValue.Value;
                        }
                        else
                        {
                            doc.Tables[1].Cell(i + 4, 7).Range.Text = "";
                        }
                        doc.Tables[1].Cell(i + 4, 7).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        propertyIndexCompilator++;
                        if (productProperties.Count() == propertyIndexCompilator)
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
