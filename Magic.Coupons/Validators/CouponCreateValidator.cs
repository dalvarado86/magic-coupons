using FluentValidation;
using Magic.Coupons.Models.DTO;

namespace Magic.Coupons.Validators
{
    public class CouponCreateValidator : AbstractValidator<CouponCreateRequestDto>
    {
        public CouponCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty();

            RuleFor(x => x.Percent)
                .InclusiveBetween(1, 100);
        }
    }
}
