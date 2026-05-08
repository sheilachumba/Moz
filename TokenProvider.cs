using Microsoft.IdentityModel.Tokens;
using MOZ_UPGRADE.Interfaces;
using MOZ_UPGRADE.Utils;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MOZ_UPGRADE.Models;

namespace MOZ_UPGRADE
{
    public class TokenProvider
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TokenProvider(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<EmailSendStatus> LoginUser(string strUserName, string password)
        {
            EmailSendStatus statusmessage = new EmailSendStatus();
            try
            {
                Encryption encryption = new Encryption();

                string pass = encryption.Encryptstring(password);

                Users user = await GetUserDetails(strUserName, pass);

                if (user != null)
                {
                    //if (user.Active_Status)
                    //{

                        var token = GenerateToken(user);

                        // Store token in session for authentication state provider
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"[TokenProvider] Setting token in session. Token length: {token.Length}");
                            System.Diagnostics.Debug.WriteLine($"[TokenProvider] Session ID before: {_httpContextAccessor.HttpContext?.Session?.Id}");
                            
                            _httpContextAccessor.HttpContext?.Session?.SetString("JWToken", token);
                            
                            // IMPORTANT: Commit the session to ensure it's saved
                            _httpContextAccessor.HttpContext?.Session?.CommitAsync().Wait();
                            
                            System.Diagnostics.Debug.WriteLine($"[TokenProvider] Token set and session committed");
                            System.Diagnostics.Debug.WriteLine($"[TokenProvider] Session ID after: {_httpContextAccessor.HttpContext?.Session?.Id}");
                            
                            // Verify it was set
                            var verify = _httpContextAccessor.HttpContext?.Session?.GetString("JWToken");
                            System.Diagnostics.Debug.WriteLine($"[TokenProvider] Verification - Token in session: {(verify != null ? "YES (" + verify.Length + " chars)" : "NO")}");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[TokenProvider] ERROR: Could not set session token: {ex.Message}");
                            System.Diagnostics.Debug.WriteLine($"[TokenProvider] Stack trace: {ex.StackTrace}");
                        }

                        statusmessage.status = true;
                        statusmessage.message = token; // Return the token itself
                    //}
                    //else
                    //{
                    //    statusmessage.status = false;
                    //    statusmessage.message = "In-active Account,Please contact the syste Admin";

                    //}
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

        public async Task<Users> GetUserDetails(string strUserName, string password)
        {
            return await _unitOfWork.userRepository.Login(strUserName, password);

        }
        public string GenerateToken(Users user)
        {
            var key = Encoding.ASCII.GetBytes("YourKey-2374-OFFKDI940NG7:56753253-tyuw-5769-0921-kfirox29zoxv");
            string _issuer = "MyIssuer";
            string _audience = "MyAudience";

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                new Claim(ClaimTypes.Name, $"{user.strid.ToString()},{user.AccountType.ToString()},{user.IsTwoFAEnabled}"),
                new Claim(ClaimTypes.GivenName, $"{user.FullName}"),
                new Claim(ClaimTypes.Email, $"{user.Email}"),
                new Claim("AccountType", user.AccountType.ToString()),
               // new Claim(ClaimTypes.Role, ""),
                 
        }),
                Expires = DateTime.UtcNow.AddMinutes(60),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
