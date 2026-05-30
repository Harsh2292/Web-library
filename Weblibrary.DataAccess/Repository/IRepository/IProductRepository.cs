using System;
using System.Collections.Generic;
using System.Text;
using WebLibrary.Models;

namespace WebLibrary.DataAccess.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product product);
        void Save();
    }
}
