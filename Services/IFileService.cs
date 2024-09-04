using FileModel = Microfichas_App.Models.File;

namespace Microfichas_App.Services
{
    public interface IFileService
    {
        Task<FileModel> GetFileByIdAsync(int id);
        Task UpdateFileAsync(FileModel file);
    }

}
