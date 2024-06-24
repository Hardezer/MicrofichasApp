using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microfichas_App.Services;

namespace Microfichas_App.Controllers
{
    public class ImportController : Controller
    {
        private readonly FolderService _folderService;

        public ImportController(FolderService folderService)
        {
            _folderService = folderService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Import(string rootPath)
        {
            if (string.IsNullOrEmpty(rootPath))
            {
                ModelState.AddModelError("", "Por favor, proporcione una ruta válida.");
                return View("Index");
            }

            try
            {
                await _folderService.SaveFoldersAndFiles(rootPath);
                ViewBag.Message = "¡Importación exitosa!";
            }
            catch (System.Exception ex)
            {
                ViewBag.Message = $"Error: {ex.Message}";
            }

            return View("Index");
        }
    }
}
