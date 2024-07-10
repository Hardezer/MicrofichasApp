using Microfichas_App.Data;
using Microfichas_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Microfichas_App.Controllers
{
    public class FoldersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FoldersController> _logger;

        public FoldersController(ApplicationDbContext context, ILogger<FoldersController> logger)
        {
            _context = context;
            _logger = logger;
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var folder = await _context.Folders.FindAsync(id);
            int? parentFolderId = folder?.ParentFolderId;

            if (folder != null)
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        await DeleteFolderRecursivelyAsync(id);
                        await transaction.CommitAsync();
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        await transaction.RollbackAsync();
                        ViewData["ErrorMessage"] = "La carpeta no pudo ser eliminada. Inténtalo de nuevo. " + ex.Message;
                        return RedirectToAction(nameof(Index), new { parentFolderId = parentFolderId });
                    }
                }
            }

            return RedirectToAction(nameof(Index), new { parentFolderId = parentFolderId });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            var folder = await _context.Folders.FindAsync(id);
            if (folder == null)
            {
                return Json(new { success = false, message = "Carpeta no encontrada." });
            }

            // Si la carpeta tiene archivos o subcarpetas, elimínelos también
            var files = await _context.Files.Where(f => f.FolderId == id).ToListAsync();
            _context.Files.RemoveRange(files);

            var subfolders = await _context.Folders.Where(f => f.ParentFolderId == id).ToListAsync();
            foreach (var subfolder in subfolders)
            {
                var subfolderFiles = await _context.Files.Where(f => f.FolderId == subfolder.FolderId).ToListAsync();
                _context.Files.RemoveRange(subfolderFiles);
                _context.Folders.Remove(subfolder);
            }

            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
        private async Task DeleteFolderRecursivelyAsync(int folderId)
        {
            // Obtener y eliminar archivos dentro de la carpeta
            var files = await _context.Files.Where(f => f.FolderId == folderId).ToListAsync();
            _context.Files.RemoveRange(files);
            await _context.SaveChangesAsync();

            // Obtener subcarpetas dentro de la carpeta
            var subfolders = await _context.Folders.Where(f => f.ParentFolderId == folderId).ToListAsync();

            // Eliminar cada subcarpeta recursivamente
            foreach (var subfolder in subfolders)
            {
                await DeleteFolderRecursivelyAsync(subfolder.FolderId);
            }

            // Eliminar la carpeta principal
            var folder = await _context.Folders.FindAsync(folderId);
            if (folder != null)
            {
                _context.Folders.Remove(folder);
                await _context.SaveChangesAsync();
            }
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
            var breadcrumbs = new List<Breadcrumb>();
            breadcrumbs.Add(new Breadcrumb { FolderId = 0, FolderName = "Inicio" }); // Añadir el primer breadcrumb "Inicio"
            while (folderId != 0)
            {
                var folder = await _context.Folders.FindAsync(folderId);
                if (folder == null) break;
                breadcrumbs.Insert(1, new Breadcrumb { FolderId = folder.FolderId, FolderName = folder.FolderName }); // Insertar después de "Inicio"
                folderId = folder.ParentFolderId ?? 0;
            }
            return breadcrumbs;
        }

    }
}
