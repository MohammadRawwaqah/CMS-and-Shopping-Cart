using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        [Authorize(Roles ="User")] //just user logged in can step into this action to check out
        public ActionResult Index()
        {
            // 1 minutes to read..
            #region discuss about (as) keyword
            //this means you wait for Session["cart"] vlaue to come in your handy,
            //once it comes you didn't know what a type of it Right, so you till compiler hey:
            //try to convert this Session["cart"] to List<CartVM>
            //if you did so good, but if you can't don't throw an exception on my face (just make cart null)
            // and if cart null i write ?? so if it null it will be have new List<CartVM>(); Value.
            //INSTEAD of writing this (List<CartVM>)Session["cart"] this casting maybe failed
            //so it will throw an exception on my face. So i don't want this happen, for that i used ((as)) KeyWord.
            //hover your mouse on var and see type it's List<CartVM> why because it is null NOW, i said now
            //because we don't have a value for Session["cart"] yet so it was took   ??if it null   value 
            //compiler can't convert Session["cart"] TO --> List<CartVM>
            //why !! because Session["cart"] is null NOW i said NOW .. Session["cart"] value didn't reach us YET.

            //as keyword used if you don't want compiler throw an exception if casting was failed
            //just make a value null for variable (cart in this e.g) 
            //and if cart null ?? so go to  ?? new List<CartVM>();  
            #endregion

            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Your cart is empty.";
                return View();
            }

            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;


            return View(cart);
        }

        public ActionResult CartPartial()
        {
            CartVM model = new CartVM();

            int qty = 0;
            decimal price = 0m;

            if (Session["cart"] != null)
            {
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }

                model.Quantity = qty;
                model.Price = price;
            }
            else
            {
                model.Quantity = 0;
                model.Price = 0m;
            }

            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            CartVM model = new CartVM();

            using (Db db = new Db())
            {
                ProductDTO product = db.Products.Find(id);//bring product that (Maybe) user want to buy it.

                //if our cart havn't selected product yet, so add it to cart because it is still new
                //but if it does in cart so just increment this productInCart.Quantity++;

                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                if (productInCart == null)//means that it isn't in our cart.
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.price,
                        Image = product.ImageName

                    });
                }
                else                     //means that HEY it is in our cart so just i will increment it by 1
                {
                    productInCart.Quantity++;
                }
            }

            int qty = 0;        //must be here and equal ZERO before foreach to reset qty and start from beginning
            decimal price = 0m; //must be here and equal ZERO before foreach to reset price and start from beginning
                                //don't worry, foreach will iterate over all items and calculate it from beginning

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;
            Session["cart"] = cart;//----------------------------------HERE we assign Session["cart"] VALUE

            return PartialView(model);
        }

        //BTW you can leave type as ActionResult, BUT i prefer use JsonResult.
        public JsonResult IncrementProduct(int productId)
        {
            //why session.. in order to keep customer cart when he move from one page to another
            //HEY! Session["cart"] value was assigned in AddToCartPartial() controller, it was equal = cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                model.Quantity++;

                var result = new { qty = model.Quantity, price = model.Price };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DecrementProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                if (model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);//delete it from Session["cart"] so when you refrish page this product will be not in Session["cart"] or cart it self
                }
                var result = new { qty = model.Quantity, price = model.Price };

                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        public void RemoveProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;
            using (Db db = new Db())
            {
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                cart.Remove(model);

            }

        }

        public ActionResult PaypalPartial()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            return PartialView(cart);
        }

        [HttpPost]
        public void PlaceOrder()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            string username = User.Identity.Name;

            int orderId = 0;

            using (Db db = new Db())
            {
                OrderDTO orderDTO = new OrderDTO();

                var q = db.Users.FirstOrDefault(x => x.Username == username);

                int? userId = q.Id;

                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);

                db.SaveChanges();

                orderId = orderDTO.OrderId;

                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);

                    db.SaveChanges();
                }

                var EmailForUser = q.EmailAddress;
            

            var client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("8cf7e7cd2dd182", "85cd25c8475a65"),
                EnableSsl = true
            };
            client.Send(EmailForUser, "admin@example.com", "New Order", "You have a new order, order number" + orderId);

            Session["cart"] = null;
            }

        }

        public ActionResult CartPartialEmpty()
        {
            return PartialView();
        }

    }
}