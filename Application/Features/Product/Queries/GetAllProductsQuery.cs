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

namespace Application.Features.Product.Queries
{
    public class GetAllProductsQuery : IRequest<ApiResponse<IEnumerable<Domain.Entites.Product>>>
    {
        internal class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, ApiResponse<IEnumerable<Domain.Entites.Product>>>
        {
            private readonly IApplicationDbContext _context;
            public GetAllProductsQueryHandler(IApplicationDbContext context)
            {
                _context = context;
            }
            public async Task<ApiResponse<IEnumerable<Domain.Entites.Product>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
            {
               var result = await _context.Products.ToListAsync(cancellationToken);
                if (result == null)
                {
                    throw new ApiException("Product not found.");
                }

                return new ApiResponse<IEnumerable<Domain.Entites.Product>>(result, "Data Fetched successfully");
            }
        }
    }
}
