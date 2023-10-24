using Application.Exceptions;
using Application.Interfaces;
using Application.Wrappers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Product.Commands
{
    public class DeleteProductCommand : IRequest<ApiResponse<int>>
    {
        public int Id { get; set; }
        internal class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, ApiResponse<int>>
        {
            private readonly IApplicationDbContext _context;
            public DeleteProductCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }
            public async Task<ApiResponse<int>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
            {
                var product = await _context.Products.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                if (product == null)
                {
                    throw new ApiException("Product Not found");
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return new ApiResponse<int>(product.Id, "Product deleted successfully.");
            }
        }
    }
}
