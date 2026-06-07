using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using WebLibrary.Data;
using WebLibrary.DataAccess.Repository.IRepository;
using WebLibrary.Models;
using WebLibrary.Models.ViewModels;
using WebLibrary.Utilities.ValidationAttributes;

namespace WebLibrary.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository, IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objectCategoryList = _productRepository.GetAll().ToList();
            objectCategoryList.ForEach(u => u.Category = _categoryRepository.Get(c => c.Id == u.CategoryId));
            return View(objectCategoryList);
        }

        public IActionResult Create()
        {
            ProductVM productVM = new ProductVM()
            {
                Product = new Product(),
                CategoryList = _categoryRepository.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };
            return View(productVM);
        }
        [HttpPost]
        public IActionResult Create(ProductVM productVM, [ExternalValidationAttributes] IFormFile? formFile)
        {
            if (ModelState.IsValid)
            {
                productVM.Product.ImageUrl = ImageFileOperation(formFile, "");

                _productRepository.Add(productVM.Product);
                _productRepository.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index", "Product");
            }
            else
            {
                productVM.CategoryList = _categoryRepository.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                if (ModelState.ContainsKey("formFile") && ModelState["formFile"]?.ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                {
                    var errorMessage = ModelState["formFile"]?.Errors.FirstOrDefault()?.ErrorMessage;
                    ModelState.AddModelError("Product.ImageUrl", errorMessage ?? "The uploaded file is not a valid image.");
                }
                return View(productVM);
            }
        }

        public IActionResult Edit(int? id)
        {
            ProductVM productVM = new ProductVM();
            if (id == null || id == 0)
            {
                return NotFound();
            }
            productVM.Product = _productRepository.Get(u => u.Id == id);
            productVM.CategoryList = _categoryRepository.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });

            if (productVM == null)
            {
                return NotFound();
            }

            return View(productVM);
        }
        [HttpPost]
        public IActionResult Edit(ProductVM productVM, [ExternalValidationAttributes] IFormFile? formFile)
        {

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(productVM.Product.ImageUrl) && formFile != null)
                {
                    var oldImageUrl = Path.Combine(_webHostEnvironment.WebRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImageUrl))
                    {
                        System.IO.File.Delete(oldImageUrl);
                    }
                }

                productVM.Product.ImageUrl = ImageFileOperation(formFile, productVM.Product.ImageUrl);

                _productRepository.Update(productVM.Product);
                _productRepository.Save();
                TempData["success"] = "Category Updated successfully";
                return RedirectToAction("Index", "Product");
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product product = _productRepository.Get(u => u.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteCategory(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product product = _productRepository.Get(u => u.Id == id);
            var oldImageUrl = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImageUrl))
            {
                System.IO.File.Delete(oldImageUrl);
            }

            if (ModelState.IsValid && product != null)
            {
                _productRepository.Remove(product);
                _productRepository.Save();
                TempData["success"] = "Product Deleted successfully";
                return RedirectToAction("Index", "Product");
            }
            return View();
        }

        #region Image File Operation

        public string ImageFileOperation([ExternalValidationAttributes] IFormFile? formFile, string ImageUrl)
        {
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (!wwwRootPath.IsNullOrEmpty() && formFile != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
                string path = Path.Combine(wwwRootPath, @"images/product");

                using (var fileStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                {
                    formFile.CopyTo(fileStream);
                }
                ;
                ImageUrl = @"\images\product\" + fileName;
            }

            return ImageUrl;

        }

        #endregion

        #region API CALL FOR AJAX

        [HttpGet]
        public IActionResult ProductList()
        {
            List<Product> objectCategoryList = _productRepository.GetAll().ToList();
            objectCategoryList.ForEach(u => u.Category = _categoryRepository.Get(c => c.Id == u.CategoryId));
            return View(objectCategoryList);
        }

        #endregion
    }
}
