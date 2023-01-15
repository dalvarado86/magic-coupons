using AutoMapper;
using Magic.Coupons.Models;
using Magic.Coupons.Models.DTO;

namespace Magic.Coupons.Profiles
{
    public class CouponProfile : Profile
    {
        public CouponProfile() 
        { 
            CreateMap<Coupon, CouponCreateRequestDto>().ReverseMap();
            CreateMap<Coupon, CouponResponseDto>().ReverseMap();
        }
    }
}
