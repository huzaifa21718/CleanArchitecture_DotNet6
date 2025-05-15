using Application.DTOs;
using Application.Enums;
using Application.Exceptions;
using Application.Interfaces;
using Application.Wrappers;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
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
        private readonly IEmailService _emailService;
        public AccountService(UserManager<ApplicationUser> userManager, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<ApiResponse<AuthenticationResponse>> Authenticate(AuthenticationRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new ApiException($"User not registered with this {request.Email}");
            }

            if (!user.EmailConfirmed)
            {
                throw new ApiException($"Email not confirmed, pls confirm your email to login");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                throw new ApiException($"Your account is locked, please try agina later.");
            }

            var succeeded = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!succeeded)
            {
                await _userManager.AccessFailedAsync(user);
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
            userModel.PhoneNumberConfirmed = true;

            var result = await _userManager.CreateAsync(userModel, registerRequest.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(userModel, Roles.Basic.ToString());


                string emailTemplate = @"<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Welcome Email</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }
        .container {
            max-width: 600px;
            background: #ffffff;
            margin: 20px auto;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            text-align: center;
        }
        .header {
            font-size: 24px;
            font-weight: bold;
            color: #333;
        }
        .content {
            font-size: 16px;
            color: #666;
            margin-top: 10px;
        }
        .button {
            display: inline-block;
            background-color: #FF0000;
            color: #ffffff;
            padding: 12px 20px;
            margin-top: 20px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
        }
        .footer {
            margin-top: 20px;
            font-size: 12px;
            color: #999;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">Welcome [UserName]</div>
        <div class=""content"">
            Thank you for joining us. We are excited to have you on board. Click the button below to subscribe to my channel!
        </div>
        <a href=""https://www.youtube.com/@CodeWithHanif"" target=""_blank"" class=""button"">Subscribe Now</a>
        <div class=""footer"">
            If you have any questions, feel free to contact us at <a href=""https://www.linkedin.com/in/muhammad-hanif-shahzad-a49409214"" target=""_blank"">LinkedIn</a>
        </div>
    </div>
</body>
</html>
";

                //var emailRequest = new EmailRequest()
                //{
                //    To = userModel.Email,
                //    Body = emailTemplate.Replace("[UserName]", userModel.Email),
                //    Subject = $"Welcome {userModel.Email} to CodeWithHanif",
                //    IsHtmlBody = true,
                //};

                //await _emailService.SendAsync(emailRequest);

                await SendConfirmationEmailAsync(userModel);

                return new ApiResponse<Guid>(userModel.Id, "Verification email has been sent to your account, pls verify your account.");
            }
            else
            {
                throw new ApiException(result.Errors.ToString());
            }


        }

        private async Task SendConfirmationEmailAsync(ApplicationUser userModel)
        {
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(userModel);

            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            string verificationUrl = $"{_configuration["ClientUrl"]}/api/account/confirm-email?userId={userModel.Id}&token={token}";

            var emailRequest = new EmailRequest()
            {
                To = userModel.Email,
                Body =  $"<p>Please verify your account by click on this link: {verificationUrl} </p> <br> <p>If this email is not realted to you please ignore it.</p>" ,
                Subject = $"Confirm your email {userModel.Email} to CodeWithHanif",
                IsHtmlBody = true,
            };

            await _emailService.SendAsync(emailRequest);
        }

        public async Task<ApiResponse<bool>> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new ApiException($"User not found with this {userId}");
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                return new ApiResponse<bool>(true, "Email confirmed successfully");
            }
            else
            {
                throw new ApiException(result.Errors.ToString());
            }
        }

        public async Task<ApiResponse<bool>> ResendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new ApiException($"User not found with this {email}");
            }

            if (user.EmailConfirmed)
            {
                throw new ApiException($"Email already confirmed");
            }

            await SendConfirmationEmailAsync(user);
            return new ApiResponse<bool>(true, "Verification email has been sent to your account, pls verify your account.");
        }

        public async Task<ApiResponse<bool>> ForgotPasswordAsync(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            if (user == null)
            {
                throw new ApiException($"User not found with this {userEmail}");
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);

            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            string resetPasswordLink = $"{_configuration["ClientUrl"]}/api/account/reset-password?email={userEmail}&token={token}";

            var emailRequest = new EmailRequest()
            {
                To = userEmail,
                Body = $"<p>To reset your password click on this link: <a href='{resetPasswordLink}'>Click here to reset password</a> </p>",
                Subject = $"Reset password",
                IsHtmlBody = true,
            };

            await _emailService.SendAsync(emailRequest);
            return new ApiResponse<bool>(true, "Reset password link has been sent to your account, pls check your email.");
        }

        public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequest resetPassword)
        {
            var user = await _userManager.FindByEmailAsync(resetPassword.Email);
            if (user == null)
            {
                throw new ApiException($"User not found with this {resetPassword.Email}");
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(resetPassword.Token));

            var result = await _userManager.ResetPasswordAsync(user, token, resetPassword.NewPassword);

            if (result.Succeeded)
            {
                return new ApiResponse<bool>(true, "Password reset successfully");
            }
            else
            {
                throw new ApiException(result.Errors.ToString());
            }
        }
    }
}
