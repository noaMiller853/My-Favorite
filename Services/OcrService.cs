using Google.Cloud.Vision.V1;
using System.IO;
using System.Threading.Tasks;

public class OcrService : IOcrService
{
    public async Task<string> ExtractText(Stream fileStream)
    {
        var client = ImageAnnotatorClient.Create();
        var responsee = await client.DetectTextAsync(Google.Cloud.Vision.V1.Image.FromFile("C:\\Users\\לנובו2\\source\\repos\\WebApplicationUser\\WebApplicationUser\\picture.png"));


        // קריאת התמונה מתוך ה-Stream
        fileStream.Position = 0;
        var image = Image.FromStream(fileStream);

        // ניתוח התמונה עם Google Vision API
        var response = await client.DetectTextAsync(image);

        // חיבור כל התוצאות לטקסט אחד
        return string.Join(" ", response.Select(t => t.Description));
    }
}

