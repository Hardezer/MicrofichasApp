using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microfichas_App.Data;
using AppFile = Microfichas_App.Models.File;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Microfichas_App.Controllers
{
    public class FilesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<FilesController> _logger;

        public FilesController(ApplicationDbContext context, ILogger<FilesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? parentFolderId)
        {
            var files = await _context.Files
                .Where(f => f.FolderId == parentFolderId)
                .ToListAsync();

            return View(files);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppFile file)
        {
            if (ModelState.IsValid)
            {
                file.CreatedDate = DateTime.Now;
                file.ModifiedDate = DateTime.Now;
                _context.Add(file);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Folders", new { parentFolderId = file.FolderId });
            }
            return View(file);
        }

        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Método GET de Edit llamado con id: {Id}", id);
            var file = await _context.Files.Include(f => f.Folder).FirstOrDefaultAsync(f => f.FileId == id);
            if (file == null)
            {
                _logger.LogWarning("Archivo con id: {Id} no encontrado", id);
                return NotFound();
            }
            return View(file);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FileId,FileName,FileType,FolderId,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate")] AppFile file)
        {
            _logger.LogInformation("Método POST de Edit llamado con id: {Id}, FileName: {FileName}, FolderId: {FolderId}", id, file.FileName, file.FolderId);

            if (id != file.FileId)
            {
                _logger.LogWarning("El ID del archivo no coincide. Esperado: {Id}, Actual: {FileId}", id, file.FileId);
                return NotFound();
            }

            ModelState.Remove("Folder");

            if (ModelState.IsValid)
            {
                try
                {
                    file.ModifiedDate = DateTime.Now;
                    _context.Update(file);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Archivo '{FileName}' actualizado exitosamente.", file.FileName);
                    return RedirectToAction("Index", "Folders", new { parentFolderId = file.FolderId });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!FileExists(file.FileId))
                    {
                        _logger.LogWarning("Archivo con ID {FileId} no encontrado.", file.FileId);
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Error actualizando archivo con ID {FileId}", file.FileId);
                        throw;
                    }
                }
            }
            else
            {
                _logger.LogWarning("El estado del modelo es inválido. Errores: {Errors}", string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            }
            return View(file);
        }
        [HttpPost]
        public IActionResult EditFile([FromBody] AppFile file)
        {
            if (file == null || string.IsNullOrWhiteSpace(file.FileName))
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            var existingFile = _context.Files.Find(file.FileId);
            if (existingFile == null)
            {
                return Json(new { success = false, message = "File not found" });
            }

            existingFile.FileName = file.FileName;
            existingFile.ModifiedDate = DateTime.Now;
            _context.Update(existingFile);
            _context.SaveChanges();

            return Json(new { success = true });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var file = await _context.Files.FindAsync(id);
            if (file == null)
            {
                return NotFound();
            }
            return View(file);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var file = await _context.Files.FindAsync(id);
            if (file != null)
            {
                _context.Files.Remove(file);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Folders", new { parentFolderId = file.FolderId });
        }

        
        [HttpPost]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var file = await _context.Files.FindAsync(id);
            if (file == null)
            {
                return Json(new { success = false, message = "File not found" });
            }

            _context.Files.Remove(file);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        private bool FileExists(int id)
        {
            return _context.Files.Any(e => e.FileId == id);
        }
    }
}
