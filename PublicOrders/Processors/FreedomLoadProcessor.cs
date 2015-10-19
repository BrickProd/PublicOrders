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
    class FreedomLoadProcessor
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
                if (tbl.Columns.Count != 6)
                {
                    message = "Количество столбцов таблицы не совпадает со спецификацией";
                    return ResultType.Error;
                }


                DocumentDbContext dc = new DocumentDbContext();
                // Заполняем продукты
                for (int i = 4; i <= tbl.Rows.Count; i++)
                {
                    Product product = new Product();

                    // Название продукта
                    product.Name = Globals.DeleteNandSpaces(Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 2).Range.Text.Trim())));
                    product.TradeMark = Globals.DeleteNandSpaces(Globals.CleanWordCell(tbl.Cell(i, 5).Range.Text.Trim()));

                    if (product.Name == "") continue;

                    // Проверить на повтор
                    if (dc.Products.FirstOrDefault(m => (m.Name == product.Name && m.TradeMark == product.TradeMark)) != null)
                    {
                        productRepeatCount++;
                        continue;
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

                    pv.Param = dc.Params.FirstOrDefault(m => m.Name == "Требования заказчика" && m.Template.Name.ToLower() == "свобода");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 3).Range.Text.Trim()));

                    // Требования заказчика
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = dc.Params.FirstOrDefault(m => m.Name == "Требования участника" && m.Template.Name.ToLower() == "свобода");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 4).Range.Text.Trim()));

                    // Сертификация
                    pv = new ParamValue();
                    property.ParamValues.Add(pv);

                    pv.Param = dc.Params.FirstOrDefault(m => m.Name == "Сертификация" && m.Template.Name.ToLower() == "свобода");
                    pv.Property = property;
                    pv.Value = Globals.ConvertTextExtent(Globals.CleanWordCell(tbl.Cell(i, 6).Range.Text.Trim()));

                    // Добавляем к продукту значения, шаблон, рубрику 
                    product.Properties.Add(property);
                    product.Templates.Add(dc.Templates.FirstOrDefault(m => m.Name.ToLower() == "свобода"));
                    product.Rubric = dc.Rubrics.FirstOrDefault(m => m.Name.ToLower() == "--без рубрики--");



                    dc.Products.Add(product);
                    productAddedCount++;
                }


                // Закрываем приложение
                application.Quit(ref missing, ref missing, ref missing);
                application = null;

                dc.SaveChanges();

                return ResultType.Done;
            }
            catch (Exception ex)
            {
                message = ex.Message + '\n' + ex.StackTrace;
                return ResultType.Error;
            }
        }
    }
}
