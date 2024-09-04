using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microfichas_App.Data;
using Microfichas_App.Hubs;
using Microfichas_App.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using FileModel = Microfichas_App.Models.File;

namespace Microfichas_App.Services
{
    public class FolderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ProgressHub> _hubContext;

        public FolderService(ApplicationDbContext context, IHubContext<ProgressHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task SaveFoldersAndFiles(string rootPath)
        {
            DirectoryInfo rootDirectory = new DirectoryInfo(rootPath);
            if (!rootDirectory.Exists)
                throw new DirectoryNotFoundException($"The directory {rootPath} does not exist.");

            var allFilesAndFolders = rootDirectory.GetFiles("*", SearchOption.AllDirectories).Length
                                   + rootDirectory.GetDirectories("*", SearchOption.AllDirectories).Length;

            await ProcessDirectory(rootDirectory, null, allFilesAndFolders, 0);
        }

        private async Task<int> ProcessDirectory(DirectoryInfo directoryInfo, int? parentFolderId, int totalItems, int processedItems)
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

            processedItems += directoryInfo.GetFiles().Length + 1; // +1 for the folder itself
            int progress = (int)((double)processedItems / totalItems * 100);
            await _hubContext.Clients.All.SendAsync("ReceiveProgress", progress);

            foreach (DirectoryInfo subDirectory in directoryInfo.GetDirectories())
            {
                processedItems = await ProcessDirectory(subDirectory, folder.FolderId, totalItems, processedItems);
            }

            return processedItems;
        }


        public async Task<List<Breadcrumb>> BuildBreadcrumbs(int folderId)
        {
            var breadcrumbs = new List<Breadcrumb>();
            breadcrumbs.Add(new Breadcrumb { FolderId = 0, FolderName = "Inicio" });
            while (folderId != 0)
            {
                var folder = await _context.Folders.FindAsync(folderId);
                if (folder == null) break;
                breadcrumbs.Insert(1, new Breadcrumb { FolderId = folder.FolderId, FolderName = folder.FolderName });
                folderId = folder.ParentFolderId ?? 0;
            }
            return breadcrumbs;
        }
    }
}