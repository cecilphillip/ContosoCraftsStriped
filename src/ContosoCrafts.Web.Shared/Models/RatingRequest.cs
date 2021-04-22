namespace ContosoCrafts.Web.Shared.Models
{
    public class RatingRequest
    {
        public string ProductId { get; set; }
        public int Rating { get; set; }
    }
    public record CheckoutResponse(string CheckoutSessionID);
}