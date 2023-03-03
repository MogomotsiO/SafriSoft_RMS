using Rotativa;
using SafriSoftv1._3.Models;
using SafriSoftv1._3.Models.Rental;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SafriSoftv1._3.Controllers.API;
using Newtonsoft.Json.Linq;
using SafriSoftv1._3.Models.Data;

namespace SafriSoftv1._3.Controllers
{
    //[Authorize, RoutePrefix("Rental")]
    public class RentalController : Controller
    {
        // GET: Rental
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult RentalUsers()
        {
            return View();
        }

        // GET: Tenants
        public ActionResult Tenants()
        {
            return View();
        }

        // GET: Units
        public ActionResult Units()
        {
            return View();
        }

        // GET: Projections
        public ActionResult Projections()
        {
            return View();
        }

        // GET: Statements/Invoices
        public ActionResult Statements()
        {
            return View();
        }

        // GET: PayNow
        public ActionResult PayNow(string TransactionFor, string Date)
        {
            ViewBag.TransactionFor = TransactionFor;
            ViewBag.Date = Date;
            return View();
        }

        //[Route("DownloadFile")]
        public async Task<ActionResult> DownloadFile(string fileId = "")
        {
            string fileName = "";
            byte[] fileByte = null;
            string fileContentType = "";

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = string.Format("SELECT [Id],[FileName],[FileByte],[FileContentType],[DateFileCreated] from [{0}].[dbo].[Document] where Id = '{1}'", conn.Database, fileId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            fileName = reader.GetString(1);
                            fileByte = DeCompressStream((byte[])reader.GetValue(2));
                            fileContentType = reader.GetString(3);
                        }
                        reader.NextResult();
                        reader.Close();
                    }

                }
                return File(fileByte, fileContentType, fileName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error", ex);
            }

        }

        public ActionResult TenantsReport()
        {
            return View();
        }

        public ActionResult TenantsReportPdf()
        {
            return new ActionAsPdf("TenantsReport")
            {
                FileName = Server.MapPath("~/Content/TenantsReport.pdf")
            };
        }

        public ActionResult UnitsReport()
        {
            return View();
        }

        public ActionResult UnitsReportPdf()
        {
            return new ActionAsPdf("UnitsReport")
            {
                FileName = Server.MapPath("~/Content/UnitsReport.pdf")
            };
        }

        public ActionResult ExpectedPaymentsReport()
        {
            return View();
        }

        public ActionResult ExpectedPaymentsReportPdf()
        {
            return new ActionAsPdf("ExpectedPaymentsReport")
            {
                FileName = Server.MapPath("~/Content/ExpectedPaymentsReport.pdf")
            };
        }

        public ActionResult RecievedPaymentsReport()
        {
            return View();
        }

        public ActionResult RecievedPaymentsReportPdf()
        {
            return new ActionAsPdf("RecievedPaymentsReport")
            {
                FileName = Server.MapPath("~/Content/RecievedPaymentsReport.pdf")
            };
        }


        public ActionResult OnlineOnceOffPayment(string TransactionFor, string Date)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();

            var merchantId = "10019272";
            var merchantKey = "cs04x9ioe64yz";
            var passPhrase = "Onceoffonlinepayment1";
            var processUrl = "https://sandbox.payfast.co.za/eng/process?";
            //var validateUrl = "https://sandbox.payfast.co.za/eng/query/validate";
            var returnUrl = "http://safrisoft.com/Rental/PayFastReturn?payFastReturnVal=Success";
            var cancelUrl = "http://safrisoft.com/Rental/PayFastReturn?payFastReturnVal=Cancel";
            var notifyUrl = "http://safrisoft.com/Rental/PayFastReturn?payFastReturnVal=Notify";

            decimal? totalAmount = 0;
            var description = "";
            var transactionId = TransactionFor + "/" + Date;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var orderCmd = conn.CreateCommand();
                orderCmd.CommandText = string.Format("SELECT [Id],[TransactionCode],[TransactionName],[TransactionAmount],[TransactionDate],[TenantId] from [{0}].[dbo].[Transaction] Where TenantId = '{1}' AND TransactionDate = '{2}' AND (TransactionCode = '{3}' OR TransactionCode = '{4}')", conn.Database, TransactionFor, DateTime.Parse(Date), 10, 20);


                using (var reader = orderCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        description = reader.GetString(2) + " " + description;
                        totalAmount = totalAmount + reader.GetDecimal(3);
                    }
                    reader.NextResult();
                    reader.Close();
                }
                conn.Close();
            }

            int tenantId = int.Parse(TransactionFor);
            var organisation = SafriSoft.Organisations.FirstOrDefault();
            var tenant = SafriSoft.Tenants.FirstOrDefault(x => x.Id == tenantId);

            var onceOffRequest = new PayFastRequestModel(passPhrase);

            // Merchant Details
            onceOffRequest.merchant_id = merchantId;
            onceOffRequest.merchant_key = merchantKey;
            onceOffRequest.return_url = returnUrl + "&TransactionFor=" + TransactionFor + "&Date=" + Date;
            onceOffRequest.cancel_url = cancelUrl + "&TransactionFor=" + TransactionFor + "&Date=" + Date;
            onceOffRequest.notify_url = notifyUrl + "&TransactionFor=" + TransactionFor + "&Date=" + Date;

            // Buyer Details
            onceOffRequest.email_address = tenant.TenantEmail;

            // Transaction Details
            onceOffRequest.m_payment_id = transactionId;
            onceOffRequest.amount = totalAmount.ToString().Replace(",", ".");
            onceOffRequest.item_name = "Rental Amount";
            onceOffRequest.item_description = "Rental Amount";

            // Transaction Options
            onceOffRequest.email_confirmation = true;
            onceOffRequest.confirmation_address = organisation.OrganisationEmail;

            var redirectUrl = $"{processUrl}{onceOffRequest.ToString()}";

            return Redirect(redirectUrl);
        }


        public ActionResult PayFastReturn(string payFastReturnVal, string TransactionFor, string Date)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            var listTransaction = new List<Transaction>();
            JObject o = new JObject();

            if (payFastReturnVal == "Success")
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                {
                    conn.Open();
                    var orderCmd = conn.CreateCommand();
                    orderCmd.CommandText = string.Format("SELECT [Id],[TransactionCode],[TransactionName],[TransactionAmount],[TransactionDate],[TenantId] from [{0}].[dbo].[Transaction] Where TenantId = '{1}' AND TransactionDate = '{2}' AND (TransactionCode = '{3}' OR TransactionCode = '{4}')", conn.Database, TransactionFor, DateTime.Parse(Date), 10, 20);


                    using (var reader = orderCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var transaction = new Transaction();
                            transaction.Id = reader.GetInt32(0);
                            transaction.TransactionCode = reader.GetInt32(1);
                            transaction.TransactionName = reader.GetString(2);
                            transaction.TransactionAmount = reader.GetDecimal(3);
                            transaction.TransactionDate = reader.GetDateTime(4);
                            transaction.TenantId = reader.GetInt32(5);
                            listTransaction.Add(transaction);
                        }
                        reader.NextResult();
                        reader.Close();
                    }
                    conn.Close();
                }

                foreach (var transaction in listTransaction)
                {
                    var transactionCode = 0;
                    var transactionName = "";
                    var transactionLink = 0;
                    var dateNow = DateTime.Now;
                    var date = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day);

                    if (transaction.TransactionCode == 20)
                    {
                        transactionCode = 25;
                        transactionName = "Initial - Deposit - Paid";
                        transactionLink = transaction.Id;

                    }
                    else if (transaction.TransactionCode == 10)
                    {
                        transactionCode = 15;
                        transactionName = "Monthly - Rental - Paid";
                        transactionLink = transaction.Id;
                    }

                    o = RaiseTransaction(transactionCode, transactionName, transaction.TransactionAmount, date, int.Parse(TransactionFor), transactionLink);
                }
            }            

            return RedirectToAction("PayNow", "Rental", new {TransactionFor,Date });
        }

        // Reusable functions at the bottom
        public static JObject RaiseTransaction(int TransactionCode, string TransactionName, decimal? TransactionAmount, DateTime DateTransactionRaised, int TenantId, int TransactionLink)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            JObject o = new JObject();

            var transactionCheck = SafriSoft.Transactions.FirstOrDefault(x => x.TransactionDate == DateTransactionRaised && x.TenantId == TenantId && x.TransactionCode == TransactionCode && x.TransactionLink == TransactionLink);

            if (transactionCheck != null)
            {
                o["Success"] = false;
                o["Message"] = "This transaction already exists in the database";
            }
            else
            {
                try
                {
                    using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                    {
                        conn.Open();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[Transaction] ([TransactionCode],[TransactionName],[TransactionDate],[TenantId],[TransactionLink]) VALUES('{1}','{2}','{3}','{4}','{5}')", conn.Database, TransactionCode, TransactionName, DateTransactionRaised, TenantId, TransactionLink);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    var transaction = SafriSoft.Transactions.First(x => x.TransactionDate == DateTransactionRaised && x.TenantId == TenantId && x.TransactionCode == TransactionCode && x.TransactionLink == TransactionLink);
                    transaction.TransactionAmount = TransactionAmount;
                    SafriSoft.SaveChanges();
                    o["Success"] = true;
                }
                catch (Exception Ex)
                {
                    o["Success"] = false;
                    o["Message"] = Ex.Message;
                }
            }

            return o;
        }

        public static byte[] DeCompressStream(byte[] compressedData)
        {
            using (var msi = new MemoryStream(compressedData))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    msi.CopyTo(mso);

                    return mso.ToArray();
                }
            }
        }

    }
}