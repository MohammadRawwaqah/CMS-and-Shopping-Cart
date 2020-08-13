using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Account;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;


namespace CmsShoppingCart.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            string username = User.Identity.Name;
            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("user-profile");

            return View();
        }

        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }
            }

            if (!isValid)
            {
                ModelState.AddModelError("", "Invalid username or password.");
            }
            else
            {
                FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
            }

            return View();
        }

        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Passwords don't match");
                return View("CreateAccount", model);
            }

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", "Username" + model.Username + " is taken");
                    model.Username = "";
                    return View("CreateAccount", model);
                }

                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    Username = model.Username,
                    Password = model.Password
                };

                db.Users.Add(userDTO);
                db.SaveChanges();
                //you can write this since you make SaveChanges(), you added a user, so id placed for sure.
                int? id = userDTO.Id;

                UserRoleDTO userRolesDTO = new UserRoleDTO()
                {
                    UserId = id ?? -1,
                    RoleId = 2
                };

                db.UserRoles.Add(userRolesDTO);
                db.SaveChanges();

            }
            TempData["SM"] = "You are now registered and can login";

            return Redirect("~/account/login");
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            //return RedirectToAction("Login"); You can write this also but the one below is nicer;
            return Redirect("~/account/login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            string username = User.Identity.Name;

            UserNavPartialVM model;

            using (Db db = new Db())
            {
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                };

            }

            return PartialView(model);
        }

        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {
            string username = User.Identity.Name;

            UserProfileVM model;

            using (Db db = new Db())
            {
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                model = new UserProfileVM(dto);
            }

            return View("UserProfile", model);
        }

        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Passwords do not match.");
                    return View("UserProfile", model);
                }
            }
            using (Db db = new Db())
            {
                string username = User.Identity.Name;

                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Username == username))
                {
                    ModelState.AddModelError("", "Username" + model.Username + " already exists");
                    model.Username = "";
                    return View("UserProfile", model);
                }
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.Username = model.Username;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }
                db.SaveChanges();
            }
            TempData["SM"] = "You have edited your profile!";


            //this will pop up an error on your face when you edit your username
            //why because when compiler reload page he looks up for old username in DB that you changed
            //and he didn't found it, you changed john to john2 right, so in account controller when compiler
            //goes to UserNavPartial() here he didn't found this:
            //UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username); username now that he was logged in 
            //didn't exisit in DB now so compiler can't view page for you,
            //SO you must LOGOUT using this return RedirectToAction("Logout"); other than this return below
            //you must logged out and then login again using your new username.

            //return RedirectToAction("Logout");------------true(use this)
            return Redirect("~/account/user-profile");//----error but we do as voja

            //

        }

        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            List<OrdersForUserVM> orderForUser = new List<OrdersForUserVM>();
            using (Db db = new Db())
            {
                UserDTO user = db.Users.Where(x => x.Username == User.Identity.Name).FirstOrDefault();
                int? userId = user.Id;
                //here orders for logged in user >>>>>>> and order may have alot of products in one order
                // e.g : order1 : PC, Pen, Coffee .. like this
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();
                //orders here is just for specific user who logged in right now.
                //here just for order this table will not shown all
                // details for this order like:(product# or productQuantity) and so on 
                //you can see these info in orderDetails table so first of all i will take order-order-order in first foreach
                //and for every order i will go to his details using OrderId because it is exisit in order and OrderDetails Table.
                //so must be ---foreach--- inside ---foreach---           
                foreach (var order in orders)
                {
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    decimal total = 0m;
                    //to get/know details for one user logged in.
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    foreach (var orderDetails in orderDetailsDTO)
                    {
                        //to get/know product Price and Name
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                        decimal price = product.price;

                        string productName = product.Name;

                        productsAndQty.Add(productName, orderDetails.Quantity);

                        total += orderDetails.Quantity * price;
                    }

                    //and FINALLY let us fill list to send it for View>>> YOU still in foreach Block
                    orderForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        productsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt

                    });

                }//foreach END
            }

            return View(orderForUser);
        }

        [HttpGet]
        public ActionResult SendingEmail()
        {
            return PartialView();
        }

        [HttpPost]
        public ActionResult SendingEmail(SendingEmailVM se)
        {
            //We must check.. why ??
            //first of all we write this in layout  @Html.Action("SendingEmail", "Account", null) 
            //so in the first run of app it will go to HttpGet of  SendingEmail() --->why: because we don't have data in email Footer.
            //now where is the problem ? the prblem is when we go to login page and we fill data and PRESS/CLICK Login
            //it will go to login page prepare it for showing and THEN going to ((((layout)))) 
            //NOW in the (((layout))) we said hey go to ---> @Html.Action("SendingEmail", "Account", null) 
            //so MVC will ask it self ?OK but what type HttpPost or HttpGet 
            //the answer is HttpPost ----You said hey mvc!! why post i don't fill any data
            //he said yes... but you click on LogIn so the Http request still Post .. Right
            //so for this reason i redirect you here 
            // and also for this reason i check if email AND message are empty i will redirect you to HttpGet :)
            //NOTE: you must as a developer don't give all trust for client side validation, also you must write
            //server side validation (So write two types client and server side validation).            

            if (se.Email == null && se.Message == null)
            {
                return PartialView("SendingEmail");
            }

            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("8cf7e7cd2dd182", "85cd25c8475a65"),
                EnableSsl = true
            };
            client.Send(se.Email, "Admin@example.com", "Message From Client", se.Message);

            return Json(new { sta = "success" }, JsonRequestBehavior.AllowGet);

            //return RedirectToAction("Pages","Index");
            //return Json(data: new { success = true, message = "Your request good" }, JsonRequestBehavior.AllowGet);
            //return Json(new { success = true, message = "Your request good" }, JsonRequestBehavior.AllowGet);
            //return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            //return RedirectToAction("Index","Pages");



        }
    }
}
