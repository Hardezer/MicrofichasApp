using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microfichas_App.Data;
using Microfichas_App.Models;
using Microsoft.EntityFrameworkCore;
using FileModel = Microfichas_App.Models.File;

namespace Microfichas_App.Services
{
    public class FolderService
    {
        private readonly ApplicationDbContext _context;

        public FolderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveFoldersAndFiles(string rootPath)
        {
            DirectoryInfo rootDirectory = new DirectoryInfo(rootPath);
            if (!rootDirectory.Exists)
                throw new DirectoryNotFoundException($"The directory {rootPath} does not exist.");

            await ProcessDirectory(rootDirectory, null);
        }

        private async Task ProcessDirectory(DirectoryInfo directoryInfo, int? parentFolderId)
        {
            Folder folder = new Folder
            {
                FolderName = directoryInfo.Name,
                ParentFolderId = parentFolderId,
                CreatedBy = "System",
                CreatedDate = DateTime.Now,
                ModifiedBy = "System",
                ModifiedDate = DateTime.Now
            };

            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();


            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                FileModel file = new FileModel
                {
                    FileName = fileInfo.Name,
                    FileType = fileInfo.Extension,
                    FolderId = folder.FolderId,
                    CreatedBy = "System",
                    CreatedDate = DateTime.Now,
                    ModifiedBy = "System",
                    ModifiedDate = DateTime.Now,
                    Server = "file:///", // O tu servidor específico para almacenamiento
                    ContainerPath = fileInfo.DirectoryName + "/", // Ruta completa del directorio
                    FullFileName = fileInfo.Name // Nombre del archivo con extensión
                };

                _context.Files.Add(file);
            }
            await _context.SaveChangesAsync();

            await _context.SaveChangesAsync();

            foreach (DirectoryInfo subDirectory in directoryInfo.GetDirectories())
            {
                await ProcessDirectory(subDirectory, folder.FolderId);
            }
        }
    }
}
