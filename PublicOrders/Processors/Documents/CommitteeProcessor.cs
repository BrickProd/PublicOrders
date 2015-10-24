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
                if (tbl.Columns.Count != 9)
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
                for (int i = 2; i <= tbl.Rows.Count; i++)
                {
                    // Название продукта
                    try
                    {
                        Microsoft.Office.Interop.Word.Cell cell = tbl.Cell(i, 2);
                        if ((Globals.CleanWordCell(cell.Range.Text.Trim()) == productName) || (Globals.CleanWordCell(cell.Range.Text.Trim()) == ""))
                        {
                            isNewProduct = false;
                        }
                        else
                        {
                            isNewProduct = true;
                            productName = Globals.CleanWordCell(cell.Range.Text.Trim());
                            /*if (productName == "Задвижки") {
                                string sss = "авыавыа";
                            }*/
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
                            product.TradeMark = Globals.CleanWordCell(tbl.Cell(i, 9).Range.Text.Trim());
                        }
                        catch
                        {
                            product.TradeMark = "";
                        }

                        // Проверить на повтор
                        Product repeatProduct = mvm.dc.Products.FirstOrDefault(m => (m.Name == product.Name && m.TradeMark == product.TradeMark));
                        if (repeatProduct != null)
                        {
                            if (repeatProduct.Templates.FirstOrDefault(m => m.Name.Trim().ToLower() == "комитет") == null)
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

                        product.Templates.Add(mvm.dc.Templates.FirstOrDefault(m => m.Name.ToLower() == "комитет"));
                        productAddedCount++;
                    }
                    // Добавляем свойство
                    property = new Property();
                    product.Properties.Add(property);

                    // Наименование показателя
                    ParamValue pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = mvm.dc.Params.FirstOrDefault(m => m.Name == "Наименование показателя" && m.Template.Name.ToLower() == "комитет");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 3).Range.Text.Trim()));

                    // Минимальное значение показателей
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = mvm.dc.Params.FirstOrDefault(m => m.Name == "Минимальные значения показателей" && m.Template.Name.ToLower() == "комитет");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 4).Range.Text.Trim()));

                    // Максимальное значение показателей
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = mvm.dc.Params.FirstOrDefault(m => m.Name == "Максимальные значения показателей" && m.Template.Name.ToLower() == "комитет");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 5).Range.Text.Trim()));

                    // Значения показателей, которые не могут изменяться
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = mvm.dc.Params.FirstOrDefault(m => m.Name == "Значения показателей, которые не могут изменяться" && m.Template.Name.ToLower() == "комитет");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 6).Range.Text.Trim()));

                    // Конкретные показатели
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = mvm.dc.Params.FirstOrDefault(m => m.Name == "Конкретные показатели" && m.Template.Name.ToLower() == "комитет");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 7).Range.Text.Trim()));

                    // Единица измерения
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = mvm.dc.Params.FirstOrDefault(m => m.Name == "Единица измерения" && m.Template.Name.ToLower() == "комитет");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 8).Range.Text.Trim()));
                }

                // Закрываем приложение
                application.Quit(ref missing, ref missing, ref missing);
                application = null;

                mvm.dc.SaveChanges();
                mvm.TemplateCollection = new ObservableCollection<Template>(mvm.dc.Templates);

                return ResultType_enum.Done;

                // Заносим продукты в БД
                //return dbEngineDocs.SetProducts(DocTemplate.Template_3, products, out productAddedCount, out productRepeatCount, out message);
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
                    foreach (Product product in document.Products)
                    {
                        var myTemplate = product.Templates.FirstOrDefault(m => m.Name.Trim().ToLower() == "комитет");
                        IEnumerable<Property> productProperties = product.Properties.SelectMany(m => m.ParamValues.Where(p => myTemplate.Param.Contains(p.Param))).Select(f => f.Property).Distinct();

                        propertiesCount += productProperties.Count();
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
                        // Получаем свойства продукта на шаблон
                        var myTemplate = document.Products.ElementAt(productIndexCompilator).Templates.FirstOrDefault(m => m.Name.Trim().ToLower() == "комитет");
                        IEnumerable<Property> productProperties = document.Products.ElementAt(productIndexCompilator).Properties.SelectMany(m => m.ParamValues.Where(p => myTemplate.Param.Contains(p.Param))).Select(f => f.Property).Distinct();

                        if (propertyIndexCompilator == 0)
                        {
                            // Объединяем ячейки по продукту
                            // Номер
                            object begCell = wordtable.Cell(i + 2, 1).Range.Start;
                            object endCell = wordtable.Cell(i + 2 + productProperties.Count() - 1, 1).Range.End;
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
                            endCell = wordtable.Cell(i + 2 + productProperties.Count() - 1, 2).Range.End;
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
                            endCell = wordtable.Cell(i + 2 + productProperties.Count() - 1, 9).Range.End;
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

                            doc.Tables[1].Cell(i + 2, 2).Range.Text = document.Products.ElementAt(productIndexCompilator).Name;
                            doc.Tables[1].Cell(i + 2, 2).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                            doc.Tables[1].Cell(i + 2, 9).Range.Text = document.Products.ElementAt(productIndexCompilator).TradeMark;
                            doc.Tables[1].Cell(i + 2, 9).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;
                        }

                        ParamValue paramValue = null;
                        // Наименование показателя
                        paramValue = productProperties.ElementAt(propertyIndexCompilator).ParamValues.FirstOrDefault(m => m.Param.Name == "Наименование показателя");
                        if (paramValue != null)
                        {
                            doc.Tables[1].Cell(i + 2, 3).Range.Text = paramValue.Value;
                        }
                        else
                        {
                            doc.Tables[1].Cell(i + 2, 3).Range.Text = "";
                        }
                        doc.Tables[1].Cell(i + 2, 3).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        // Минимальные значения показателей
                        paramValue = productProperties.ElementAt(propertyIndexCompilator).ParamValues.FirstOrDefault(m => m.Param.Name == "Минимальные значения показателей");
                        if (paramValue != null)
                        {
                            doc.Tables[1].Cell(i + 2, 4).Range.Text = paramValue.Value;
                        }
                        else
                        {
                            doc.Tables[1].Cell(i + 2, 4).Range.Text = "";
                        }
                        doc.Tables[1].Cell(i + 2, 4).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        // Максимальные значения показателей
                        paramValue = productProperties.ElementAt(propertyIndexCompilator).ParamValues.FirstOrDefault(m => m.Param.Name == "Максимальные значения показателей");
                        if (paramValue != null)
                        {
                            doc.Tables[1].Cell(i + 2, 5).Range.Text = paramValue.Value;
                        }
                        else
                        {
                            doc.Tables[1].Cell(i + 2, 5).Range.Text = "";
                        }
                        doc.Tables[1].Cell(i + 2, 5).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        // Значения показателей, которые не могут изменяться
                        paramValue = productProperties.ElementAt(propertyIndexCompilator).ParamValues.FirstOrDefault(m => m.Param.Name == "Значения показателей, которые не могут изменяться");
                        if (paramValue != null)
                        {
                            doc.Tables[1].Cell(i + 2, 6).Range.Text = paramValue.Value;
                        }
                        else
                        {
                            doc.Tables[1].Cell(i + 2, 6).Range.Text = "";
                        }
                        doc.Tables[1].Cell(i + 2, 6).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        // Конкретные показатели
                        paramValue = productProperties.ElementAt(propertyIndexCompilator).ParamValues.FirstOrDefault(m => m.Param.Name == "Конкретные показатели");
                        if (paramValue != null)
                        {
                            doc.Tables[1].Cell(i + 2, 7).Range.Text = paramValue.Value;
                        }
                        else
                        {
                            doc.Tables[1].Cell(i + 2, 7).Range.Text = "";
                        }
                        doc.Tables[1].Cell(i + 2, 7).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        // Единица измерения
                        paramValue = productProperties.ElementAt(propertyIndexCompilator).ParamValues.FirstOrDefault(m => m.Param.Name == "Единица измерения");
                        if (paramValue != null)
                        {
                            doc.Tables[1].Cell(i + 2, 8).Range.Text = paramValue.Value;
                        }
                        else
                        {
                            doc.Tables[1].Cell(i + 2, 8).Range.Text = "";
                        }
                        doc.Tables[1].Cell(i + 2, 8).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

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
