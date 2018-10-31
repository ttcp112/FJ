using ClosedXML.Excel;
using Newtonsoft.Json;
using NuWebNCloud.Shared.Models;
using NuWebNCloud.Shared.Models.Api;
using NuWebNCloud.Shared.Models.Sandbox.Import;
using NuWebNCloud.Shared.Models.Settings;
using NuWebNCloud.Shared.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NuWebNCloud.Shared.Factory.Settings
{
    public class ChatTemplateFactory : BaseFactory
    {
        private BaseFactory _baseFactory = null;
        public ChatTemplateFactory()
        {
            _baseFactory = new BaseFactory();
        }

        public List<ChatTemplateModels> GetListChattingTemplate(List<string> ListOrgID , bool IsActive = false, int? type = null,string Id = null)
        {
            List<ChatTemplateModels> listdata = new List<ChatTemplateModels>();
            try
            {
                ChatTemplateApiModels paraBody = new ChatTemplateApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ListOrgID = ListOrgID;
                paraBody.IsActive = IsActive;
                paraBody.ChatTemplateType = type;
                paraBody.Id = Id;
                NSLog.Logger.Info("GetListGeneralSetting_Input", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ChattingGet, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListChatTemplate"];
                var lstContent = JsonConvert.SerializeObject(lstZ);
                NSLog.Logger.Info("GetListGeneralSetting_Output", lstContent);
                listdata = JsonConvert.DeserializeObject<List<ChatTemplateModels>>(lstContent);
                return listdata;
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("GetListGeneralSetting_error: ", e);
                return listdata;
            }
        }

        public bool InsertOrUpdateChattingTemplate(ChatTemplateModels model,ref string msg)
        {
            try
            {
                ChatTemplateApiModels paraBody = new ChatTemplateApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ListOrgID = model.ListOrgID;
                paraBody.ChatTemplateDTO = model;
                NSLog.Logger.Info("InsertOrUpdateChattingTemplate_Input", paraBody);
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ChattingCreateOrEdit, null, paraBody);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        NSLog.Logger.Info("InsertOrUpdateChattingTemplate_Output", result);
                        return false;
                    }
                }
                else
                {
                    NSLog.Logger.Info("InsertOrUpdateChattingTemplate_Output", result);
                    return false;
                }
            }
            catch (Exception e)
            {
                NSLog.Logger.Error("InsertOrUpdateChattingTemplate_Error: ", e);
                return false;
            }
        }

        public bool DeleteChatting(string ID, ref string msg)
        {
            try
            {
                ChatTemplateApiModels paraBody = new ChatTemplateApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;
                paraBody.Id = ID;

                NSLog.Logger.Info("DeleteChatting_Input", paraBody);
                //====================
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ChattingDelete, null, paraBody);
                NSLog.Logger.Info("DeleteChatting_Output", result);
                if (result != null)
                {
                    if (result.Success)
                        return true;
                    else
                    {
                        msg = result.Message;
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                msg = e.ToString();
                NSLog.Logger.Error("DeleteChatting_Error ", e);
                return false;
            }
        }

        public StatusResponse Export(ref IXLWorksheet ws, List<string> ListOrgID , int? Type = null)
        {
            StatusResponse Response = new StatusResponse();
            try
            {
                ChatTemplateApiModels paraBody = new ChatTemplateApiModels();
                paraBody.AppKey = Commons.AppKey;
                paraBody.AppSecret = Commons.AppSecret;
                paraBody.CreatedUser = Commons.CreateUser;

                paraBody.ListOrgID = ListOrgID;
                paraBody.ChatTemplateType = Type;
                NSLog.Logger.Info("Export_Input", paraBody);
                var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ChattingExport, null, paraBody);
                dynamic data = result.Data;
                var lstZ = data["ListChatTemplate"];
                var lstContent = JsonConvert.SerializeObject(lstZ);
                NSLog.Logger.Info("Export_Output", lstContent);
                var listdata = JsonConvert.DeserializeObject<List<ChatTemplateModels>>(lstContent);
                int cols = 5;
                //Header
                int row = 1;
                ws.Cell("A" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Index");
                ws.Cell("B" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Name");
                ws.Cell("C" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Status");
                ws.Cell("D" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Description");
                ws.Cell("E" + row).Value = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Type");

                //Item
                row = 2;
                int countIndex = 1;
                foreach (var item in listdata)
                {
                    ws.Cell("A" + row).Value = countIndex;
                    ws.Cell("B" + row).Value = item.Name;
                    ws.Cell("C" + row).Value = item.IsActive ? _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Active") : _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("InActive");
                    ws.Cell("D" + row).Value = item.Description;

                    ws.Cell("E" + row).Value = item.sType;
                    row++;
                    countIndex++;
                }
                FormatExcelExport(ws, row, cols);
                Response.Status = true;
            }
            catch (Exception e)
            {
                Response.Status = false;
                Response.MsgError = e.Message;
            }
            return Response;
        }

        public StatusResponse Import(HttpPostedFileBase excelFile, List<string> ListOrgID, ref ImportModel importModel, ref string msg)
        {
            StatusResponse Response = new StatusResponse();
            DataTable dt = new DataTable();
            FileInfo[] lstFileImg = new FileInfo[] { };
            Response = ProcessDataImport(ref dt, excelFile, "SChattingTeamplate.xlsx", out lstFileImg);

            if (!Response.Status)
            {
                msg = Response.MsgError;
                return Response;
            }

            ImportItem itemErr = null;
            bool flagInsert = true;
            string msgError = "";

            List<ChatTemplateModels> listData = new List<ChatTemplateModels>();
            foreach (DataRow row in dt.Rows)
            {
                try
                {
                    flagInsert = true;
                    msgError = "";

                    string rowText = "";

                    for (int i = 0; i < dt.Columns.Count; i++)
                        rowText += row[i].ToString().Trim();

                    if (string.IsNullOrEmpty(rowText))
                        continue;

                    ChatTemplateModels model = new ChatTemplateModels();
                    model.Index = Convert.ToInt32(row[0]);
                    // 1 - Name
                    model.Name = row[1].ToString().Trim().Replace("  ", " ");
                    // 2 - IsActive (status)
                    model.IsActive = GetBoolValue(dt.Columns[2].ColumnName.Replace("#", "."), row[0].ToString(), row[2].ToString());
                    //description
                    model.Description = row[3].ToString().Trim().Replace("  ", " ");
                    model.ChatTemplateType = row[4].ToString().Trim().ToLower().Replace("  ", " ").Equals("artiste") ? (int)Commons.EChatTemplate.Artiste : (int)Commons.EChatTemplate.Customer;
                    //==============

                    if (flagInsert)
                    {
                        listData.Add(model);
                    }
                    else
                    {
                        itemErr = new ImportItem();
                        itemErr.Name = model.Name;
                        itemErr.ListFailStoreName.Add("");
                        itemErr.ListErrorMsg.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ":" + model.Index + msgError);
                        importModel.ListImport.Add(itemErr);
                    }
                }
                catch (Exception e)
                {
                    NSLog.Logger.Error("Import Chatting Error", e);
                    importModel.ListImport.Add(new ImportItem { Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Chatting Template"), ListFailStoreName = ListOrgID, ListErrorMsg = new List<string> { e.Message } });
                }
            }
            Response.Status = true;
            //=====================

            //try
            //{
            ChatTemplateApiModels paraBody = new ChatTemplateApiModels();
            paraBody.AppKey = Commons.AppKey;
            paraBody.AppSecret = Commons.AppSecret;
            paraBody.CreatedUser = Commons.CreateUser;
            paraBody.ListOrgID = ListOrgID;
            paraBody.ListTemplate = listData;
            //====================
            NSLog.Logger.Info("Import Chatting", paraBody);
            var result = (ResponseApiModels)ApiResponse.Post<ResponseApiModels>(Commons.ChattingImport, null, paraBody);
            NSLog.Logger.Info("Import Chatting result", paraBody);
            if (result != null)
            {
                dynamic data = result.Data;
                var lstC = data["ListProperty"];
                var lstContent = JsonConvert.SerializeObject(lstC);
                var listError = JsonConvert.DeserializeObject<List<ImportResult>>(lstContent);

                foreach (ImportResult itemError in listError)
                {
                    ImportItem item = new ImportItem();
                    item.Name = itemError.Property;
                    item.ListFailStoreName.Add(itemError.StoreName);
                    item.ListErrorMsg.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Row") + ": " + itemError.Index + "<br/>"
                        + _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey(itemError.Error));
                    importModel.ListImport.Add(item);
                }
                if (importModel.ListImport.Count == 0)
                {
                    ImportItem item = new ImportItem();
                    item.Name = _AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Chat List");
                    item.ListSuccessStoreName.Add(_AttributeForLanguage.CurrentUser.GetLanguageTextFromKey("Import Tax Successful"));
                    importModel.ListImport.Add(item);
                }
            }
            return Response;
        }
    }
}
