using Microsoft.AspNetCore.Mvc;
using MoviesBE.Services.User;

namespace MoviesBE.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;

    public AuthController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> RegisterOrUpdateUser()
    {
        var userDto = await _userService.RegisterOrUpdateUserAsync();
        return Ok(userDto);
    }
}