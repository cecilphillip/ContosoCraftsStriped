using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ContosoCrafts.Web.Shared.Models;
using Microsoft.Extensions.FileProviders;
using Stripe;
using Stripe.Checkout;

namespace ContosoCrafts.Web.Server.Services
{
    public class JsonFileProductService : IProductService
    {
        public JsonFileProductService()
        {
            var manifestEmbeddedProvider = new ManifestEmbeddedFileProvider(typeof(Program).Assembly);
            var fileInfo = manifestEmbeddedProvider.GetFileInfo("_data/products.json");
            using var reader = new StreamReader(fileInfo.CreateReadStream());


            var fileContent = reader.ReadToEnd();

            Products = JsonSerializer.Deserialize<List<Shared.Models.Product>>(fileContent,
                   new JsonSerializerOptions
                   {
                       PropertyNameCaseInsensitive = true
                   });
        }

        private List<Shared.Models.Product> Products { get; }

        public Task<IEnumerable<Shared.Models.Product>> GetProducts() => Task.FromResult(Products.AsEnumerable());

        public Task<Shared.Models.Product> GetProduct(string productId)
        {
            var product = Products.FirstOrDefault(x => x.Id == productId);
            return Task.FromResult(product);
        }

        public async Task AddRating(string productId, int rating)
        {
            var products = await GetProducts();

            if (products.First(x => x.Id == productId).Ratings == null)
            {
                products.First(x => x.Id == productId).Ratings = new int[] { rating };
            }
            else
            {
                var ratings = products.First(x => x.Id == productId).Ratings.ToList();
                ratings.Add(rating);
                products.First(x => x.Id == productId).Ratings = ratings.ToArray();
            }
        }

        public async Task<CheckoutResponse> CheckOut(IEnumerable<CartItem> Items, string callbackRoot)
        {


            var sessionOptions = new SessionCreateOptions()
            {
                SuccessUrl = $"{callbackRoot}/checkout/success", /// redirect after checkout
                CancelUrl = $"{callbackRoot}/checkout/failure",  /// checkout cancelled
                PaymentMethodTypes = new List<string> { "card" },
                CustomerEmail = "cecilphillip@yahoo.com",
                LineItems = new List<SessionLineItemOptions>
                {
                  new() {
                      PriceData = new() {
                            UnitAmount = 2000L,
                            ProductData = new SessionLineItemPriceDataProductDataOptions{
                                Name = "Stuff"
                            },
                            Currency = "USD"
                    },
                    Quantity = 100L
                  }
                },
                Mode = "payment"
            };

            var checkoutService = new SessionService();
            var session = await checkoutService.CreateAsync(sessionOptions);

            return new CheckoutResponse(session.Id);
        }
    }
}