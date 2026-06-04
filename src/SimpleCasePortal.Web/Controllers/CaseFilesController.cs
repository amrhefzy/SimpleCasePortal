using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SimpleCasePortal.Application.DTOs.Files;
using SimpleCasePortal.Application.Interfaces;
using SimpleCasePortal.Web.ViewModels.Files;

namespace SimpleCasePortal.Web.Controllers;

[Authorize]
public sealed class CaseFilesController : Controller
{
    private readonly ICaseFileService _caseFileService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public CaseFilesController(
        ICaseFileService caseFileService,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService)
    {
        _caseFileService = caseFileService;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    [Authorize(Policy = "Permission:Files.Upload")]
    [HttpPost]
    public async Task<IActionResult> Upload(UploadCaseFileViewModel model, CancellationToken cancellationToken)
    {
        if (model.File is null)
        {
            TempData["FileError"] = "File is required.";
            return RedirectToAction("Details", "Cases", new { id = model.CaseId });
        }

        await using var stream = model.File.OpenReadStream();
        var response = await _caseFileService.UploadCaseFileAsync(model.CaseId, new UploadCaseFileDto
        {
            FileType = model.FileType,
            OriginalFileName = model.File.FileName,
            ContentType = model.File.ContentType,
            FileSizeBytes = model.File.Length,
            Content = stream,
            UploadedByUserId = RequireUserId()
        }, cancellationToken);

        if (!response.Success)
        {
            TempData["FileError"] = string.Join(" ", response.Errors.Count > 0 ? response.Errors : [response.Message]);
        }
        else
        {
            TempData["StatusMessage"] = "File uploaded successfully.";
        }

        return RedirectToAction("Details", "Cases", new { id = model.CaseId });
    }

    [Authorize(Policy = "Permission:Files.Download")]
    [HttpGet]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var response = await _caseFileService.GetSignedDownloadUrlAsync(id, RequireUserId(), cancellationToken);
        if (!response.Success || response.Data is null)
        {
            return Forbid();
        }

        return Redirect(response.Data.Url);
    }

    [Authorize(Policy = "Permission:Files.Delete.Soft")]
    [HttpPost]
    public async Task<IActionResult> Delete(int id, int caseId, CancellationToken cancellationToken)
    {
        var response = await _caseFileService.SoftDeleteCaseFileAsync(id, RequireUserId(), cancellationToken);
        TempData[response.Success ? "StatusMessage" : "FileError"] = response.Message;

        return RedirectToAction("Details", "Cases", new { id = caseId });
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> LocalSignedDownload(string token, CancellationToken cancellationToken)
    {
        try
        {
            var download = await _fileStorageService.OpenSignedDownloadAsync(token, cancellationToken);
            return File(download.Content, download.ContentType, download.FileName);
        }
        catch
        {
            return NotFound();
        }
    }

    private string RequireUserId()
    {
        return _currentUserService.UserId ?? throw new InvalidOperationException("Authenticated user id is missing.");
    }
}
