using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using WebLibrary.Data;
using WebLibrary.Models;

namespace WebLibrary.DataAccess.Repository.IRepository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext _db;

        public CategoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Save()
        {
            _db.SaveChanges();
        }

        public void Update(Category category)
        {
            _db.Category.Update(category);
        }
    }
}
