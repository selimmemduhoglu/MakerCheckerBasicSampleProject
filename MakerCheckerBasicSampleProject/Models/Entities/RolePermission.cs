using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MakerCheckerBasicSampleProject.Models.Entities;

public class RolePermission
{
    [Required]
    public string RoleId { get; set; }

    [ForeignKey("RoleId")]
    public IdentityRole Role { get; set; }

    [Required]
    public int PermissionId { get; set; }

    [ForeignKey("PermissionId")]
    public Permission Permission { get; set; }
} 