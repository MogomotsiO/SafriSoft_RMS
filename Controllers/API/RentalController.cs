using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json.Linq;
using SafriSoftv1._3.Models;
using SafriSoftv1._3.Models.Data;
using SafriSoftv1._3.Models.Rental;
using SafriSoftv1._3.Services;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
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
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            var orgId = GetOrganisationId(getOrgClaim);

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
                    customerCountCmd.CommandText = string.Format("SELECT count([Id]) from [{0}].[dbo].[Tenants] WHERE OrganisationId = '{1}'", conn.Database, orgId);
                    try
                    {
                        tenantCount = (Int32)customerCountCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        tenantCount = 0;
                    }

                    var totalUnitsCmd = conn.CreateCommand();
                    totalUnitsCmd.CommandText = string.Format("SELECT count([Id]) from [{0}].[dbo].[Units] WHERE OrganisationId = '{1}'", conn.Database, orgId);
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
                    unitsNotSharingCmd.CommandText = string.Format("SELECT count([Id]) from [{0}].[dbo].[Units] where OrganisationId = '{2}' AND Sharing = '{1}'", conn.Database, "No", orgId);
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
                    unitsSharingCmd.CommandText = string.Format("SELECT sum([UnitRooms]) from [{0}].[dbo].[Units] where OrganisationId = '{2}' AND Sharing = '{1}'", conn.Database, "Yes", orgId);
                    try
                    {
                        unitsSharing = (Int32)unitsSharingCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        unitsSharing = 0;
                    }

                    //unitsAvailable = totalUnits;

                    var unitsOccupiedCmd = conn.CreateCommand();
                    unitsOccupiedCmd.CommandText = string.Format("SELECT count(DISTINCT a.[UnitId]) from [{0}].[dbo].[Assigneds] a JOIN [{0}].[dbo].[Units] u on u.Id = a.UnitId ", conn.Database);
                    try
                    {
                        unitsOccupied = (Int32)unitsOccupiedCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        unitsOccupied = 0;
                    }

                    unitsAvailable = totalUnits - unitsOccupied;

                    var randValueSoldCmd = conn.CreateCommand();
                    randValueSoldCmd.CommandText = string.Format("SELECT sum([TransactionAmount]) from [{0}].[dbo].[Transactions] Where TransactionDate >= '{1}' AND TransactionDate <= '{2}' AND TransactionCode = '{3}' AND OrganisationId = '{4}'", conn.Database, startDate, endDate, 15, orgId);
                    try
                    {
                        randValueSold = (Decimal)randValueSoldCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        randValueSold = decimal.Parse("0");
                    }

                    
                    var expectedPaymentsCmd = conn.CreateCommand();
                    expectedPaymentsCmd.CommandText = string.Format("SELECT sum([TransactionAmount]) from [{0}].[dbo].[Transactions] Where TransactionDate >= '{1}' AND TransactionDate <= '{2}' AND TransactionCode <> '{3}' AND OrganisationId = '{4}'", conn.Database, startDate, endDate, 15, orgId);
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
            var orgId = GetOrganisationId(getOrgClaim);

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[TenantName],[TenantEmail],[TenantCell],[TenantAddress],[TenantWorkAddress],[TenantWorkCell],[DateTenantCreated],[DateLeaseStart],[DateLeaseEnd],[Status] from [{0}].[dbo].[Tenants] where OrganisationId = {2} AND Status = '{1}'", conn.Database, "Active", orgId);

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
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            var orgId = GetOrganisationId(getOrgClaim);

            var result = CheckPackageAccess("Tenants", orgId);

            if (result == true)
            {
                return Json(new { Success = false, Error = "You have exceeded the number of Tenants to add. Please upgrade Package" });
            }

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
                    cmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[Tenants] ([TenantName],[TenantEmail],[TenantCell],[TenantAddress],[TenantWorkAddress],[TenantWorkCell],[DateTenantCreated],[DateLeaseStart],[DateLeaseEnd],[Status],[OrganisationId]) VALUES('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')", conn.Database, tenantName, tenantEmail, tenantCell, tenantAddress, tenantWorkAddress, tenantWorkCell, dateTenantCreated, dateLeaseStart, dateLeaseEnd, "Active", orgId);
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
                cmd.CommandText = string.Format("SELECT [Id],[TenantName],[TenantEmail],[TenantCell],[TenantAddress],[TenantWorkAddress],[TenantWorkCell],[DateTenantCreated],[DateLeaseStart],[DateLeaseEnd],[Status] from [{0}].[dbo].[Tenants] where Status = '{1}' AND Id = '{2}'", conn.Database, "Active", Id);

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
                    cmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[NOKs] ([NOKName],[NOKEmail],[NOKCell],[NOKRelation],[DateNOKCreated],[TenantId]) VALUES('{1}','{2}','{3}','{4}','{5}','{6}')", conn.Database, nokName, nokEmail, nokCell, nokRelation, dateNOKCreated, nokTenantId);
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
                cmd.CommandText = string.Format("SELECT [Id],[NOKName],[NOKEmail],[NOKCell],[NOKRelation],[DateNOKCreated] from [{0}].[dbo].[NOKs]", conn.Database);

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
                cmd.CommandText = string.Format("SELECT [Id],[NOKName],[NOKEmail],[NOKCell],[NOKRelation],[DateNOKCreated] from [{0}].[dbo].[NOKs] where TenantId = '{1}'", conn.Database, Id);

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
                cmd.CommandText = string.Format("SELECT [Id],[NOKName],[NOKEmail],[NOKCell],[NOKRelation],[DateNOKCreated] from [{0}].[dbo].[NOKs] where Id = '{1}'", conn.Database, Id);

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
                cmd.CommandText = string.Format("SELECT [Id],[FileName],[DateFileCreated] from [{0}].[dbo].[Documents] where TenantId = '{1}'", conn.Database, Id);

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
                cmd.CommandText = string.Format("SELECT [Id],[FileName],[DateFileCreated] from [{0}].[dbo].[Documents]", conn.Database);

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
            var orgId = GetOrganisationId(getOrgClaim);

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[UnitNumber],[UnitName],[UnitRooms],[UnitDescription],[UnitPrice],[Sharing] from [{0}].[dbo].[Units] where OrganisationId = {2} AND Status = '{1}'", conn.Database,"Active", orgId);

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
                        units.NumberOfCharges = SafriSoft.UnitCharges.Count(x => x.UnitId == units.Id);
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
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            var orgId = GetOrganisationId(getOrgClaim);

            var result = CheckPackageAccess("Units", orgId);

            if (result == true)
            {
                return Json(new { Success = false, Error = "You have exceeded the number of Units to add. Please upgrade Package" });
            }

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
                    cmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[Units] ([UnitNumber],[UnitName],[UnitRooms],[UnitDescription],[UnitPrice],[Sharing],[Status],[OrganisationId]) VALUES('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')", conn.Database, unitNumber, unitName, unitRooms, unitDescription, unitPrice, unitsharing, "Active", orgId);
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
                cmd.CommandText = string.Format("SELECT [Id],[UnitNumber],[UnitName],[UnitRooms],[UnitDescription],[UnitPrice],[Sharing] from [{0}].[dbo].[Units] where Id = '{1}'", conn.Database, Id);

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
                cmdTenants.CommandText = string.Format("SELECT [Id],[TenantName],[TenantEmail],[TenantCell],[TenantAddress],[TenantWorkAddress],[TenantWorkCell],[DateTenantCreated],[DateLeaseStart],[DateLeaseEnd],[Status] from [{0}].[dbo].[Tenants] where Status = '{1}'", conn.Database, "Active");

                using (var reader = cmdTenants.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var cmdAssigned = conn.CreateCommand();
                        cmdAssigned.CommandText = string.Format("SELECT [Id],[TenantId],[UnitId],[UnitRomms] from [{0}].[dbo].[Assigneds] where TenantId = '{1}' AND UnitId = '{2}'", conn.Database, reader.GetInt32(0), Id);
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
            var orgId = GetOrganisationId(getOrgClaim);
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

            var assignedTenant = SafriSoft.Assigned.Where(x => x.TenantId == AssignTenantUnitData.Id && x.UnitId == AssignTenantUnitData.UnitId && x.OrganisationId == orgId).FirstOrDefault();

            if (assignedTenant == null)
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                {
                    conn.Open();
                    var cmdUnit = conn.CreateCommand();
                    cmdUnit.CommandText = string.Format("SELECT [UnitRooms] from [{0}].[dbo].[Units] where Id = {1}", conn.Database, AssignTenantUnitData.UnitId);

                    using (var reader = cmdUnit.ExecuteReader())
                    {
                        reader.Read();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[Assigneds] ([TenantId],[UnitId],[UnitRomms],[OrganisationId]) VALUES('{1}','{2}','{3}','{4}')", conn.Database, AssignTenantUnitData.Id, AssignTenantUnitData.UnitId, reader.GetInt32(0), orgId);
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
                    cmd.CommandText = string.Format("DELETE from [{0}].[dbo].[Assigneds] where TenantId = {1} AND UnitId = {2}", conn.Database, AssignTenantUnitData.Id, AssignTenantUnitData.UnitId);
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
            var orgId = GetOrganisationId(getOrgClaim);
            JObject o = new JObject();

            var getFirstDate = SafriSoft.Tenants.OrderBy(x=>x.DateLeaseStart).Select(x => x.DateLeaseStart).FirstOrDefault();
            var getLastDate = SafriSoft.Tenants.OrderByDescending(x => x.DateLeaseEnd).Select(x => x.DateLeaseEnd).FirstOrDefault();

            var transactions = SafriSoft.Transactions.Where(x => x.OrganisationId == orgId).ToList();

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
            var assignedUnits = SafriSoft.Assigned.Where(x => x.OrganisationId == orgId).ToList();
            
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
                    var transDateStart = new DateTime(leaseStart.Year,leaseStart.Month, 1);
                    var transDateEnd = new DateTime(leaseStart.Year, leaseStart.Month, DateTime.DaysInMonth(leaseStart.Year, leaseStart.Month));
                    var getTransaction = SafriSoft.Transactions.FirstOrDefault(x => x.TransactionDate >= transDateStart && x.TransactionDate <= transDateEnd && x.TenantId == tenantData.Id);

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
            var orgId = GetOrganisationId(getOrgClaim);
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
                        var finalDate = DateTime.Parse(dateTransactionRaised.Year + "-" + dateTransactionRaised.Month + "-" + dateTransactionRaised.Day);
                        var transactionFor = TransactionData.TransactionFor;
                        var transactionCode = 10;
                        var transactionName = "Rent";
                        decimal? finalUnitPrice = unitPrice / numberOfTenants;

                        if (tenantData.DateLeaseStart <= finalDate && tenantData.DateLeaseEnd >= finalDate)
                        {
                            var transactionRentDetails = SafriSoft.Transactions.Where(x => x.TransactionCode == transactionCode && x.TenantId == tenantData.Id && x.TransactionDate.Year == dateTransactionRaised.Year && x.TransactionDate.Month == dateTransactionRaised.Month).FirstOrDefault();

                            if (transactionRentDetails == null)
                            {
                                // Raise the monthly rental amount
                                o = RaiseTransaction(transactionCode, transactionName, finalUnitPrice, finalDate, tenantData.Id);
                            }
                            else
                            {
                                o["Success"] = true;
                                o["Message"] = "Transactions already created";
                            }

                            // lets raise all the other charges
                            var unitCharges = SafriSoft.UnitCharges.Where(x => x.UnitId == assignedUnitsData.UnitId).ToList();

                            foreach(var unitCharge in unitCharges)
                            {
                                var chargeDetails = SafriSoft.Charges.Where(x => x.Id == unitCharge.FeeId && x.Effective <= dateTransactionRaised).FirstOrDefault();
                                var chargeAmount = chargeDetails.Amount / numberOfTenants;
                                if (chargeDetails != null)
                                {
                                    if(chargeDetails.Type == 2)
                                    {
                                        var transactionDetails = SafriSoft.Transactions.Where(x => x.TransactionCode == chargeDetails.Code && x.TenantId == tenantData.Id && x.TransactionDate.Year == dateTransactionRaised.Year && x.TransactionDate.Month == dateTransactionRaised.Month).FirstOrDefault();

                                        if (transactionDetails == null)
                                        {
                                            o = RaiseTransaction(chargeDetails.Code, chargeDetails.Name, chargeAmount, finalDate, tenantData.Id);
                                        }
                                        else
                                        {
                                            o["Success"] = true;
                                            o["Message"] = "Transactions already created";
                                        }
                                    }else if(chargeDetails.Type == 1)
                                    {
                                        var transactionDetails = SafriSoft.Transactions.Where(x => x.TransactionCode == chargeDetails.Code && x.TenantId == tenantData.Id && x.TransactionDate <= dateTransactionRaised).FirstOrDefault();

                                        if (transactionDetails == null)
                                        {
                                            o = RaiseTransaction(chargeDetails.Code, chargeDetails.Name, chargeAmount, finalDate, tenantData.Id);
                                        }
                                        else
                                        {
                                            o["Success"] = true;
                                            o["Message"] = "Transactions already created";
                                        }
                                    }
                                    
                                }
                            }
                        }

                    }
                }
                else
                {
                    var tenantData = SafriSoft.Tenants.FirstOrDefault(x => x.Id == TransactionData.TransactionFor);
                    var dateTransactionRaised = DateTime.Parse(TransactionData.DateTransactionRaised);
                    var finalDate = DateTime.Parse(dateTransactionRaised.Year + "-" + dateTransactionRaised.Month + "-" + dateTransactionRaised.Day);
                    var transactionFor = TransactionData.TransactionFor;
                    var transactionCode = 10;
                    var transactionName = "Rent";
                    var assignedUnits = SafriSoft.Assigned.FirstOrDefault(x => x.TenantId == transactionFor);
                    var unitPrice = SafriSoft.Units.Where(x => x.Id == assignedUnits.UnitId).Select(x => x.UnitPrice).FirstOrDefault();
                    var numberOfTenants = SafriSoft.Assigned.Where(x => x.UnitId == assignedUnits.UnitId).Count();
                    decimal? finalUnitPrice = unitPrice / numberOfTenants;

                    if (tenantData.DateLeaseStart <= finalDate && tenantData.DateLeaseEnd >= finalDate)
                    {
                        var transactionRentDetails = SafriSoft.Transactions.Where(x => x.TransactionCode == transactionCode && x.TenantId == tenantData.Id && x.TransactionDate.Year == dateTransactionRaised.Year && x.TransactionDate.Month == dateTransactionRaised.Month).FirstOrDefault();

                        if (transactionRentDetails == null)
                        {
                            o = RaiseTransaction(transactionCode, transactionName, finalUnitPrice, finalDate, transactionFor);
                        }
                        else
                        {
                            o["Success"] = true;
                            o["Message"] = "Transactions already created";
                        }

                        // lets raise all the other charges
                        var unitCharges = SafriSoft.UnitCharges.Where(x => x.UnitId == assignedUnits.UnitId).ToList();

                        foreach (var unitCharge in unitCharges)
                        {
                            var chargeDetails = SafriSoft.Charges.Where(x => x.Id == unitCharge.FeeId && x.Effective <= dateTransactionRaised).FirstOrDefault();
                            var chargeAmount = chargeDetails.Amount / numberOfTenants;
                            if (chargeDetails != null)
                            {
                                if (chargeDetails.Type == 2)
                                {
                                    var transactionDetails = SafriSoft.Transactions.Where(x => x.TransactionCode == chargeDetails.Code && x.TenantId == tenantData.Id && x.TransactionDate.Year == dateTransactionRaised.Year && x.TransactionDate.Month == dateTransactionRaised.Month).FirstOrDefault();

                                    if (transactionDetails == null)
                                    {
                                        o = RaiseTransaction(chargeDetails.Code, chargeDetails.Name, chargeAmount, finalDate, tenantData.Id);
                                    }
                                    else
                                    {
                                        o["Success"] = true;
                                        o["Message"] = "Transactions already created";
                                    }
                                }
                                else if (chargeDetails.Type == 1)
                                {
                                    var transactionDetails = SafriSoft.Transactions.Where(x => x.TransactionCode == chargeDetails.Code && x.TenantId == tenantData.Id && x.TransactionDate <= dateTransactionRaised).FirstOrDefault();

                                    if (transactionDetails == null)
                                    {
                                        o = RaiseTransaction(chargeDetails.Code, chargeDetails.Name, chargeAmount, finalDate, tenantData.Id);
                                    }
                                    else
                                    {
                                        o["Success"] = true;
                                        o["Message"] = "Transactions already created";
                                    }
                                }

                            }
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

        [HttpPost, Route("PaymentTransactionCreate")]
        public async Task<IHttpActionResult> PaymentTransactionCreate(TransactionViewModel TransactionData)
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
                var tenantData = SafriSoft.Tenants.FirstOrDefault(x => x.Id == TransactionData.TransactionFor);
                var dateTransactionRaised = DateTime.Parse(TransactionData.DateTransactionRaised);
                var finalDate = DateTime.Parse(dateTransactionRaised.Year + "-" + dateTransactionRaised.Month + "-" + dateTransactionRaised.Day);
                var transactionFor = TransactionData.TransactionFor;
                var transactionCode = 15;
                var transactionName = "Tenant - Payment";
                decimal? paymentAmount = TransactionData.TransactionAmount;

                o = RaiseTransaction(transactionCode, transactionName, paymentAmount, finalDate, tenantData.Id);
                
            }
            catch (Exception Ex)
            {
                o["Success"] = false;
                o["Message"] = Ex.Message;
            }

            return Json(o);

        }

        [HttpGet, Route("GetStatements")]
        public async Task<IHttpActionResult> GetStatements()
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var AssignedTenantViewModel = new List<AssignedTenantViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            var orgId = GetOrganisationId(getOrgClaim);

            var statementsVm = new List<StatementsViewModel>();

            var tenants = SafriSoft.Tenants.Where(x => x.OrganisationId == orgId).ToList();

            foreach (var tenant in tenants)
            {
                var assignedUnit = SafriSoft.Assigned.FirstOrDefault(x => x.TenantId == tenant.Id);

                if (assignedUnit != null)
                {
                    var leaseStart = tenant.DateLeaseStart;
                    var leaseEnd = tenant.DateLeaseEnd;
                    decimal? balanceTrack = 0;

                    while (leaseStart <= leaseEnd)
                    {
                        int year = leaseStart.Year;
                        int month = leaseStart.Month;

                        var transactions = SafriSoft.Transactions.Where(x => x.TenantId == tenant.Id && x.TransactionDate.Year == leaseStart.Year && x.TransactionDate.Month == leaseStart.Month).ToList();

                        if(transactions.Count > 0)
                        {
                            var statementVm = new StatementsViewModel();
                            statementVm.TenantId = tenant.Id;
                            statementVm.TenantName = tenant.TenantName;
                            statementVm.DateFrom = leaseStart.ToString("yyyy-MM-dd");
                            statementVm.DateTo = new DateTime(year, month, DateTime.DaysInMonth(year, month)).ToString("yyyy-MM-dd");
                            foreach (var transaction in transactions)
                            {
                                balanceTrack = transaction.TransactionCode != 15 ? balanceTrack + transaction.TransactionAmount : balanceTrack - transaction.TransactionAmount;
                            }

                            statementVm.Balance = balanceTrack;
                            statementsVm.Add(statementVm);
                        }

                        leaseStart = leaseStart.AddMonths(1);
                    }
                    
                }
            }

            return Json(statementsVm);
        }

        [HttpPost, Route("GetStatement")]
        public async Task<IHttpActionResult> GetStatement(StatementsViewModel statementVm)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            ApplicationDbContext SafriSoftApp = new ApplicationDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var AssignedTenantViewModel = new List<AssignedTenantViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            var orgId = GetOrganisationId(getOrgClaim);

            var statementDetailsVm = new StatementDetailsViewModel();

            statementDetailsVm.organisation = SafriSoftApp.Organisations.FirstOrDefault(x => x.OrganisationId == orgId);

            var tenant = SafriSoft.Tenants.FirstOrDefault(x => x.Id == statementVm.TenantId);
            statementDetailsVm.tenant = tenant;

            var unitId = SafriSoft.Assigned.FirstOrDefault(x => x.TenantId == statementVm.TenantId).UnitId;
            var unitDetails = SafriSoft.Units.FirstOrDefault(x => x.Id == unitId);
            statementDetailsVm.unit = unitDetails;

            var leaseStart = tenant.DateLeaseStart;
            var dateFrom = DateTime.Parse(statementVm.DateFrom);
            var dateTo = DateTime.Parse(statementVm.DateTo);
            decimal? balanceTrack = 0;

            if (leaseStart != dateFrom)
            {
                while (leaseStart < dateFrom)
                {
                    int year = leaseStart.Year;
                    int month = leaseStart.Month;

                    var tenantTransactions = SafriSoft.Transactions.Where(x => x.TenantId == tenant.Id && x.TransactionDate.Year == leaseStart.Year && x.TransactionDate.Month == leaseStart.Month).ToList();

                    foreach (var tenantTransaction in tenantTransactions)
                    {
                        balanceTrack = tenantTransaction.TransactionCode != 15 ? balanceTrack + tenantTransaction.TransactionAmount : balanceTrack - tenantTransaction.TransactionAmount;
                    }

                    leaseStart = leaseStart.AddMonths(1);
                }
            }

            statementDetailsVm.BalanceBF = balanceTrack;
            
            var transactions = SafriSoft.Transactions.Where(x => x.TenantId == tenant.Id && x.TransactionDate.Year == dateFrom.Year && x.TransactionDate.Month == dateFrom.Month).ToList();

            if (transactions.Count > 0)
            {
                
                foreach (var transaction in transactions)
                {
                    statementDetailsVm.transactions.Add(transaction);
                    balanceTrack = transaction.TransactionCode != 15 ? balanceTrack + transaction.TransactionAmount : balanceTrack - transaction.TransactionAmount;
                }
            }

            statementDetailsVm.Balance = balanceTrack;

            return Json(statementDetailsVm);
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
                        cmd.CommandText = string.Format("DELETE from [{0}].[dbo].[Documents] where Id = {1}", conn.Database, RecordData.Id);
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
                        cmd.CommandText = string.Format("DELETE from [{0}].[dbo].[NOKs] where Id = {1}", conn.Database, RecordData.Id);
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
                    orderCmd.CommandText = string.Format("SELECT [Id],[TransactionCode],[TransactionName],[TransactionAmount],[TransactionDate],[TenantId] from [{0}].[dbo].[Transactions] Where TenantId = '{1}' AND TransactionDate = '{2}' AND (TransactionCode = '{3}' OR TransactionCode = '{4}')", conn.Database, TransactionData.TransactionFor, DateTime.Parse(TransactionData.DateTransactionRaised), 10, 20);

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
                    customerCmd.CommandText = string.Format("SELECT [TenantName],[TenantEmail],[TenantCell],[TenantAddress] from [{0}].[dbo].[Tenants] where Id = '{1}'", conn.Database, customerId);

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

        [HttpPost, Route("SafriOrderRequest")]
        public async Task<IHttpActionResult> SafriOrderRequest(OrderRequestViewModel orderRequest)
        {
            try
            {
                var createEmail = new SafriSoftEmailService();
                var toAddress = new List<string>();
                var toCCAddress = new List<string>();
                toAddress.Add("support@safrisoft.com");

                var subject = "Order Request - " + orderRequest.UserOrganisation;
                var emailBody = "An order has been request for: " + orderRequest.Package + " - Rental <br/><br/> This email has been sent by: " + orderRequest.UserEmail;
                createEmail.SaveEmail(subject, emailBody, "support@safrisoft.com", toAddress.ToArray(), toCCAddress.ToArray());
                return Json(new { Success = true, Message = "Order request has been sent" });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.ToString() });
            }
        }

        [HttpPost, Route("SendCustomEmail")]
        public async Task<IHttpActionResult> SendCustomEmail(EmailViewModel email)
        {
            try
            {
                ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                string userId = IdentityExtensions.GetUserId(User.Identity);
                var safriSoftImsDb = new SafriSoftDbContext();
                var createEmail = new SafriSoftEmailService();
                var toAddress = new List<string>();
                var toCCAddress = new List<string>();

                toAddress.Add(email.EmailAddress);

                var subject = email.EmailSubject;
                var emailBody = email.EmailBody;

                createEmail.SaveEmail(subject, emailBody, "support@safrisoft.com", toAddress.ToArray(), toCCAddress.ToArray());

                return Json(new { Success = true, Message = "Email has been sent" });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.ToString() });
            }
        }

        [HttpPost, Route("SendTenantStatementEmail")]
        public async Task<IHttpActionResult> SendTenantStatementEmail(SendTenantStatementViewModel sendTenantStatementVm)
        {
            try
            {
                ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
                string userId = IdentityExtensions.GetUserId(User.Identity);
                var safriSoftRMSDb = new SafriSoftDbContext();
                var tenant = safriSoftRMSDb.Tenants.FirstOrDefault(x => x.Id == sendTenantStatementVm.TenantId);
                var createEmail = new SafriSoftEmailService();
                var toAddress = new List<string>();
                var toCCAddress = new List<string>();
                var sb = new StringBuilder();

                toAddress.Add(tenant.TenantEmail);

                var subject = "Tax Invoice & Statement";

                sb.Append($"Your statement & Invoice is ready to be viewed.<br /><br />");
                sb.Append($"Statement & Invoice for period: <br />");
                sb.Append($"{sendTenantStatementVm.DateFrom} To {sendTenantStatementVm.DateTo} <br /><br />");
                sb.Append($"Please click here <a href='https://rms.safrisoft.com/Rental/TenantStatementPDF?TenantId={sendTenantStatementVm.TenantId}&DateFrom={sendTenantStatementVm.DateFrom}&DateTo={sendTenantStatementVm.DateTo}&OrgName={sendTenantStatementVm.OrganisationName}'>Statement & Invoice</a>");

                var emailBody = sb.ToString();

                createEmail.SaveEmail(subject, emailBody, "support@safrisoft.com", toAddress.ToArray(), toCCAddress.ToArray());

                return Json(new { Success = true, Message = "Email statement has been sent" });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Error = ex.ToString() });
            }
        }

        [HttpGet, Route("GetUserData")]
        public async Task<IHttpActionResult> GetUserData()
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var UserViewModel = new List<UserViewModel>();
            var countUsers = 0;
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            var username = "";
            var userState = "";

            var safriDbConn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString());
            safriDbConn.Open();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["IdentityDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT u.[Id],[Email],[UserName] from [dbo].[AspNetUsers] u, [dbo].[AspNetUserClaims] c WHERE u.Id = c.UserId AND c.ClaimType = 'Organisation' AND c.ClaimValue = '{1}'", conn.Database, getOrgClaim);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        try
                        {
                            var usernameClaim = userManager.GetClaims(reader.GetString(0)).First(x => x.Type == "Username");
                            username = usernameClaim.Value;
                        }
                        catch (Exception Ex)
                        {
                            username = reader.GetString(1);
                        }
                        try
                        {
                            var usernameState = userManager.GetClaims(reader.GetString(0)).First(x => x.Type == "AccountLocked");
                            userState = usernameState.Value;
                        }
                        catch (Exception Ex)
                        {
                            userState = "";
                        }

                        var Users = new UserViewModel();
                        countUsers += 1;
                        Users.Id = countUsers;
                        Users.UserId = reader.GetString(0);
                        Users.Email = reader.GetString(1);
                        Users.Username = username;
                        Users.UserRole = userManager.GetRoles(Users.UserId).First();
                        Users.UserState = userState;
                        var numberOfOrdersCmd = conn.CreateCommand();
                        //numberOfOrdersCmd.CommandText = string.Format("SELECT count([Id]) from [{0}].[dbo].[Orders] WHERE UserId = '{1}'", safriDbConn.Database, reader.GetString(0));
                        Users.NumberOfOrders = 0;
                        var randValueSoldCmd = conn.CreateCommand();
                        //randValueSoldCmd.CommandText = string.Format("SELECT sum([OrderWorth]) from [{0}].[dbo].[Orders] WHERE UserId = '{1}'", safriDbConn.Database, reader.GetString(0));
                        //try
                        //{
                        //    Users.RandValueSold = (Decimal)randValueSoldCmd.ExecuteScalar();
                        //}
                        //catch (Exception Ex)
                        //{
                        //    Users.RandValueSold = 0;
                        //}

                        UserViewModel.Add(Users);
                    }
                    reader.NextResult();
                    reader.Close();
                }
                safriDbConn.Close();
                conn.Close();
                return Json(UserViewModel);
            }
        }

        [HttpPost, Route("UserCreate")]
        public async Task<IHttpActionResult> UserCreate(RegisterViewModel UserData)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var UserViewModel = new List<UserViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            var orgId = GetOrganisationId(getOrgClaim);

            var exceeded = CheckPackageAccess("Users", orgId);

            if (exceeded == true)
            {
                return Json(new { Success = false, Error = "You have exceeded the number of Users to add. Please upgrade Package" });
            }

            var user = new ApplicationUser { UserName = UserData.Email, Email = UserData.Email };
            var result = await userManager.CreateAsync(user, UserData.Password);

            if (result.Succeeded)
            {
                Claim OrganisationClaim = new Claim("Organisation", UserData.OrganisationName);
                var saveClaim = await userManager.AddClaimAsync(user.Id, OrganisationClaim);
                Claim UsernameClaim = new Claim("Username", UserData.Username);
                var saveUsernameClaim = await userManager.AddClaimAsync(user.Id, UsernameClaim);
                var saveRole = userManager.AddToRole(user.Id, UserData.Role);
                var subject = "SafriSoft - Access";
                var emailBody = $"You have been granted access to the SafriSoft Rental Management Software by your Organisation Admin. <br/><br/> Please use the below details to access the software: <a href='https://rms.safrisoft.com'>Rental Management Software</a> <br/> Username: {user.UserName} <br/> Password: {UserData.Password}";

                var toAddress = new List<string>();
                var toCCAddress = new List<string>();
                toAddress.Add(user.Email);
                var createEmail = new SafriSoftEmailService();
                createEmail.SaveEmail(subject, emailBody, "support@safrisoft.com", toAddress.ToArray(), toCCAddress.ToArray());
                //await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);

                // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

            }

            return Json(result);
        }

        [HttpPost, Route("UserLock")]
        public async Task<IHttpActionResult> UserLock(UserViewModel UserData)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var UserViewModel = new List<UserViewModel>();
            var userState = "";

            try
            {
                var usernameState = userManager.GetClaims(UserData.UserId).First(x => x.Type == "AccountLocked");
                userState = usernameState.Value;
            }
            catch (Exception Ex)
            {
                userState = "Error";
            }

            try
            {
                if (userState == "Error")
                {
                    Claim UsernameStateClaim = new Claim("AccountLocked", "Locked");
                    var saveUsernameStateClaim = await userManager.AddClaimAsync(UserData.UserId, UsernameStateClaim);
                }
                else if (userState == "Locked")
                {
                    Claim UsernameStateClaim = new Claim("AccountLocked", "Locked");
                    userManager.RemoveClaim(UserData.UserId, UsernameStateClaim);
                    Claim UsernameStateClaimU = new Claim("AccountLocked", "UnLocked");
                    var saveUsernameStateClaim = await userManager.AddClaimAsync(UserData.UserId, UsernameStateClaimU);
                }
                else if (userState == "UnLocked")
                {
                    Claim UsernameStateClaimU = new Claim("AccountLocked", "UnLocked");
                    userManager.RemoveClaim(UserData.UserId, UsernameStateClaimU);
                    Claim UsernameStateClaimL = new Claim("AccountLocked", "Locked");
                    var saveUsernameStateClaim = await userManager.AddClaimAsync(UserData.UserId, UsernameStateClaimL);
                }
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.Message.ToString() });
            }

        }

        [HttpGet, Route("GetOrganisationDetails")]
        public async Task<IHttpActionResult> GetOrganisationDetails()
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var OrganisationViewModel = new List<OrganisationViewModel>();
            var countUsers = 0;
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            ApplicationDbContext SafriSoftAppDb = new ApplicationDbContext();

            var getOrganisation = SafriSoftAppDb.Organisations.FirstOrDefault(x => x.OrganisationName == getOrgClaim);

            //var OrganisationDetails = new OrganisationViewModel();

            //OrganisationDetails.OrganisationId = getOrganisation.OrganisationId;
            //OrganisationDetails.OrganisationName = getOrganisation.OrganisationName;
            //OrganisationDetails.OrganisationEmail = getOrganisation.OrganisationEmail;
            //OrganisationDetails.OrganisationCell = getOrganisation.OrganisationCell;
            //OrganisationDetails.OrganisationLogo = getOrganisation.OrganisationLogo;
            //OrganisationDetails.OrganisationStreet = getOrganisation.OrganisationStreet;
            //OrganisationDetails.OrganisationSuburb = getOrganisation.OrganisationSuburb;
            //OrganisationDetails.OrganisationCity = getOrganisation.OrganisationCity;
            //OrganisationDetails.OrganisationCode = getOrganisation.OrganisationCode;
            //OrganisationDetails.AccountName = "";
            //OrganisationDetails.AccountNo = 0;
            //OrganisationDetails.BankName = "";
            //OrganisationDetails.BranchName = "";
            //OrganisationDetails.BranchCode = "";
            //OrganisationDetails.ClientReference = "";
            //OrganisationDetails.VATNumber = 0;
            //OrganisationViewModel.Add(OrganisationDetails);

            return Json(getOrganisation);

            //using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["IdentityDbContext"].ToString()))
            //{
            //    conn.Open();
            //    var cmd = conn.CreateCommand();
            //    cmd.CommandText = string.Format("SELECT [OrganisationId],[OrganisationName],[OrganisationEmail],[OrganisationCell],[OrganisationLogo],[OrganisationStreet],[OrganisationSuburb],[OrganisationCity],[OrganisationCode],[AccountName],[AccountNo],[BankName],[BranchName],[BranchCode],[ClientReference],[VATNumber] from [{0}].[dbo].[Organisations] WHERE [OrganisationName] = '{1}'", conn.Database, getOrgClaim);

            //    using (var reader = cmd.ExecuteReader())
            //    {
            //        while (reader.Read())
            //        {
            //            var OrganisationDetails = new OrganisationViewModel();
            //            OrganisationDetails.OrganisationId      = reader.GetInt32(0);
            //            OrganisationDetails.OrganisationName    = reader.GetString(1);
            //            OrganisationDetails.OrganisationEmail   = reader.GetString(2) != null ? reader.GetString(2) : "";
            //            OrganisationDetails.OrganisationCell    = reader.GetString(3) != null ? reader.GetString(3) : "";
            //            OrganisationDetails.OrganisationLogo    = reader.GetString(4);
            //            OrganisationDetails.OrganisationStreet  = reader.GetString(5);
            //            OrganisationDetails.OrganisationSuburb  = reader.GetString(6);
            //            OrganisationDetails.OrganisationCity    = reader.GetString(7);
            //            OrganisationDetails.OrganisationCode    = reader.GetInt32(8);
            //            OrganisationDetails.AccountName         = reader.GetString(9);
            //            OrganisationDetails.AccountNo           = reader.GetInt32(10);
            //            OrganisationDetails.BankName            = reader.GetString(11);
            //            OrganisationDetails.BranchName          = reader.GetString(12);
            //            OrganisationDetails.BranchCode          = reader.GetString(13);
            //            OrganisationDetails.ClientReference     = reader.GetString(14);
            //            OrganisationDetails.VATNumber           = reader.GetInt32(15);
            //            OrganisationViewModel.Add(OrganisationDetails);
            //        }
            //        reader.NextResult();
            //        reader.Close();
            //    }

            //    return Json(OrganisationViewModel);
            //}
        }

        [HttpPost, Route("SaveOrganisationDetails")]
        public async Task<IHttpActionResult> SaveOrganisationDetails(Organisations Organisation)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var OrganisationViewModel = new List<OrganisationViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            ApplicationDbContext SafriSoft = new ApplicationDbContext();

            try
            {
                SafriSoft.Entry(Organisation).State = EntityState.Modified;
                SafriSoft.SaveChanges();

                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString() });
            }
        }

        [HttpGet, Route("GetCharges")]
        public async Task<IHttpActionResult> GetCharges()
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            var orgId = GetOrganisationId(getOrgClaim);

            var chargeVm = new List<ChargeViewModel>();

            var charges = SafriSoft.Charges.Where(x => x.OrganisationId == orgId).ToList();
            foreach(var c in charges)
            {
                var charge = new ChargeViewModel();
                charge.Id = c.Id;
                charge.Code = c.Code;
                charge.Type = c.Type;
                charge.Amount = c.Amount;
                charge.Name = c.Name;
                charge.Effective = c.Effective.ToString("yyyy-MM-dd");
                chargeVm.Add(charge);
            }
            return Json(chargeVm);
        }

        [HttpPost, Route("SaveCharge")]
        public async Task<IHttpActionResult> SaveCharge(ChargeViewModel chargeVm)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            var orgId = GetOrganisationId(getOrgClaim);
            string message = string.Empty;

            int result;

            var exists = SafriSoft.Charges.AsNoTracking().Where(x => x.Code == chargeVm.Code && x.Id != chargeVm.Id).FirstOrDefault();

            if (exists != null)
                return Json(new { success = false, message = "Charge code already exists" });

            var charge = new Charge();
            charge.Id = chargeVm.Id;
            charge.Name = chargeVm.Name;
            charge.Code = chargeVm.Code;
            charge.Type = chargeVm.Type;
            charge.Amount = chargeVm.Amount;
            charge.Effective = DateTime.Parse(chargeVm.Effective);
            charge.OrganisationId = orgId;

            if (chargeVm.Id == 0)
            {
                var addedCharge = SafriSoft.Charges.Add(charge);
                result = await SafriSoft.SaveChangesAsync();
                message = "Successfully created charge!";
            }
            else
            {
                SafriSoft.Entry(charge).State = EntityState.Modified;
                SafriSoft.SaveChanges();
                result = 1;
                message = "Successfully updated charge!";
            }            

            if (result > 0)
                return Json(new { success = true, message = message });
            else
                return Json(new { success = false, message = "Error saving the charge" });

        }

        [HttpPost, Route("RemoveCharge")]
        public async Task<IHttpActionResult> RemoveCharge(ChargeViewModel chargeVm)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            var orgId = GetOrganisationId(getOrgClaim);
            string message = string.Empty;

            var charge = new Charge();
            charge.Id = chargeVm.Id;
            charge.Name = chargeVm.Name;
            charge.Code = chargeVm.Code;
            charge.Type = chargeVm.Type;
            charge.Amount = chargeVm.Amount;
            charge.Effective = DateTime.Parse(chargeVm.Effective);
            charge.OrganisationId = orgId;

            try
            {
                SafriSoft.Entry(charge).State = EntityState.Deleted;
                var result = SafriSoft.Charges.Remove(charge);
                SafriSoft.SaveChanges();
                message = "Successfully removed charged!";
                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet, Route("GetUnitCharges/{Unitid}")]
        public async Task<IHttpActionResult> GetUnitCharges(int UnitId)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            var orgId = GetOrganisationId(getOrgClaim);

            var chargeVm = new List<ChargeViewModel>();

            var charges = SafriSoft.Charges.Where(x => x.OrganisationId == orgId).ToList();
            foreach (var c in charges)
            {
                int count = SafriSoft.UnitCharges.Where(x => x.FeeId == c.Id && x.UnitId == UnitId).Count();
                var charge = new ChargeViewModel();
                charge.Id = c.Id;
                charge.Code = c.Code;
                charge.Type = c.Type;
                charge.Amount = c.Amount;
                charge.Name = c.Name;
                charge.Effective = c.Effective.ToString("yyyy-MM-dd");
                charge.UnitAssigned = count > 0 ? true : false;
                charge.UnitChargeId = count > 0 ? SafriSoft.UnitCharges.Where(x => x.FeeId == c.Id && x.UnitId == UnitId).Select(x => x.Id).FirstOrDefault() : 0;
                chargeVm.Add(charge);
            }
            return Json(chargeVm);
        }

        [HttpPost, Route("AddUnitCharge")]
        public async Task<IHttpActionResult> AddUnitCharge(ChargeViewModel chargeVm)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            var orgId = GetOrganisationId(getOrgClaim);
            string message = string.Empty;

            var UnitCharge = new UnitCharge();
            UnitCharge.FeeId = chargeVm.Id;
            UnitCharge.UnitId = chargeVm.UnitId;

            try
            {
                SafriSoft.UnitCharges.Add(UnitCharge);
                SafriSoft.SaveChanges();
                message = "Successfully added charge to unit!";
                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost, Route("RemoveUnitCharge")]
        public async Task<IHttpActionResult> RemoveUnitCharge(ChargeViewModel chargeVm)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            var orgId = GetOrganisationId(getOrgClaim);
            string message = string.Empty;

            var UnitCharge = new UnitCharge();
            UnitCharge.Id = chargeVm.UnitChargeId;
            UnitCharge.FeeId = chargeVm.Id;
            UnitCharge.UnitId = chargeVm.UnitId;

            try
            {
                SafriSoft.Entry(UnitCharge).State = EntityState.Deleted;
                var result = SafriSoft.UnitCharges.Remove(UnitCharge);
                SafriSoft.SaveChanges();
                message = "Successfully removed charge from unit!";
                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Reusable functions at the bottom
        public static JObject RaiseTransaction(int TransactionCode, string TransactionName, decimal? TransactionAmount, DateTime DateTransactionRaised, int TenantId)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            JObject o = new JObject();

            var organisationId = SafriSoft.Tenants.FirstOrDefault(x => x.Id == TenantId).OrganisationId;

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
                        cmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[Transactions] ([TransactionCode],[TransactionName],[TransactionDate],[TenantId],[OrganisationId]) VALUES('{1}','{2}','{3}','{4}','{5}')", conn.Database, TransactionCode, TransactionName, DateTransactionRaised, TenantId, organisationId);
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

        public bool CheckPackageAccess(string feature, int orgnaisationId)
        {
            SafriSoftDbContext SafriSoftImsDb = new SafriSoftDbContext();
            ApplicationDbContext SafriSoftDb = new ApplicationDbContext();
            var identityConn = new SqlConnection(ConfigurationManager.ConnectionStrings["IdentityDbContext"].ToString());
            identityConn.Open();
            var identityPackageCmd = identityConn.CreateCommand();
            identityPackageCmd.CommandText = string.Format("SELECT pf.[Limit], os.[PackageId] from [dbo].[Organisations] o JOIN [dbo].[OrganisationSoftwares] os on os.[OrganisationId] = o.[OrganisationId] AND os.[SoftwareId] = 2 JOIN [dbo].[PackageFeatures] pf on pf.PackageId = os.[PackageId] AND pf.FeatureName = '{1}' WHERE o.[OrganisationId] = {2}", identityConn.Database, feature, orgnaisationId);
            var identityPackageReader = identityPackageCmd.ExecuteReader();
            var packageFeatureLimit = 0;
            var limitExceeded = false;

            if (identityPackageReader.Read())
            {
                packageFeatureLimit = identityPackageReader.GetInt32(0);
                var package = identityPackageReader.GetInt32(1);
                if (feature == "Tenants")
                {
                    var numberOfCustomers = SafriSoftImsDb.Tenants.Where(x => x.OrganisationId == orgnaisationId).ToList().Count();
                    if (numberOfCustomers >= packageFeatureLimit && package != 3)
                    {
                        limitExceeded = true;
                    }
                }
                else if (feature == "Units")
                {
                    var numberOfOrders = SafriSoftImsDb.Units.Where(x => x.OrganisationId == orgnaisationId).ToList().Count();
                    if (numberOfOrders >= packageFeatureLimit && package != 3)
                    {
                        limitExceeded = true;
                    }
                }else if (feature == "Users")
                {
                    var organisationName = SafriSoftDb.Organisations.FirstOrDefault(x => x.OrganisationId == orgnaisationId).OrganisationName;
                    var currentNumber = SafriSoftDb.Users.Where(x => x.Claims.Where(c => c.ClaimType == "Organisation").FirstOrDefault().ClaimValue == organisationName).ToList().Count();
                    if (currentNumber >= packageFeatureLimit)
                    {
                        limitExceeded = true;
                    }
                }
            }

            identityConn.Close();

            return limitExceeded;
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

        public int GetOrganisationId(string organisationName)
        {
            int organisationId = 0;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["IdentityDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [OrganisationId] from [{0}].[dbo].[Organisations] where OrganisationName = '{1}'", conn.Database, organisationName);
                organisationId = (int)cmd.ExecuteScalar();
                conn.Close();
            }

            return organisationId;
        }
    }
}
