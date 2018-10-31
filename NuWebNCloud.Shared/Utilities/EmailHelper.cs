using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Exchange.WebServices.Data;

namespace NuWebNCloud.Shared.Utilities
{
    public class EmailHelper
    {
        string _email = string.Empty, _passWord = string.Empty;
        public void SendMail(string toEmail, string subject)
        {
            SendMail(toEmail, string.Empty, subject, null, null, null);
        }
        public void SendMail(string toEmail, string subject, List<string> attachmentFilePaths)
        {
            SendMail(toEmail, string.Empty, subject, null, null, attachmentFilePaths);
        }

        public void SendMail(string toEmail, string subject, List<string> cc, List<string> attachmentFilePaths)
        {
            SendMail(toEmail, string.Empty, subject, cc, null, attachmentFilePaths);
        }
        public void SendMail(string toEmail, string subject, List<string> cc, List<string> bcc, List<string> attachmentFilePaths)
        {
            SendMail(toEmail, string.Empty, subject, cc, bcc, attachmentFilePaths);
        }
        public void SendMail(string toEmail, string content, string subject, List<string> cc, List<string> bcc, List<string> attachmentFilePaths)
        {
            try
            {
                //var Context = new HQDBDataContext(Utility._HQConnection);
                //if (string.IsNullOrEmpty(_email))
                //    _email = Context.G_GeneralSettings.Where(o => o.Index == Constants.EmailIndex).ToList().FirstOrDefault().Value;
                //if (string.IsNullOrEmpty(_passWord))
                //    _passWord = Context.G_GeneralSettings.Where(o => o.Index == Constants.PasswordIndex).ToList().FirstOrDefault().Value;
                if (_email != "" && _passWord != "")
                {
                    ExchangeService service = new ExchangeService(ExchangeVersion.Exchange2007_SP1);
                    service.Url = new Uri("https://owa.newstead.com.sg/ews/Exchange.asmx");
                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
                    service.UseDefaultCredentials = true;
                    service.Credentials = new WebCredentials(_email, _passWord);
                    EmailMessage message = new EmailMessage(service);
                    message.Subject = subject;
                    message.Body = content;
                    message.ToRecipients.Add(toEmail);
                    //BCC
                    if (bcc != null && bcc.Count > 0)
                    {
                        foreach (var address in bcc.Where(bccValue => !String.IsNullOrWhiteSpace(bccValue)))
                        {
                            message.BccRecipients.Add(address.Trim());
                        }
                    }
                    //CC 
                    if (cc != null && cc.Count > 0)
                    {
                        foreach (var address in cc.Where(ccValue => !String.IsNullOrWhiteSpace(ccValue)))
                        {
                            message.CcRecipients.Add(address.Trim());
                        }
                    }
                    if (attachmentFilePaths != null && attachmentFilePaths.Count > 0)
                    {
                        foreach (var item in attachmentFilePaths)
                        {
                            message.Attachments.AddFileAttachment(item);
                        }
                    }
                    message.Save();
                    message.SendAndSaveCopy();

                }
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                //to do
            }
        }

     
    }
}
