using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        //T will be like Category class
        //Both of these classify as read
        //Getting all the values
        IEnumerable<T> GetAll(string? includeProperties = null);
        //Getting a particular value
        T Get(Expression<Func<T, bool>> filter, string? includeProperties = null);
        //Create the value
        void Add(T entity);
        //Delete the value
        void Remove(T entity);
        //Delete the Range
        void RemoveFromRange(IEnumerable<T> entity);
    }
}
