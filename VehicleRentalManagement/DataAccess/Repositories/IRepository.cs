using System.Collections.Generic;

namespace VehicleRentalManagement.DataAccess.Repositories
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        int Add(T entity);
        bool Update(T entity);
        bool Delete(int id);
    }
}
