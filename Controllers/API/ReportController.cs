using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SafriSoftv1._3.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using SafriSoftv1._3.Models.Rental;

namespace SafriSoftv1._3.Controllers.API
{
    [RoutePrefix("api/Report")]
    public class ReportController : ApiController
    {
        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
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
                cmd.CommandText = string.Format("SELECT [Id],[TenantName],[TenantEmail],[TenantCell],[TenantAddress],[TenantWorkAddress],[TenantWorkCell],[DateTenantCreated],[DateLeaseStart],[DateLeaseEnd],[Status] from [{0}].[dbo].[Tenants] where OrganisationId = '{2}' AND Status = '{1}'", conn.Database, "Active", orgId);

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
                cmd.CommandText = string.Format("SELECT [Id],[UnitNumber],[UnitName],[UnitRooms],[UnitDescription],[UnitPrice],[Sharing] from [{0}].[dbo].[Units] where OrganisationId = '{2}' AND Status = '{1}'", conn.Database, "Active", orgId);

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
                        units.NumberOfTenants = SafriSoft.Assigned.Count(x => x.UnitId == units.Id);
                        var tenants = SafriSoft.Assigned.Where(x => x.UnitId == units.Id).ToArray();
                        List<string> tenantsNames = new List<string>();

                        for (int i = 0; i < tenants.Length; i++)
                        {
                            var tId = tenants[i].TenantId;
                            var name = SafriSoft.Tenants.Where(x => x.Id == tId).Select(x => x.TenantName).FirstOrDefault();
                            tenantsNames.Add(name);
                        }

                        //units.DateFileCreated = reader.GetDateTime(2).ToString("dd MMMM yyyy");
                        units.TenantName = tenantsNames.Count > 0 ? string.Join(" & ", tenantsNames) : "";
                        UnitViewModel.Add(units);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(UnitViewModel);
            }
        }

        [HttpGet, Route("GetExpectedTransactions")]
        public async Task<IHttpActionResult> GetExpectedTransactions()
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var transactionViewModel = new List<TransactionReportsViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            var orgId = GetOrganisationId(getOrgClaim);


            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                var today = DateTime.Now;
                var startDate = new DateTime(today.Year, today.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[TransactionCode],[TransactionName],[TransactionAmount],[TransactionDate],[TenantId] from [{0}].[dbo].[Transactions] Where OrganisationId = '{2}' AND TransactionCode <> '{1}'", conn.Database, 15, orgId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tran = new TransactionReportsViewModel();
                        tran.TransactionCode = reader.GetInt32(1).ToString();
                        tran.TransactionName = reader.GetString(2);
                        tran.TransactionAmount = reader.GetDecimal(3);
                        tran.TransactionDate = reader.GetDateTime(4).ToString("dd MMMM yyyy");
                        var tId = reader.GetInt32(5);
                        tran.Tenant = SafriSoft.Tenants.FirstOrDefault(x => x.Id == tId).TenantName;
                        transactionViewModel.Add(tran);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(transactionViewModel);
            }
        }

        [HttpGet, Route("GetReceivedTransactions")]
        public async Task<IHttpActionResult> GetReceivedTransactions()
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var transactionViewModel = new List<TransactionReportsViewModel>();
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            var orgId = GetOrganisationId(getOrgClaim);

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                var today = DateTime.Now;
                var startDate = new DateTime(today.Year, today.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[TransactionCode],[TransactionName],[TransactionAmount],[TransactionDate],[TenantId] from [{0}].[dbo].[Transactions] Where OrganisationId = '{2}' AND TransactionCode = '{1}'", conn.Database, 15, orgId);


                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var tran = new TransactionReportsViewModel();
                        tran.TransactionCode = reader.GetInt32(1).ToString();
                        tran.TransactionName = reader.GetString(2);
                        tran.TransactionAmount = reader.GetDecimal(3);
                        tran.TransactionDate = reader.GetDateTime(4).ToString("dd MMMM yyyy");
                        var tId = reader.GetInt32(5);
                        tran.Tenant = SafriSoft.Tenants.FirstOrDefault(x => x.Id == tId).TenantName;
                        transactionViewModel.Add(tran);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(transactionViewModel);
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
            }

            return organisationId;
        }

    }
}