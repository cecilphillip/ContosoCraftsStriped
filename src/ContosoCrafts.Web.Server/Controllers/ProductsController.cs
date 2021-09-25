using System.Threading.Tasks;
using ContosoCrafts.Web.Server.Services;
using ContosoCrafts.Web.Shared.Models;
using Microsoft.AspNetCore.Mvc;
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

        public ProductsController(IProductService productService, IHubContext<EventsHub> eventsHub, IConfiguration configuration, ILogger<ProductsController> logger)
        {
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
    }
}
