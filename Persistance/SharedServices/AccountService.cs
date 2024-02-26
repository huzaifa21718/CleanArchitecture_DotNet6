using Application.DTOs;
using Application.Enums;
using Application.Exceptions;
using Application.Interfaces;
using Application.Wrappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Persistance.IdentityModels;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Persistance.SharedServices
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        public AccountService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<ApiResponse<AuthenticationResponse>> Authenticate(AuthenticationRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new ApiException($"User not registered with this {request.Email}");
            }

            var succeeded = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!succeeded)
            {
                throw new ApiException($"Email or password is incorrect");
            }

            var jwtSecurity = await GenerateTokenAsync(user);
            var authenticationResponse = new AuthenticationResponse();

            authenticationResponse.Id = user.Id;
            authenticationResponse.UserName = user.UserName;
            authenticationResponse.Email = user.Email;
            authenticationResponse.IsVerified = user.EmailConfirmed;

            var roles = await _userManager.GetRolesAsync(user);
            authenticationResponse.Roles = roles.ToList();

            authenticationResponse.JWToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurity);

            return new ApiResponse<AuthenticationResponse>(authenticationResponse, "Authenticated User");
        }

        private async Task<JwtSecurityToken> GenerateTokenAsync(ApplicationUser user)
        {
            var dbClaim = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = new List<Claim>();

            for (int i = 0; i < roles.Count; i++)
            {
                roleClaims.Add(new Claim("roles", roles[i]));
            }

            //string ipAddress = "192.33";

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id.ToString()),
            }
            .Union(dbClaim)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: signingCredentials);
            return jwtSecurityToken;
        }

        public async Task<ApiResponse<Guid>> RegisterUser(RegisterRequest registerRequest)
        {
            var user = await _userManager.FindByEmailAsync(registerRequest.Email);
            if (user != null)
            {
                throw new ApiException($"User already taken {registerRequest.Email}");
            }

            var userModel = new ApplicationUser();

            userModel.UserName = registerRequest.UserName;
            userModel.Email = registerRequest.Email;
            userModel.FirstName = registerRequest.FirstName;
            userModel.LastName = registerRequest.LastName;
            userModel.Gender = registerRequest.Gender;
            userModel.EmailConfirmed = true;
            userModel.PhoneNumberConfirmed = true;

            var result = await _userManager.CreateAsync(userModel, registerRequest.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(userModel, Roles.Basic.ToString());
                return new ApiResponse<Guid>(userModel.Id, "User Register successfuly");
            }
            else
            {
                throw new ApiException(result.Errors.ToString());
            }


        }
    }
}
