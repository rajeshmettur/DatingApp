using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helper;
using DatingApp.API.Model;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            this._context = context;

        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipentId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId == recipentId);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p=>p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
           var photo = await _context.Photos.FirstOrDefaultAsync(p=>p.Id == id);
           return photo;
        }

        public async Task<User> GetUser(int id)
        {
           var user = await _context.Users.Include(p=>p.Photos).FirstOrDefaultAsync(u=>u.Id== id);
           return user;
        }

        public async Task<PageList<User>> GetUsers(UserParams userparams)
        {
          var user = _context.Users.Include(p=>p.Photos)
          .OrderByDescending(u => u.LastActive).AsQueryable();
          user = user.Where(u=>u.Id != userparams.UserId);
          user = user.Where(u=>u.Gender == userparams.Gender);

          if(userparams.Likers)
          {
              var userLikers = await GetUserLikes(userparams.UserId, userparams.Likers);
              user = user.Where(u => userLikers.Contains(u.Id));
          }
          
          if(userparams.Likees)
          {
              var userLikees = await GetUserLikes(userparams.UserId, userparams.Likers);
              user = user.Where(u => userLikees.Contains(u.Id));
          }

          if(userparams.MinAge != 18 && userparams.MaxAge != 75)
          {
              var minDob = DateTime.Today.AddYears(-userparams.MaxAge - 1);
              var maxDob = DateTime.Today.AddYears(-userparams.MinAge);

              user = user.Where(u=>u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
          }

          if(!string.IsNullOrEmpty(userparams.OrderBy))
          {
              switch(userparams.OrderBy)
              {
                  case "created": 
                  user = user.OrderByDescending(u => u.Created);
                  break;
                  
                  default:
                  user = user.OrderByDescending(u => u.LastActive);
                  break;
              }
          }

          return await PageList<User>.CreateAsync(user, userparams.PageNumber, userparams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users
                       .Include(x => x.Likers).Include( x => x.Likees)
                       .FirstOrDefaultAsync(u => u.Id == id);
            if(likers)
               return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            else
               return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);

        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        
    }
}