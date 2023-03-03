using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json.Linq;
using SafriSoftv1._3.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using SafriSoftv1._3.Models.Data;
using System.Data.Entity;
using System.IO;
using System.Drawing;
using System.Net.Mail;
using ExcelDataReader;

namespace SafriSoftv1._3.Controllers.API
{
    [RoutePrefix("api/Inventory")]
    public class InventoryController : ApiController
    {
        
        // GET: api/Inventory
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Inventory/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Inventory
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Inventory/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Inventory/5
        public void Delete(int id)
        {
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
                        numberOfOrdersCmd.CommandText = string.Format("SELECT count([Id]) from [{0}].[dbo].[Orders] WHERE UserId = '{1}'", safriDbConn.Database, reader.GetString(0));
                        Users.NumberOfOrders = (Int32)numberOfOrdersCmd.ExecuteScalar();
                        var randValueSoldCmd = conn.CreateCommand();
                        randValueSoldCmd.CommandText = string.Format("SELECT sum([OrderWorth]) from [{0}].[dbo].[Orders] WHERE UserId = '{1}'", safriDbConn.Database, reader.GetString(0));
                        try
                        {
                            Users.RandValueSold = (Decimal)randValueSoldCmd.ExecuteScalar();
                        }
                        catch (Exception Ex)
                        {
                            Users.RandValueSold = 0;
                        }
                        
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

            var user = new ApplicationUser { UserName = UserData.Email, Email = UserData.Email };
            var result = await userManager.CreateAsync(user, UserData.Password);

            if (result.Succeeded)
            {
                Claim OrganisationClaim = new Claim("Organisation", UserData.OrganisationName);
                var saveClaim = await userManager.AddClaimAsync(user.Id, OrganisationClaim);
                Claim UsernameClaim = new Claim("Username", UserData.Username);
                var saveUsernameClaim = await userManager.AddClaimAsync(user.Id, UsernameClaim);
                var saveRole = userManager.AddToRole(user.Id, UserData.Role);
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
                return Json(new { Success = false, Error = Ex.Message.ToString()});
            }
            
        }

        [HttpGet, Route("GetProducts/{Id}")]
        public async Task<IHttpActionResult> GetProducts(int Id)
        {
            SafriSoftDbContext SafriSoftDb = new SafriSoftDbContext();
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var ProductViewModel = new List<ProductViewModel>();
            var countUsers = 0;
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[ProductName],[ProductReference],[SellingPrice],[ItemsSold],[ItemsAvailable],[Status],[ProductCategory],[ProductImage],[ProductCode] from [{0}].[dbo].[Product] where Id = {1}", conn.Database,Id);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var Products = new ProductViewModel();
                        countUsers += 1;
                        Products.Id = reader.GetInt32(0);
                        Products.ProductName = reader.GetString(1);
                        Products.ProductReference = reader.GetString(2);
                        Products.SellingPrice = reader.GetDouble(3);
                        Products.ItemsSold = reader.GetInt32(4);
                        Products.ItemsAvailable = reader.GetInt32(5);
                        Products.ProductCategory = reader.GetString(7);
                        Products.ProductImage = reader.GetString(8);
                        Products.ProductCode = reader.GetString(9);
                        ProductViewModel.Add(Products);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(ProductViewModel);
            }
        }

        [HttpGet, Route("GetProducts")]
        public async Task<IHttpActionResult> GetProducts()
        {
            SafriSoftDbContext SafriSoftDb = new SafriSoftDbContext();
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var ProductViewModel = new List<ProductViewModel>();
            var countUsers = 0;
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[ProductName],[ProductReference],[SellingPrice],[ItemsSold],[ItemsAvailable],[Status],[ProductCategory],[ProductImage],[ProductCode] from [{0}].[dbo].[Product] where Status = '{1}'", conn.Database, "Active");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var Products = new ProductViewModel();
                        countUsers += 1;
                        Products.Id = reader.GetInt32(0);
                        Products.ProductName = reader.GetString(1);
                        Products.ProductReference = reader.GetString(2);
                        Products.SellingPrice = reader.GetDouble(3);
                        Products.ItemsSold = reader.GetInt32(4);
                        Products.ItemsAvailable = reader.GetInt32(5);
                        Products.ProductCategory = reader.GetString(7);
                        Products.ProductImage = reader.GetString(8);
                        Products.ProductCode = reader.GetString(9);
                        ProductViewModel.Add(Products);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(ProductViewModel);
            }
        }

        [HttpPost, Route("ProductCreate")]
        public async Task<IHttpActionResult> ProductCreate(JObject ProductData)
        {
            var productCode = ProductData.Value<string>("ProductCode");
            var productCategory = ProductData.Value<string>("ProductCategory");
            var productName = ProductData.Value<string>("ProductName");
            var productReference = ProductData.Value<string>("ProductReference");
            var sellingPrice = ProductData.Value<string>("SellingPrice");
            var itemsSold = ProductData.Value<string>("ItemsSold");
            var itemsAvailable = ProductData.Value<string>("ItemsAvailable");
            var productImage = ProductData.Value<string>("ProductImage");

            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();

            var product = SafriSoft.Products.FirstOrDefault(x => x.ProductReference == productReference || x.ProductCode == productCode);

            if(product != null)
            {
                return BadRequest("Product already exists");
            }

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[Product] ([ProductName],[ProductReference],[SellingPrice],[ItemsSold],[ItemsAvailable],[Status],[ProductCategory],[ProductImage],[ProductCode]) VALUES('{1}','{2}',{3},'{4}','{5}','{6}','{7}','{8}','{9}')", conn.Database, productName, productReference, sellingPrice, itemsSold, itemsAvailable, "Active",productCategory,productImage, productCode);
                    await cmd.ExecuteNonQueryAsync();

                }
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return BadRequest(Ex.ToString());
            }
            
        }

        [HttpPost, Route("ProductUpdate")]
        public async Task<IHttpActionResult> ProductUpdate(ProductViewModel ProductData)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();

            try
            {
                var dbProduct = SafriSoft.Products.First(x => x.Id == ProductData.Id);

                if (dbProduct.ProductName != ProductData.ProductName)
                {
                    dbProduct.ProductName = ProductData.ProductName;
                }

                if (dbProduct.ProductReference != ProductData.ProductReference)
                {
                    dbProduct.ProductReference = ProductData.ProductReference;
                }

                if (dbProduct.SellingPrice != ProductData.SellingPrice)
                {
                    dbProduct.SellingPrice = ProductData.SellingPrice;
                }

                if (dbProduct.ItemsAvailable != ProductData.ItemsAvailable)
                {
                    dbProduct.ItemsAvailable = ProductData.ItemsAvailable;
                }

                if (dbProduct.ProductCode != ProductData.ProductCode)
                {
                    dbProduct.ProductCode = ProductData.ProductCode;
                }

                if (dbProduct.ProductCategory != ProductData.ProductCategory)
                {
                    dbProduct.ProductCategory = ProductData.ProductCategory;
                }

                if (dbProduct.ProductImage != ProductData.ProductImage)
                {
                    dbProduct.ProductImage = ProductData.ProductImage;
                }

                SafriSoft.SaveChanges();
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return BadRequest(Ex.ToString());
            }

        }

        [HttpPost, Route("ProductDelete")]
        public async Task<IHttpActionResult> ProductDelete(ProductViewModel ProductData)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();

            try
            {
                var dbProduct = SafriSoft.Products.First(x => x.Id == ProductData.Id);
                dbProduct.Status = "Deleted";
                SafriSoft.SaveChanges();
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return BadRequest(Ex.ToString());
            }

        }

        [HttpGet, Route("GetCustomers")]
        public async Task<IHttpActionResult> GetCustomers()
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var CustomerViewModel = new List<CustomerViewModel>();
            var countUsers = 0;
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[CustomerName],[CustomerEmail],[CustomerCell],[CustomerAddress],[DateCustomerCreated],[NumberOfOrders] from [{0}].[dbo].[Customer] where Status = '{1}'", conn.Database, "Active");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var Customers = new CustomerViewModel();
                        countUsers += 1;
                        Customers.Id = reader.GetInt32(0);
                        Customers.CustomerName = reader.GetString(1);
                        Customers.CustomerEmail = reader.GetString(2);
                        Customers.CustomerCell = reader.GetString(3);
                        Customers.CustomerAddress = reader.GetString(4);
                        Customers.DateCustomerCreated = reader.GetString(5);
                        var numberOfOrders = SafriSoft.Orders.Where(x => x.CustomerId == Customers.Id).Count();
                        var customer = SafriSoft.Customers.Where(x => x.Id == Customers.Id).FirstOrDefault();
                        customer.NumberOfOrders = numberOfOrders;
                        SafriSoft.SaveChanges();
                        Customers.NumberOfOrders = numberOfOrders;
                        CustomerViewModel.Add(Customers);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(CustomerViewModel);
            }
        }

        [HttpGet, Route("GetCustomer/{Id}")]
        public async Task<IHttpActionResult> GetCustomer(int Id)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var CustomerViewModel = new List<CustomerViewModel>();
            var countUsers = 0;
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[CustomerName],[CustomerEmail],[CustomerCell],[CustomerAddress],[DateCustomerCreated],[NumberOfOrders] from [{0}].[dbo].[Customer] where Id = {1}", conn.Database, Id);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var Customers = new CustomerViewModel();
                        countUsers += 1;
                        Customers.Id = reader.GetInt32(0);
                        Customers.CustomerName = reader.GetString(1);
                        Customers.CustomerEmail = reader.GetString(2);
                        Customers.CustomerCell = reader.GetString(3);
                        Customers.CustomerAddress = reader.GetString(4);
                        Customers.DateCustomerCreated = reader.GetString(5);
                        var numberOfOrders = SafriSoft.Orders.Where(x => x.CustomerId == Customers.Id).Count();
                        var customer = SafriSoft.Customers.Where(x => x.Id == Customers.Id).FirstOrDefault();
                        customer.NumberOfOrders = numberOfOrders;
                        SafriSoft.SaveChanges();
                        Customers.NumberOfOrders = numberOfOrders;
                        CustomerViewModel.Add(Customers);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(CustomerViewModel);
            }
        }

        [HttpPost, Route("CustomerCreate")]
        public async Task<IHttpActionResult> CustomerCreate(CustomerViewModel CustomerData)
        {
            var customerName = CustomerData.CustomerName;
            var customerEmail = CustomerData.CustomerEmail;
            var customerAddress = CustomerData.CustomerAddress;
            var customerCell = CustomerData.CustomerCell;
            var dateCustomerCreated = CustomerData.DateCustomerCreated;

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[Customer] ([CustomerName],[CustomerEmail],[CustomerCell],[CustomerAddress],[DateCustomerCreated],[Status],[NumberOfOrders]) VALUES('{1}','{2}','{3}','{4}','{5}','{6}','{7}')", conn.Database, customerName, customerEmail, customerCell, customerAddress, dateCustomerCreated, "Active",1);
                    await cmd.ExecuteNonQueryAsync();

                }
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString() });
            }

        }

        [HttpPost, Route("CustomerUpdate")]
        public async Task<IHttpActionResult> CustomerUpdate(CustomerViewModel CustomerData)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();

            try
            {
                var dbCustomer = SafriSoft.Customers.First(x => x.Id == CustomerData.Id);

                if (dbCustomer.CustomerName != CustomerData.CustomerName)
                {
                    dbCustomer.CustomerName = CustomerData.CustomerName;
                }

                if (dbCustomer.CustomerEmail != CustomerData.CustomerEmail)
                {
                    dbCustomer.CustomerEmail = CustomerData.CustomerEmail;
                }

                if (dbCustomer.CustomerCell != CustomerData.CustomerCell)
                {
                    dbCustomer.CustomerCell = CustomerData.CustomerCell;
                }

                if (dbCustomer.CustomerAddress != CustomerData.CustomerAddress)
                {
                    dbCustomer.CustomerAddress = CustomerData.CustomerAddress;
                }

                SafriSoft.SaveChanges();
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return BadRequest(Ex.ToString());
            }

        }

        [HttpPost, Route("CustomerDelete")]
        public async Task<IHttpActionResult> CustomerDelete(CustomerViewModel CustomerData)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();

            try
            {
                var dbCustomer = SafriSoft.Customers.First(x => x.Id == CustomerData.Id);
                dbCustomer.Status = "Deleted";
                SafriSoft.SaveChanges();
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return BadRequest(Ex.ToString());
            }

        }

        [HttpGet, Route("GetOrders")]
        public async Task<IHttpActionResult> GetOrders()
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var OrderViewModel = new List<OrderViewModel>();
            var countUsers = 0;
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [OrderId],[ProductName],[NumberOfItems],[CustomerName],[OrderStatus],[OrderProgress],[DateOrderCreated],[ExpectedDeliveryDate],[OrderWorth],[ShippingCost] from [{0}].[dbo].[Orders] where Status = '{1}' ORDER BY OrderProgress ASC", conn.Database, "Active");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var Orders = new OrderViewModel();
                        countUsers += 1;
                        Orders.OrderId = reader.GetString(0);
                        Orders.ProductName = reader.GetString(1);
                        Orders.NumberOfItems = reader.GetInt32(2);
                        Orders.CustomerName = reader.GetString(3);
                        Orders.OrderStatus = reader.GetString(4);
                        Orders.OrderProgress = reader.GetInt32(5);
                        Orders.DateOrderCreated = reader.GetString(6);
                        Orders.ExpectedDeliveryDate = reader.GetString(7);
                        Orders.OrderWorth = reader.GetDecimal(8);
                        Orders.ShippingCost = reader.GetDecimal(9);
                        OrderViewModel.Add(Orders);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(OrderViewModel);
            }
        }

        [HttpGet, Route("GetOrders/{id}")]
        public async Task<IHttpActionResult> GetOrders(int id)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var OrderViewModel = new List<OrderViewModel>();
            var countUsers = 0;
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [OrderId],[ProductName],[NumberOfItems],[CustomerName],[OrderStatus],[OrderProgress],[DateOrderCreated],[ExpectedDeliveryDate],[OrderWorth] from [{0}].[dbo].[Orders] WHERE CustomerId = {1} ORDER BY OrderProgress ASC", conn.Database,id);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var Orders = new OrderViewModel();
                        countUsers += 1;
                        Orders.OrderId = reader.GetString(0);
                        Orders.ProductName = reader.GetString(1);
                        Orders.NumberOfItems = reader.GetInt32(2);
                        Orders.CustomerName = reader.GetString(3);
                        Orders.OrderStatus = reader.GetString(4);
                        Orders.OrderProgress = reader.GetInt32(5);
                        Orders.DateOrderCreated = reader.GetString(6);
                        Orders.ExpectedDeliveryDate = reader.GetString(7);
                        Orders.OrderWorth = reader.GetDecimal(8);
                        OrderViewModel.Add(Orders);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(OrderViewModel);
            }
        }

        [HttpPost, Route("OrderCreate")]
        public async Task<IHttpActionResult> OrderCreate(OrderViewModel OrderData)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            var customerId = OrderData.CustomerId;
            var customerName = OrderData.CustomerName;
            var productName = OrderData.ProductName;
            var productReference = OrderData.ProductReference.Replace('_', '/');
            var numberOfItems = OrderData.NumberOfItems;
            var dateOrderCreated = OrderData.DateOrderCreated;
            var expectedDateOfDelivery = OrderData.ExpectedDeliveryDate;
            var description = "Inception - This order was created!";
            var changed = "Inception";

            Random rnd = new Random();

            string generateOrderId = "#0" ;

            for(int i = 0; i <= 9; i++)
            {
                generateOrderId = generateOrderId + rnd.Next(1, 9).ToString();
            }
            
            var product = SafriSoft.Products.Where(x => x.ProductReference == productReference).FirstOrDefault();

            if (product.ItemsAvailable == 0)
            {
                return Json(new { Success = false, Error = "This product is no longer available!" });
            }

            if (product.ItemsAvailable < numberOfItems)
            {
                return Json(new { Success = false, Error = "The number of items exceed what's in stock!" });
            }

            var orderWorth = product.SellingPrice * numberOfItems;

            var finalNumberOfItems = product.ItemsAvailable - numberOfItems;
            var finalSold = product.ItemsSold + numberOfItems;
            product.ItemsAvailable = finalNumberOfItems;
            product.ItemsSold = finalSold;
            SafriSoft.SaveChanges();

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[Orders] ([OrderId],[ProductName],[NumberOfItems],[CustomerId],[CustomerName],[OrderStatus],[OrderProgress],[DateOrderCreated],[ExpectedDeliveryDate]) VALUES('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}')", conn.Database, generateOrderId, productName, numberOfItems, customerId, customerName,"Processed",10,dateOrderCreated, expectedDateOfDelivery);
                    await cmd.ExecuteNonQueryAsync();

                    var auditCmd = conn.CreateCommand();
                    auditCmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[OrderAudit] ([Description],[Changed],[CreatedDate],[UserId],[OrderId]) VALUES('{1}','{2}','{3}','{4}','{5}')", conn.Database, description, changed, DateTime.Now, userId, generateOrderId);
                    await auditCmd.ExecuteNonQueryAsync();

                }
                if(orderWorth > 0)
                {
                    var order = SafriSoft.Orders.First(x => x.OrderId == generateOrderId);
                    order.OrderWorth = Decimal.Parse(orderWorth.ToString());
                    order.ShippingCost = Decimal.Parse(OrderData.ShippingCost.ToString());
                    order.Status = "Active";
                    SafriSoft.SaveChanges();
                }
                var customerIdParse = int.Parse(customerId);
                var customer = SafriSoft.Customers.FirstOrDefault(x=>x.Id == customerIdParse);
                var downloadLink = "http://safrisoft.com/Inventory/CustomerInvoicePdf?OrderId=" + generateOrderId.Substring(1,generateOrderId.Length - 1);
                var emailBody = "Hi " + customerName + ",<br/><br /> Your order has been created.<br/><br/> You will be notified when your order is Packaged, Intransit or Successfully delivered.<br /><br/> Download invoice here: " + downloadLink;

                using (MailMessage mt = new MailMessage())
                {
                    mt.From = new MailAddress("admin@safrisoft.com");
                    mt.IsBodyHtml = true;
                    mt.Subject = "SafriSoft - Order Created";
                    mt.Body = "<span><h1 style='color:#17a2b8;'>SafriSoft.</h1></span></br><div style='font-family:Courier New;'> " + emailBody + "</div></br><br /><p style='font-family:Courier New;'>Regards,</p><div style='height:2px;background-color:#17a2b8;width:40%;margin-bottom:5px;'></div><div><p style='font-family:Courier New;'>Administrator</p><p style='font-family:Courier New;'>Website: www.safrisoft.com</p><p style='font-family:Courier New;'>Email: admin@safrisoft.com</p><p style='font-family:Courier New;'>Cell: 067 272 7320</p></div><div style='height:2px;background-color:#17a2b8;width:40%;margin-bottom:5px;'></div>";
                    mt.To.Add(customer.CustomerEmail);
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

                return Json(new { Success = true, CustomerID = customerId });
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString(), OrderWorth = orderWorth, OrderWorthToString = orderWorth.ToString("#.##") });
            }

        }

        [HttpPost, Route("ChangeStatus")]
        public async Task<IHttpActionResult> ChangeStatus(OrderViewModel OrderData)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var description = "Status - This order was changed to " + OrderData.OrderStatus + " !";
            var changed = OrderData.OrderStatus;

            try
            {
                var order = SafriSoft.Orders.Where(x => x.OrderId == OrderData.OrderId).FirstOrDefault();
                order.OrderProgress = OrderData.OrderProgress;
                order.OrderStatus = OrderData.OrderStatus;
                SafriSoft.SaveChanges();

                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                {
                    conn.Open();
                    var auditCmd = conn.CreateCommand();
                    auditCmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[OrderAudit] ([Description],[Changed],[CreatedDate],[UserId],[OrderId]) VALUES('{1}','{2}','{3}','{4}','{5}')", conn.Database, description, changed, DateTime.Now, userId, order.OrderId);
                    await auditCmd.ExecuteNonQueryAsync();

                }

                var customer = SafriSoft.Customers.FirstOrDefault(x => x.Id == order.CustomerId);
                var subject = OrderData.OrderStatus;
                var emailBody = "Hi " + customer.CustomerName + ",<br/><br /> Your order has been updated.<br/><br/> Order Id: " + OrderData.OrderId + "<br/><br/>" + description;

                using (MailMessage mt = new MailMessage())
                {
                    mt.From = new MailAddress("admin@safrisoft.com");
                    mt.IsBodyHtml = true;
                    mt.Subject = "SafriSoft - " + subject;
                    mt.Body = "<span><h1 style='color:#17a2b8;'>SafriSoft.</h1></span></br><div style='font-family:Courier New;'> " + emailBody + "</div></br><br /><p style='font-family:Courier New;'>Regards,</p><div style='height:2px;background-color:#17a2b8;width:40%;margin-bottom:5px;'></div><div><p style='font-family:Courier New;'>Administrator</p><p style='font-family:Courier New;'>Website: www.safrisoft.com</p><p style='font-family:Courier New;'>Email: admin@safrisoft.com</p><p style='font-family:Courier New;'>Cell: 067 272 7320</p></div><div style='height:2px;background-color:#17a2b8;width:40%;margin-bottom:5px;'></div>";
                    mt.To.Add(customer.CustomerEmail);
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

                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString() });
            }

        }

        [HttpPost, Route("OrderDelete")]
        public async Task<IHttpActionResult> OrderDelete(OrderViewModel OrderData)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var status = "Deleted";

            try
            {
                var order = SafriSoft.Orders.Where(x => x.OrderId == OrderData.OrderId).FirstOrDefault();
                order.Status = status;
                SafriSoft.SaveChanges();
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString() });
            }

        }

        [HttpGet, Route("Home")]
        public IHttpActionResult Home()
        {
            var customerCount = 0;
            var stockSold = 0;
            var stockAvailable = 0;
            var ordersProcessed = 0;
            var ordersPackaged = 0;
            var ordersInTransit = 0;
            var ordersDelivered = 0;
            decimal randValueSold = 0;

            try
            {
                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                {
                    conn.Open();
                    var customerCountCmd = conn.CreateCommand();
                    customerCountCmd.CommandText = string.Format("SELECT count([Id]) from [{0}].[dbo].[Customer] WHERE [Status] == 'Active'", conn.Database);
                    try
                    {
                        customerCount = (Int32)customerCountCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        customerCount = 0;
                    }
                    

                    var stockSoldCmd = conn.CreateCommand();
                    stockSoldCmd.CommandText = string.Format("SELECT sum([ItemsSold]) from [{0}].[dbo].[Product] WHERE [Status] = 'Active'", conn.Database);
                    try
                    {
                        stockSold = (Int32)stockSoldCmd.ExecuteScalar();
                    }
                    catch(Exception Ex)
                    {
                        stockSold = 0;
                    }
                    

                    var stockAvailableCmd = conn.CreateCommand();
                    stockAvailableCmd.CommandText = string.Format("SELECT sum([ItemsAvailable]) from [{0}].[dbo].[Product] WHERE [Status] = 'Active'", conn.Database);
                    try
                    {
                        stockAvailable = (Int32)stockAvailableCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        stockAvailable = 0;
                    }
                    

                    var ordersProcessedCmd = conn.CreateCommand();
                    ordersProcessedCmd.CommandText = string.Format("SELECT count([Id]) from [{0}].[dbo].[Orders] WHERE [OrderStatus] = 'Processed'", conn.Database);
                    try
                    {
                        ordersProcessed = (Int32)ordersProcessedCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        ordersProcessed = 0;
                    }
                    

                    var ordersPackagedCmd = conn.CreateCommand();
                    ordersPackagedCmd.CommandText = string.Format("SELECT count([Id]) from [{0}].[dbo].[Orders] WHERE [OrderStatus] = 'Packaged'", conn.Database);
                    try
                    {
                        ordersPackaged = (Int32)ordersPackagedCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        ordersProcessed = 0;
                    }
                    

                    var ordersInTransitCmd = conn.CreateCommand();
                    ordersInTransitCmd.CommandText = string.Format("SELECT count([Id]) from [{0}].[dbo].[Orders] WHERE [OrderStatus] = 'InTransit'", conn.Database);
                    try
                    {
                        ordersInTransit = (Int32)ordersInTransitCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        ordersInTransit = 0;
                    }
                    

                    var ordersDeliveredCmd = conn.CreateCommand();
                    ordersDeliveredCmd.CommandText = string.Format("SELECT count([Id]) from [{0}].[dbo].[Orders] WHERE [OrderStatus] = 'Delivered'", conn.Database);
                    try
                    {
                        ordersDelivered = (Int32)ordersDeliveredCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        ordersDelivered = 0;
                    }
                    

                    var randValueSoldCmd = conn.CreateCommand();
                    randValueSoldCmd.CommandText = string.Format("SELECT sum([OrderWorth]) from [{0}].[dbo].[Orders] Where WHERE [OrderStatus] = 'Delivered' && [Status] = 'Active'", conn.Database);
                    try
                    {
                        randValueSold = (Decimal)randValueSoldCmd.ExecuteScalar();
                    }
                    catch (Exception Ex)
                    {
                        randValueSold = decimal.Parse("0");
                    }
                    

                }
                return Json(new { Success = true, Customers = customerCount, StockSold = stockSold, StockAvailable = stockAvailable, OrdersProcessed = ordersProcessed, OrdersPackaged = ordersPackaged, OrdersInTransit = ordersInTransit, OrdersDelivered = ordersDelivered, RandValueSold = randValueSold });
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString() });
            }

        }

        [HttpGet, Route("GetMessages")]
        public async Task<IHttpActionResult> GetMessages()
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var InboxMessageViewModel = new List<InboxViewModel>();
            var countUsers = 0;
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Body],[From],[To],[Status],[DateCreated],[Id] from [{0}].[dbo].[InboxMessages] where [To] = '{1}' and Status <> '{2}' order by DateCreated DESC", conn.Database, userId, "Deleted");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var InboxMessages = new InboxViewModel();
                        var usernameFrom = userManager.GetEmail(reader.GetString(1));
                        var usernameTo = userManager.GetEmail(reader.GetString(2));
                        countUsers += 1;
                        InboxMessages.Id = reader.GetInt32(5);
                        InboxMessages.Body = reader.GetString(0);
                        InboxMessages.From = usernameFrom;
                        InboxMessages.To = usernameTo;
                        InboxMessages.Status = reader.GetString(3);
                        var date = reader.GetDateTime(4);
                        InboxMessages.DateCreated = date.ToString("MM/dd/yyyy HH:mm:ss");
                        InboxMessageViewModel.Add(InboxMessages);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(InboxMessageViewModel);
            }
        }

        [HttpGet, Route("GetChats/{Id}")]
        public async Task<IHttpActionResult> GetChats(string Id)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var InboxMessageViewModel = new List<InboxViewModel>();
            var countUsers = 0;
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[Body],[From],[To],[Status],[DateCreated] from [{0}].[dbo].[InboxMessages] where ([To] = '{1}' and [From] = '{2}') or ([To] = '{2}' and [From] = '{1}') order by DateCreated ASC", conn.Database, Id, userId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var InboxMessages = new InboxViewModel();
                        var usernameFrom = userManager.GetEmail(reader.GetString(2));
                        var usernameTo = userManager.GetEmail(reader.GetString(3));
                        countUsers += 1;
                        InboxMessages.Id = countUsers;
                        InboxMessages.Body = reader.GetString(1);
                        InboxMessages.From = usernameFrom;
                        InboxMessages.To = usernameTo;
                        InboxMessages.Status = reader.GetString(4);
                        var date = reader.GetDateTime(5);
                        InboxMessages.DateCreated = date.ToString("MM/dd/yyyy HH:mm:ss");
                        InboxMessages.MainUser = userManager.GetEmail(userId);

                        var cmdReplies = conn.CreateCommand();
                        cmdReplies.CommandText = string.Format("SELECT [Subject],[Body],[From],[To],[Status],[MessageId],[DateCreated] from [{0}].[dbo].[InboxReplies] where [MessageId] = '{1}' order by DateCreated ASC", conn.Database, reader.GetInt32(0));

                        using (var repliesReader = cmdReplies.ExecuteReader())
                        {
                            while (repliesReader.Read())
                            {
                                InboxMessages.Replies.Add(new InboxReplies {Body = repliesReader.GetString(1), MessageId = repliesReader.GetInt32(5), DateCreated = repliesReader.GetDateTime(6)});
                            }
                            repliesReader.NextResult();
                            repliesReader.Close();
                        }
                        InboxMessageViewModel.Add(InboxMessages);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(InboxMessageViewModel);
            }
        }

        [HttpPost, Route("MessageCreate")]
        public async Task<IHttpActionResult> MessageCreate(InboxViewModel MessageData)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);

            try
            {
                SafriSoft.InboxMessages.Add(new InboxMessages {From = userId, To = MessageData.To, Body = MessageData.Body, Status = "Created", DateCreated = DateTime.Now });
                SafriSoft.SaveChanges();
                return Json(new { Success = true, Id = MessageData.To });
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString() });
            }

        }

        [HttpPost, Route("MessageStatusChange")]
        public async Task<IHttpActionResult> MessageStatusChange(InboxViewModel MessageData)
        {
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);

            try
            {
                var Message = SafriSoft.InboxMessages.First(x => x.Id == MessageData.Id);
                Message.Status = MessageData.Status;
                SafriSoft.SaveChanges();
                return Json(new { Success = true, UserToId = Message.From});
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString() });
            }

        }

        [HttpGet, Route("GetNotifications")]
        public async Task<IHttpActionResult> GetNotifications()
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var InboxMessageViewModel = new List<InboxViewModel>();
            var countUsers = 0;
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Body],[From],[To],[Status],[DateCreated] from [{0}].[dbo].[InboxMessages] where [To] = '{1}' and Status = '{2}' order by DateCreated ASC", conn.Database, userId, "Created");

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var InboxMessages = new InboxViewModel();
                        var usernameFrom = userManager.GetEmail(reader.GetString(1));
                        var usernameTo = userManager.GetEmail(reader.GetString(2));
                        countUsers += 1;
                        InboxMessages.Id = countUsers;
                        InboxMessages.Body = reader.GetString(0);
                        InboxMessages.From = usernameFrom;
                        InboxMessages.To = usernameTo;
                        InboxMessages.Status = reader.GetString(3);
                        var date = reader.GetDateTime(4);
                        InboxMessages.DateCreated = date.ToString("MM/dd/yyyy HH:mm:ss");
                        InboxMessageViewModel.Add(InboxMessages);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(InboxMessageViewModel);
            }
        }

        [HttpPost, Route("GetOrderAudit")]
        public async Task<IHttpActionResult> GetOrderAudit(OrderViewModel OrderData)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var OrderAuditViewModel = new List<OrderAuditViewModel>();
            var countUsers = 0;
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [Id],[Description],[Changed],[CreatedDate],[UserId],[OrderId] from [{0}].[dbo].[OrderAudit] where [OrderId] = '{1}' order by CreatedDate ASC", conn.Database, OrderData.OrderId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var OrderAuditReords = new OrderAuditViewModel();
                        var usernameUser = userManager.GetEmail(reader.GetString(4));
                        countUsers += 1;
                        OrderAuditReords.Id = countUsers;
                        OrderAuditReords.Description = reader.GetString(1);
                        OrderAuditReords.Changed = reader.GetString(2);
                        OrderAuditReords.UserId = usernameUser;
                        var date = reader.GetDateTime(3);
                        OrderAuditReords.CreatedDate = date.ToString("MM/dd/yyyy HH:mm:ss");
                        OrderAuditViewModel.Add(OrderAuditReords);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(OrderAuditViewModel);
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

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = string.Format("SELECT [OrganisationId],[OrganisationName],[OrganisationEmail],[OrganisationCell],[OrganisationLogo],[OrganisationStreet],[OrganisationSuburb],[OrganisationCity],[OrganisationCode],[AccountName],[AccountNo],[BankName],[BranchName],[BranchCode],[ClientReference],[VATNumber] from [{0}].[dbo].[Organisations]", conn.Database);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var OrganisationDetails = new OrganisationViewModel();
                        OrganisationDetails.OrganisationId      = reader.GetInt32(0);
                        OrganisationDetails.OrganisationName    = reader.GetString(1);
                        OrganisationDetails.OrganisationEmail   = reader.GetString(2) != null ? reader.GetString(2) : "";
                        OrganisationDetails.OrganisationCell    = reader.GetString(3) != null ? reader.GetString(3) : "";
                        OrganisationDetails.OrganisationLogo    = reader.GetString(4);
                        OrganisationDetails.OrganisationStreet  = reader.GetString(5);
                        OrganisationDetails.OrganisationSuburb  = reader.GetString(6);
                        OrganisationDetails.OrganisationCity    = reader.GetString(7);
                        OrganisationDetails.OrganisationCode    = reader.GetInt32(8);
                        OrganisationDetails.AccountName         = reader.GetString(9);
                        OrganisationDetails.AccountNo           = reader.GetInt32(10);
                        OrganisationDetails.BankName            = reader.GetString(11);
                        OrganisationDetails.BranchName          = reader.GetString(12);
                        OrganisationDetails.BranchCode          = reader.GetString(13);
                        OrganisationDetails.ClientReference     = reader.GetString(14);
                        OrganisationDetails.VATNumber           = reader.GetInt32(15);
                        OrganisationViewModel.Add(OrganisationDetails);
                    }
                    reader.NextResult();
                    reader.Close();
                }

                return Json(OrganisationViewModel);
            }
        }

        [HttpPost, Route("SaveOrganisationDetails")]
        public async Task<IHttpActionResult> SaveOrganisationDetails(Organisations Organisation)
        {
            ApplicationUserManager userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            string userId = IdentityExtensions.GetUserId(User.Identity);
            var OrganisationViewModel = new List<OrganisationViewModel>();
            var countUsers = 0;
            var organisationClaim = userManager.GetClaims(userId).First(x => x.Type == "Organisation");
            var getOrgClaim = organisationClaim.Value;
            SafriSoftDbContext SafriSoft = new SafriSoftDbContext();

            try
            {
                try
                {
                    SafriSoft.Entry(Organisation).State = EntityState.Modified;
                    SafriSoft.SaveChanges();
                }
                catch (Exception Ex)
                {
                    SafriSoft.Organisations.Add(Organisation);
                    SafriSoft.SaveChanges();
                }
                
                return Json(new { Success = true });
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString() });
            }
        }

        [HttpPost, Route("GetInvoiceData")]
        public async Task<IHttpActionResult> GetInvoiceData(OrderViewModel OrderData)
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
                    orderCmd.CommandText = string.Format("SELECT [OrderId],[ProductName],[NumberOfItems],[CustomerId],[OrderWorth],[ShippingCost],[DateOrderCreated] from [{0}].[dbo].[Orders] where OrderId = '{1}'", conn.Database,OrderData.OrderId);
                    
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
                    InvoiceDetails.VatAmount = InvoiceDetails.OrderWorth * decimal.Parse("0.15");
                    InvoiceDetails.InvoiceTotal = InvoiceDetails.OrderWorth + InvoiceDetails.VatAmount + InvoiceDetails.ShippingCost;

                    InvoiceViewModel.Add(InvoiceDetails);
                }
                return Json(InvoiceViewModel);
            }
            catch (Exception Ex)
            {
                return Json(new { Success = false, Error = Ex.ToString() });
            }
        }

        [HttpPost, Route("UploadExcelData/{Id}")]
        public async Task<IHttpActionResult> UploadExcelData(HttpRequestMessage request, string Id)
        {
            HttpContext context = HttpContext.Current;
            HttpPostedFile postedFile = context.Request.Files["files[]"];

            string fileName = postedFile.FileName;
            string fileContentType = postedFile.ContentType;
            byte[] fileBytes = new byte[postedFile.ContentLength];
            postedFile.InputStream.Read(fileBytes, 0, Convert.ToInt32(postedFile.ContentLength));

            //Process the file
            IExcelDataReader excelReader;
            //ExcelPackage excelPackage = null;
            MemoryStream ms = new MemoryStream(fileBytes);

            if (postedFile.FileName.Contains("xlsx"))
            {
                //1. Reading from a OpenXml Excel file (2007 format; *.xlsx)
                excelReader = ExcelReaderFactory.CreateOpenXmlReader(ms);
            }
            else
            {
                //2. Reading from a binary Excel file ('97-2003 format; *.xls)
                excelReader = ExcelReaderFactory.CreateBinaryReader(ms);
            }

            Dictionary<string, int> fileColumns = new Dictionary<string, int>();
            excelReader.Read();

            //if (excelReader.re)
            var fieldCount = excelReader.FieldCount;
        
            while (excelReader.Read())
            {
                string productCode = excelReader.GetString(0);
                string productCategory = excelReader.GetString(1);
                string productName = excelReader.GetString(2);
                string NOIA = excelReader.GetDouble(3).ToString();
                string productPrice = excelReader.GetDouble(4).ToString();
                string productImage = excelReader.GetString(5);
                string productReference = productCode + "-" + DateTime.Now.ToString("yyyy/MM/dd");

                SafriSoftDbContext SafriSoft = new SafriSoftDbContext();

                var product = SafriSoft.Products.FirstOrDefault(x => x.ProductReference == productReference || x.ProductCode == productCode);

                if (product == null)
                {
                    using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SafriSoftDbContext"].ToString()))
                    {
                        conn.Open();
                        var cmd = conn.CreateCommand();
                        cmd.CommandText = string.Format("INSERT INTO  [{0}].[dbo].[Product] ([ProductName],[ProductReference],[SellingPrice],[ItemsSold],[ItemsAvailable],[Status],[ProductCode],[ProductCategory],[ProductImage]) VALUES('{1}','{2}',{3},'{4}','{5}','{6}','{7}','{8}','{9}')", conn.Database, productName, productReference, productPrice, 0, NOIA, "Active", productCode, productCategory, productImage);
                        await cmd.ExecuteNonQueryAsync();
                        conn.Close();
                    }
                }
                               

            }

            excelReader.Close();

            return Json(new { Success = true});
        }

        public static Bitmap Base64StringToBitmap(string base64String)
        {
            Bitmap bmpReturn = null;
            //Convert Base64 string to byte[]
            byte[] byteBuffer = Convert.FromBase64String(base64String);
            MemoryStream memoryStream = new MemoryStream(byteBuffer);

            memoryStream.Position = 0;

            bmpReturn = (Bitmap)Bitmap.FromStream(memoryStream);

            memoryStream.Close();
            memoryStream = null;
            byteBuffer = null;

            return bmpReturn;
        }

    }
}
