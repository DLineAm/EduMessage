using System.Collections.Generic;

namespace EduMessage.Services
{
    public interface IStorage<T>
    {
        public T GetById(int id);
        public IEnumerable<T> GetListById(int id);
        public void Add(T entity);
        public void AddRange(IEnumerable<T> entity);
    }
}