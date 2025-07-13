using Tesseract;

namespace Hackathon.Services
{
    public class OcrService
    {
        public string ExtractText(string imagePath)
        {
            using var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            using var img = Pix.LoadFromFile(imagePath);
            using var page = engine.Process(img);
            return page.GetText();
        }
    }
}