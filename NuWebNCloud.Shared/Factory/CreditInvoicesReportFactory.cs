using ClosedXML.Excel;
using NuWebNCloud.Data;
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
    public class CreditInvoicesReportFactory : ReportFactory
    {
        private BaseFactory _baseFactory = null;
        public CreditInvoicesReportFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<CreditInvoiceReportModels> GetDataReport(BaseReportModel input, int typeCash)
        {
            var lstReturns = new List<CreditInvoiceReportModels>();
            try
            {
                using (var cxt = new NuWebContext())
                {
                   var  data = cxt.R_CashInOutReport
                                    .Where(o => o.CashType == typeCash &&
                                                o.CreatedDate >= input.FromDate && o.CreatedDate <= input.ToDate &&
                                                input.ListStores.Contains(o.StoreId) && o.Mode == input.Mode)
                                    .OrderBy(o => o.CreatedDate)
                                    .Select(o => new CreditInvoiceReportModels
                                    {
                                        StoreId = o.StoreId,
                                        CreatedDate = o.CreatedDate,
                                        Reason = o.Reason,
                                        Amount = o.CashValue,
                                    }).ToList();

                    if(data != null && data.Count > 0)
                    {
                        data.ForEach(o =>
                        {
                            if(!string.IsNullOrEmpty(o.Reason))
                            {
                                var _Split = o.Reason.Split('|').ToList();
                                if(_Split != null && _Split.Count > 0)
                                {
                                    o.InvoiceNo = _Split[0];
                                    if(_Split.Count > 1)
                                        o.SupplierName = _Split[1];
                                    if (_Split.Count > 2)
                                        o.Remark = _Split[2];
                                }
                            }
                        });
                        /* get list supplier */
                        lstReturns = data.GroupBy(o => new { o.StoreId, o.SupplierName  }).Select(o => o.First()).ToList();
                        /* get list invoice of supplier */
                        if(lstReturns != null && lstReturns.Count > 0)
                        {
                            lstReturns.ForEach(o =>
                            {
                                o.Item = data.Where(s => s.SupplierName.Equals(o.SupplierName) && s.StoreId.Equals(o.StoreId))
                                             .Select(s => new CreditInvoiceItemReportModels
                                             {
                                                 Amount = s.Amount,
                                                 CreatedDate = s.CreatedDate,
                                                 InvoiceNo = s.InvoiceNo,
                                                 Remark = s.Remark
                                             }).ToList();
                                 o.TotalAmout = o.Item.Sum(m => m.Amount);
                            });
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                NSLog.Logger.Error("GetDataReport_CreditInvoiceReport : ", ex);
            }
            return lstReturns;
        }

        public XLWorkbook ExportExcelCreditInvoice(BaseReportModel model, List<StoreModels> lstStore, int cashType)
        {
            string sheetName = "Credit_Invoice_Report";
            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(sheetName);

            // Set header report
            CreateReportHeaderNew(ws, 5, model.FromDate, model.ToDate, _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Credit Invoice Report"));
            ws.Range(1, 1, 4, 5).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range(1, 1, 4, 5).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            //Get business day
            var _lstBusDayAllStore = _baseFactory.GetBusinessDays(model.FromDate, model.ToDate, model.ListStores, model.Mode);
            if (_lstBusDayAllStore == null || !_lstBusDayAllStore.Any())
            {
                return wb;
            }

            // Get data use business day
            model.FromDate = _lstBusDayAllStore.Min(oo => oo.DateFrom);
            model.ToDate = _lstBusDayAllStore.Max(oo => oo.DateTo);

            List<CreditInvoiceReportModels> lstCreditModel = GetDataReport(model, cashType);
            if (lstCreditModel == null || !lstCreditModel.Any())
            {
                return wb;
            }

            ws.Cell("A5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Date");
            ws.Cell("B5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Time");
            ws.Cell("C5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Invoice No.");
            ws.Cell("D5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount");
            ws.Cell("E5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Remark");
            //ws.Cell("F5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Amount");
            //ws.Cell("G5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Reason");
            //ws.Cell("H5").Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("User");

            //set width column
            ws.Column("A").Width = 150;
            ws.Column("B").Width = 150;
            ws.Column("C").Width = 250;
            ws.Column("D").Width = 250;
            ws.Column("E").Width = 180;
            //ws.Column("F").Width = 20;
            //ws.Column("G").Width = 35;
            //ws.Column("H").Width = 15;
            ws.Range("A" + 5 + ":E" + 5).Style.Font.SetBold(true);
            ws.Range("A" + 5 + ":E" + 5).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
            ws.Range(5, 1, 5, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int index = 7;
            DateTime dFrom = DateTime.Now, dTo = DateTime.Now;
            string storeId = string.Empty;

            for (int i = 0; i < lstStore.Count(); i++)
            {
                storeId = lstStore[i].Id;
                var lstBusDayByStore = _lstBusDayAllStore.Where(ww => ww.StoreId == storeId).ToList();
                if (lstBusDayByStore != null && lstBusDayByStore.Any())
                {
                    dFrom = lstBusDayByStore.Min(ss => ss.DateFrom);
                    dTo = lstBusDayByStore.Max(ss => ss.DateTo);

                    var lstCreditModelByStore = lstCreditModel.Where(ww => ww.StoreId == storeId && ww.CreatedDate >= dFrom && ww.CreatedDate <= dTo).ToList();
                    if (lstCreditModelByStore != null && lstCreditModelByStore.Any())
                    {
                        ws.Range("A" + (index - 1) + ":E" + (index - 1)).Merge().SetValue(lstStore[i].Name);
                        ws.Range("A" + (index - 1) + ":E" + (index - 1)).Style.Font.SetBold(true);
                        ws.Range("A" + (index - 1) + ":E" + (index - 1)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));

                        for (int j = 0; j < lstCreditModelByStore.Count; j++)
                        {
                            ws.Range("A" + (index) + ":C" + (index)).Merge().SetValue(lstCreditModelByStore[j].SupplierName);
                            ws.Range("A" + (index) + ":E" + (index)).Style.Font.SetBold(true);
                            ws.Range("A" + (index) + ":E" + (index)).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#d9d9d9"));
                            ws.Cell("D" + (index)).Value =  "'" +  lstCreditModelByStore[j].TotalAmout.ToString("F");
                            ws.Cell("D" + (index)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            foreach (var itemSupplier in lstCreditModelByStore[j].Item)
                            {
                                ws.Cell("A" + (index + 1)).Value = "'" + itemSupplier.CreatedDate.ToString("MM/dd/yyyy");
                                ws.Cell("B" + (index + 1)).Value = "'" + itemSupplier.CreatedDate.ToString("hh:mm tt");
                                ws.Cell("C" + (index + 1)).Value = itemSupplier.InvoiceNo;
                                ws.Cell("D" + (index + 1)).Value =  "'" + itemSupplier.Amount.ToString("F");
                                
                                ws.Cell("E" + (index + 1)).Value =  "'" + itemSupplier.Remark;
                                ws.Cell("E" + (index + 1)).Style.Alignment.WrapText = true;
                                ws.Range("A" + (index + 1) + ":C" + (index + 1)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                ws.Cell("D" + (index + 1)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                ws.Cell("C" + (index + 1)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                index++;
                            }
                            index++;
                        }
                        index++;
                    }
                }

            }//End loop store

            //ws.Range("F5" + ":F" + index).Style.NumberFormat.Format = "#,##0.00";

            //set Border      
            ws.Range("A5:E" + (index - 2)).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            ws.Range("A5:E" + (index - 2)).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            ws.Columns().AdjustToContents();

            return wb;
        }
    }
}
