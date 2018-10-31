using ClosedXML.Excel;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Drawing;
using NuWebNCloud.Shared.Utilities;

namespace NuWebNCloud.Shared.Factory
{
    public class ReportFactory
    {
        public void CreateReportHeader(IXLWorksheet ws, int totalCols, DateTime fromDate, DateTime toDate, string reportName)
        {
            string merchantName = Commons.MerchantName;

            // Merchant Name
            ws.Cell(1, 1).Value = merchantName;
            ws.Range(1, 1, 1, totalCols).Merge();
            ws.Range(1, 1, 1, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));

            // Report Name
            //ws.Cell(2, 1).Value = string.Format("{0} from {1} to {2}", reportName, fromDate.ToString("MM/dd/yyyy"), toDate.ToString("MM/dd/yyyy"));
            ws.Cell(2, 1).Value = string.Format("{0} " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("From").ToLower() + " {1} " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To").ToLower() + " {2}", reportName, fromDate.ToString("MM/dd/yyyy"), toDate.ToString("MM/dd/yyyy"));
            ws.Range(2, 1, 2, totalCols).Merge();
            ws.Range(2, 1, 2, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Row(2).Style.Font.FontSize = 16;
            ws.Row(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(2).Height = 40;

            // Current Date 
            ws.Range(3, 1, 3, totalCols).Merge();
            ws.Cell(3, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Generated on") + ": " + DateTime.Now.ToString();
            ws.Row(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            ws.Row(1).Style.Font.SetBold(true);
            ws.Row(2).Style.Font.SetBold(true);
            ws.Row(3).Style.Font.SetBold(true);
        }

        public void CreateReportHeaderNew(IXLWorksheet ws, int totalCols, DateTime fromDate, DateTime toDate, string reportName)
        {
            string merchantName = Commons.MerchantName;

            // Merchant Name
            ws.Cell(1, 1).Value = merchantName;
            ws.Range(1, 1, 1, totalCols).Merge();
            ws.Range(1, 1, 1, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));

            // Report Name
            ws.Cell(2, 1).Value = string.Format("{0}", reportName);
            ws.Range(2, 1, 2, totalCols).Merge();
            ws.Range(2, 1, 2, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Row(2).Style.Font.FontSize = 16;
            ws.Row(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(2).Height = 40;

            // Time
            ws.Range(3, 1, 3, totalCols).Merge();
            ws.Cell(3, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time: From") + " " + fromDate.ToString("HH:mm") + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To") + " " + toDate.ToString("HH:mm");
            ws.Row(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            // Date 
            ws.Range(4, 1, 4, totalCols).Merge();
            ws.Cell(4, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date: From") + " " + fromDate.ToString("MM/dd/yyyy") + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To") + " " + toDate.ToString("MM/dd/yyyy");
            ws.Row(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            ws.Row(1).Style.Font.SetBold(true);
            ws.Row(2).Style.Font.SetBold(true);
            ws.Row(3).Style.Font.SetBold(true);
            ws.Row(4).Style.Font.SetBold(true);
        }

        public void CreateReportHeaderForTopSale(IXLWorksheet ws, int totalCols, DateTime date, string reportName)
        {
            string merchantName = Commons.MerchantName;

            // Merchant Name
            ws.Cell(1, 1).Value = merchantName;
            ws.Range(1, 1, 1, totalCols).Merge();
            ws.Range(1, 1, 1, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));

            // Report Name
            ws.Cell(2, 1).Value = string.Format("{0}", reportName);
            ws.Range(2, 1, 2, totalCols).Merge();
            ws.Range(2, 1, 2, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Row(2).Style.Font.FontSize = 16;
            ws.Row(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(2).Height = 40;

            // Date 
            ws.Range(3, 1, 3, totalCols).Merge();
            ws.Cell(3, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("As of this date") + ": " + date.ToString("MM/dd/yyyy");
            ws.Row(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            ws.Row(1).Style.Font.SetBold(true);
            ws.Row(2).Style.Font.SetBold(true);
            ws.Row(3).Style.Font.SetBold(true);
         
        }

        public void FormatAllReport(IXLWorksheet ws, int currentRow, int totalCols)
        {
            ws.Range(1, 1, currentRow - 1, totalCols).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, currentRow - 1, totalCols).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 1, totalCols).Style.Border.BottomBorder = XLBorderStyleValues.None;
            ws.Range(2, 1, 2, totalCols).Style.Border.TopBorder = XLBorderStyleValues.None;
            ws.Columns(1, totalCols).AdjustToContents();
        }

        public void FormatStoreNameRow(IXLWorksheet ws, int currentRow, int totalCols)
        {
            ws.Range(currentRow, 1, currentRow, totalCols).Merge();
            ws.Row(currentRow).Height = 20;
            ws.Row(currentRow).Style.Font.SetBold(true);
            ws.Row(currentRow).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Row(currentRow).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range(currentRow, 1, currentRow, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
        }

        public void CreateReportHeaderChart(ExcelWorksheet ws, int totalCols, DateTime fromDate, DateTime toDate, string reportName)
        {
            string merchantName = Commons.MerchantName;

            // Merchant Name
            ws.Cells[1, 1].Value = merchantName;
            ws.Cells[1, 1, 1, totalCols].Merge = true;
            ws.Cells[1, 1, 1, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[1, 1, 1, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));

            // Report Name
            ws.Cells[2, 1].Value = reportName;
            ws.Cells[2, 1, 2, totalCols].Merge = true;
            ws.Cells[2, 1, 2, totalCols].Style.Fill.PatternType = ExcelFillStyle.Solid;
            ws.Cells[2, 1, 2, totalCols].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(217, 217, 217));
            ws.Row(2).Style.Font.Size = 16;
            ws.Row(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Row(2).Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Row(2).Height = 40;

            // Time 
            ws.Cells[3, 1, 3, totalCols].Merge = true;
            ws.Cells[3, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time: From") + " " + fromDate.ToString("HH:mm") + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To") + " " + toDate.ToString("HH:mm");
            ws.Row(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;


            // Date 
            ws.Cells[4, 1, 4, totalCols].Merge = true;
            ws.Cells[4, 1].Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date: From") + " " + fromDate.ToString("MM/dd/yyyy") + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To") + " " + toDate.ToString("MM/dd/yyyy");
            ws.Row(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;


            ws.Row(1).Style.Font.Bold = true;
            ws.Row(2).Style.Font.Bold = true;
            ws.Row(3).Style.Font.Bold = true;
            ws.Row(4).Style.Font.Bold = true;
        }

        public void CreateReportHeaderNewFilterTime(IXLWorksheet ws, int totalCols, DateTime fromDate, DateTime toDate, int fromTime, int toTime, string reportName)
        {
            string merchantName = Commons.MerchantName;

            // Merchant Name
            ws.Cell(1, 1).Value = merchantName;
            ws.Range(1, 1, 1, totalCols).Merge();
            ws.Range(1, 1, 1, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));

            // Report Name
            ws.Cell(2, 1).Value = string.Format("{0}", reportName);
            ws.Range(2, 1, 2, totalCols).Merge();
            ws.Range(2, 1, 2, totalCols).Style.Fill.SetBackgroundColor(XLColor.FromHtml(Commons.BgColorHeader));
            ws.Row(2).Style.Font.FontSize = 16;
            ws.Row(2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(2).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(2).Height = 40;

            // Time
            ws.Range(3, 1, 3, totalCols).Merge();
            ws.Cell(3, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time: From") + " " + fromTime.ToString("0#") + ":00 " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To") + " " + toTime.ToString("0#") + ":00 ";
            ws.Row(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            // Date 
            ws.Range(4, 1, 4, totalCols).Merge();
            ws.Cell(4, 1).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date: From") + " " + fromDate.ToString("MM/dd/yyyy") + " " + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("To") + " " + toDate.ToString("MM/dd/yyyy");
            ws.Row(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            ws.Row(1).Style.Font.SetBold(true);
            ws.Row(2).Style.Font.SetBold(true);
            ws.Row(3).Style.Font.SetBold(true);
            ws.Row(4).Style.Font.SetBold(true);

            // Format header report
            ws.Range(1, 1, 4, totalCols).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, totalCols).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }
    }
}
