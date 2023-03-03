using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json.Linq;
using SafriSoftv1._3.Models;
using SafriSoftv1._3.Models.Data;
using SafriSoftv1._3.Models.Rental;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace SafriSoftv1._3.Controllers.API
{
    [RoutePrefix("api/Rental")]
    public class RentalController : ApiController
    {
        [HttpGet, Route("Home")]
        public IHttpActionResult Home()
        {
            var tenantCount = 0;
            var totalUnits = 0;
            var unitsNotSharing = 0;
            var unitsSharing = 0;
            var unitsAvailable = 0;
            var unitsOccupied = 0;
            decimal randValueSold = 0;
            decimal expectedPayments = 0;
            var today = DateTime.Now;
            var startDate = new DateTime(today.Year, today.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                {
                    conn.Open();
                    var customerCountCmd = conn.CreateCommand();
                    customerCountCmd.CommandText = string.Format("SELECT count([Id]) from [{0}].[dbo].[Tenant]", conn.Database);
                    try
                    {
                        tenantCount = (Int32)customerCountCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        tenantCount = 0;
                    }

                    var totalUnitsCmd = conn.CreateCommand();
                    totalUnitsCmd.CommandText = string.Format("SELECT count([Id]) from [{0}].[dbo].[Unit]", conn.Database);
                    try
                    {
                        totalUnits = (Int32)totalUnitsCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        totalUnits = 0;
                    }

                    // Sum up all the units that are not sharing
                    var unitsNotSharingCmd = conn.CreateCommand();
                    unitsNotSharingCmd.CommandText = string.Format("SELECT count([Id]) from [{0}].[dbo].[Unit] where Sharing = '{1}'", conn.Database, "No");
                    try
                    {
                        unitsNotSharing = (Int32)unitsNotSharingCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        unitsNotSharing = 0;
                    }

                    // Sum up all the units that are sharing
                    var unitsSharingCmd = conn.CreateCommand();
                    unitsSharingCmd.CommandText = string.Format("SELECT sum([UnitRooms]) from [{0}].[dbo].[Unit] where Sharing = '{1}'", conn.Database, "Yes");
                    try
                    {
                        unitsSharing = (Int32)unitsSharingCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        unitsSharing = 0;
                    }

                    unitsAvailable = unitsNotSharing + unitsSharing;

                    var unitsOccupiedCmd = conn.CreateCommand();
                    unitsOccupiedCmd.CommandText = string.Format("SELECT count([Id]) from [{0}].[dbo].[Assigned]", conn.Database);
                    try
                    {
                        unitsOccupied = (Int32)unitsOccupiedCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        unitsOccupied = 0;
                    }

                    unitsAvailable = unitsAvailable - unitsOccupied;

                    var randValueSoldCmd = conn.CreateCommand();
                    randValueSoldCmd.CommandText = string.Format("SELECT sum([TransactionAmount]) from [{0}].[dbo].[Transaction] Where TransactionDate >= '{1}' AND TransactionDate <= '{2}' AND (TransactionCode = '{3}' OR TransactionCode = '{4}')", conn.Database, startDate, endDate, 15, 25);
                    try
                    {
                        randValueSold = (Decimal)randValueSoldCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        randValueSold = decimal.Parse("0");
                    }

                    
                    var expectedPaymentsCmd = conn.CreateCommand();
                    expectedPaymentsCmd.CommandText = string.Format("SELECT sum([TransactionAmount]) from [{0}].[dbo].[Transaction] Where TransactionDate >= '{1}' AND TransactionDate <= '{2}' AND (TransactionCode = '{3}' OR TransactionCode = '{4}')", conn.Database, startDate, endDate, 10, 20);
                    try
                    {
                        expectedPayments = (Decimal)expectedPaymentsCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        expectedPayments = decimal.Parse("0");
                    }


                }
                return Json(new { Success = true, Tenants = tenantCount, TotalUnits = totalUnits, UnitsAvailable = unitsAvailable, UnitsOccupied = unitsOccupied, RandValueSold = randValueSold, ExpectedPayments = expectedPayments, Sharing = unitsSharing, NotSharing = unitsNotSharing });
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString() });
            }

        }

        [HttpGet, Route("GetTenants")]
        public async Task<IHttpActionResult> GetTenants()
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var TenantViewModel = new List<TenantViewModel>();
            var countUsers = 0;
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[TenantName],[TenantEmail],[TenantCell],[TenantAddress],[TenantWorkAddress],[TenantWorkCell],[DateTenantCreated],[DateLeaseStart],[DateLeaseEnd],[Status] from [{0}].[dbo].[Tenant] where Status = '{1}'", conn.Database, "Active");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var Tenants = new TenantViewModel();
                        countUsers += 1;
                        Tenants.Id = reader.GetInt32(0);
                        Tenants.TenantName = reader.GetString(1);
                        Tenants.TenantEmail = reader.GetString(2);
                        Tenants.TenantCell = reader.GetString(3);
                        Tenants.TenantAddress = reader.GetString(4);
                        Tenants.TenantWorkAddress = reader.GetString(5);
                        Tenants.TenantWorkCell = reader.GetString(6);
                        Tenants.DateTenantCreated = reader.GetDateTime(7).ToString("dd MMMM yyyy");
                        Tenants.DateLeaseStart = reader.GetDateTime(8).ToString("dd MMMM yyyy");
                        Tenants.DateLeaseEnd = reader.GetDateTime(9).ToString("dd MMMM yyyy");
                        Tenants.Documents = SafriSoft.Documents.Where(x => x.TenantId == Tenants.Id).Count();
                        Tenants.NOK = SafriSoft.NOKs.Where(x => x.TenantId == Tenants.Id).Count();
                        TenantViewModel.Add(Tenants);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(TenantViewModel);
            }
        }
        
        [HttpPost, Route("TenantCreate")]
        public async Task<IHttpActionResult> TenantCreate(TenantViewModel TenantData)
        {
            var tenantName = TenantData.TenantName;
            var tenantEmail = TenantData.TenantEmail;
            var tenantAddress = TenantData.TenantAddress;
            var tenantCell = TenantData.TenantCell;
            var tenantWorkCell = TenantData.TenantWorkCell;
            var tenantWorkAddress = TenantData.TenantWorkAddress;
            var dateTenantCreated = DateTime.Now;
            var dateLeaseStart = TenantData.DateLeaseStart != null && TenantData.DateLeaseStart != "" ? DateTime.Parse(TenantData.DateLeaseStart) : DateTime.Now;
            var dateLeaseEnd = TenantData.DateLeaseEnd != null && TenantData.DateLeaseEnd != "" ? DateTime.Parse(TenantData.DateLeaseEnd) : DateTime.Now;

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[Tenant] ([TenantName],[TenantEmail],[TenantCell],[TenantAddress],[TenantWorkAddress],[TenantWorkCell],[DateTenantCreated],[DateLeaseStart],[DateLeaseEnd],[Status]) VALUES('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}')", conn.Database, tenantName, tenantEmail, tenantCell, tenantAddress, tenantWorkAddress, tenantWorkCell, dateTenantCreated, dateLeaseStart, dateLeaseEnd, "Active");
                    await cmd.ExecuteNonQueryAsync();

                }
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString() });
            }

        }

        [HttpGet, Route("GetTenant/{Id}")]
        public async Task<IHttpActionResult> GetTenant(int Id)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var TenantViewModel = new List<TenantViewModel>();
            var countUsers = 0;
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[TenantName],[TenantEmail],[TenantCell],[TenantAddress],[TenantWorkAddress],[TenantWorkCell],[DateTenantCreated],[DateLeaseStart],[DateLeaseEnd],[Status] from [{0}].[dbo].[Tenant] where Status = '{1}' AND Id = '{2}'", conn.Database, "Active", Id);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var Tenants = new TenantViewModel();
                        countUsers += 1;
                        Tenants.Id = reader.GetInt32(0);
                        Tenants.TenantName = reader.GetString(1);
                        Tenants.TenantEmail = reader.GetString(2);
                        Tenants.TenantCell = reader.GetString(3);
                        Tenants.TenantAddress = reader.GetString(4);
                        Tenants.TenantWorkAddress = reader.GetString(5);
                        Tenants.TenantWorkCell = reader.GetString(6);
                        Tenants.DateTenantCreated = reader.GetDateTime(7).ToString("MM/dd/yyyy");
                        Tenants.DateLeaseStart = reader.GetDateTime(8).ToString("yyyy-MM-dd");
                        Tenants.DateLeaseEnd = reader.GetDateTime(9).ToString("yyyy-MM-dd");
                        Tenants.Documents = 0;
                        Tenants.NOK = 0;
                        TenantViewModel.Add(Tenants);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(TenantViewModel);
            }
        }

        [HttpPost, Route("TenantUpdate")]
        public async Task<IHttpActionResult> TenantUpdate(TenantViewModel TenantData)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();

            try
            {
                var dbTenant = SafriSoft.Tenants.First(x => x.Id == TenantData.Id);

                if (dbTenant.TenantName != TenantData.TenantName)
                {
                    dbTenant.TenantName = TenantData.TenantName;
                }

                if (dbTenant.TenantEmail != TenantData.TenantEmail)
                {
                    dbTenant.TenantEmail = TenantData.TenantEmail;
                }

                if (dbTenant.TenantCell != TenantData.TenantCell)
                {
                    dbTenant.TenantCell = TenantData.TenantCell;
                }

                if (dbTenant.TenantAddress != TenantData.TenantAddress)
                {
                    dbTenant.TenantAddress = TenantData.TenantAddress;
                }

                if (dbTenant.TenantWorkAddress != TenantData.TenantWorkAddress)
                {
                    dbTenant.TenantWorkAddress = TenantData.TenantWorkAddress;
                }

                if (dbTenant.TenantWorkCell != TenantData.TenantWorkCell)
                {
                    dbTenant.TenantWorkCell = TenantData.TenantWorkCell;
                }

                if (dbTenant.DateLeaseStart != DateTime.Parse(TenantData.DateLeaseStart))
                {
                    dbTenant.DateLeaseStart = DateTime.Parse(TenantData.DateLeaseStart);
                }

                if (dbTenant.DateLeaseEnd != DateTime.Parse(TenantData.DateLeaseEnd))
                {
                    dbTenant.DateLeaseEnd = DateTime.Parse(TenantData.DateLeaseEnd);
                }

                SafriSoft.SaveChanges();
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return BadRequest(Ex.ToString());
            }

        }

        [HttpPost, Route("NOKCreate")]
        public async Task<IHttpActionResult> NOKCreate(NOKViewModel NOKData)
        {
            var nokName = NOKData.NOKName;
            var nokEmail = NOKData.NOKEmail;
            var nokCell = NOKData.NOKCell;
            var nokRelation = NOKData.NOKRelation;
            var nokTenantId = NOKData.TenantId;
            var dateNOKCreated = DateTime.Now;

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[NOK] ([NOKName],[NOKEmail],[NOKCell],[NOKRelation],[DateNOKCreated],[TenantId]) VALUES('{1}','{2}','{3}','{4}','{5}','{6}')", conn.Database, nokName, nokEmail, nokCell, nokRelation, dateNOKCreated, nokTenantId);
                    await cmd.ExecuteNonQueryAsync();

                }
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString() });
            }

        }

        [HttpGet, Route("GetNOKs")]
        public async Task<IHttpActionResult> GetNOKs()
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var NOKViewModel = new List<NOKViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[NOKName],[NOKEmail],[NOKCell],[NOKRelation],[DateNOKCreated] from [{0}].[dbo].[NOK]", conn.Database);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var NOKs = new NOKViewModel();
                        NOKs.Id = reader.GetInt32(0);
                        NOKs.NOKName = reader.GetString(1);
                        NOKs.NOKEmail = reader.GetString(2);
                        NOKs.NOKCell = reader.GetString(3);
                        NOKs.NOKRelation = reader.GetString(4);
                        NOKs.DateNOKCreated = reader.GetDateTime(5).ToString("dd MMMM yyyy");
                        NOKViewModel.Add(NOKs);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(NOKViewModel);
            }
        }

        [HttpGet, Route("GetNOKs/{Id}")]
        public async Task<IHttpActionResult> GetNOKs(int Id)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var NOKViewModel = new List<NOKViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[NOKName],[NOKEmail],[NOKCell],[NOKRelation],[DateNOKCreated] from [{0}].[dbo].[NOK] where TenantId = '{1}'", conn.Database, Id);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var NOKs = new NOKViewModel();
                        NOKs.Id = reader.GetInt32(0);
                        NOKs.NOKName = reader.GetString(1);
                        NOKs.NOKEmail = reader.GetString(2);
                        NOKs.NOKCell = reader.GetString(3);
                        NOKs.NOKRelation = reader.GetString(4);
                        NOKs.DateNOKCreated = reader.GetDateTime(5).ToString("dd MMMM yyyy");
                        NOKViewModel.Add(NOKs);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(NOKViewModel);
            }
        }

        [HttpGet, Route("GetNOK/{Id}")]
        public async Task<IHttpActionResult> GetNOK(int Id)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var NOKViewModel = new List<NOKViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[NOKName],[NOKEmail],[NOKCell],[NOKRelation],[DateNOKCreated] from [{0}].[dbo].[NOK] where Id = '{1}'", conn.Database, Id);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var NOKs = new NOKViewModel();
                        NOKs.Id = reader.GetInt32(0);
                        NOKs.NOKName = reader.GetString(1);
                        NOKs.NOKEmail = reader.GetString(2);
                        NOKs.NOKCell = reader.GetString(3);
                        NOKs.NOKRelation = reader.GetString(4);
                        NOKs.DateNOKCreated = reader.GetDateTime(5).ToString("dd MMMM yyyy");
                        NOKViewModel.Add(NOKs);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(NOKViewModel);
            }
        }

        [HttpPost, Route("NOKUpdate")]
        public async Task<IHttpActionResult> NOKUpdate(NOKViewModel NOKData)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();

            try
            {
                var dbNOK = SafriSoft.NOKs.First(x => x.Id == NOKData.Id);

                if (dbNOK.NOKName != NOKData.NOKName)
                {
                    dbNOK.NOKName = NOKData.NOKName;
                }

                if (dbNOK.NOKEmail != NOKData.NOKEmail)
                {
                    dbNOK.NOKEmail = NOKData.NOKEmail;
                }

                if (dbNOK.NOKCell != NOKData.NOKCell)
                {
                    dbNOK.NOKCell = NOKData.NOKCell;
                }

                if (dbNOK.NOKRelation != NOKData.NOKRelation)
                {
                    dbNOK.NOKRelation = NOKData.NOKRelation;
                }

                SafriSoft.SaveChanges();
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return BadRequest(Ex.ToString());
            }

        }

        [HttpPost, Route("UploadDocument/{Id}")]
        public async Task<IHttpActionResult> UploadDocument(HttpRequestMessage request, int Id)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            HttpContext context = HttpContext.Current;
            HttpPostedFile postedFile = context.Request.Files["files[]"];

            string fileName = postedFile.FileName;
            string fileContentType = postedFile.ContentType;
            byte[] fileBytes = new byte[postedFile.ContentLength];
            postedFile.InputStream.Read(fileBytes, 0, Convert.ToInt32(postedFile.ContentLength));

            byte[] compressedBytes = CompressStream(fileBytes);
            var compressed = fileBytes.Length > compressedBytes.Length ? true : false;

            try
            {
                var document = new Models.Data.Document();
                document.FileName = fileName;
                document.DateFileCreated = DateTime.Now;
                document.FileByte = compressedBytes;
                document.FileContentType = fileContentType;
                document.TenantId = Id;

                var documentAdd = SafriSoft.Documents.Add(document);
                SafriSoft.SaveChanges();
                
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString() });
            }
                          
        }

        [HttpGet, Route("GetDocuments/{Id}")]
        public async Task<IHttpActionResult> GetDocuments(int Id)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var DocumentViewModel = new List<DocumentViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[FileName],[DateFileCreated] from [{0}].[dbo].[Document] where TenantId = '{1}'", conn.Database, Id);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var documents = new DocumentViewModel();
                        documents.Id = reader.GetInt32(0);
                        documents.FileName = reader.GetString(1);
                        documents.DateFileCreated = reader.GetDateTime(2).ToString("dd MMMM yyyy");
                        DocumentViewModel.Add(documents);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(DocumentViewModel);
            }
        }

        [HttpGet, Route("GetDocuments")]
        public async Task<IHttpActionResult> GetDocuments()
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var DocumentViewModel = new List<DocumentViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[FileName],[DateFileCreated] from [{0}].[dbo].[Document]", conn.Database);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var documents = new DocumentViewModel();
                        documents.Id = reader.GetInt32(0);
                        documents.FileName = reader.GetString(1);
                        documents.DateFileCreated = reader.GetDateTime(2).ToString("dd MMMM yyyy");
                        DocumentViewModel.Add(documents);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(DocumentViewModel);
            }
        }

        [HttpGet, Route("GetUnits")]
        public async Task<IHttpActionResult> GetUnits()
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var UnitViewModel = new List<UnitViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[UnitNumber],[UnitName],[UnitRooms],[UnitDescription],[UnitPrice],[Sharing] from [{0}].[dbo].[Unit] where Status = '{1}'", conn.Database,"Active");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var units = new UnitViewModel();
                        units.Id = reader.GetInt32(0);
                        units.UnitNumber = reader.GetString(1);
                        units.UnitName= reader.GetString(2);
                        units.UnitDescription = reader.GetString(4);
                        units.UnitRooms = reader.GetInt32(3);
                        units.UnitPrice = reader.GetDecimal(5);
                        units.UnitSharing = reader.GetString(6);
                        units.NumberOfTenants = SafriSoft.Assigned.Count(x => x.UnitId == units.Id);
                        //units.DateFileCreated = reader.GetDateTime(2).ToString("dd MMMM yyyy");
                        UnitViewModel.Add(units);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(UnitViewModel);
            }
        }

        [HttpPost, Route("UnitCreate")]
        public async Task<IHttpActionResult> UnitCreate(UnitViewModel UnitData)
        {
            var unitNumber = UnitData.UnitNumber;
            var unitName = UnitData.UnitName;
            var unitDescription = UnitData.UnitDescription;
            var unitRooms = UnitData.UnitRooms;
            var unitPrice = UnitData.UnitPrice;
            var unitsharing = UnitData.UnitSharing;
            var dateNOKCreated = DateTime.Now;

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[Unit] ([UnitNumber],[UnitName],[UnitRooms],[UnitDescription],[UnitPrice],[Sharing],[Status]) VALUES('{1}','{2}','{3}','{4}','{5}','{6}','{7}')", conn.Database, unitNumber, unitName, unitRooms, unitDescription, unitPrice, unitsharing, "Active");
                    await cmd.ExecuteNonQueryAsync();

                }
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString() });
            }

        }

        [HttpGet, Route("GetUnit/{Id}")]
        public async Task<IHttpActionResult> GetUnits(int Id)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var UnitViewModel = new List<UnitViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[UnitNumber],[UnitName],[UnitRooms],[UnitDescription],[UnitPrice],[Sharing] from [{0}].[dbo].[Unit] where Id = '{1}'", conn.Database, Id);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var units = new UnitViewModel();
                        units.Id = reader.GetInt32(0);
                        units.UnitNumber = reader.GetString(1);
                        units.UnitName = reader.GetString(2);
                        units.UnitDescription = reader.GetString(4);
                        units.UnitRooms = reader.GetInt32(3);
                        units.UnitPrice = reader.GetDecimal(5);
                        units.UnitSharing = reader.GetString(6);
                        units.NumberOfTenants = 3 /*SafriSoft.Assigned.Count(x=>x.UnitId == reader.GetInt32(0))*/;
                        //units.DateFileCreated = reader.GetDateTime(2).ToString("dd MMMM yyyy");
                        UnitViewModel.Add(units);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(UnitViewModel);
            }
        }

        [HttpPost, Route("UnitUpdate")]
        public async Task<IHttpActionResult> UnitUpdate(UnitViewModel UnitData)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();

            try
            {
                var dbUnit = SafriSoft.Units.First(x => x.Id == UnitData.Id);

                if (dbUnit.UnitNumber != UnitData.UnitNumber)
                {
                    dbUnit.UnitNumber = UnitData.UnitNumber;
                }

                if (dbUnit.UnitName != UnitData.UnitName)
                {
                    dbUnit.UnitName = UnitData.UnitName;
                }

                if (dbUnit.UnitDescription != UnitData.UnitDescription)
                {
                    dbUnit.UnitDescription = UnitData.UnitDescription;
                }

                if (dbUnit.UnitRooms != UnitData.UnitRooms)
                {
                    dbUnit.UnitRooms = UnitData.UnitRooms;
                }

                if (dbUnit.UnitPrice != UnitData.UnitPrice)
                {
                    dbUnit.UnitPrice = UnitData.UnitPrice;
                }

                if (dbUnit.Sharing != UnitData.UnitSharing)
                {
                    dbUnit.Sharing = UnitData.UnitSharing;
                }

                SafriSoft.SaveChanges();
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return BadRequest(Ex.ToString());
            }

        }

        [HttpGet, Route("GetAssignedTenants/{Id}")]
        public async Task<IHttpActionResult> GetAssignedTenants(int Id)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var AssignedTenantViewModel = new List<AssignedTenantViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmdTenants = conn.CreateCommand();
                cmdTenants.CommandText = string.Format("SELECT [Id],[TenantName],[TenantEmail],[TenantCell],[TenantAddress],[TenantWorkAddress],[TenantWorkCell],[DateTenantCreated],[DateLeaseStart],[DateLeaseEnd],[Status] from [{0}].[dbo].[Tenant] where Status = '{1}'", conn.Database, "Active");

                using (var reader = cmdTenants.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var cmdAssigned = conn.CreateCommand();
                        cmdAssigned.CommandText = string.Format("SELECT [Id],[TenantId],[UnitId],[UnitRomms] from [{0}].[dbo].[Assigned] where TenantId = '{1}' AND UnitId = '{2}'", conn.Database, reader.GetInt32(0), Id);
                        var readerAssigned = cmdAssigned.ExecuteReader();

                        var assignedTenants = new AssignedTenantViewModel();
                        assignedTenants.Id = reader.GetInt32(0);
                        assignedTenants.TenantName = reader.GetString(1);
                        assignedTenants.TenantEmail = reader.GetString(2);
                        assignedTenants.TenantCell = reader.GetString(3);
                        assignedTenants.Assigned = readerAssigned.HasRows ? "True" : "False";
                        assignedTenants.UnitId = Id;
                        AssignedTenantViewModel.Add(assignedTenants);
                        readerAssigned.Close();
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(AssignedTenantViewModel);
            }
        }

        [HttpPost, Route("AssignTenantUnit")]
        public async Task<IHttpActionResult> AssignTenantUnit(AssignedTenantViewModel AssignTenantUnitData)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var AssignedTenantViewModel = new List<AssignedTenantViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            JObject o = new JObject();

            int numberOfTenants = SafriSoft.Assigned.Count(x => x.UnitId == AssignTenantUnitData.UnitId);
            var unit = SafriSoft.Units.Find(AssignTenantUnitData.UnitId);

            if (numberOfTenants == unit.UnitRooms)
            {
                o["Found"] = true;
                o["Message"] = "No room available for this unit!";
                return Json(o);
            }

            if (numberOfTenants >= 1 && unit.Sharing == "No")
            {
                o["Found"] = true;
                o["Message"] = "Sharing not allowed for this unit!";
                return Json(o);
            }

            var assignedTenant = SafriSoft.Assigned.Where(x => x.TenantId == AssignTenantUnitData.Id && x.UnitId == AssignTenantUnitData.UnitId).FirstOrDefault();

            if (assignedTenant == null)
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                {
                    conn.Open();
                    var cmdUnit = conn.CreateCommand();
                    cmdUnit.CommandText = string.Format("SELECT [UnitRooms] from [{0}].[dbo].[Unit] where Id = {1}", conn.Database, AssignTenantUnitData.UnitId);

                    using (var reader = cmdUnit.ExecuteReader())
                    {
                        reader.Read();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[Assigned] ([TenantId],[UnitId],[UnitRomms]) VALUES('{1}','{2}','{3}')", conn.Database, AssignTenantUnitData.Id, AssignTenantUnitData.UnitId, reader.GetInt32(0));
                        await cmd.ExecuteNonQueryAsync();
                        o["Found"] = false;
                    }
                    conn.Close();
                }
            }
            else
            {
                o["Found"] = true;
            }
            
            return Json(o);

        }

        [HttpPost, Route("RemoveTenantUnit")]
        public async Task<IHttpActionResult> RemoveTenantUnit(AssignedTenantViewModel AssignTenantUnitData)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var AssignedTenantViewModel = new List<AssignedTenantViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            JObject o = new JObject();

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = string.Format("DELETE from [{0}].[dbo].[Assigned] where TenantId = {1} AND UnitId = {2}", conn.Database, AssignTenantUnitData.Id, AssignTenantUnitData.UnitId);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
                o["Success"] = true;
            }
            catch (Exception Ex)
            {
                o["Success"] = false;
                o["Error"] = Ex.Message;
            }
            

            return Json(o);

        }

        [HttpGet, Route("GetProjections")]
        public async Task<IHttpActionResult> GetProjections()
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var ProjectionsViewModel = new List<ProjectionsViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            JObject o = new JObject();

            var getFirstDate = SafriSoft.Tenants.OrderBy(x=>x.DateLeaseStart).Select(x => x.DateLeaseStart).FirstOrDefault();
            var getLastDate = SafriSoft.Tenants.OrderByDescending(x => x.DateLeaseEnd).Select(x => x.DateLeaseEnd).FirstOrDefault();

            var transactions = SafriSoft.Transactions.ToList();

            foreach (var transaction in transactions)
            {
                var tenant = SafriSoft.Tenants.FirstOrDefault(x => x.Id == transaction.TenantId);
                var transacts = new ProjectionsViewModel();
                transacts.TenantName = tenant.TenantName;
                transacts.TransDate = transaction.TransactionDate.ToString("yyyy-MM-dd");
                transacts.TransName = transaction.TransactionName;
                transacts.TransProposed = 0;
                transacts.TransActual = transaction.TransactionCode == 15 || transaction.TransactionCode == 25 ? -transaction.TransactionAmount : transaction.TransactionAmount;
                transacts.Balance = 0;
                ProjectionsViewModel.Add(transacts);
            }

            // Lets get all assigned records
            var assignedUnits = SafriSoft.Assigned.ToList();
            
            foreach (var assignedUnitsData in assignedUnits)
            {
                // Lets get all initial data
                var unitPrice = SafriSoft.Units.Where(x => x.Id == assignedUnitsData.UnitId).Select(x => x.UnitPrice).FirstOrDefault();
                var numberOfTenants = SafriSoft.Assigned.Count(x => x.UnitId == assignedUnitsData.UnitId);
                var tenantData = SafriSoft.Tenants.FirstOrDefault(x => x.Id == assignedUnitsData.TenantId);

                var leaseStart = tenantData.DateLeaseStart;
                var leaseEnd = tenantData.DateLeaseEnd;
                int tenantCounter = 1;

                while (leaseStart <= leaseEnd)
                {
                    var getTransaction = SafriSoft.Transactions.FirstOrDefault(x => x.TransactionDate == leaseStart && x.TenantId == tenantData.Id);

                    if (getTransaction == null)
                    { 
                        if (tenantCounter == 1)
                        {
                            var projection = new ProjectionsViewModel();
                            projection.TenantName = tenantData.TenantName;
                            projection.TransDate = leaseStart.ToString("yyyy-MM-dd");
                            projection.TransName = "Monthly";
                            projection.TransProposed = unitPrice / numberOfTenants;
                            projection.TransActual = 0;
                            projection.Balance = 0;
                            ProjectionsViewModel.Add(projection);
                            var depositProjection = new ProjectionsViewModel();
                            depositProjection.TenantName = tenantData.TenantName;
                            depositProjection.TransDate = leaseStart.ToString("yyyy-MM-dd");
                            depositProjection.TransName = "Deposit";
                            depositProjection.TransProposed = unitPrice / numberOfTenants;
                            depositProjection.TransActual = 0;
                            depositProjection.Balance = 0;
                            ProjectionsViewModel.Add(depositProjection);
                        }
                        else
                        {
                            var projection = new ProjectionsViewModel();
                            projection.TenantName = tenantData.TenantName;
                            projection.TransDate = leaseStart.ToString("yyyy-MM-dd");
                            projection.TransName = "Monthly";
                            projection.TransProposed = unitPrice / numberOfTenants;
                            projection.TransActual = 0;
                            projection.Balance = 0;
                            ProjectionsViewModel.Add(projection);
                        }
                    }
                    tenantCounter++;
                    leaseStart = leaseStart.AddMonths(1);
                }

            }
            
            ProjectionsViewModel = ProjectionsViewModel.OrderBy(x => x.TransDate).ToList();
            int projectionCount = 1;
            decimal? finalBalance = 0;

            foreach (var eachProjection in ProjectionsViewModel)
            {
                eachProjection.Id = projectionCount++;
                finalBalance = finalBalance + eachProjection.TransActual;
                eachProjection.Balance = finalBalance;
            }

            return Json(ProjectionsViewModel);
        }

        [HttpPost, Route("TransactionCreate")]
        public async Task<IHttpActionResult> TransactionCreate(TransactionViewModel TransactionData)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var AssignedTenantViewModel = new List<AssignedTenantViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            JObject o = new JObject();
            
            try
            {
                if (TransactionData.TransactionFor == 0)
                {
                    var assignedUnits = SafriSoft.Assigned.ToList();

                    foreach (var assignedUnitsData in assignedUnits)
                    {
                        // Lets get all initial data
                        var unitPrice = SafriSoft.Units.Where(x => x.Id == assignedUnitsData.UnitId).Select(x => x.UnitPrice).FirstOrDefault();
                        var numberOfTenants = SafriSoft.Assigned.Where(x => x.UnitId == assignedUnitsData.UnitId).Count();
                        var tenantData = SafriSoft.Tenants.FirstOrDefault(x => x.Id == assignedUnitsData.TenantId);

                        var dateTransactionRaised = DateTime.Parse(TransactionData.DateTransactionRaised);
                        var finalDate = DateTime.Parse(dateTransactionRaised.Year + "-" + dateTransactionRaised.Month + "-" + tenantData.DateLeaseStart.Day);
                        var transactionFor = TransactionData.TransactionFor;
                        var transactionCode = 10;
                        var transactionName = "Monthly - Rental";
                        decimal? finalUnitPrice = unitPrice / numberOfTenants;

                        if (tenantData.DateLeaseStart <= finalDate && tenantData.DateLeaseEnd >= finalDate)
                        {
                            try
                            {
                                o = RaiseTransaction(transactionCode, transactionName, finalUnitPrice, finalDate, tenantData.Id);
                            }
                            catch (Exception Ex)
                            {
                                o["Success"] = false;
                                o["Message"] = Ex.Message;
                            }

                            if (tenantData.DateLeaseStart == finalDate)
                            {
                                var depositTransactionCode = 20;
                                var depositTransactionName = "Initial - Deposit";

                                try
                                {
                                    o = RaiseTransaction(depositTransactionCode, depositTransactionName, finalUnitPrice, finalDate, tenantData.Id);
                                }
                                catch (Exception Ex)
                                {
                                    o["Success"] = false;
                                    o["Message"] = Ex.Message;
                                }
                            }

                            try
                            {
                                var deposit = tenantData.DateLeaseStart == finalDate ? true : false;
                                var amount = finalUnitPrice;
                                var sendDate = finalDate.Year + "/" + finalDate.Month + "/" + finalDate.Day;
                                var tenantName = tenantData.TenantName;
                                var payNowLink = "http://safrisoft.com/rental/paynow?TransactionFor=" + tenantData.Id + "&Date=" + sendDate;
                                var emailBody = "Hi " + tenantName + ",<br/><br /> On behalf of your landlord please click link below to pay your rent.<br/><br />" + payNowLink;

                                using (MailMessage mt = new MailMessage())
                                {
                                    mt.From = new MailAddress("admin@safrisoft.com");
                                    mt.IsBodyHtml = true;
                                    mt.Subject = "SafriSoft - Pay Now";
                                    mt.Body = "<span><h1 style='color:#17a2b8;'>SafriSoft.</h1></span></br><div style='font-family:Courier New;'> " + emailBody + "</div></br><br /><p style='font-family:Courier New;'>Regards,</p><div style='height:2px;background-color:#17a2b8;width:40%;margin-bottom:5px;'></div><div><p style='font-family:Courier New;'>Administrator</p><p style='font-family:Courier New;'>Website: www.safrisoft.com</p><p style='font-family:Courier New;'>Email: admin@safrisoft.com</p><p style='font-family:Courier New;'>Cell: 067 272 7320</p></div><div style='height:2px;background-color:#17a2b8;width:40%;margin-bottom:5px;'></div>";
                                    mt.To.Add(tenantData.TenantEmail);
                                    mt.IsBodyHtml = true;
                                    SmtpClient smtp2 = new SmtpClient();
                                    smtp2.Host = "mail.safrisoft.com";
                                    smtp2.EnableSsl = false;
                                    smtp2.UseDefaultCredentials = false;
                                    NetworkCredential networkCred2 = new NetworkCredential();
                                    networkCred2.UserName = "admin@safrisoft.com";
                                    networkCred2.Password = "@SafriAdmin&1";
                                    smtp2.Credentials = networkCred2;
                                    smtp2.Port = 25;
                                    await smtp2.SendMailAsync(mt);
                                }
                                o["Success"] = true;
                            }
                            catch (Exception Ex)
                            {
                                o["Success"] = false;
                                o["Message"] = Ex.Message;
                            }

                        }

                    }
                }
                else
                {
                    var tenantData = SafriSoft.Tenants.FirstOrDefault(x => x.Id == TransactionData.TransactionFor);
                    var dateTransactionRaised = DateTime.Parse(TransactionData.DateTransactionRaised);
                    var finalDate = DateTime.Parse(dateTransactionRaised.Year + "-" + dateTransactionRaised.Month + "-" + tenantData.DateLeaseStart.Day);
                    var transactionFor = TransactionData.TransactionFor;
                    var transactionCode = 10;
                    var transactionName = "Monthly - Rental";
                    var assignedUnits = SafriSoft.Assigned.FirstOrDefault(x => x.TenantId == transactionFor);
                    var unitPrice = SafriSoft.Units.Where(x => x.Id == assignedUnits.UnitId).Select(x => x.UnitPrice).FirstOrDefault();
                    var numberOfTenants = SafriSoft.Assigned.Where(x => x.UnitId == assignedUnits.UnitId).Count();
                    decimal? finalUnitPrice = unitPrice / numberOfTenants;

                    if (tenantData.DateLeaseStart <= finalDate && tenantData.DateLeaseEnd >= finalDate)
                    {
                        try
                        {
                            o = RaiseTransaction(transactionCode, transactionName, finalUnitPrice, finalDate, transactionFor);
                            try
                            {
                                var deposit = tenantData.DateLeaseStart == finalDate ? true : false;
                                var amount = finalUnitPrice;
                                var sendDate = finalDate.Year + "/" + finalDate.Month + "/" + finalDate.Day;
                                var tenantName = tenantData.TenantName;
                                var payNowLink = "http://safrisoft.com/rental/paynow?TransactionFor=" + tenantData.Id + "&Date=" + sendDate;
                                var emailBody = "Hi " + tenantName + ",<br/><br /> On behalf of your landlord please click link below to pay your rent.<br/><br />" + payNowLink;

                                using (MailMessage mt = new MailMessage())
                                {
                                    mt.From = new MailAddress("admin@safrisoft.com");
                                    mt.IsBodyHtml = true;
                                    mt.Subject = "SafriSoft - Pay Now";
                                    mt.Body = "<span><h1 style='color:#17a2b8;'>SafriSoft.</h1></span></br><div style='font-family:Courier New;'> " + emailBody + "</div></br><br /><p style='font-family:Courier New;'>Regards,</p><div style='height:3px;background-color:#17a2b8;width:40%;margin-bottom:5px;'></div><div><p style='font-family:Courier New;'>Administrator</p><p style='font-family:Courier New;'>Website: www.safrisoft.com</p><p style='font-family:Courier New;'>Email: admin@safrisoft.com</p><p style='font-family:Courier New;'>Cell: 067 272 7320</p></div><div style='height:3px;background-color:#17a2b8;width:40%;margin-bottom:5px;'></div>";
                                    mt.To.Add(tenantData.TenantEmail);
                                    mt.IsBodyHtml = true;
                                    SmtpClient smtp2 = new SmtpClient();
                                    smtp2.Host = "mail.safrisoft.com";
                                    smtp2.EnableSsl = false;
                                    smtp2.UseDefaultCredentials = false;
                                    NetworkCredential networkCred2 = new NetworkCredential();
                                    networkCred2.UserName = "admin@safrisoft.com";
                                    networkCred2.Password = "@SafriAdmin&1";
                                    smtp2.Credentials = networkCred2;
                                    smtp2.Port = 25;
                                    await smtp2.SendMailAsync(mt);
                                }
                                o["Success"] = true;
                            }
                            catch (Exception Ex)
                            {
                                o["Success"] = false;
                                o["Message"] = Ex.Message;
                            }
                        }
                        catch (Exception Ex)
                        {
                            o["Success"] = false;
                            o["Message"] = Ex.Message;
                        }
                    }
                }                
                
            }
            catch (Exception Ex)
            {
                o["Success"] = false;
                o["Message"] = Ex.Message;
            }
            
            return Json(o);

        }

        [HttpPost, Route("ChangeRecordStatus")]
        public async Task<IHttpActionResult> ChangeRecordStatus(RecordViewModel RecordData)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var AssignedTenantViewModel = new List<AssignedTenantViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            JObject o = new JObject();

            try
            {
                if (RecordData.Name == "Tenant")
                {
                    var tenant = SafriSoft.Tenants.FirstOrDefault(x=>x.Id == RecordData.Id);
                    tenant.Status = "Disabled";
                    SafriSoft.SaveChanges();
                }else if (RecordData.Name == "Unit")
                {
                    var unit = SafriSoft.Units.FirstOrDefault(x => x.Id == RecordData.Id);
                    unit.Status = "Disabled";
                    SafriSoft.SaveChanges();
                }else if (RecordData.Name == "Documents")
                {
                    using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                    {
                        conn.Open();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = string.Format("DELETE from [{0}].[dbo].[Document] where Id = {1}", conn.Database, RecordData.Id);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                else if (RecordData.Name == "NOK")
                {
                    using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                    {
                        conn.Open();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = string.Format("DELETE from [{0}].[dbo].[NOK] where Id = {1}", conn.Database, RecordData.Id);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                }
                o["Success"] = true;
                o["Message"] = "Success";
            }
            catch (Exception Ex)
            {
                o["Success"] = false;
                o["Message"] = Ex.Message;
            }

            return Json(o);
        }

        [HttpPost, Route("GetPayNowInvoiceData")]
        public async Task<IHttpActionResult> GetPayNowInvoiceData(TransactionViewModel TransactionData)
        {
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
                    orderCmd.CommandText = string.Format("SELECT [Id],[TransactionCode],[TransactionName],[TransactionAmount],[TransactionDate],[TenantId] from [{0}].[dbo].[Transaction] Where TenantId = '{1}' AND TransactionDate = '{2}' AND (TransactionCode = '{3}' OR TransactionCode = '{4}')", conn.Database, TransactionData.TransactionFor, DateTime.Parse(TransactionData.DateTransactionRaised), 10, 20);

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
                            InvoiceDetails.OrderId = reader.GetInt32(0).ToString();
                            InvoiceDetails.ProductName = reader.GetString(2);
                            InvoiceDetails.NumberOfItems = reader.GetInt32(1);
                            customerId = reader.GetInt32(5).ToString();
                            InvoiceDetails.OrderWorth = reader.GetDecimal(3);
                            InvoiceDetails.DateOrderCreated = reader.GetDateTime(4).ToString("dd MMMM yyyy");
                        }
                        reader.NextResult();
                        reader.Close();
                    }

                    var customerCmd = conn.CreateCommand();
                    customerCmd.CommandText = string.Format("SELECT [TenantName],[TenantEmail],[TenantCell],[TenantAddress] from [{0}].[dbo].[Tenant] where Id = '{1}'", conn.Database, customerId);

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
                    InvoiceDetails.InvoiceTotal = InvoiceDetails.OrderWorth;

                    InvoiceViewModel.Add(InvoiceDetails);
                }
                return Json(InvoiceViewModel);
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString() });
            }
        }

        // Reusable functions at the bottom
        public static JObject RaiseTransaction(int TransactionCode, string TransactionName, decimal? TransactionAmount, DateTime DateTransactionRaised, int TenantId)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            JObject o = new JObject();

            var transactionCheck = SafriSoft.Transactions.FirstOrDefault(x => x.TransactionDate == DateTransactionRaised && x.TenantId == TenantId && x.TransactionCode == TransactionCode);

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
                        cmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[Transaction] ([TransactionCode],[TransactionName],[TransactionDate],[TenantId]) VALUES('{1}','{2}','{3}','{4}')", conn.Database, TransactionCode, TransactionName, DateTransactionRaised, TenantId);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }
                    var transaction = SafriSoft.Transactions.First(x => x.TransactionDate == DateTransactionRaised && x.TenantId == TenantId && x.TransactionCode == TransactionCode);
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

        
        public static byte[] CompressStream(byte[] uncompressedData)
        {
            using (var msi = new MemoryStream(uncompressedData))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Compress))
                {
                    msi.CopyTo(mso);

                    return mso.ToArray();
                }
            }
        }
                
    }
}
