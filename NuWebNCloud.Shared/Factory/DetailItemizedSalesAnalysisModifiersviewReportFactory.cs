using ClosedXML.Excel;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Reports;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuWebNCloud.Shared.Factory
{
    public class DetailItemizedSalesAnalysisModifiersviewReportFactory : ReportFactory
    {

        public XLWorkbook Report(BaseReportModel viewmodel, List<CategoryOnStore> ListCatgoryOnstoreChoose, List<Modifier> ListModifierChoose)
        {
            string sheetName = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("ModifiersviewReport");
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);
            var totalheader = (ListModifierChoose.Count * 2) + 2;
            var totalModifier = ListModifierChoose.Count * 2;
            //Format Header
            CreateReportHeaderNew(ws, totalheader, viewmodel.FromDateFilter, viewmodel.ToDateFilter, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Detail Itemized Sales Analysis Report"));
            ws.Range(5, 1, 6, 1).Merge(true).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Item ID"));
            ws.Range(5, 2, 6, 2).Merge(true).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Dishes"));
            int columnStart = 3;
            int columnEnd = 4;
            foreach (var item in ListModifierChoose)
            {
                ws.Range(5, columnStart, 5, columnEnd).Merge(true).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(item.ModifierName.ToString()));
                ws.Column(columnStart).Width = 25;
                ws.Column(columnEnd).Width = 25;
                ws.Cell(6, columnStart).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Quantity"));
                ws.Cell(6, columnEnd).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount"));
                columnStart += 2;
                columnEnd += 2;
            }
            ws.Column(1).Width = 20;
            ws.Column(2).Width = 30;
            ws.Range(5, 1, 6, totalheader).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(5, 1, 6, totalheader).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(5, 1, 6, totalheader).Style.Font.SetBold(true);
            ws.Row(5).Height = 60;
            // end format
            int row = 6;
            List<SumAmount> ListSumAmount = new List<SumAmount>();
            SumAmount sumAmount = null;
            //body
            foreach (var itemStore in ListCatgoryOnstoreChoose)
            {
                ws.Range(++row, 1, row, totalheader).Merge(true).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemStore.StoreName)).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorStore)).Font.SetBold(true); ;
                foreach (var itemCategory in itemStore.ListCategory)
                {
                    ws.Range(++row, 1, row, totalheader).Merge(true).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemCategory.CategoryName)).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader)).Font.SetBold(true); ;
                    foreach (var itemDish in itemCategory.ListDish)
                    {
                        row++;
                        ws.Range(row, 1, row, 1).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemDish.DishId));
                        ws.Range(row, 2, row, 2).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemDish.DishName));
                        ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        int cols = totalheader - 2;
                        foreach (var item in itemDish.ListModifierOnDish)
                        {
                            cols--;
                            ws.Cell(row, totalheader - cols).SetValue(item.DishQuantity).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            cols--;
                            ws.Cell(row, totalheader - cols).SetValue(item.DishAmount).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            sumAmount = new SumAmount();
                            sumAmount.ModifierId = item.ModifierId;
                            sumAmount.TotalAmount += item.DishAmount;
                            ListSumAmount.Add(sumAmount);

                        }
                    }
                }
            }
            var lstGroupAmount = ListSumAmount.GroupBy(x => x.ModifierId);
            List<SumAmount> ListSumAllAmount = new List<SumAmount>();
            SumAmount sumAllAmounts = null;
            foreach (var item in lstGroupAmount)
            {
                sumAllAmounts = new SumAmount();
                sumAllAmounts.ModifierId = item.Key;
                sumAllAmounts.TotalAmount = item.Select(x => x.TotalAmount).Sum();
                ListSumAllAmount.Add(sumAllAmounts);
            }

            //Total
            columnStart = 3;
            columnEnd = columnStart + 1;
            ws.Range(++row, 1, row, 2).Merge(true).SetValue(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("TOTAL")).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            for (int i = 0; i < ListSumAllAmount.Count; i++)
            {                
                ws.Range(row, columnStart, row, columnEnd).Merge().SetValue(Math.Round(ListSumAllAmount[i].TotalAmount, 2)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                columnStart += 2;
                columnEnd += 2;
            }

            //format body
            ws.Range(1, 1, row, totalheader).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, row, totalheader).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(row, 1, row, totalheader).Style.Font.SetBold(true);
            //endbody
            return wb;
        }
    }
}
