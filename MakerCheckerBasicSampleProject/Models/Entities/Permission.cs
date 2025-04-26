using System.ComponentModel.DataAnnotations;

namespace MakerCheckerBasicSampleProject.Models.Entities;

public class Permission
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    [MaxLength(255)]
    public string Description { get; set; }
    
    [MaxLength(50)]
    public string Module { get; set; }

    // Many-to-many relationship with roles
    public ICollection<RolePermission> RolePermissions { get; set; }
} 