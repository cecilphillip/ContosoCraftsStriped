using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ContosoCrafts.Web.Server.Services;
using ContosoCrafts.Web.Shared.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using ContosoCrafts.Web.Server.Hubs;
using Microsoft.Extensions.Configuration;

namespace ContosoCrafts.Web.Server.Controllers
{
    [ApiController]
    [Route("api")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService productService;
        private readonly ILogger<ProductsController> logger;
        private readonly IHubContext<EventsHub> eventsHub;
        private readonly IConfiguration configuration;

        public ProductsController(IProductService productService, IHubContext<EventsHub> eventsHub, IConfiguration configuration, ILogger<ProductsController> logger)
        {
            this.configuration = configuration;
            this.eventsHub = eventsHub;
            this.logger = logger;
            this.productService = productService;
        }

        [HttpGet("products")]
        public async Task<ActionResult> GetProducts()
        {
            var products = await productService.GetProducts();
            return Ok(products);
        }

        [HttpGet("products/{id}")]
        public async Task<ActionResult> GetSingle(string id)
        {
            var result = await productService.GetProduct(id);
            return Ok(result);
        }


        [HttpPatch("products")]
        public async Task<ActionResult> Patch([FromBody] RatingRequest request)
        {
            await productService.AddRating(request.ProductId, request.Rating);

            return Ok();
        }

        [HttpPost("checkout")]
        public async Task<ActionResult> CheckoutOrder(IEnumerable<CartItem> items, [FromServices] IServiceProvider sp)
        {

            logger.LogInformation("Order received...");

            var host = $"{Request.Scheme}://{Request.Host.ToString()}";
            var server = sp.GetRequiredService<IServer>();
            var address = server.Features.Get<IServerAddressesFeature>().Addresses.FirstOrDefault();

            var checkoutResponse = await productService.CheckOut(items, address);
            var pubKey = configuration["Stripe:PubKey"];

            await eventsHub.Clients.All.SendAsync("CheckoutSessionStarted", pubKey, checkoutResponse);
            return Ok();
        }

        [HttpGet("/checkout/success")]
        public ActionResult CheckoutSuccess()
        {
#pragma warning disable CS4014 
            Task.Run(async () =>
            {
                await Task.Delay(1500);
                await eventsHub.Clients.All.SendAsync("CheckoutCompleted", "success");
            });
#pragma warning restore CS4014
            return Redirect("/");
        }

        [HttpGet("/checkout/failure")]
        public ActionResult CheckoutFailure()
        {

#pragma warning disable CS4014 
            Task.Run(async () =>
            {
                await Task.Delay(1500);
                await eventsHub.Clients.All.SendAsync("CheckoutCompleted", "failure");
            });
#pragma warning restore CS4014 

            return Redirect("/");
        }
    }
}