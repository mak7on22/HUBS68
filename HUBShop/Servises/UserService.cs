using HUBShop.Models;
using HUBShop.Models.Users;
using Microsoft.Ajax.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HUBShop.Servises
{
    public class UserService
    {
        private HubContext context;
        private IMemoryCache cache;

        public UserService(HubContext context, IMemoryCache cache) 
        {
            this.cache = cache;
            this.context = context;
        }
        public async Task<IEnumerable<User>> GetUsers() => await context.Users.ToListAsync();

        public async Task AddUSer(User user) 
        {
            await context.Users.AddAsync(user);
            int s = await context.SaveChangesAsync();
            if (s > 0)
            cache.Set(user.Id,user,new MemoryCacheEntryOptions {AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)});
        }
        public async Task<User> GetUser(int id) 
        {
            User user = null;
            if (!cache.TryGetValue(id,out user)) 
            {
                user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);
                if (user != null)
                    cache.Set(user.Id,user, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            }
            return user;
        }
    }
  
}
