using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]

    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        //To access root path of our application, No need to register this as it is provided by .NET Core
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork,IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Upsert(int ? id) {
            //We are using selectlistItem for dropdown selection.
            //ICategory is converted to SelectListItem with the help of projects in EF Core.
            //IEnumerable<SelectListItem> CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
            //{
            //    //These are the properties given to SelectListItem
            //    Text = u.Name,
            //    Value = u.Id.ToString()
            //});
            //ViewBag.CategoryList = CategoryList;
            //ViewData["CategoryList"] = CategoryList;


            ProductVM productVM = new ProductVM()
            {
                CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    //These are the properties given to SelectListItem
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };

            if (id == null || id == 0)
            {
                //Create
                return View(productVM);
            }
            else {
                //Edit
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        //IFormFile is used when you are uploading a file, in our case we are uploading image URL
        public IActionResult Upsert(ProductVM productVM, IFormFile file) {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null) { 
                    //Uploading the file
                    //This will give me new name
                    string fileName = Guid.NewGuid().ToString()+Path.GetExtension(file.FileName);
                    //To treat image/product as a file name instead of escape characters we make use of this thing
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        //delete the old image
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create)) { 
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }
                //Verifying add of update
                if (productVM.Product.Id == 0)
                {
                    //THis means add
                    _unitOfWork.Product.Add(productVM.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                }
                //_unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.Save();
                TempData["Success"] = "Product created Successfully";
                return RedirectToAction("Index", "Product");
            }
            else {
                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    //These are the properties given to SelectListItem
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
            }            
            return View(productVM);
        }

        //[HttpGet]
        public IActionResult Index()
        {
            List<Product> products = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();            

            return View(products);
        }

        //public IActionResult Edit(int? id) {
        //    if (id == 0)
        //    {
        //        return NotFound();
        //    }
        //    //else
        //    //{
        //        Product product = _unitOfWork.Product.Get(u => u.Id == id);
        //        if (product == null) { 
        //            return NotFound();
        //        }
        //        return View(product);
        //    //}
        //}

        [HttpPost]
        public IActionResult Edit(Product product) {
            if (ModelState.IsValid) {
                _unitOfWork.Product.Update(product);
                _unitOfWork.Save();
                TempData["success"] = "Product updated successfully";
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        //public IActionResult Delete(int? id)
        //{
        //    if (id == 0) {
        //        return NotFound();
        //    }

        //    Product product = _unitOfWork.Product.Get(u => u.Id == id);
        //    if (product == null) {
        //        return NotFound();
        //    }
        //    return View(product);
        //}

        //[HttpPost, ActionName("Delete")]
        //public IActionResult DeletePOST(int? id)
        //{
        //    Product? product = _unitOfWork.Product.Get(u=>u.Id==id);
        //    if (product != null) {
        //        _unitOfWork.Product.Remove(product);
        //        _unitOfWork.Save();
        //        TempData["success"] = "Product deleted successfully";
        //        return RedirectToAction("Index");
        //    }
        //    return NotFound();
        //}


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = products  });
        }


        [HttpDelete]
        public IActionResult Delete(int ? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u=>u.Id == id);
            if (productToBeDeleted == null) {
                return Json(new { success = false, message = "Error while deleting" });
            }
            var oldImagePath =
                           Path.Combine(_webHostEnvironment.WebRootPath,
                           productToBeDeleted.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}
