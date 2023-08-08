using Application.Features.Product.Commands;
using Application.Features.Product.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var result = await _mediator.Send(new GetAllProductsQuery());
            return Ok(result);
        }

        [HttpPost("CreateProduct")]
        public async Task<IActionResult> CreateProduct(CreateProductCommand createProduct, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(createProduct, cancellationToken);
            return Ok(result);
        }

        [HttpPost("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(UpdateProductCommand updateProduct, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(updateProduct, cancellationToken);
            return Ok(result);
        }

        [HttpPost("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(DeleteProductCommand deleteProduct, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(deleteProduct, cancellationToken);
            return Ok(result);
        }
    }
}
