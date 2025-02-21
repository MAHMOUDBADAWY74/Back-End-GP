using System;
using System.Collections.Generic;
using System.Linq;

namespace OnlineLibrary.Service.HandleResponse
{
    public class ValidationErrorResponse : UserException
    {

        public ValidationErrorResponse() : base(400)
        {
            Errors = []; // Initialize with an empty list
        }

        public ValidationErrorResponse(IEnumerable<string> errors) : base(400)
        {
            Errors = errors;
        }

        public IEnumerable<string> Errors { get; set; }
    }
}