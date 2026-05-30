using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace WebLibrary.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        // T -  Category
        IEnumerable<T> GetAll();
        T Get(Expression<Func<T,bool>> filter); 
        void Add(T entity);            
        void Remove(T entity);         
        void RemoveRange(IEnumerable<T> entity);         
    }
}
