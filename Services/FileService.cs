using System.Threading.Tasks;
using Microfichas_App.Data;
using Microfichas_App.Services;
using Microsoft.EntityFrameworkCore;
using FileModel = Microfichas_App.Models.File;

public class FileService : IFileService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FileService> _logger;

    public FileService(ApplicationDbContext context, ILogger<FileService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FileModel> GetFileByIdAsync(int id)
    {
        return await _context.Files.FindAsync(id);
    }

    public async Task UpdateFileAsync(FileModel file)
    {
        _context.Files.Update(file);
        _logger.LogInformation($"Updating File: AzureToken = {file.AzureToken}, AzureDocumentId = {file.AzureDocumentId}");
        await _context.SaveChangesAsync();
    }
}
