using System.Security.Claims;

namespace MOZ_UPGRADE.Utils
{
    public interface IGet_LoggedIn_User
    {
        Task<string> UserId();
        Task<string> UserAccountType();
        Task<string> UserFullName();
        Task<string> UserEmail();
        Task LogOut();
    }
    public class GetLoggedinUser : IGet_LoggedIn_User
    {
        private readonly CustomAuthenticationStateProvider _customAuthenticationStateProvider;

        public GetLoggedinUser(CustomAuthenticationStateProvider CustomAuthenticationStateProvider)
        {
            _customAuthenticationStateProvider = CustomAuthenticationStateProvider;
        }
        public async Task<string> UserId()
        {
            string id = "";
            var authState = await _customAuthenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                var user = authState.User;
                id = user.FindFirst(ClaimTypes.Name)?.Value.Split(',')[0];
            }
            return id;
        }
        public async Task<string> UserAccountType()
        {
            string id = "";
            var authState = await _customAuthenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                var user = authState.User;
                id = user.FindFirst(ClaimTypes.Name)?.Value.Split(',')[1];
            }
            return id;
        }
        public async Task<string> UserFullName()
        {
            string id = "n/a";
            var authState = await _customAuthenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                var user = authState.User;
                id = user.FindFirst(ClaimTypes.GivenName)?.Value;
            }
            return id;
        }
        public async Task<string> UserEmail()
        {
            string id = "n/a";
            var authState = await _customAuthenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                var user = authState.User;
                id = user.FindFirst(ClaimTypes.Email)?.Value;
            }
            return id;
        }

        public async Task LogOut()
        {
            await _customAuthenticationStateProvider.MarkUserAsLoggedOut();

        }
    }

}
