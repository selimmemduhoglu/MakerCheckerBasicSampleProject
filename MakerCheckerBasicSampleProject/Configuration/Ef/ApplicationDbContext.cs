using MakerCheckerBasicSampleProject.Models;
using MakerCheckerBasicSampleProject.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security;

namespace MakerCheckerBasicSampleProject.Configuration.Ef;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
	{
	}

	public DbSet<Transaction> Transactions { get; set; }
	public DbSet<TransactionLog> TransactionLogs { get; set; }
	public DbSet<ApprovalWorkflow> ApprovalWorkflows { get; set; }
	public DbSet<ApprovalLevel> ApprovalLevels { get; set; }
	public DbSet<TransactionApproval> TransactionApprovals { get; set; }
	public DbSet<Permission> Permissions { get; set; }
	public DbSet<RolePermission> RolePermissions { get; set; }
	public DbSet<UserSetting> UserSettings { get; set; }
	public DbSet<UserActivity> UserActivities { get; set; }
	public DbSet<Notification> Notifications { get; set; }



	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		// Configure composite key for RolePermission
		builder.Entity<RolePermission>()
			.HasKey(rp => new { rp.RoleId, rp.PermissionId });

		// Configure relationships
		builder.Entity<RolePermission>()
			.HasOne(rp => rp.Role)
			.WithMany()
			.HasForeignKey(rp => rp.RoleId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Entity<RolePermission>()
			.HasOne(rp => rp.Permission)
			.WithMany(p => p.RolePermissions)
			.HasForeignKey(rp => rp.PermissionId)
			.OnDelete(DeleteBehavior.Cascade);

		// Configure one-to-many relationships for User and Transactions
		builder.Entity<Transaction>()
			.HasOne(t => t.CreatedBy)
			.WithMany(u => u.CreatedTransactions)
			.HasForeignKey(t => t.CreatedById)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<Transaction>()
			.HasOne(t => t.ApprovedBy)
			.WithMany(u => u.ApprovedTransactions)
			.HasForeignKey(t => t.ApprovedById)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Entity<Transaction>()
			.HasOne(t => t.RejectedBy)
			.WithMany(u => u.RejectedTransactions)
			.HasForeignKey(t => t.RejectedById)
			.OnDelete(DeleteBehavior.Restrict);

		// Configure one-to-many relationship for Transaction and Logs
		builder.Entity<TransactionLog>()
			.HasOne(l => l.Transaction)
			.WithMany(t => t.Logs)
			.HasForeignKey(l => l.TransactionId)
			.OnDelete(DeleteBehavior.Cascade);

		// Configure one-to-many relationship for Transaction and Approvals
		builder.Entity<TransactionApproval>()
			.HasOne(a => a.Transaction)
			.WithMany(t => t.Approvals)
			.HasForeignKey(a => a.TransactionId)
			.OnDelete(DeleteBehavior.Cascade);

		// Seed initial data
		SeedInitialData(builder);
	}


	private void SeedInitialData(ModelBuilder builder)
	{
		// Seed Roles
		builder.Entity<IdentityRole>().HasData(
			new IdentityRole { Id = "1", Name = "Admin", NormalizedName = "ADMIN" },
			new IdentityRole { Id = "2", Name = "Maker", NormalizedName = "MAKER" },
			new IdentityRole { Id = "3", Name = "Checker", NormalizedName = "CHECKER" },
			new IdentityRole { Id = "4", Name = "SupervisorChecker", NormalizedName = "SUPERVISORCHECKER" }
		);

		// Seed Permissions
		builder.Entity<Permission>().HasData(
			new Permission { Id = 1, Name = "Create_Transaction", Description = "Create a new transaction", Module = "Transactions" },
			new Permission { Id = 2, Name = "Approve_Transaction", Description = "Approve a transaction", Module = "Transactions" },
			new Permission { Id = 3, Name = "Reject_Transaction", Description = "Reject a transaction", Module = "Transactions" },
			new Permission { Id = 4, Name = "View_All_Transactions", Description = "View all transactions", Module = "Transactions" },
			new Permission { Id = 5, Name = "Manage_Users", Description = "Manage users", Module = "Administration" },
			new Permission { Id = 6, Name = "Manage_Roles", Description = "Manage roles", Module = "Administration" },
			new Permission { Id = 7, Name = "Manage_Workflows", Description = "Manage approval workflows", Module = "Administration" }
		);

		// Seed Role-Permission mappings
		builder.Entity<RolePermission>().HasData(
			// Admin permissions
			new RolePermission { RoleId = "1", PermissionId = 1 },
			new RolePermission { RoleId = "1", PermissionId = 2 },
			new RolePermission { RoleId = "1", PermissionId = 3 },
			new RolePermission { RoleId = "1", PermissionId = 4 },
			new RolePermission { RoleId = "1", PermissionId = 5 },
			new RolePermission { RoleId = "1", PermissionId = 6 },
			new RolePermission { RoleId = "1", PermissionId = 7 },

			// Maker permissions
			new RolePermission { RoleId = "2", PermissionId = 1 },
			new RolePermission { RoleId = "2", PermissionId = 4 },

			// Checker permissions
			new RolePermission { RoleId = "3", PermissionId = 2 },
			new RolePermission { RoleId = "3", PermissionId = 3 },
			new RolePermission { RoleId = "3", PermissionId = 4 },

			// Supervisor Checker permissions
			new RolePermission { RoleId = "4", PermissionId = 2 },
			new RolePermission { RoleId = "4", PermissionId = 3 },
			new RolePermission { RoleId = "4", PermissionId = 4 }
		);

		// Seed Approval Workflows
		builder.Entity<ApprovalWorkflow>().HasData(
			new ApprovalWorkflow
			{
				Id = 1,
				Name = "Standard Approval",
				RequiredApprovals = 1,
				Description = "Single level approval by a Checker",
				IsActive = true
			},
			new ApprovalWorkflow
			{
				Id = 2,
				Name = "Multi-level Approval",
				RequiredApprovals = 2,
				Description = "Two level approval process - first by Checker, then by Supervisor",
				IsActive = true
			}
		);

		// Seed Approval Levels
		builder.Entity<ApprovalLevel>().HasData(
			// Standard workflow - single level
			new ApprovalLevel
			{
				Id = 1,
				WorkflowId = 1,
				Level = 1,
				Name = "Checker Approval",
				RoleId = "3" // Checker role
			},

			// Multi-level workflow - first level
			new ApprovalLevel
			{
				Id = 2,
				WorkflowId = 2,
				Level = 1,
				Name = "Initial Approval",
				RoleId = "3" // Checker role
			},

			// Multi-level workflow - second level
			new ApprovalLevel
			{
				Id = 3,
				WorkflowId = 2,
				Level = 2,
				Name = "Supervisor Approval",
				RoleId = "4" // SupervisorChecker role
			}
		);
	}
}
