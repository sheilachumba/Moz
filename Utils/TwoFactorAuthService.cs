using Microsoft.Extensions.Configuration;

namespace MOZ_UPGRADE.Utils
{
    public interface ITwoFactorAuthService
    {
        string GenerateCode();
        bool VerifyCode(string storedCode, DateTime expiryTime, string providedCode);
        DateTime GetExpiryTime();
    }

    public class TwoFactorAuthService : ITwoFactorAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly Random _random = new Random();

        public TwoFactorAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateCode()
        {
            var codeLength = _configuration.GetValue<int>("TwoFactorAuth:CodeLength", 6);
            var code = "";
            for (int i = 0; i < codeLength; i++)
            {
                code += _random.Next(0, 10).ToString();
            }
            return code;
        }

        public bool VerifyCode(string storedCode, DateTime expiryTime, string providedCode)
        {
            // Check if code has expired
            if (DateTime.UtcNow > expiryTime)
            {
                return false;
            }

            // Check if codes match
            return storedCode == providedCode;
        }

        public DateTime GetExpiryTime()
        {
            var codeExpiryMinutes = _configuration.GetValue<int>("TwoFactorAuth:CodeExpiryMinutes", 10);
            return DateTime.UtcNow.AddMinutes(codeExpiryMinutes);
        }
    }
}
