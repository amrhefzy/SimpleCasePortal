namespace SimpleCasePortal.Domain.Entities;

public sealed class ApplicationUser
{
    public string Id { get; set; } = default!;

    public string UserName { get; set; } = default!;

    public string FullName { get; set; } = default!;

    public string Email { get; set; } = default!;

    public string PasswordHash { get; set; } = default!;

    public int? DoctorClinicId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedOn { get; set; }

    public DateTime? UpdatedOn { get; set; }

    public bool IsDeleted { get; set; }

    public DoctorClinic? DoctorClinic { get; set; }

    public ICollection<UserRole> UserRoles { get; set; } = [];
}
