using Application.Exceptions;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Behaviors
{
    public class ValidationBehaviors<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validator;
        public ValidationBehaviors(IEnumerable<IValidator<TRequest>> validator)
        {
            _validator = validator;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            //Pre-Processing logic here
            //For Example, Logging, validation

            if (_validator.Any())
            {
                var validationContext = new ValidationContext<TRequest>(request);
                var result = await Task.WhenAll(_validator.Select(v => v.ValidateAsync(validationContext, cancellationToken)));
                var failers = result.SelectMany(r => r.Errors).Where(f => f != null).ToList();

                if (failers.Count > 0)
                {
                    throw new ValidationErrorException(failers);
                }
            }

            //Next
            var response = await next();

            //Post-Processing logic here
            //For Example, Response modification...

            return response;
        }
    }
}
