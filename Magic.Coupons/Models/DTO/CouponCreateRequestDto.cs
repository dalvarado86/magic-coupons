namespace Magic.Coupons.Models.DTO
{
    public class CouponCreateRequestDto
    {
        public string Name { get; set; }
        public int Percent { get; set; }
        public bool IsActive { get; set; }
    }
}
