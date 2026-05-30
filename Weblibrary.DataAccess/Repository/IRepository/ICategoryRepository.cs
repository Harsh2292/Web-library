using System;
using System.Collections.Generic;
using System.Text;
using WebLibrary.Models;

namespace WebLibrary.DataAccess.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category category);
        void Save();
    }
}
