using CmsShoppingCart.Areas.Admin.Models.ViewModels.Shop;
using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Pages.Shop;
using CmsShoppingCart.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            List<CategoryVM> categoryVMList;

            using (Db db = new Db())
            {
                categoryVMList = db.Categories
                                .ToArray()
                                .OrderBy(x => x.Sorting)
                                .Select(x => new CategoryVM(x))//this statment take (x) that was DTO type and change it to CategoryVM type
                                .ToList();
            }

            return View(categoryVMList);
        }
        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            string id;

            using (Db db = new Db())
            {
                if (db.Categories.Any(x => x.Name == catName))
                    return "titletaken";

                CategoryDTO dto = new CategoryDTO();

                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                db.Categories.Add(dto);
                db.SaveChanges();

                id = dto.Id.ToString();

            }

            return id;
        }


        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {

                int count = 1;

                CategoryDTO dto;

                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            };

        }

        //POST: Admin/Shop/DeletePage/id

        public ActionResult DeleteCategory(int id)
        {
            CategoryDTO dto;
            using (Db db = new Db())
            {
                dto = db.Categories.Single(x => x.Id == id);

                db.Categories.Remove(dto);

                db.SaveChanges();
            }
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                if (db.Categories.Any(x => x.Name == newCatName))
                    return "titletaken";

                CategoryDTO dto = db.Categories.Single(x => x.Id == id);

                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                db.SaveChanges();

            }
            return "ok";

        }


        [HttpGet] // why this HttpGet . because we want to get Categories from db ... NOT add something !!!
        public ActionResult AddProduct()
        {
            ProductVM model = new ProductVM();

            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);

                }
            }

            using (Db db = new Db())
            {
                if (db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That product name is taken!");
                    return View(model);
                }
            }

            int id;

            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.price = model.price;
                product.CategoryId = model.CategoryId;

                //i write this to bring CategoryName because i just recieve categoryID from 
                //user not everything... Remember selectList
                //we define it as this: new SelectList(db.Categories.ToList(), "Id", "Name");
                //so the id was sent not name, name just shown to the users.
                //and then we put CategoryName for product obj
                //and the last thing we added this new obj to the DataBase.
                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;
            }


            TempData["SM"] = "You have added a product!";

            #region Upload Image
            //it is the same as Server.MapPath("~") or Server.MapPath("/") 
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
            //You should use Path.Combine to concat two paths with each other
            //because it checks most of things like: 
            //(slashes so if there one slash it doesn't add another one... and so on)
            //btw you can use cancatination but we prefer combine to combine paths.

            //var pathString1 = originalDirectory.ToString() + "Products";
            //var pathString2 = originalDirectory.ToString() + "Products\\" + id.ToString();
            //var pathString3 = originalDirectory.ToString() + "Products\\" + id.ToString() + "\\Thumbs";
            //var pathString4 = originalDirectory.ToString() + "Products\\" + id.ToString() + "\\Gallery";
            //var pathString5 = originalDirectory.ToString() + "Products\\" + id.ToString() + "\\Gallery\\Thumbs";

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);

            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);

            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);

            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);

            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            if (file != null && file.ContentLength > 0)
            {
                string ext = file.ContentType.ToLower();

                //if file doesn't equal any type from these types, so this file is prohibited.
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension.");
                        return View(model);
                    }
                }


                //Now we want add imageName to product that we insert it above,
                //because it have all information exept ImageName.
                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }
                var path = string.Format("{0}\\{1}", pathString2, imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                file.SaveAs(path);

                /*The file is a set of data stored in a disk with a specific name and a directory path.
                when a file is opened for reading or writing, it becomes a stream.
                The stream is basically the sequence of bytes passing through the communication path.
                There are two main streams: the input stream and the output stream.
                The input stream is used for reading data from file (read operation) 
                and the output stream is used for writing into the file (write operation).
                SO HERE we will use InputStream, because we want to read this image 
                HINT : new WebImage() inside Parentheses you can put stream / filePath / content.
                but we put stream inside i mean data we read data.
                 */

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }

            #endregion

            return RedirectToAction("AddProduct");

        }

        public ActionResult Products(int? page, int? catId)
        {
            List<ProductVM> listOfProductVM;

            var pageNumber = page ?? 1;

            using (Db db = new Db())
            {
                listOfProductVM = db.Products.ToArray()
                                   //first x here is an input from Products and this input or this value
                                   //will be returned just if catId = null or catId = 0 
                                   //or CategoryId of x it self x.CategoryId == catId
                                   //so if one atleast of these three conditions is true,
                                   //so x will be returned.
                                   //(input => conditions)
                                   .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                   .Select(x => new ProductVM(x))//To convert ProductDTO i mean (x) To ProductVM
                                   .ToList();

                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                ViewBag.SelectedCat = catId.ToString();

            }
            //pageNumber here is pagination 1 2 3 and 3 here is number of items you want to be displayed.
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;

            return View(listOfProductVM);
        }

        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            ProductVM model;

            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                if (dto == null)
                {
                    return Content("That product does not exist.");
                }

                model = new ProductVM(dto);

                //How model.Categories = select list !!! because Categories it is an attribute in
                //ProductVM class and the type for this attribute (Categories) is
                //IEnumerable<SelectListItem>
                //so you can assign value for it like this:
                //  new SelectList(db.Categories.ToList(), "Id", "Name");
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))//Thumbs here is an Empty Folder yet.
                                               .Select(fn => Path.GetFileName(fn));
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            int id = model.Id;

            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))//Thumbs here is an Empty Folder yet.
                               .Select(fn => Path.GetFileName(fn));

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That product name is taken");
                    return View(model);
                }

            }

            using (Db db = new Db())
            {
                //here we bring the product from database.
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.price = model.price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                //why we do this: because we don't have CategoryName for model, But we have CategoryId that user
                //enter it in WebSite from selectlist, SO WE USE THIS ID TO TAKE THE NAME, AND THEN GIVE IT FOR dto.
                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }
            TempData["SM"] = "You have edited the product!";

            #region Image Upload


            if (file != null && file.ContentLength > 0)
            {
                string ext = file.ContentType.ToLower();

                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png"
                    )
                {
                    using (Db db = new Db())
                    {
                        ModelState.AddModelError("", "The image was not uploaded - wrong image extension.");
                        return View(model);
                    }
                }

                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (FileInfo file2 in di1.GetFiles())
                    file2.Delete();//here it will delete file in some folder that his name 
                                   //is the same of id for the product that you want to edit.
                                   //so just it will delete files not folders.


                foreach (FileInfo file3 in di2.GetFiles())
                    file3.Delete();//here it will delete files inside Thumbs.BTW it is one file.

                string imageName = file.FileName;

                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;

                    db.SaveChanges();
                }

                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }




            #endregion

            //here if you don't send routes values to EditProduct, so ok it will take 
            //the previous value = 1 but from where this previous value come
            //if you rememper we went to EditProduct using this sentence 
            // @Html.ActionLink("Edit", "EditProduct", new { id = product.Id }) that inside product.cshtml
            //and id there still equal 1 so if you don't send any value in bottom return RedirectToAction...
            //so EditProduct will take previous value because id there still has a value old value.
            //this work just if you define name of variable as id ... wny other names will not be taken or stored
            //as old value but you you must send it within RedirectToAction.

            return RedirectToAction("EditProduct");
        }

        public ActionResult DeleteProduct(int id)
        {
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }
            var op = new DirectoryInfo(string.Format("{0}/Images/Uploads", Server.MapPath("~/")));

            string path = Path.Combine(op.ToString(), "products/" + id.ToString());


            ///////////////////////////////////////////////////

            //if (Directory.Exists(path))
            //{
            //    try
            //    {
            //        Directory.Delete(path, recursive: true);                //throws if directory doesn't exist.
            //    }
            //    catch
            //    {
            //        //HACK because the recursive delete will throw with an "Directory is not empty." 
            //        //exception after it deletes all the contents of the diretory if the directory
            //        //is open in the left nav of Windows's explorer tree.  This appears to be a caching
            //        //or queuing latency issue.  Waiting 2 secs for the recursive delete of the directory's
            //        //contents to take effect solved the issue for me.  Hate it I do, but it was the only
            //        //way I found to work around the issue.
            //        Thread.Sleep(2000);     //wait 2 seconds
            //        Directory.Delete(path, recursive: true);
            //    }
            //}

            //////////////////////////////////////////////////

            //if you doesn't check that if directory exisit or not before deleting
            //it will throw an exception on your face "Directory is not empty" 
            //and it will just only remove files inside folder eg: 12
            //and it will still folder exisit empty...
            //but on windows deletion will happen without exception thrown, and it will delete folder for you.
            //NOTE: maybe my server i puplished on is stupid, maybe if you try on another server it will work good like windows 10 IIS 10;
            if (Directory.Exists(path))
                Directory.Delete(path, true);

            return RedirectToAction("Products");
    }

        [HttpPost]
    public void SaveGalleryImages(int id)
    {
        foreach (string fileName in Request.Files)
        {
            HttpPostedFileBase file = Request.Files[fileName];

            if (file != null && file.ContentLength > 0)
            {
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                string pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
                string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                var path = string.Format("{0}\\{1}", pathString1, file.FileName);
                var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                file.SaveAs(path);
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);//add , false, true after 900 to prevent aspect ratio. just (900 * 900)
                img.Save(path2);

            }


        }
    }

    [HttpPost]
    public void DeleteImage(int id, string imageName)
    {
        string fullPath1 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/" + imageName);
        string fullPath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);


        if (System.IO.File.Exists(fullPath1))
            System.IO.File.Delete(fullPath1);

        if (System.IO.File.Exists(fullPath2))
            System.IO.File.Delete(fullPath2);
    }

    // GET: Admin/Shop/Orders
    // this action just to collect data from different sources(tables) to view it in Admin page.
    public ActionResult Orders()
    {
        List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

        using (Db db = new Db())
        {
            List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

            foreach (var order in orders)
            {
                //to add productName with his Price.
                Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                decimal total = 0m;

                List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                string username = user.Username;

                foreach (var orderDetails in orderDetailsList)
                {
                    ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();

                    decimal price = product.price;

                    string productName = product.Name;

                    //adding productName with his Price, every product has it's own dictionary.
                    productsAndQty.Add(productName, orderDetails.Quantity);

                    total += orderDetails.Quantity * price;
                }
                ordersForAdmin.Add(new OrdersForAdminVM()
                {
                    OrderNumber = order.OrderId,
                    Username = username,
                    Total = total,
                    productsAndQty = productsAndQty,
                    CreatedAt = order.CreatedAt
                });
            }//end of foreach

        }

        return View(ordersForAdmin);
    }
}
}