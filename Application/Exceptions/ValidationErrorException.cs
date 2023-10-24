using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Exceptions
{
    public class ValidationErrorException : Exception
    {
        public ValidationErrorException() : base("One or more validations occured")
        {
            Errors = new List<string>();
        }

        public List<string> Errors { get; set; }

        public ValidationErrorException(List<ValidationFailure> failures) : this()
        {
            foreach (var error in failures)
            {
                Errors.Add(error.ErrorMessage);
            }
        }
    }
}
