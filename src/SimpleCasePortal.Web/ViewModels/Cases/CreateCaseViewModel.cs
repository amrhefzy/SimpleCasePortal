using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SimpleCasePortal.Web.ViewModels.Cases;

public sealed class CreateCaseViewModel
{
    [Display(Name = "Doctor/Clinic")]
    public int? DoctorClinicId { get; set; }

    [Required]
    [StringLength(250)]
    [Display(Name = "Patient name")]
    public string PatientName { get; set; } = string.Empty;

    [Range(0, 130)]
    public int? Age { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date of birth")]
    public DateTime? DateOfBirth { get; set; }

    [StringLength(20)]
    public string? Gender { get; set; }

    [StringLength(4000)]
    public string? Notes { get; set; }

    public bool CanSelectDoctorClinic { get; set; }

    public IReadOnlyCollection<SelectListItem> DoctorClinicOptions { get; set; } = [];
}
