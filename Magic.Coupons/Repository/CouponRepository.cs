using Magic.Coupons.Data;
using Magic.Coupons.Models;
using Microsoft.EntityFrameworkCore;

namespace Magic.Coupons.Repository
{
    public class CouponRepository : ICouponRepository
    {
        private readonly ApplicationDbContext context;

        public CouponRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task CreateAsync(Coupon coupon)
        {
            await this.context.AddAsync(coupon);
        }

        public async Task<ICollection<Coupon>> GetAllAsync()
        {
            return await this.context.Coupons.ToListAsync();
        }

        public async Task<Coupon> GetByIdAsync(int id)
        {
            return await context.Coupons.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Coupon> GetByNameAsync(string name)
        {
            return await context.Coupons.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower());
        }

        public async Task SaveChangesAsync()
        {
            await this.context.SaveChangesAsync();
        }
        public void Remove(Coupon coupon)
        {
            this.context.Remove(coupon);
        }

        public void Update(Coupon coupon)
        {
            this.context.Update(coupon);
        }
    }
}
