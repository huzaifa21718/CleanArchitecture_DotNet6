using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Product.Commands
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotNull()
                .NotEmpty().WithMessage("{PropertyName} is required").
                Length(2, 10).WithMessage("{PropertyName} is between 2 and 10");


            RuleFor(x => x.Rate)
             .NotNull()
             .NotEmpty().WithMessage("{PropertyName} is required")
             .LessThan(500);

        }
    }
}
