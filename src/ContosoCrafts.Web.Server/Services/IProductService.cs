using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoCrafts.Web.Shared.Models;

namespace ContosoCrafts.Web.Server.Services
{
    public interface IProductService
    {
        Task AddRating(string productId, int rating);
        Task<IEnumerable<Product>> GetProducts();
        Task<Product> GetProduct(string id);
        Task<CheckoutResponse> CheckOut(IEnumerable<CartItem> Items, string callbackRoot);
    }
}
