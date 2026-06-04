using Microsoft.AspNetCore.Mvc.Rendering;
using SimpleCasePortal.Application.DTOs.Cases;

namespace SimpleCasePortal.Web.ViewModels.Cases;

public sealed class CaseListViewModel
{
    public CaseFilterViewModel Filter { get; set; } = new();

    public IReadOnlyCollection<CaseDto> Cases { get; set; } = [];

    public IReadOnlyCollection<SelectListItem> DoctorClinicOptions { get; set; } = [];

    public bool CanCreate { get; set; }

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }

    public bool CanFilterByDoctorClinic { get; set; }
}
