using System;
using System.Collections.Generic;
using System.Text;
using WebLibrary.Data;
using WebLibrary.Models;

namespace WebLibrary.DataAccess.Repository.IRepository
{    
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Save()
        {
            _db.SaveChanges();
        }
        public void Update(Product product)
        {
            _db.Products.Update(product);
        }
    }
}
