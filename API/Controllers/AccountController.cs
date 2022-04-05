using System.Security.Cryptography;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using Microsoft.AspNetCore.Mvc;
using API.Entities;
using System.Text;
using API.Dtos;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {

            if(await UserExists(registerDto.Username))
            {
                return BadRequest("UserName is taken");
            }

            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto{
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
{
        var user = await _context.Users.Include(x=>x.Photos).SingleOrDefaultAsync(x=>x.UserName == loginDto.Username);
        if(user == null){
            return Unauthorized("Invalid USer");
        }

        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
        for(int i=0;i< computeHash.Length; i++)
        {
            if(computeHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
        }
        return new UserDto{
                Username = user.UserName,
                Token = _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x=>x.IsMain)?.Url
            };;
}
        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x=>x.UserName == username.ToLower());
        }
    }
}