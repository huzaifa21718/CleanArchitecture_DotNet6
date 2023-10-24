using Application.Exceptions;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Product.Queries
{
    public class GetProductByIdQuery : IRequest<Domain.Entites.Product>
    {
        public int Id { get; set; }
        internal class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Domain.Entites.Product>
        {
            private readonly IApplicationDbContext _context;
            public GetProductByIdQueryHandler(IApplicationDbContext context)
            {
                _context = context;
            }
            public async Task<Domain.Entites.Product> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
            {
                var result = await _context.Products.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                if (result == null)
                {
                    throw new ApiException("Product not found.");
                }

                return result;
            }
        }
    }
}
