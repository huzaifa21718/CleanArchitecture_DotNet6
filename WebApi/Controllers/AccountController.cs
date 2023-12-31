using Application.DTOs;
using Application.Features.Product.Commands;
using Application.Features.Product.Queries;
using Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser(RegisterRequest registerModel, CancellationToken cancellationToken)
        {
            var result = await _accountService.RegisterUser(registerModel);
            return Ok(result);
        }

    }
}
