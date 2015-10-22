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
        Object missingObj = System.Reflection.Missing.Value;

        private MainViewModel mvm = Application.Current.Resources["MainViewModel"] as MainViewModel;

        public ResultType Learn(string docPath, out int productAddedCount, out int productRepeatCount, out string message)
        {
            productAddedCount = 0;
            productRepeatCount = 0;
            try
            {
                message = "";

                // Проверка пути документа
                if (!File.Exists(docPath))
                {
                    message = "Документа по пути <" + docPath + "> не существует";
                    return ResultType.Error;
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
                    return ResultType.Error;
                }
                if (tbl == null)
                {
                    message = "В документе не найдена таблица для обучения";
                    return ResultType.Error;
                }
                if (tbl.Columns.Count != 6)
                {
                    message = "Количество столбцов таблицы не совпадает со спецификацией";
                    return ResultType.Error;
                }


                //DocumentDbContext dc = new DocumentDbContext();
                // Заполняем продукты
                for (int i = 4; i <= tbl.Rows.Count; i++)
                {
                    Product product = new Product();

                    // Название продукта
                    product.Name = Globals.DeleteNandSpaces(Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 2).Range.Text.Trim())));
                    product.TradeMark = Globals.DeleteNandSpaces(Globals.CleanWordCell(tbl.Cell(i, 5).Range.Text.Trim()));

                    if (product.Name == "") continue;

                    // Проверить на повтор
                    Product repeatProduct = mvm.dc.Products.FirstOrDefault(m => (m.Name == product.Name && m.TradeMark == product.TradeMark));
                    if (repeatProduct != null)
                    {
                        if (repeatProduct.Templates.FirstOrDefault(m => m.Name.Trim().ToLower() == "свобода") == null)
                        {
                            product = repeatProduct;
                        }
                        else
                        {
                            productRepeatCount++;
                            continue;
                        }
                    }

                    // Проверка заполнения атрибутов строки 
                    if ((Globals.CleanWordCell(tbl.Cell(i, 3).Range.Text.Trim()) == "") &&
                        (Globals.CleanWordCell(tbl.Cell(i, 4).Range.Text.Trim()) == "") &&
                        (Globals.CleanWordCell(tbl.Cell(i, 6).Range.Text.Trim())) == "")
                    {
                        continue;
                    }

                    // У данного шаблона одно свойство
                    Property property = new Property();

                    // Требования заказчика
                    ParamValue pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = mvm.dc.Params.FirstOrDefault(m => m.Name == "Требования заказчика" && m.Template.Name.ToLower() == "свобода");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 3).Range.Text.Trim()));

                    // Требования заказчика
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = mvm.dc.Params.FirstOrDefault(m => m.Name == "Требования участника" && m.Template.Name.ToLower() == "свобода");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 4).Range.Text.Trim()));

                    // Сертификация
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = mvm.dc.Params.FirstOrDefault(m => m.Name == "Сертификация" && m.Template.Name.ToLower() == "свобода");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 6).Range.Text.Trim()));

                    // Добавляем к продукту значения, шаблон, рубрику 
                    product.Properties.Add(property);
                    product.Templates.Add(mvm.dc.Templates.FirstOrDefault(m => m.Name.ToLower() == "свобода"));
                    product.Rubric = mvm.dc.Rubrics.FirstOrDefault(m => m.Name.ToLower() == "--без рубрики--");

                    

                    mvm.dc.Products.Add(product);
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        mvm.ProductCollection.Add(product);
                    }));



                    productAddedCount++;
                }


                // Закрываем приложение
                application.Quit(ref missing, ref missing, ref missing);
                application = null;

                mvm.dc.SaveChanges();
                mvm.TemplateCollection = new ObservableCollection<Template>(mvm.dc.Templates);

                return ResultType.Done;
            }
            catch (Exception ex)
            {
                message = ex.Message + '\n' + ex.StackTrace;
                return ResultType.Error;
            }
        }

        public ResultType Create(Document document, Word.Application application, out Word._Document doc, out string message)
        {
            try
            {
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
                    Word.Table wordtable = doc.Tables.Add(doc.Paragraphs[5].Range, 3 + document.Products.Count, 6,
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
                    for (int i = 0; i < document.Products.Count; i++)
                    {
                        doc.Tables[1].Cell(i + 4, 1).Range.Text = Convert.ToString(i + 1) + '.';
                        doc.Tables[1].Cell(i + 4, 1).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                        doc.Tables[1].Cell(i + 4, 2).Range.Text = document.Products.ElementAt(i).Name;
                        doc.Tables[1].Cell(i + 4, 2).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        doc.Tables[1].Cell(i + 4, 5).Range.Text = document.Products.ElementAt(i).TradeMark;
                        doc.Tables[1].Cell(i + 4, 5).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        // Получим свойства продукта на шаблон
                        //Product productTest = document.Products.ElementAt(i);
                        //IColl<Param> paramsTemplate = mvm.dc.Templates.FirstOrDefault(p => p.Name.Trim().ToLower() == "свобода").Param;
                        //ICollection<Property> productProperties = document.Products.ElementAt(i).SelectMany(m => m.ParamValues.Where(l => l.Param.Template.Name.Trim().ToLower() == "свобода"));
                        //ICollection<Param> paramColl = (ICollection<Param>)paramsTemplate;
                        var myTemplate = document.Products.ElementAt(i).Templates.FirstOrDefault(m => m.Name.Trim().ToLower() == "свобода");
                        IEnumerable<Property> productProperties = document.Products.ElementAt(i).Properties.SelectMany(m => m.ParamValues.Where(p => myTemplate.Param.Contains(p.Param))).Select(f => f.Property).Distinct();
                        
                        // В данном шаблоне одно свойство(строка) у продукта
                        if (productProperties.Count() != 1)
                        {
                            message = "Ошибка при составлении шаблона";
                            return ResultType.Error;
                        }
                        Property property = productProperties.First();

                        ParamValue paramValue = null;
                        // Требования заказчика
                        paramValue = property.ParamValues.FirstOrDefault(m => m.Param.Name == "Требования заказчика");
                        if (paramValue != null)
                        {
                            doc.Tables[1].Cell(i + 4, 3).Range.Text = paramValue.Value;
                        }
                        else
                        {
                            doc.Tables[1].Cell(i + 4, 3).Range.Text = "";
                        }
                        doc.Tables[1].Cell(i + 4, 3).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        // Требования участника
                        paramValue = property.ParamValues.FirstOrDefault(m => m.Param.Name == "Требования участника");
                        if (paramValue != null)
                        {
                            doc.Tables[1].Cell(i + 4, 4).Range.Text = paramValue.Value;
                        }
                        else
                        {
                            doc.Tables[1].Cell(i + 4, 4).Range.Text = "";
                        }
                        doc.Tables[1].Cell(i + 4, 4).Range.Paragraphs.Alignment = Word.WdParagraphAlignment.wdAlignParagraphJustify;

                        // Сертификация
                        paramValue = property.ParamValues.FirstOrDefault(m => m.Param.Name == "Сертификация");
                        if (paramValue != null)
                        {
                            doc.Tables[1].Cell(i + 4, 6).Range.Text = paramValue.Value;
                        }
                        else
                        {
                            doc.Tables[1].Cell(i + 4, 6).Range.Text = "";
                        }
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


                return ResultType.Done;
            }
            catch (Exception ex)
            {
                doc = null;
                message = ex.Message + '\n' + ex.StackTrace;
                return ResultType.Error;
            }
        }
    }
}
