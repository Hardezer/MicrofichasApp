using Microfichas_App.Data;
using Microfichas_App.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Microfichas_App.Controllers
{
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

            public async Task<IActionResult> Index(int? parentFolderId)
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
            private async Task<List<Breadcrumb>> BuildBreadcrumbs(int folderId)
            {
                var breadcrumbs = new List<Breadcrumb>();
                while (folderId != null)
                {
                    var folder = await _context.Folders.FindAsync(folderId);
                    if (folder == null) break;
                    breadcrumbs.Insert(0, new Breadcrumb { FolderId = folder.FolderId, FolderName = folder.FolderName });
                    folderId = folder.ParentFolderId ?? 0;
                }
                return breadcrumbs;
            }


            public IActionResult Create(int? parentFolderId)
            {
                var folder = new Folder
                {
                    ParentFolderId = parentFolderId
                };
                return View(folder);
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
                        return RedirectToAction(nameof(Index), new { parentFolderId = folder.ParentFolderId });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error al crear la carpeta.");
                    }
                }
                else
                {
                    _logger.LogWarning("ModelState es inválido. Errores: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                }
                return View(folder);
            }

            public async Task<IActionResult> Edit(int id)
            {
                _logger.LogInformation("Método GET de Edit llamado con id: {Id}", id);
                var folder = await _context.Folders.FindAsync(id);
                if (folder == null)
                {
                    _logger.LogWarning("Carpeta con id: {Id} no encontrada", id);
                    return NotFound();
                }
                return View(folder);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(int id, [Bind("FolderId,FolderName,ParentFolderId,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate")] Folder folder)
            {
                _logger.LogInformation("Método POST de Edit llamado con id: {Id}, FolderName: {FolderName}, ParentFolderId: {ParentFolderId}", id, folder.FolderName, folder.ParentFolderId);

                if (id != folder.FolderId)
                {
                    _logger.LogWarning("El ID de la carpeta no coincide. Esperado: {Id}, Actual: {FolderId}", id, folder.FolderId);
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        folder.ModifiedDate = DateTime.Now;
                        _context.Update(folder);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation("Carpeta '{FolderName}' actualizada.", folder.FolderName);
                        return RedirectToAction(nameof(Index), new { parentFolderId = folder.ParentFolderId });
                    }
                    catch (DbUpdateConcurrencyException ex)
                    {
                        if (!FolderExists(folder.FolderId))
                        {
                            _logger.LogWarning("Carpeta con id {FolderId} no encontrada.", folder.FolderId);
                            return NotFound();
                        }
                        else
                        {
                            _logger.LogError(ex, "Error actualizando carpeta con ID {FolderId}", folder.FolderId);
                            throw;
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("Modelo inválido. Errores: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                }
                return View(folder);
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
                    // Eliminar archivos dentro de la carpeta
                    var files = await _context.Files.Where(f => f.FolderId == id).ToListAsync();
                    _context.Files.RemoveRange(files);

                    // Eliminar subcarpetas y sus archivos
                    var subfolders = await _context.Folders.Where(f => f.ParentFolderId == id).ToListAsync();
                    foreach (var subfolder in subfolders)
                    {
                        var subfolderFiles = await _context.Files.Where(f => f.FolderId == subfolder.FolderId).ToListAsync();
                        _context.Files.RemoveRange(subfolderFiles);
                        _context.Folders.Remove(subfolder);
                    }

                    // Eliminar la carpeta principal
                    _context.Folders.Remove(folder);

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index), new { parentFolderId = parentFolderId });
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
                _context.Update(existingFolder);
                _context.SaveChanges();

                return Json(new { success = true });
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


            private bool FolderExists(int id)
            {
                return _context.Folders.Any(e => e.FolderId == id);
            }

        }
    }
}
