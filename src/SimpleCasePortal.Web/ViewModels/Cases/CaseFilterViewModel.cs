using System.ComponentModel.DataAnnotations;
using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Web.ViewModels.Cases;

public sealed class CaseFilterViewModel
{
    [Display(Name = "Case number")]
    public string? CaseNumber { get; set; }

    [Display(Name = "Patient name")]
    public string? PatientName { get; set; }

    public CaseStatusEnum? Status { get; set; }

    [Display(Name = "Doctor/Clinic")]
    public int? DoctorClinicId { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Created from")]
    public DateTime? CreatedFrom { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Created to")]
    public DateTime? CreatedTo { get; set; }
}
