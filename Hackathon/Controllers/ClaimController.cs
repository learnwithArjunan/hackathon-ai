using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.IO;
using Hackathon.Models;
using Hackathon.Services;

public class ClaimController : Controller
{
    private readonly OcrService _ocrService = new();
    private readonly OpenAiService _openAiService = new();
    private readonly ValidationService _validationService = new();

    [HttpGet]
    public IActionResult Upload() => View();

    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return View();

        var filePath = Path.Combine("uploads", file.FileName);
        Directory.CreateDirectory("uploads");

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var ocrText = _ocrService.ExtractText(filePath);
        var jsonOutput = await _openAiService.ExtractFieldsAsync(ocrText);
        var claimData = JsonConvert.DeserializeObject<ClaimData>(jsonOutput);
        
        var validation = _validationService.Validate(claimData);

        ViewBag.Claim = claimData;
        ViewBag.Result = validation;
        ViewBag.RawText = ocrText;

        return View("Result");
    }

    [HttpPost]
    public async Task<IActionResult> AskAssistant(string question, string ocrText)
    {
        var answer = await _openAiService.AskAssistantAsync(ocrText, question);
        return Json(new { response = answer });
    }
}