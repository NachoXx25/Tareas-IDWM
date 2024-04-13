using System.Security.Cryptography;
using System.Text;
using courses_dotnet_api.Src.DTOs.Account;
using courses_dotnet_api.Src.Interfaces;
using courses_dotnet_api.Src.Models;
using Microsoft.AspNetCore.Mvc;

namespace courses_dotnet_api.Src.Controllers;

public class AccountController : BaseApiController
{
    private readonly IUserRepository _userRepository;
    private readonly IAccountRepository _accountRepository;

    private readonly ITokenService _tokenService;

    public AccountController(IUserRepository userRepository, IAccountRepository accountRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _accountRepository = accountRepository;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<IResult> Login(LoginDto loginDto)
    {
        User? user = await _userRepository.GetUserByEmailAsync(loginDto.Email);

        if (user == null)
        {
            return TypedResults.BadRequest("Credentials are invalid");
        }

        using var hmac = new HMACSHA512(user.PasswordSalt);

        byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i])
            {
                return TypedResults.BadRequest("Credentials are invalid");
            }
        }

        AccountDto accountDto = new()
        {
            Rut = user.Rut,
            Name = user.Name,
            Email = user.Email,
            Token = _tokenService.CreateToken(user.Rut)
        };
        return TypedResults.Ok(accountDto);
    }

    [HttpPost("register")]
    public async Task<IResult> Register(RegisterDto registerDto)
    {
        if (
            await _userRepository.UserExistsByEmailAsync(registerDto.Email)
            || await _userRepository.UserExistsByRutAsync(registerDto.Rut)
        )
        {
            return TypedResults.BadRequest("User already exists");
        }

        await _accountRepository.AddAccountAsync(registerDto);

        if (!await _accountRepository.SaveChangesAsync())
        {
            return TypedResults.BadRequest("Failed to save user");
        }

        AccountDto? accountDto = await _accountRepository.GetAccountAsync(registerDto.Email);

        return TypedResults.Ok(accountDto);
    }
}
