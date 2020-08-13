using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            List<PageVM> pageList;
            using (Db db = new Db())//this is a constructor! BTW you can define it like this Db db = new Db(); *but when you use "using" it will clean it self after it finish.
            {
                //we brought all pages from dbcontext
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();
            }

            return View(pageList);
        }
        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();

        }
       
        // POST: Admin/Pages/AddPage
        public ActionResult AddPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using(Db db = new Db())
            {
                string slug;
                pageDTO dto = new pageDTO();


                if (string.IsNullOrWhiteSpace(model.Slug))          //NullOrWhiteSpaces is 1) we don't writing anything. 
                {                                                   //OR 2) we just press spaces key on keyboard.
                    slug = model.Title.Replace(" ", "-".ToLower()); //So we will make this (empty slug) equal Title.
                }
                else //otherwise, so slug has a value.
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }


                if(db.Pages.Any(x=>x.Title == model.Title) || db.Pages.Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists");
                    return View(model);
                }



                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;
                dto.Title = model.Title;

                db.Pages.Add(dto);
                db.SaveChanges();
            }

            TempData["SM"] = "You have added a new page!";



            return RedirectToAction("AddPage");
        }

        //GET: Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            PageVM model;

            using (Db db = new Db())
            {
                pageDTO dto = db.Pages.Find(id);

                if(dto == null)
                {
                    return Content("The Page Does Not Exist.");
                }

                model = new PageVM(dto);
            }

            return View(model);
        }

        //POST: Admin/Pages/EditPage
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using(Db db = new Db())
            {
                pageDTO dto = db.Pages.Single(x => x.Id == model.Id);

                string slug;

                if (string.IsNullOrWhiteSpace(model.Slug))          //NullOrWhiteSpaces is 1) we don't writing anything. 
                {                                                   //OR 2) we just press spaces key on keyboard.
                    slug = model.Title.Replace(" ", "-".ToLower()); //So we will make this (empty slug) equal Title.
                }
                else //otherwise, so slug has a value.
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }


                if (db.Pages.Where(x=>x.Id !=model.Id).Any(x => x.Title == model.Title) ||
                     db.Pages.Where(x=>x.Id !=model.Id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That title or slug already exists");
                    return View(model);
                }

                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;
                dto.Title = model.Title;

                
                db.SaveChanges();

                TempData["SM"] = "The Page Edited succesfully!";

                return RedirectToAction("EditPage");

            }
             
        }


        public ActionResult PageDetails(int id)
        {
            PageVM model;
            using(Db db = new Db())
            {
                pageDTO dto = db.Pages.Single(x => x.Id == id);

                model = new PageVM(dto);

                return View(model);
            }
            

        }


        //POST: Admin/Pages/DeletePage/id

        public ActionResult DeletePage(int id)
        {
            pageDTO dto;
            using(Db db = new Db())
            {
                dto = db.Pages.Single(x => x.Id == id);

                db.Pages.Remove(dto);

                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public void ReorderPages(int[] id)//,8,4,3,4,2,7  ~~~>  1,2,3,4,5,6
        {
            using (Db db = new Db())
            {

                int count = 1;

                pageDTO dto;

                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;// So sorting of all pages will be sorting 1,2,3,4...

                    db.SaveChanges();

                    count++;
                }
            };
            
        }


        [HttpGet]
        public ActionResult EditSidebar()
        {
            SidebarVM model;

            using(Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);

                model = new SidebarVM(dto);
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                SidebarDTO dto = db.Sidebar.Find(1);

                dto.Body = model.Body;

                db.SaveChanges();
            }

            TempData["SM"] = "You have edited the sidebar!";

            return RedirectToAction("EditSidebar");
        }

    }
}