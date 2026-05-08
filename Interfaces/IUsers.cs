using Microsoft.EntityFrameworkCore;
using MOZ_UPGRADE.Context;
using MOZ_UPGRADE.Models;
using MOZ_UPGRADE.Utils;
using System.Security.Claims;

namespace MOZ_UPGRADE.Interfaces
{
    public interface IUserRepository : IGenericRepository<Users>
    {
        Task<Users> GetByEmail(string email);
        Task<Users> Login(string Username, string Password);
        Task<Users> FindUserwithMail(string Email);
        Task<bool> HasPermission(string roleName);
        Task<string> Get_UserId();
    }


    public class UserRepository : GenericRepo<Users>, IUserRepository
    {
        private readonly CustomAuthenticationStateProvider _customAuthenticationStateProvider;

        private readonly IServiceScopeFactory _scopeFactory;
        public UserRepository(IServiceScopeFactory scopeFactory, CustomAuthenticationStateProvider customAuthenticationStateProvider)
     : base(scopeFactory)
        {
            _scopeFactory = scopeFactory;
            _customAuthenticationStateProvider = customAuthenticationStateProvider ?? throw new ArgumentNullException(nameof(customAuthenticationStateProvider));
        }


        public async Task<Users> FindUserwithMail(string Email)
        {

            using var scope = _scopeFactory.CreateScope();
            var _appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return await _appDbContext.Users.FirstOrDefaultAsync(x => x.Email == Email);
        }

        public async Task<Users> GetByEmail(string email)
        {
            using var scope = _scopeFactory.CreateScope();
            var _appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return await _appDbContext.Users.FirstOrDefaultAsync(k => k.Email == email);
        }



        public async Task<Users> Login(string Username, string Password)
        {
            using var scope = _scopeFactory.CreateScope();
            var _appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            return await _appDbContext.Users.FirstOrDefaultAsync(x => x.Email == Username && x.password_hash == Password);

        }
        public async Task<string> Get_UserId()
        {
            string id = "";
            var authState = await _customAuthenticationStateProvider.GetAuthenticationStateAsync();
            if (authState.User.Identity.IsAuthenticated)
            {
                var user = authState.User;
                id = user.FindFirst(ClaimTypes.Name)?.Value;
            }
            return id;
        }

        //public async Task<string> Get_UserId()
        //{
        //    if (_customAuthenticationStateProvider == null)
        //    {
        //        throw new InvalidOperationException("Authentication provider is not available.");
        //    }

        //    var authState = await _customAuthenticationStateProvider.GetAuthenticationStateAsync();
        //    if (authState == null)
        //    {
        //        throw new InvalidOperationException("Authentication state is null.");
        //    }

        //    if (!authState.User.Identity?.IsAuthenticated ?? true)
        //    {
        //        throw new UnauthorizedAccessException("User is not authenticated.");
        //    }

        //    var idClaim = authState.User.FindFirst(ClaimTypes.NameIdentifier);
        //    if (idClaim == null)
        //    {
        //        throw new InvalidOperationException("Claim 'NameIdentifier' is not available.");
        //    }

        //    return idClaim.Value;
        //}

        public async Task<bool> HasPermission(string roleName)
        {
            try
            {
                //using var scope = _scopeFactory.CreateScope();
                //var _appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                //List<string> permissions = new List<string>();
                //List<Permissions> permissionsList = await _appDbContext.Permissions.ToListAsync();
                //List<Roles_Permissions> rolesPermissions = await _appDbContext.Roles_Permissions.ToListAsync();

                //permissions.Clear();
                //var userId = await Get_UserId();
                //var userRoles = await _appDbContext.User_Roles.Where(k => k.UserId == userId).ToListAsync();

                //foreach (var role in userRoles)
                //{
                //    var userPermissions = rolesPermissions.Where(k => k.RoleId == role.RoleId).ToList();
                //    if (userPermissions != null)
                //    {
                //        foreach (var permission in userPermissions)
                //        {
                //            var permissionEntity = permissionsList.FirstOrDefault(k => k.strid == permission.PermissionId);
                //            if (permissionEntity != null && !permissions.Contains(permissionEntity.Permission))
                //            {
                //                permissions.Add(permissionEntity.Permission);
                //            }
                //        }
                //    }
                //}

                //return permissions.Contains(roleName);
                return true;
            }
            catch (Exception ex)
            {
                // Log exception (e.g., with ILogger)
                return false;
            }
        }

    }

}
