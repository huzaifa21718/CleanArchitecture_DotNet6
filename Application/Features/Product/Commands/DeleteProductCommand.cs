using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Product.Commands
{
    public class DeleteProductCommand : IRequest<int>
    {
        public int Id { get; set; }
        internal class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, int>
        {
            private readonly IApplicationDbContext _context;
            public DeleteProductCommandHandler(IApplicationDbContext context)
            {
                _context = context;
            }
            public async Task<int> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
            {
                var product = await _context.Products.Where(x => x.Id == request.Id).FirstOrDefaultAsync();
                if (product == null)
                {
                    return default;
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                return request.Id;
            }
        }
    }
}
