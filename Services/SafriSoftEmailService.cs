using SafriSoftv1._3.Models;
using SafriSoftv1._3.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SafriSoftv1._3.Services
{
    public class SafriSoftEmailService
    {
        public StringBuilder BuildEmailHeader()
        {
            var header = new StringBuilder();

            header.Append("<h1 style='color:#17a2b8;'>SafriSoft.</h1>");
            header.Append("<font style='color:#595a5c;text-align: left;'>Dear Client<br/><br/>");

            return header;
        }

        public bool SaveEmail(string subject, string body, string fromAddress, string[] toAddress, string[] toCcAddress)
        {
            ApplicationDbContext SafriSoft = new ApplicationDbContext();

            var email = new Email();

            bool success = false;

            try
            {
                email.Subject = subject;
                email.Body = body;
                email.FromAddress = fromAddress;
                email.ToAddress = string.Join(";", toAddress);
                email.CcAddress = string.Join(";", toCcAddress);
                email.EmailStatus = "Process";

                SafriSoft.Emails.Add(email);
                SafriSoft.SaveChanges();

                success = true;
            }
            catch (Exception ex)
            {
                success = false;
            }

            return success;
        }
    }
}