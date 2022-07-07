using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Services
{
    public interface IDataService<T>
    {
        Task<List<T>> GetAll();

        Task<T> GetById(string id);

        Task<T> Create(T entity);

        Task<T> Update(string id,T entity);

        Task<bool> Delete(string id);

        Task<bool> DeleteAll();
    }

    
}
