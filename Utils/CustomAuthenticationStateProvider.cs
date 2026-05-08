using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;
using System.Text;

namespace MOZ_UPGRADE.Utils
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CustomAuthenticationStateProvider(IHttpContextAccessor HttpContextAccessor)
        {
            _httpContextAccessor = HttpContextAccessor;

        }
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                string Token = _httpContextAccessor.HttpContext?.Session?.GetString("JWToken");
                
                System.Diagnostics.Debug.WriteLine($"[Auth] Token from session: {(Token != null ? "Found" : "NOT FOUND")}");
                System.Diagnostics.Debug.WriteLine($"[Auth] Session ID: {_httpContextAccessor.HttpContext?.Session?.Id}");

                if (!string.IsNullOrEmpty(Token))
                {
                    var claims = ParseClaimsFromJwt(Token);
                    System.Diagnostics.Debug.WriteLine($"[Auth] Claims parsed: {claims.Count()} claims");
                    foreach (var claim in claims)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Auth] Claim: {claim.Type} = {claim.Value}");
                    }
                    
                    var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
                    var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
                    return authState;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[Auth] No token found - returning anonymous");
                    return Task.FromResult(new AuthenticationState(_anonymous));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Auth] Exception in GetAuthenticationStateAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[Auth] Stack trace: {ex.StackTrace}");
                return Task.FromResult(new AuthenticationState(_anonymous));
            }
        }
        public async Task MarkUserAsAuthenticated(string token)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }
        public async Task MarkUserAsLoggedOut()
        {
            _httpContextAccessor.HttpContext.Session.Clear();
            var authState = Task.FromResult(new AuthenticationState(_anonymous));
            NotifyAuthenticationStateChanged(authState);

        }
        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            try
            {
                System.Diagnostics.Debug.WriteLine($"[Auth] Parsing JWT token. Length: {jwt.Length}");
                
                var parts = jwt.Split('.');
                System.Diagnostics.Debug.WriteLine($"[Auth] JWT parts: {parts.Length}");
                
                if (parts.Length < 2)
                {
                    System.Diagnostics.Debug.WriteLine($"[Auth] ERROR: Invalid JWT format - not enough parts");
                    return claims;
                }

                var payload = parts[1];
                System.Diagnostics.Debug.WriteLine($"[Auth] Payload length: {payload.Length}");
                
                byte[] jsonBytes = ParseBase64WithoutPadding(payload);
                string decodedString = Encoding.UTF8.GetString(jsonBytes);
                
                System.Diagnostics.Debug.WriteLine($"[Auth] Decoded payload: {decodedString.Substring(0, Math.Min(100, decodedString.Length))}...");
                
                var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(decodedString);
                System.Diagnostics.Debug.WriteLine($"[Auth] Deserialized {keyValuePairs.Count} key-value pairs");
                
                keyValuePairs.TryGetValue("name", out object UserId);
                keyValuePairs.TryGetValue("given_name", out object GivenName);
                keyValuePairs.TryGetValue("email", out object Email);

                System.Diagnostics.Debug.WriteLine($"[Auth] UserId: {UserId}, GivenName: {GivenName}, Email: {Email}");

                if (UserId != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, UserId.ToString()));
                    claims.Add(new Claim(ClaimTypes.GivenName, GivenName?.ToString() ?? ""));
                    claims.Add(new Claim(ClaimTypes.Email, Email?.ToString() ?? ""));
                    System.Diagnostics.Debug.WriteLine($"[Auth] Added {claims.Count} claims");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[Auth] WARNING: UserId is null, no claims added");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Auth] Exception parsing JWT: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[Auth] Stack trace: {ex.StackTrace}");
                throw ex;
            }
            return claims;
        }
        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }

    }
}
