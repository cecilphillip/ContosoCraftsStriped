using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ContosoCrafts.Web.Shared.Models;
using EventAggregator.Blazor;
using Microsoft.AspNetCore.Components;

namespace ContosoCrafts.Web.Client.Shared
{
    public class ProductListBase : ComponentBase
    {
        [Inject]
        private IEventAggregator EventAggregator { get; set; }

        [Inject]
        private ILocalStorageService LocalStorage { get; set; }

        [Inject]
        private IHttpClientFactory ClientFactory { get; set; }

        protected IEnumerable<Product> products;
        protected Product selectedProduct;

        protected override async Task OnInitializedAsync()
        {

            if (products == null)
            {
                var client = ClientFactory.CreateClient("localapi");
                products = await client.GetFromJsonAsync<IEnumerable<Product>>("/api/products");
            }

            var state = await LocalStorage.GetItemAsync<Dictionary<string, CartItem>>("state.cart") ?? new();
            if (state.Any())
            {
                await EventAggregator.PublishAsync(new ShoppingCartUpdated { ItemCount = state.Keys.Count });
            }

        }

        protected void SelectProduct(string productId)
        {
            selectedProduct = products.Where(p => p.Id == productId).SingleOrDefault();
        }

        public async Task SubmitRating(int rating)
        {
            var client = ClientFactory.CreateClient("localapi");
            var ratings = selectedProduct.Ratings;

            // resize ratings array
            Array.Resize(ref ratings, ratings.Length + 1);
            ratings[^1] = rating;
            selectedProduct.Ratings = ratings;

            await client.PutAsJsonAsync($"/api/products/{selectedProduct.Id}", new { rating = rating });
        }

        protected async Task AddToCart(string productId, string title)
        {
            // get state
            var state = await LocalStorage.GetItemAsync<Dictionary<string, CartItem>>("state.cart") ?? new();
            if (state.ContainsKey(productId))
            {
                // Product already in cart
                CartItem selectedItem = state[productId];
                selectedItem.Quantity++;
                state[productId] = selectedItem;
            }
            else
            {
                // Add product to cart
                state[productId] = new CartItem { Id = productId, Title = title, Quantity = 1 };
            }

            // persist state in dapr
            await LocalStorage.SetItemAsync("state.cart", state);
            await EventAggregator.PublishAsync(new ShoppingCartUpdated { ItemCount = state.Keys.Count });
        }
    }
}
