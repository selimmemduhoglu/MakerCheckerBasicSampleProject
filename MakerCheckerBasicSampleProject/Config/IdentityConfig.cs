using MakerCheckerBasicSampleProject.Configuration.Ef;
using MakerCheckerBasicSampleProject.Models;
using MakerCheckerBasicSampleProject.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using static MakerCheckerBasicSampleProject.Models.ApplicationUser;

namespace MakerCheckerBasicSampleProject.Config.Configuration
{
	public static class IdentityConfig
	{
		public static IServiceCollection AddCustomIdentity(this IServiceCollection services)
		{
			// Configure ASP.NET Core Identity
			services.AddIdentity<ApplicationUser, IdentityRole>(options =>
			{
				// Password settings
				options.Password.RequireDigit = true;
				options.Password.RequireLowercase = true;
				options.Password.RequireNonAlphanumeric = true;
				options.Password.RequireUppercase = true;
				options.Password.RequiredLength = 8;
				options.Password.RequiredUniqueChars = 1;

				// Lockout settings
				options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.AllowedForNewUsers = true;

				// User settings
				options.User.RequireUniqueEmail = true;
				options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

				// SignIn settings
				options.SignIn.RequireConfirmedAccount = true;
				options.SignIn.RequireConfirmedEmail = true;
			})
			.AddEntityFrameworkStores<ApplicationDbContext>()
			.AddDefaultTokenProviders();

			// Configure cookie policy
			services.ConfigureApplicationCookie(options =>
			{
				options.Cookie.HttpOnly = true;
				options.ExpireTimeSpan = TimeSpan.FromHours(8);
				options.LoginPath = "/Identity/Account/Login";
				options.LogoutPath = "/Identity/Account/Logout";
				options.AccessDeniedPath = "/Identity/Account/AccessDenied";
				options.SlidingExpiration = true;

				// Add cookie security
				options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
				options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
			});

			// Add custom services
			services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, CustomClaimsFactory>();
			services.AddScoped<UserManager<ApplicationUser>>();
			services.AddScoped<SignInManager<ApplicationUser>>();
			services.AddScoped<RoleManager<IdentityRole>>();

			// Add permission service
			services.AddScoped<IPermissionService, PermissionService>();

			// Add user activity logging service
			services.AddScoped<IUserActivityService, UserActivityService>();

			return services;
		}
	}

	// Custom claims factory to add additional claims for the user
	public class CustomClaimsFactory : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IPermissionService _permissionService;

		public CustomClaimsFactory(
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager,
			IOptions<IdentityOptions> optionsAccessor,
			IPermissionService permissionService)
			: base(userManager, roleManager, optionsAccessor)
		{
			_userManager = userManager;
			_permissionService = permissionService;
		}

		protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
		{
			var identity = await base.GenerateClaimsAsync(user);

			// Add full name claim
			identity.AddClaim(new Claim("FullName", user.FullName ?? string.Empty));

			// Add department claim
			if (!string.IsNullOrEmpty(user.Department))
			{
				identity.AddClaim(new Claim("Department", user.Department));
			}

			// Add position claim
			if (!string.IsNullOrEmpty(user.Position))
			{
				identity.AddClaim(new Claim("Position", user.Position));
			}

			// Add user permissions as claims
			var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);
			foreach (var permission in permissions)
			{
				identity.AddClaim(new Claim("Permission", permission.Name));
			}

			return identity;
		}
	}

	// Permission service interface
	public interface IPermissionService
	{
		Task<List<Permission>> GetUserPermissionsAsync(string userId);
		Task<bool> HasPermissionAsync(string userId, string permissionName);
		Task<List<Permission>> GetRolePermissionsAsync(string roleId);
		Task AddPermissionToRoleAsync(string roleId, int permissionId);
		Task RemovePermissionFromRoleAsync(string roleId, int permissionId);
	}

	// Permission service implementation
	public class PermissionService : IPermissionService
	{
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public PermissionService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		public async Task<List<Permission>> GetUserPermissionsAsync(string userId)
		{
			// Get user roles
			var user = await _userManager.FindByIdAsync(userId);
			if (user == null)
			{
				return new List<Permission>();
			}

			var userRoles = await _userManager.GetRolesAsync(user);
			if (userRoles == null || !userRoles.Any())
			{
				return new List<Permission>();
			}

			// Get role IDs
			var roleIds = _context.Roles
				.Where(r => userRoles.Contains(r.Name))
				.Select(r => r.Id)
				.ToList();

			// Get permissions for these roles
			var permissions = await _context.RolePermissions
				.Where(rp => roleIds.Contains(rp.RoleId))
				.Select(rp => rp.Permission)
				.Distinct()
				.ToListAsync();

			return permissions;
		}

		public async Task<bool> HasPermissionAsync(string userId, string permissionName)
		{
			var permissions = await GetUserPermissionsAsync(userId);
			return permissions.Any(p => p.Name == permissionName);
		}

		public async Task<List<Permission>> GetRolePermissionsAsync(string roleId)
		{
			var permissions = await _context.RolePermissions
				.Where(rp => rp.RoleId == roleId)
				.Select(rp => rp.Permission)
				.ToListAsync();

			return permissions;
		}

		public async Task AddPermissionToRoleAsync(string roleId, int permissionId)
		{
			// Check if already exists
			var exists = await _context.RolePermissions
				.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

			if (!exists)
			{
				_context.RolePermissions.Add(new RolePermission
				{
					RoleId = roleId,
					PermissionId = permissionId
				});

				await _context.SaveChangesAsync();
			}
		}

		public async Task RemovePermissionFromRoleAsync(string roleId, int permissionId)
		{
			var rolePermission = await _context.RolePermissions
				.FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

			if (rolePermission != null)
			{
				_context.RolePermissions.Remove(rolePermission);
				await _context.SaveChangesAsync();
			}
		}
	}

	// User activity service interface
	public interface IUserActivityService
	{
		Task LogActivityAsync(string userId, string activity, string module, string ipAddress,
			string userAgent, bool isSuccessful, string details = null);
		Task<List<UserActivity>> GetUserActivitiesAsync(string userId, int page = 1, int pageSize = 10);
	}

	// User activity service implementation
	public class UserActivityService : IUserActivityService
	{
		private readonly ApplicationDbContext _context;

		public UserActivityService(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task LogActivityAsync(string userId, string activity, string module, string ipAddress,
			string userAgent, bool isSuccessful, string details = null)
		{
			var userActivity = new UserActivity
			{
				UserId = userId,
				Activity = activity,
				Module = module,
				Timestamp = DateTime.Now,
				IpAddress = ipAddress,
				UserAgent = userAgent,
				IsSuccessful = isSuccessful,
				Details = details
			};

			_context.UserActivities.Add(userActivity);
			await _context.SaveChangesAsync();
		}

		public async Task<List<UserActivity>> GetUserActivitiesAsync(string userId, int page = 1, int pageSize = 10)
		{
			return await _context.UserActivities
				.Where(ua => ua.UserId == userId)
				.OrderByDescending(ua => ua.Timestamp)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();
		}
	}

	// Permission requirement for authorization
	public class PermissionRequirement : IAuthorizationRequirement
	{
		public string Permission { get; }

		public PermissionRequirement(string permission)
		{
			Permission = permission;
		}
	}

	// Permission authorization handler
	public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
	{
		private readonly IPermissionService _permissionService;
		private readonly UserManager<ApplicationUser> _userManager;

		public PermissionAuthorizationHandler(IPermissionService permissionService, UserManager<ApplicationUser> userManager)
		{
			_permissionService = permissionService;
			_userManager = userManager;
		}

		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
		{
			if (context.User == null || !context.User.Identity.IsAuthenticated)
			{
				return;
			}

			var userId = _userManager.GetUserId(context.User);
			if (userId == null)
			{
				return;
			}

			// Check if the user has the required permission
			if (await _permissionService.HasPermissionAsync(userId, requirement.Permission))
			{
				context.Succeed(requirement);
			}
		}
	}

	// Authorization policy provider for permissions
	public class PermissionPolicyProvider : IAuthorizationPolicyProvider
	{
		private const string POLICY_PREFIX = "Permission";
		private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

		public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
		{
			_fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
		}

		public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

		public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();

		public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
		{
			if (policyName.StartsWith($"{POLICY_PREFIX}:", StringComparison.OrdinalIgnoreCase))
			{
				var permission = policyName.Substring(POLICY_PREFIX.Length + 1);
				var policy = new AuthorizationPolicyBuilder();
				policy.AddRequirements(new PermissionRequirement(permission));
				return Task.FromResult(policy.Build());
			}

			return _fallbackPolicyProvider.GetPolicyAsync(policyName);
		}
	}
}