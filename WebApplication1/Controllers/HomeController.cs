using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebLibrary.DataAccess.Repository.IRepository;
using WebLibrary.Models;

namespace WebLibrary.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }
        public IActionResult Index()
        {
            List<Product> objectCategoryList = _productRepository.GetAll().ToList();
            objectCategoryList.ForEach(u => u.Category = _categoryRepository.Get(c => c.Id == u.CategoryId));
            return View(objectCategoryList);
        }

        public IActionResult Details(int productId)
        {
            Product product = _productRepository.Get(u => u.Id == productId);
            product.Category = _categoryRepository.Get(c => c.Id == product.CategoryId);
            return View(product);
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
