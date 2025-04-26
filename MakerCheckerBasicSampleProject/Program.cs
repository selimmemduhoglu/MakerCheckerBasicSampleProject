using MakerCheckerBasicSampleProject.Config.Configuration;
using MakerCheckerBasicSampleProject.Configuration.Ef;
using MakerCheckerBasicSampleProject.Models;
using MakerCheckerBasicSampleProject.Services;
using MakerCheckerBasicSampleProject.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(
		builder.Configuration.GetConnectionString("DefaultConnection")));

// Add custom identity 
builder.Services.AddCustomIdentity();

// Add authorization policy provider for permissions
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

// Add services
builder.Services.AddScoped<TransactionService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<IUserActivityService, UserActivityService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Add controllers and views
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add SignalR
builder.Services.AddSignalR();

// Add session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
	//app.UseMigrationsEndPoint();
}
else
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

// Map SignalR hub
app.MapHub<NotificationHub>("/notificationHub");

// Map controllers
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seed the database if needed
using (var scope = app.Services.CreateScope())
{
	var services = scope.ServiceProvider;
	try
	{
		var context = services.GetRequiredService<ApplicationDbContext>();
		var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
		var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

		// Ensure database is created and migrations are applied
		context.Database.Migrate();

		// Seed initial data if needed
		SeedData.Initialize(services).Wait();
	}
	catch (Exception ex)
	{
		var logger = services.GetRequiredService<ILogger<Program>>();
		logger.LogError(ex, "An error occurred while seeding the database.");
	}
}

app.Run();

// SeedData class for initializing database with demo data
public static class SeedData
{
	public static async Task Initialize(IServiceProvider serviceProvider)
	{
		var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
		var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

		// Create default roles if they don't exist
		string[] roleNames = { "Admin", "Maker", "Checker", "SupervisorChecker" };
		foreach (var roleName in roleNames)
		{
			if (!await roleManager.RoleExistsAsync(roleName))
			{
				await roleManager.CreateAsync(new IdentityRole(roleName));
			}
		}

		// Create default admin user
		var adminEmail = "admin@example.com";
		var adminUser = await userManager.FindByEmailAsync(adminEmail);
		if (adminUser == null)
		{
			adminUser = new ApplicationUser
			{
				UserName = adminEmail,
				Email = adminEmail,
				EmailConfirmed = true,
				FullName = "System Administrator",
				Department = "IT",
				Position = "Administrator",
				CreatedAt = DateTime.Now,
				IsActive = true
			};

			var result = await userManager.CreateAsync(adminUser, "Admin123!");
			if (result.Succeeded)
			{
				await userManager.AddToRoleAsync(adminUser, "Admin");
			}
		}

		// Create default maker user
		var makerEmail = "maker@example.com";
		var makerUser = await userManager.FindByEmailAsync(makerEmail);
		if (makerUser == null)
		{
			makerUser = new ApplicationUser
			{
				UserName = makerEmail,
				Email = makerEmail,
				EmailConfirmed = true,
				FullName = "John Maker",
				Department = "Finance",
				Position = "Financial Analyst",
				CreatedAt = DateTime.Now,
				IsActive = true
			};

			var result = await userManager.CreateAsync(makerUser, "Maker123!");
			if (result.Succeeded)
			{
				await userManager.AddToRoleAsync(makerUser, "Maker");
			}
		}

		// Create default checker user
		var checkerEmail = "checker@example.com";
		var checkerUser = await userManager.FindByEmailAsync(checkerEmail);
		if (checkerUser == null)
		{
			checkerUser = new ApplicationUser
			{
				UserName = checkerEmail,
				Email = checkerEmail,
				EmailConfirmed = true,
				FullName = "Sarah Checker",
				Department = "Finance",
				Position = "Team Lead",
				CreatedAt = DateTime.Now,
				IsActive = true
			};

			var result = await userManager.CreateAsync(checkerUser, "Checker123!");
			if (result.Succeeded)
			{
				await userManager.AddToRoleAsync(checkerUser, "Checker");
			}
		}

		// Create default supervisor checker user
		var supervisorEmail = "supervisor@example.com";
		var supervisorUser = await userManager.FindByEmailAsync(supervisorEmail);
		if (supervisorUser == null)
		{
			supervisorUser = new ApplicationUser
			{
				UserName = supervisorEmail,
				Email = supervisorEmail,
				EmailConfirmed = true,
				FullName = "Mark Supervisor",
				Department = "Finance",
				Position = "Department Manager",
				CreatedAt = DateTime.Now,
				IsActive = true
			};

			var result = await userManager.CreateAsync(supervisorUser, "Supervisor123!");
			if (result.Succeeded)
			{
				await userManager.AddToRoleAsync(supervisorUser, "SupervisorChecker");
			}
		}
	}
}