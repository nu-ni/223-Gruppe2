using L_Bank_W_Backend.Core.Models;
using L_Bank_W_Backend.Dto;
using L_Bank_W_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace L_Bank_W_Backend.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class LoginController(IUserRepository userRepository, ILoginService loginService) : ControllerBase
    {
        private readonly IUserRepository _userRepository =
            userRepository ?? throw new ArgumentNullException(nameof(userRepository));

        private readonly ILoginService _loginService =
            loginService ?? throw new ArgumentNullException(nameof(loginService));

        [HttpPost]
        public async Task<IActionResult> Post([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] LoginDto login)
        {
            return await Task.Run(() =>
            {
                IActionResult response;

                var user = _userRepository.Authenticate(login.Username, login.Password);

                if (user == null)
                {
                    response = Unauthorized();
                }
                else
                {
                    response = Ok(new { token = _loginService.CreateJwt(user) });
                }

                return response;
            });
        }
    }
}