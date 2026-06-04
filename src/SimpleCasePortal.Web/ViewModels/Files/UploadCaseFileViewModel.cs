using System.ComponentModel.DataAnnotations;
using SimpleCasePortal.Domain.Enums;

namespace SimpleCasePortal.Web.ViewModels.Files;

public sealed class UploadCaseFileViewModel
{
    public int CaseId { get; set; }

    [Required]
    [Display(Name = "File type")]
    public FileTypeEnum FileType { get; set; } = FileTypeEnum.UpperSTL;

    [Required]
    [Display(Name = "File")]
    public IFormFile? File { get; set; }
}
