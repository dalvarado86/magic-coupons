using FluentValidation;
using Magic.Coupons.Models.DTO;

namespace Magic.Coupons.Validators
{
    public class CouponUpdateValidator : AbstractValidator<CouponUpdateRequestDto>
    {
        public CouponUpdateValidator()
        {
            RuleFor(x => x.Id)
               .NotEmpty()
               .GreaterThan(0);

            RuleFor(x => x.Name)
                .NotEmpty();

            RuleFor(x => x.Percent)
                .InclusiveBetween(1, 100);
        }
    }
}
