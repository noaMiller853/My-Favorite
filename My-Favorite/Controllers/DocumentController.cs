using Microsoft.AspNetCore.Mvc;
using WebApplicationUser.Models.WebApplicationUser.Models;
using WebApplicationUser.Models;
using WebApplicationUser.Interfaces;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly WebApplicationUserDbContext _context;
    private readonly IGoogleDrive _driveService;
    private readonly IOcrService _ocrService;

    public DocumentsController(WebApplicationUserDbContext context, GoogleDriveService driveService,IOcrService ocrService)
    {
        _context = context;
        _driveService = driveService;
        _ocrService=ocrService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentMeta>>> GetDocuments()
    {
        var documents =await _context.DocumentMeta.ToListAsync(); 
        if (documents == null )
        {
            return NotFound("No documents found.");
        }
        return Ok(documents);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentMeta>> GetDocument(int id)
    {
        var document = await _context.DocumentMeta.FindAsync(id);
        if (document == null)
        {
            return NotFound($"Document with ID {id} not found.");
        }
       return Ok(document);
    }

    [HttpGet("download/{id}")]
    public async Task<ActionResult<DocumentMeta>> DownloadDocument(int id)
    {
        var document = await _driveService.DownloadFileFromDrive(id); 
        if (document == null)
        {
            return NotFound($"Document with ID {id} not found.");
        }
      
        return Ok(document);
    }

    [HttpPost("upload")]
    public async Task<ActionResult<DocumentMeta>> UploadDocument(IFormFile file)
    {
        if (file == null)
        {
            return BadRequest("No file uploaded.");
        }

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        string driveFileId = await _driveService.UploadFileToDrive(memoryStream, file.FileName, file.ContentType);
        string extractedText = await _ocrService.ExtractText(memoryStream); // הפעלת OCR

        var documentMeta = new DocumentMeta
        {
            FileName = file.FileName,
            FileSize = file.Length,
            ContentType = file.ContentType,
            UploadedAt = DateTime.UtcNow,
            FilePath = driveFileId,
            Content = extractedText
        };

        _context.DocumentMeta.Add(documentMeta);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDocument), new { id = documentMeta.Id }, documentMeta);
    }


    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<DocumentMeta>>> SearchDocuments(string query)
    {
        var results = await _context.DocumentMeta
            .Where(d => d.Content.Contains(query))
            .ToListAsync();
        

        return Ok(results);
    }


    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteDocument(int id)
    {
        var document = await _context.DocumentMeta.FindAsync(id);
        if (document == null)
        {
            return NotFound($"Document with ID {id} not found.");
        }

        _context.DocumentMeta.Remove(document);
        await _driveService.DeleteFileFromDrive(id.ToString());
        await _context.SaveChangesAsync();
        return Ok();
    }
}
