using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Domain.Entities;

public sealed class DoctorClinic
{
    public int Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? Address { get; set; }

    public UserTypeEnum UserType { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool IsDeleted { get; set; }

    public ICollection<ApplicationUser> Users { get; set; } = [];

    public ICollection<Case> Cases { get; set; } = [];
}
