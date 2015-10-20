using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PublicOrders.Models;
using System.IO;
using Word = Microsoft.Office.Interop.Word;

namespace PublicOrders.Processors
{
    class CommitteeLoadProcessor
    {
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
                if (tbl.Columns.Count != 9)
                {
                    message = "Количество столбцов таблицы не совпадает со спецификацией";
                    return ResultType.Error;
                }

                // Заполняем продукты
                Product product = null;
                Property property = null;
                bool isNewProduct = false;
                string productName = "";
                DocumentDbContext dc = new DocumentDbContext();
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
                        if (dc.Products.FirstOrDefault(m => (m.Name == product.Name && m.TradeMark == product.TradeMark)) != null)
                        {
                            productRepeatCount++;
                            continue;
                        }

                        product.Templates.Add(dc.Templates.FirstOrDefault(m => m.Name.ToLower() == "комитет"));
                        product.Rubric = dc.Rubrics.FirstOrDefault(m => m.Name.ToLower() == "--без рубрики--");

                        dc.Products.Add(product);
                        productAddedCount++;
                    }
                    // Добавляем свойство
                    property = new Property();
                    product.Properties.Add(property);

                    // Наименование показателя
                    ParamValue pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = dc.Params.FirstOrDefault(m => m.Name == "Наименование показателя" && m.Template.Name.ToLower() == "комитет");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 3).Range.Text.Trim()));

                    // Минимальное значение показателей
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = dc.Params.FirstOrDefault(m => m.Name == "Минимальные значения показателей" && m.Template.Name.ToLower() == "комитет");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 4).Range.Text.Trim()));

                    // Максимальное значение показателей
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = dc.Params.FirstOrDefault(m => m.Name == "Максимальные значения показателей" && m.Template.Name.ToLower() == "комитет");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 5).Range.Text.Trim()));

                    // Значения показателей, которые не могут изменяться
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = dc.Params.FirstOrDefault(m => m.Name == "Значения показателей, которые не могут изменяться" && m.Template.Name.ToLower() == "комитет");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 6).Range.Text.Trim()));

                    // Конкретные показатели
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = dc.Params.FirstOrDefault(m => m.Name == "Конкретные показатели" && m.Template.Name.ToLower() == "комитет");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 7).Range.Text.Trim()));

                    // Единица измерения
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = dc.Params.FirstOrDefault(m => m.Name == "Единица измерения" && m.Template.Name.ToLower() == "комитет");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 8).Range.Text.Trim()));
                }

                // Закрываем приложение
                application.Quit(ref missing, ref missing, ref missing);
                application = null;

                dc.SaveChanges();

                return ResultType.Done;

                // Заносим продукты в БД
                //return dbEngineDocs.SetProducts(DocTemplate.Template_3, products, out productAddedCount, out productRepeatCount, out message);
            }
            catch (Exception ex)
            {
                message = ex.Message + '\n' + ex.StackTrace;
                return ResultType.Error;
            }
        }
    }
}
