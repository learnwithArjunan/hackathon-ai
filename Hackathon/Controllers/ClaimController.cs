using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Web;
using Hackathon.Models;
using Hackathon.Services;
using Microsoft.AspNetCore.Hosting.Server;
using System.Collections.Generic;
using System.IO;

public class ClaimController : Controller
{
    private readonly OcrService _ocrService = new();
    private readonly OpenAiService _openAiService = new();
    private readonly ValidationService _validationService = new();
    private static List<ClaimInfo> _claimInfo = new();
    private readonly IWebHostEnvironment _env;
    public ClaimController(IWebHostEnvironment env)
    {
        _env = env;
    }
    [HttpGet]
    public IActionResult Upload()
    {
        return View(_claimInfo);
    }
    [HttpPost]
    public async Task<IActionResult> UploadDetails(string name, IFormFile docFile, string claimType)
    {
        if (docFile != null && docFile.Length > 0 && !string.IsNullOrEmpty(name))
        {
            //string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            //Directory.CreateDirectory(uploadsFolder);
            var uploadsFolder = Path.Combine("uploads", docFile.FileName);
            Directory.CreateDirectory("uploads");

            string fileName = Path.GetFileName(docFile.FileName);
            //string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(uploadsFolder, FileMode.Create))
            {
                await docFile.CopyToAsync(stream);
            }

            _claimInfo.Add(new ClaimInfo
            {
                Name = name,
                FileName = fileName,
                Status = "Ready to validate",
                ClaimType = claimType
            });
        }

        return RedirectToAction("Upload");
    }

    [HttpPost]
    public IActionResult Reset()
    {
        _claimInfo.Clear();
        return RedirectToAction("Upload");
    }

    [HttpPost]
    public async Task<IActionResult> Validate()
    {
        string uploadPath = Path.Combine(_env.WebRootPath, "uploads");

        foreach (var doc in _claimInfo)
        {
            string filePath = Path.Combine("uploads", doc.FileName);

            if (!System.IO.File.Exists(filePath))
            {
                doc.Status = "File Not Found";
                continue;
            }

            // Example: Check file size (e.g., max 5MB)
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > 5 * 1024 * 1024)
            {
                doc.Status = "File Too Large";
                continue;
            }

            // Optional: Check file extension
            var allowedExtensions = new[] { ".pdf", ".jpg", ".png" };
            string extension = Path.GetExtension(doc.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                doc.Status = "Invalid File Type";
                continue;
            }

            var ocrText = _ocrService.ExtractText(filePath);

            string jsonOutput = string.Empty;
            if (doc.ClaimType == "diagnosis")
                jsonOutput = await _openAiService.ExtractFieldsAsync(ocrText);
            else if (doc.ClaimType == "itemized")
                jsonOutput = await _openAiService.GetNonClaimableItemsAsync(ocrText);
            
            var claimData = JsonConvert.DeserializeObject<ClaimData>(jsonOutput);

            doc.ClaimData = new List<string>
            {
                "Date:" + claimData.Date,
                "Hospital:" + claimData.Hospital,
                "Diagnosis:" + claimData.Diagnosis
            };
            var validation = _validationService.Validate(claimData);

            // All checks passed
            if (validation.IsDateValid && validation.IsDiagnosisCovered && validation.IsHospitalApproved)
            {
                doc.Status = "Approved";
            }
            else
            {
                doc.Status = "Rejected";
            }
        }
        return RedirectToAction("Upload");
    }


    [HttpGet]
    public async Task<IActionResult> ViewDetails(string fileName, string name, string status, string claimType)
    {
        if (string.IsNullOrEmpty(fileName))
            return BadRequest("Filename is required.");

        var filePath = Path.Combine("uploads", fileName);
        Directory.CreateDirectory("uploads");

        var ocrText = _ocrService.ExtractText(filePath);
        string jsonOutput = string.Empty;

        if (claimType.ToLower() == "diagnosis")
            jsonOutput = await _openAiService.ExtractFieldsAsync(ocrText);
        else if (claimType.ToLower() == "itemized")
            jsonOutput = await _openAiService.GetNonClaimableItemsAsync(ocrText);

        var claimData = JsonConvert.DeserializeObject<ClaimData>(jsonOutput);

        var validation = _validationService.Validate(claimData);
        ViewBag.Name = name;
        ViewBag.Claim = claimData;
        ViewBag.Result = validation;
        ViewBag.RawText = ocrText;
        ViewBag.Status = status;
        ViewBag.claimType = claimType;

        ViewBag.NonPayableItems = validation.NonPayableItems;
        ViewBag.NonPayableAmount = validation.NonClaimableTotal;
        ViewBag.ApprovedAmount = validation.ApprovedAmount;
        ViewBag.TotalAmount = validation.ClaimedAmount;

        return View("Result");
    }

    [HttpPost]
    public async Task<IActionResult> AskAssistant(string question, string ocrText)
    {
        var answer = await _openAiService.AskAssistantAsync(ocrText, question);
        return Json(new { response = answer });
    }
}