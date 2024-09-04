using Microfichas_App.Data;
using Microfichas_App.Hubs;
using Microfichas_App.Models;
using Microfichas_App.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace Microfichas_App.Controllers
{
    public class FoldersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FoldersController> _logger;
        private readonly FolderService _folderService;
        private readonly IHubContext<ProgressHub> _hubContext;
        private readonly IFileService _fileService;
        private readonly IAzureService _azureService;

        public FoldersController(ApplicationDbContext context, ILogger<FoldersController> logger, FolderService folderService, IHubContext<ProgressHub> hubContext, IFileService fileService, IAzureService azureService)
        {
            _context = context;
            _logger = logger;
            _folderService = folderService;
            _hubContext = hubContext;
            _fileService = fileService;
            _azureService = azureService;
        }

        public async Task<IActionResult> Index(int? parentFolderId, string errorMessage = null)
        {
            var folders = await _context.Folders
                .Where(f => f.ParentFolderId == parentFolderId)
                .ToListAsync();

            var files = await _context.Files
                .Where(f => f.FolderId == parentFolderId)
                .ToListAsync();

            int? grandParentFolderId = null;
            List<Breadcrumb> breadcrumbs = null;

            if (parentFolderId.HasValue)
            {
                var parentFolder = await _context.Folders
                    .FirstOrDefaultAsync(f => f.FolderId == parentFolderId);
                grandParentFolderId = parentFolder?.ParentFolderId;
                breadcrumbs = await BuildBreadcrumbs(parentFolderId.Value);
            }

            var viewModel = new FolderFilesViewModel
            {
                Folders = folders,
                Files = files,
                ParentFolderId = parentFolderId,
                GrandParentFolderId = grandParentFolderId,
                Breadcrumbs = breadcrumbs
            };

            if (!string.IsNullOrEmpty(errorMessage))
            {
                ViewData["ErrorMessage"] = errorMessage;
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Publish(int id)
        {
            var file = await _fileService.GetFileByIdAsync(id);
            if (file == null)
            {
                return NotFound();
            }

            // Obtener el token de Azure
            JObject tokenResponse = await _azureService.GetTokenAsync("microfichas@vitacura.cl");
            if (tokenResponse == null || (bool)tokenResponse["tipo"])
            {
                TempData["ErrorMessage"] = "Error al obtener el token de acceso.";
                return RedirectToAction(nameof(Index));
            }

            var token = (string)tokenResponse["datos"]["token"];
            _logger.LogInformation($"Token: {token}");

            // Leer el contenido del archivo desde el path
            string filePath = Path.Combine(file.Server, file.ContainerPath, file.FullFileName);
            byte[] fileContent = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileContentBase64 = Convert.ToBase64String(fileContent);
            _logger.LogInformation(fileContentBase64);

            // Publicar el documento en Azure
            JObject publishResponse;
            try
            {
                publishResponse = await _azureService.PublishDocumentAsync(token, file.FileName, 2730, fileContentBase64);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error during PublishDocumentAsync: {ex.Message}");
                TempData["ErrorMessage"] = "Error al publicar el documento: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }

            if (publishResponse == null || (bool)publishResponse["tipo"])
            {
                TempData["ErrorMessage"] = "Error al publicar el documento.";
                return RedirectToAction(nameof(Index));
            }

            var documentId = (string)publishResponse["datos"]["id"];
            var documentUrl = (string)publishResponse["datos"]["url"];
            _logger.LogInformation($"Document ID: {documentId}");
            _logger.LogInformation($"Document URL: {documentUrl}");

            // Parse the document URL to update the Server, ContainerPath, and FullFileName properties
            Uri uri = new Uri(documentUrl);
            string server = $"{uri.Scheme}://{uri.Host}";
            string containerPath = uri.AbsolutePath.Substring(1, uri.AbsolutePath.LastIndexOf('/') - 1);
            string fullFileName = uri.AbsolutePath.Substring(uri.AbsolutePath.LastIndexOf('/') + 1);

            _logger.LogInformation($"Parsed Server: {server}");
            _logger.LogInformation($"Parsed ContainerPath: {containerPath}");
            _logger.LogInformation($"Parsed FullFileName: {fullFileName}");

            // Actualizar la base de datos con el ID del documento y la respuesta completa del servidor
            file.AzureDocumentId = documentId;
            file.AzureToken = publishResponse.ToString();
            file.Server = server;
            file.ContainerPath = containerPath;
            file.FullFileName = fullFileName;

            _logger.LogInformation($"Before Update: AzureDocumentId = {file.AzureDocumentId}, AzureToken = {file.AzureToken}, Server = {file.Server}, ContainerPath = {file.ContainerPath}, FullFileName = {file.FullFileName}");

            await _fileService.UpdateFileAsync(file);

            TempData["Message"] = "Documento publicado exitosamente.";
            return RedirectToAction(nameof(Index));
        }



        [HttpGet]
        public IActionResult GetFile(string filePath)
        {
            var net = new System.Net.WebClient();
            var data = net.DownloadData(filePath);
            var content = new System.IO.MemoryStream(data);
            var contentType = "APPLICATION/octet-stream";
            var fileName = Path.GetFileName(filePath);
            return File(content, contentType, fileName);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FolderName,ParentFolderId")] Folder folder)
        {
            _logger.LogInformation("Método POST de Create llamado.");

            folder.CreatedBy = "System";
            folder.ModifiedBy = "System";
            folder.CreatedDate = DateTime.Now;
            folder.ModifiedDate = DateTime.Now;

            ModelState.Clear();
            TryValidateModel(folder);

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("ModelState es válido.");
                    _context.Add(folder);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Carpeta creada exitosamente con ID: {FolderId}", folder.FolderId);
                    return Json(new { success = true, message = "Carpeta creada exitosamente." });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear la carpeta.");
                    return Json(new { success = false, message = "Error al crear la carpeta." });
                }
            }
            else
            {
                _logger.LogWarning("ModelState es inválido. Errores: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return Json(new { success = false, message = "Datos inválidos en el modelo." });
            }
        }

        [HttpPost]
        public IActionResult EditFolder([FromBody] Folder folder)
        {
            if (folder == null || string.IsNullOrWhiteSpace(folder.FolderName))
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            var existingFolder = _context.Folders.Find(folder.FolderId);
            if (existingFolder == null)
            {
                return Json(new { success = false, message = "Folder not found" });
            }

            existingFolder.FolderName = folder.FolderName;
            existingFolder.ModifiedDate = DateTime.Now; // Actualiza la fecha de modificación
            _context.Update(existingFolder);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var folder = await _context.Folders.FindAsync(id);
            if (folder == null)
            {
                return NotFound();
            }
            return View(folder);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            var folder = await _context.Folders.FindAsync(id);
            if (folder == null)
            {
                return Json(new { success = false, message = "Carpeta no encontrada." });
            }

            var totalItems = 1; // La carpeta raíz cuenta como un ítem.
            var processedItems = 0;

            var files = await _context.Files.Where(f => f.FolderId == id).ToListAsync();
            totalItems += files.Count;

            var subfolders = await _context.Folders.Where(f => f.ParentFolderId == id).ToListAsync();
            await _hubContext.Clients.All.SendAsync("ReceiveProgress", 1);
            foreach (var subfolder in subfolders)
            {
                var subfolderFiles = await _context.Files.Where(f => f.FolderId == subfolder.FolderId).ToListAsync();
                totalItems += subfolderFiles.Count;
            }
            totalItems += subfolders.Count;
            await _hubContext.Clients.All.SendAsync("ReceiveProgress", 2);
            // Eliminación de archivos directos
            _context.Files.RemoveRange(files);
            processedItems += files.Count;
            await _hubContext.Clients.All.SendAsync("ReceiveProgress", processedItems * 100 / totalItems);
            await _context.SaveChangesAsync();

            // Eliminación de subcarpetas y sus archivos
            foreach (var subfolder in subfolders)
            {
                var subfolderFiles = await _context.Files.Where(f => f.FolderId == subfolder.FolderId).ToListAsync();
                _context.Files.RemoveRange(subfolderFiles);
                processedItems += subfolderFiles.Count;
                await _hubContext.Clients.All.SendAsync("ReceiveProgress", processedItems * 100 / totalItems);

                _context.Folders.Remove(subfolder);
                processedItems++;
                await _hubContext.Clients.All.SendAsync("ReceiveProgress", processedItems * 100 / totalItems);
                await _context.SaveChangesAsync();
            }

            // Eliminación de la carpeta raíz
            _context.Folders.Remove(folder);
            processedItems++;
            await _hubContext.Clients.All.SendAsync("ReceiveProgress", processedItems * 100 / totalItems);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        private bool FolderExists(int id)
        {
            return _context.Folders.Any(e => e.FolderId == id);
        }

        public IActionResult Search(string searchQuery)
        {
            var folders = _context.Folders
                .Where(f => f.FolderName.Contains(searchQuery))
                .ToList();
            var files = _context.Files
                .Where(f => f.FileName.Contains(searchQuery))
                .ToList();

            var folderBreadcrumbs = new Dictionary<int, List<Breadcrumb>>();
            var fileBreadcrumbs = new Dictionary<int, List<Breadcrumb>>();

            foreach (var folder in folders)
            {
                folderBreadcrumbs[folder.FolderId] = BuildBreadcrumbsSync(folder.FolderId);
            }

            foreach (var file in files)
            {
                fileBreadcrumbs[file.FileId] = BuildBreadcrumbsSync(file.FolderId);
            }

            var viewModel = new FolderFilesViewModel
            {
                Folders = folders,
                Files = files,
                IsSearching = true // Establecer la propiedad para indicar que se está realizando una búsqueda
            };

            ViewBag.FolderBreadcrumbs = folderBreadcrumbs;
            ViewBag.FileBreadcrumbs = fileBreadcrumbs;

            return PartialView("_SearchResults", viewModel);
        }

        private List<Breadcrumb> BuildBreadcrumbsSync(int folderId)
        {
            var breadcrumbs = new List<Breadcrumb>();
            while (folderId != 0)
            {
                var folder = _context.Folders.Find(folderId);
                if (folder == null) break;
                breadcrumbs.Insert(0, new Breadcrumb { FolderId = folder.FolderId, FolderName = folder.FolderName });
                folderId = folder.ParentFolderId ?? 0;
            }
            return breadcrumbs;
        }

        private async Task<List<Breadcrumb>> BuildBreadcrumbs(int folderId)
        {
            return await _folderService.BuildBreadcrumbs(folderId);
        }

    }
}
