using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Web.ViewModels.Cases;

public sealed class EditCaseViewModel
{
    public int Id { get; set; }

    [Display(Name = "Case number")]
    public string CaseNumber { get; set; } = default!;

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

    public CaseStatusEnum Status { get; set; }

    public bool CanSelectDoctorClinic { get; set; }

    public bool CanChangeStatus { get; set; }

    public IReadOnlyCollection<SelectListItem> DoctorClinicOptions { get; set; } = [];
}
