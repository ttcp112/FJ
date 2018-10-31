using NuWebNCloud.Shared.Models;
using System;
using System.Configuration;
using System.IO;
using System.Net;

namespace NuWebNCloud.Shared.Utilities
{
    public static class FTP
    {
        public static bool CopyFile(string fileName, string fileToCopy)
        {
            try
            {
                UserSession currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                {
                    Commons._ftpHost = currentUser.FTPHost;
                    Commons._userName = currentUser.FTPUser;
                    Commons._password = currentUser.FTPPassword;
                }

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(Commons._ftpHost + fileName);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                request.Credentials = new NetworkCredential(Commons._userName, Commons._password);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                Upload(fileToCopy, ToByteArray(responseStream));
                if (responseStream != null)
                    responseStream.Close();
                return true;
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                return false;
            }
        }

        public static Byte[] ToByteArray(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            byte[] chunk = new byte[4096];
            int bytesRead;
            while ((bytesRead = stream.Read(chunk, 0, chunk.Length)) > 0)
            {
                ms.Write(chunk, 0, bytesRead);
            }
            return ms.ToArray();
        }

        public static bool Upload(string fileName, byte[] image)
        {
            try
            {
                UserSession currentUser = (UserSession)System.Web.HttpContext.Current.Session["User"];
                if (currentUser != null)
                {
                    Commons._ftpHost = currentUser.FTPHost;
                    Commons._userName = currentUser.FTPUser;
                    Commons._password = currentUser.FTPPassword;
                }
                FtpWebRequest clsRequest = (FtpWebRequest)WebRequest.Create(Commons._ftpHost + fileName);
                clsRequest.Credentials = new NetworkCredential(Commons._userName, Commons._password);
                clsRequest.Method = WebRequestMethods.Ftp.UploadFile;
                Stream clsStream = clsRequest.GetRequestStream();
                clsStream.Write(image, 0, image.Length);
                clsStream.Close();
                clsStream.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                return false;
            }
        }

    }
}
