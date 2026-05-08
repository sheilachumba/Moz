using Microsoft.AspNetCore.Mvc;
using MOZ_UPGRADE;
using MOZ_UPGRADE.Context;
using MOZ_UPGRADE.Interfaces;
using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace MOZ_MOBILE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Generate_Tockens : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Generate_Tockens(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }

        public class LoginModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
        private LoginModel model = new();


        [HttpPost("login")]
        public async Task<EmailSendStatus> LoginUser([FromBody] LoginModel model)
        {
            TokenProvider tokenProvider = new TokenProvider(_unitOfWork, _httpContextAccessor);
            try
            {
                Encryption encryption = new Encryption();

                string pass = encryption.Encryptstring(model.Password);

                Users user = await tokenProvider.GetUserDetails(model.Username, pass);

                if (user != null)
                {
                    if (user.IsEmailVerified)
                    {

                        var token = tokenProvider.GenerateToken(user);

                        //  _httpContextAccessor.HttpContext.Session.SetString("JWToken", token);
                        // await SecureStorage.SetAsync("jwt_token", token);

                        //   return Ok(new { token = token });

                        statusmessage.status = true;
                        statusmessage.message = token;
                    }
                    else
                    {
                        statusmessage.status = false;
                        statusmessage.message = "📧 Please verify your email address before logging in. Check your inbox for the verification link.";

                    }
                }
                else
                {
                    statusmessage.status = false;
                    statusmessage.message = "Wrong User Credentials..Please try Again";
                    //  var data = $"Username is : {strUserName} and password is : {pass}";
                    // statusmessage.message = data;

                }
            }
            catch (Exception e)
            {
                statusmessage.status = false;
                statusmessage.message = e.Message;
            }
            return statusmessage;
        }

        [HttpPost("GetByEmail")]
        public async Task<Users> GetByEmail([FromBody] string model)
        {
            try
            {
                Users user = await _unitOfWork.userRepository.GetByEmail(model);
                if (user != null)
                {
                    return user;
                }
                else
                {
                    return new Users();
                }
            }
            catch (Exception e)
            {
                return new Users();
            }

        }
        //[HttpPost("CreateUser")]
        //public string CreateUser([FromBody] Users model)
        //{
        //    return "All is well";
        //}
        EmailSendStatus statusmessage = new EmailSendStatus();

        [HttpPost("CreateUser")]
        public async Task<EmailSendStatus> CreateUser([FromBody] Users model)
        {

            try
            {
                Users userrr = await _unitOfWork.userRepository.GetByEmail(model.Email);
                if (userrr != null)
                {
                    statusmessage.status = false;
                    statusmessage.message = "Oops! This Email is already taken. Try a different one and make it yours!";
                }
                else
                {
                    Encryption encryption = new Encryption();

                    string pass = encryption.Encryptstring(model.password_hash);
                    model.password_hash = pass;

                    await _unitOfWork.userRepository.AddNew(model);
                    _unitOfWork.save();
                    //string jsonString = JsonSerializer.Serialize(model);

                    statusmessage.status = true;
                    statusmessage.message = "Account Created Successfully";
                }

            }
            catch (Exception e)
            {
                statusmessage.status = false;
                statusmessage.message = e.Message;
            }
            return statusmessage;
        }


        [HttpPost("AddNew")]
        public async Task<EmailSendStatus> AddNew([FromBody] Users entity)
        {
            try
            {
                Users userrr = await _unitOfWork.userRepository.GetByEmail(entity.Email);
                if (userrr != null)
                {
                    statusmessage.status = false;
                    statusmessage.message = "Oops! This Email is already taken. Try a different one and make it yours!";
                }
                else
                {
                    Encryption encryption = new Encryption();

                    string pass = encryption.Encryptstring(entity.password_hash);
                    entity.password_hash = pass;

                    await _unitOfWork.userRepository.AddNew(entity);
                    _unitOfWork.save();
                    //string jsonString = JsonSerializer.Serialize(model);

                    statusmessage.status = true;
                    statusmessage.message = "Account Created Successfully";
                }
            }
            catch (Exception e)
            {
                statusmessage.status = false;
                statusmessage.message = e.Message;
            }
            return statusmessage;
        }
        [HttpGet("GetAll")]
        public async Task<List<Users>> GetAll()
        {
            try
            {
                return await _unitOfWork.userRepository.GetAll();

            }
            catch (Exception e)
            {
                statusmessage.status = false;
                statusmessage.message = e.Message;
            }
            return new List<Users>();

        }
        [HttpPost("GetById")]
        public async Task<Users> GetById([FromBody] string Id)
        {
            try
            {
                if (Id != null)
                {
                    return await _unitOfWork.userRepository.GetById(Id);
                }


            }
            catch (Exception e)
            {
                statusmessage.status = false;
                statusmessage.message = e.Message;
            }
            return new Users();
        }
        [HttpPost("Remove")]
        public async Task<EmailSendStatus> Remove([FromBody] Users entity)
        {
            try
            {
                if (entity != null)
                {
                    await _unitOfWork.userRepository.Remove(entity);
                    statusmessage.status = true;
                    statusmessage.message = "User Removed Successfully";

                }
                else
                {
                    statusmessage.status = false;
                    statusmessage.message = "User Not Found";
                }
            }
            catch (Exception e)
            {
                statusmessage.status = false;
                statusmessage.message = e.Message;
            }
            return statusmessage;
        }
        [HttpPost("Update")]
        public async Task<EmailSendStatus> Update([FromBody] Users entity)
        {
            try
            {
                if (entity != null)
                {
                    await _unitOfWork.userRepository.Update(entity);
                    statusmessage.status = true;
                    statusmessage.message = "User Updated Successfully";

                }
                else
                {
                    statusmessage.status = false;
                    statusmessage.message = "User Not Found";
                }
            }
            catch (Exception e)
            {
                statusmessage.status = false;
                statusmessage.message = e.Message;
            }
            return statusmessage;
        }


        //.............

        // ── Password Reset Models ──
        public class CompletePasswordResetRequest
        {
            public string Token { get; set; }
            public string Email { get; set; }
            public string NewPassword { get; set; }
        }

        public class PasswordResetResponse
        {
            public string Token { get; set; }
            public bool Success { get; set; }
            public string Message { get; set; }
        }

        // ── Password Reset Endpoints ──

        [HttpPost("RequestPasswordReset")]
        public async Task<PasswordResetResponse> RequestPasswordReset([FromBody] string email)
        {
            var response = new PasswordResetResponse();
            try
            {
                Users user = await _unitOfWork.userRepository.GetByEmail(email);
                if (user == null)
                {
                    response.Success = false;
                    response.Message = "No account found with that email address.";
                    return response;
                }

                // Generate a secure reset token (64 hex chars)
                var token = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

                // Store the token and expiry on the user record (30 min validity)
                user.ResetToken = token;
                user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(30);

                await _unitOfWork.userRepository.Update(user);
                _unitOfWork.save();

                response.Token = token;
                response.Success = true;
                response.Message = "Reset token generated successfully.";
            }
            catch (Exception e)
            {
                response.Success = false;
                response.Message = e.Message;
            }
            return response;
        }

        [HttpPost("CompletePasswordReset")]
        public async Task<EmailSendStatus> CompletePasswordReset([FromBody] CompletePasswordResetRequest request)
        {
            try
            {
                Users user = await _unitOfWork.userRepository.GetByEmail(request.Email);
                if (user == null)
                {
                    statusmessage.status = false;
                    statusmessage.message = "User not found.";
                    return statusmessage;
                }

                // Validate token and expiry
                if (user.ResetToken != request.Token || user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTime.UtcNow)
                {
                    statusmessage.status = false;
                    statusmessage.message = "Invalid or expired reset token.";
                    return statusmessage;
                }

                // Hash and update the new password
                Encryption encryption = new Encryption();
                user.password_hash = encryption.Encryptstring(request.NewPassword);

                // Clear the reset token so it can't be reused
                user.ResetToken = null;
                user.ResetTokenExpiry = null;

                await _unitOfWork.userRepository.Update(user);
                _unitOfWork.save();

                statusmessage.status = true;
                statusmessage.message = "Password has been reset successfully.";
            }
            catch (Exception e)
            {
                statusmessage.status = false;
                statusmessage.message = e.Message;
            }
            return statusmessage;
        }
        public class ValidateResetTokenRequest
        {
            public string Token { get; set; }
            public string Email { get; set; }
        }

        [HttpPost("ValidateResetToken")]
        public async Task<EmailSendStatus> ValidateResetToken([FromBody] ValidateResetTokenRequest request)
        {
            try
            {
                Users user = await _unitOfWork.userRepository.GetByEmail(request.Email);
                if (user == null)
                {
                    statusmessage.status = false;
                    statusmessage.message = "User not found.";
                    return statusmessage;
                }

                if (user.ResetToken != request.Token || user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTime.UtcNow)
                {
                    statusmessage.status = false;
                    statusmessage.message = "Invalid or expired reset token.";
                    return statusmessage;
                }

                statusmessage.status = true;
                statusmessage.message = "Token is valid.";
            }
            catch (Exception e)
            {
                statusmessage.status = false;
                statusmessage.message = e.Message;
            }
            return statusmessage;
        }
        [HttpGet("ResetRedirect")]
        public IActionResult ResetRedirect([FromQuery] string token, [FromQuery] string email)
        {
            var appLink = $"standardinsurance://reset-password?token={Uri.EscapeDataString(token ?? "")}&email={Uri.EscapeDataString(email ?? "")}";

            var html = $@"<!DOCTYPE html>
<html><head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <title>Reset Password - Standard Insurance</title>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background: #f5f5f7; margin: 0; padding: 40px 16px; text-align: center; }}
        .card {{ max-width: 420px; margin: 60px auto; background: white; border-radius: 16px; padding: 40px 24px; box-shadow: 0 8px 30px rgba(0,0,0,0.12); }}
        h1 {{ color: #003366; font-size: 22px; margin-bottom: 12px; }}
        p {{ color: #555; font-size: 14px; line-height: 1.6; }}
        .btn {{ display: inline-block; margin-top: 20px; padding: 12px 32px; background: #2563eb; color: white; text-decoration: none; border-radius: 999px; font-weight: 600; font-size: 14px; }}
        .btn:hover {{ background: #1d4ed8; }}
    </style>
</head><body>
    <div class='card'>
        <h1>Opening Standard Insurance App...</h1>
        <p>If the app doesn't open automatically, tap the button below.</p>
        <a href='{appLink}' class='btn'>Open in App</a>
        <p style='margin-top:24px; font-size:12px; color:#999;'>If you don't have the app installed, please download it from the app store.</p>
    </div>
    <script>
        setTimeout(function() {{ window.location.href = '{appLink}'; }}, 500);
    </script>
</body></html>";

            return Content(html, "text/html");
        }
    }

}
