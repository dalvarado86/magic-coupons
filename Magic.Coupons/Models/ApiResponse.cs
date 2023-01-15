using System.Net;

namespace Magic.Coupons.Models
{
    public class ApiResponse
    {
        public ApiResponse() 
        {
            ErrorMessages = new List<string>();
        }

        public bool IsSucess { get; set; }
        public Object Result { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public List<string> ErrorMessages { get; set; }
    }
}
