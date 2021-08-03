using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using Stripe;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ContosoCrafts.Web.Server.Hubs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ContosoCrafts.Web.Shared.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using ContosoCrafts.Web.Server.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ContosoCrafts.Web.Server.Controllers
{
    [Route("api/checkout")]
    public class CheckoutController : ControllerBase
    {
        private readonly IHubContext<EventsHub> eventsHub;
        private readonly ILogger<CheckoutController> logger;
        private readonly IConfiguration configuration;
        private readonly IProductService productService;
        private readonly IDistributedCache cache;

        public CheckoutController(IProductService productService,
                                  IHubContext<EventsHub> eventsHub,
                                  IConfiguration configuration,
                                  IDistributedCache cache,
                                  ILogger<CheckoutController> logger)
        {
            this.cache = cache;
            this.productService = productService;
            this.logger = logger;
            this.eventsHub = eventsHub;
            this.configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult> CheckoutOrder([FromBody]IEnumerable<CartItem> items, [FromServices] IServiceProvider sp)
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

        [HttpGet("session")]
        public async Task<ActionResult> CheckoutSuccess(string session_id)
        {
            var sessionService = new SessionService();
            Session session = await sessionService.GetAsync(session_id);

            var checkoutInfo = new CheckoutInfo
            {
                AmountTotal = session.AmountTotal.Value,
                CustomerEmail = session.CustomerDetails.Email
            };

            var checkoutStr = JsonSerializer.Serialize<CheckoutInfo>(checkoutInfo);
            await cache.SetStringAsync("checkout/info", checkoutStr);
            return Redirect("/checkout/success");
        }

        [HttpGet("info")]
        public async Task<ActionResult> GetCheckoutInfo()
        {
            var checkoutStr = await cache.GetAsync("checkout/info");
            var checkoutInfo = JsonSerializer.Deserialize<CheckoutInfo>(checkoutStr);

            return Ok(checkoutInfo);
        }
    }
}
