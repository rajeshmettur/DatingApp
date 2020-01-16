using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Model;
using DatingApp.API.Helper;

namespace DatingApp.API.Data
{
    public interface IDatingRepository
    {
         void Add<T>(T entity) where T:class;
         void Delete<T>(T entity) where T:class;
         Task<bool> SaveAll();

         //Task<IEnumerable<User>> GetUsers();  

         Task<PageList<User>> GetUsers(UserParams userparams);

         Task<Like> GetLike(int userId, int recipentId);

         Task<User> GetUser(int id);

         Task<Photo> GetPhoto(int id);

         Task<Photo> GetMainPhotoForUser(int userId);

    }
}