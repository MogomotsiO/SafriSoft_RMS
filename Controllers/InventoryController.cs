using OfficeOpenXml;
using Rotativa;
using SafriSoftv1._3.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SafriSoft.Controllers
{
    public class InventoryController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult User()
        {
            return View();
        }

        public ActionResult Stock()
        {
            return View();
        }

        public ActionResult Customers()
        {
            return View();
        }

        public ActionResult Orders()
        {
            return View();
        }

        public ActionResult Inbox()
        {
            return View();
        }
        
        public ActionResult CustomerInvoice(string Id)
        {
            ViewBag.Title = Id;
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            var InvoiceViewModel = new List<InvoiceViewModel>();
            var OrganisationViewModel = new List<OrganisationViewModel>();
            var OrderViewModel = new List<OrderViewModel>();
            var InvoiceDetails = new InvoiceViewModel();
            var customerId = "";

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = string.Format("SELECT [OrganisationId],[OrganisationName],[OrganisationEmail],[OrganisationCell],[OrganisationLogo],[OrganisationStreet],[OrganisationSuburb],[OrganisationCity],[OrganisationCode],[AccountName],[AccountNo],[BankName],[BranchName],[BranchCode],[ClientReference],[VATNumber] from [{0}].[dbo].[Organisations]", conn.Database);

                    var orderCmd = conn.CreateCommand();
                    orderCmd.CommandText = string.Format("SELECT [OrderId],[ProductName],[NumberOfItems],[CustomerId],[OrderWorth],[ShippingCost],[DateOrderCreated] from [{0}].[dbo].[Orders] where OrderId = '{1}'", conn.Database, "#"+ Id);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InvoiceDetails.OrganisationName = reader.GetString(1);
                            InvoiceDetails.OrganisationEmail = reader.GetString(2) != null ? reader.GetString(2) : "";
                            InvoiceDetails.OrganisationCell = reader.GetString(3) != null ? reader.GetString(3) : "";
                            InvoiceDetails.OrganisationLogo = reader.GetString(4);
                            InvoiceDetails.OrganisationStreet = reader.GetString(5);
                            InvoiceDetails.OrganisationSuburb = reader.GetString(6);
                            InvoiceDetails.OrganisationCity = reader.GetString(7);
                            InvoiceDetails.OrganisationCode = reader.GetInt32(8);
                            InvoiceDetails.AccountName = reader.GetString(9);
                            InvoiceDetails.AccountNo = reader.GetInt32(10);
                            InvoiceDetails.BankName = reader.GetString(11);
                            InvoiceDetails.BranchName = reader.GetString(12);
                            InvoiceDetails.BranchCode = reader.GetString(13);
                            InvoiceDetails.ClientReference = reader.GetString(14);
                            InvoiceDetails.VATNumber = reader.GetInt32(15);
                        }
                        reader.NextResult();
                        reader.Close();
                    }

                    using (var reader = orderCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InvoiceDetails.OrderId = reader.GetString(0);
                            InvoiceDetails.ProductName = reader.GetString(1);
                            InvoiceDetails.NumberOfItems = reader.GetInt32(2);
                            customerId = reader.GetInt32(3).ToString();
                            InvoiceDetails.OrderWorth = reader.GetDecimal(4);
                            InvoiceDetails.ShippingCost = reader.GetDecimal(5);
                            InvoiceDetails.DateOrderCreated = reader.GetString(6);
                        }
                        reader.NextResult();
                        reader.Close();
                    }

                    var customerCmd = conn.CreateCommand();
                    customerCmd.CommandText = string.Format("SELECT [CustomerName],[CustomerEmail],[CustomerCell],[CustomerAddress] from [{0}].[dbo].[Customer] where Id = '{1}'", conn.Database, customerId);

                    using (var reader = customerCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InvoiceDetails.CustomerName = reader.GetString(0);
                            InvoiceDetails.CustomerEmail = reader.GetString(1);
                            InvoiceDetails.CustomerCell = reader.GetString(2);
                            InvoiceDetails.CustomerAddress = reader.GetString(3);
                        }
                        reader.NextResult();
                        reader.Close();
                    }

                    // Totals
                    InvoiceDetails.VatAmount = InvoiceDetails.OrderWorth * decimal.Parse("0,15");
                    InvoiceDetails.InvoiceTotal = InvoiceDetails.OrderWorth + InvoiceDetails.VatAmount + InvoiceDetails.ShippingCost;

                    // Format values
                    InvoiceDetails.OrderWorth = decimal.Parse(InvoiceDetails.OrderWorth.ToString("C", CultureInfo.CreateSpecificCulture("en-GB")));
                    InvoiceDetails.VatAmount = decimal.Parse(InvoiceDetails.VatAmount.ToString("C", CultureInfo.CreateSpecificCulture("en-ZA")));
                    InvoiceDetails.ShippingCost = decimal.Parse(InvoiceDetails.ShippingCost.ToString("C", CultureInfo.CreateSpecificCulture("en-ZA")));
                    InvoiceDetails.InvoiceTotal = decimal.Parse(InvoiceDetails.InvoiceTotal.ToString("C", CultureInfo.CreateSpecificCulture("en-ZA")));

                    InvoiceViewModel.Add(InvoiceDetails);
                }
                
            }
            catch (Exception Ex)
            {
                
            }

            return View(InvoiceDetails);
            //using (var excelPackage = new ExcelPackage())
            //{
            //    return File(excelPackage.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AllMachinesWithOpticalDrives.xlsx");
            //}
        }

        public ActionResult CustomerInvoicePdf(string OrderId)
        {
            return new ActionAsPdf("CustomerInvoice", new { Id = OrderId })
            {
                FileName = Server.MapPath("CustomerInvoice.pdf")
            };
            //using (var excelPackage = new ExcelPackage())
            //{
            //    return File(excelPackage.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AllMachinesWithOpticalDrives.xlsx");
            //}
        }

        public ActionResult CustomerReport()
        {
            return View();
        }

        public ActionResult CustomerReportPdf()
        {
            return new ActionAsPdf("CustomerReport")
            {
                FileName = Server.MapPath("~/Content/CustomerReport.pdf")
            };
        }

        public ActionResult InvetoryReport()
        {
            return View();
        }

        public ActionResult InvetoryReportPdf()
        {
            return new ActionAsPdf("InvetoryReport")
            {
                FileName = Server.MapPath("~/Content/InvetoryReport.pdf")
            };
        }

        public ActionResult OrdersReport()
        {
            return View();
        }

        public ActionResult OrdersReportPdf()
        {
            return new ActionAsPdf("OrdersReport")
            {
                FileName = Server.MapPath("~/Content/OrdersReport.pdf")
            };
        }

        public ActionResult AttentionReport()
        {
            return View();
        }

        public ActionResult AttentionReportPdf()
        {
            return new ActionAsPdf("AttentionReport")
            {
                FileName = Server.MapPath("~/Content/AttentionReport.pdf")
            };
        }
    }
}