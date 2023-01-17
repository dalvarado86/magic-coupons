using Magic.Coupons.Models;

namespace Magic.Coupons.Repository
{
    public interface ICouponRepository
    {
        Task<ICollection<Coupon>> GetAllAsync();
        Task<Coupon> GetByIdAsync(int id);
        Task<Coupon> GetByNameAsync(string name);
        Task CreateAsync(Coupon coupon);
        void Update(Coupon coupon);
        void Remove(Coupon coupon);
        Task SaveChangesAsync();
    }
}
