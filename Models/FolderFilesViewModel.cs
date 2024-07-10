using System.Collections.Generic;
using Microfichas_App.Models;

namespace Microfichas_App.Models
{
    public class FolderFilesViewModel
    {
        public IEnumerable<Folder> Folders { get; set; }
        public IEnumerable<File> Files { get; set; }
        public int? ParentFolderId { get; set; }
        public int? GrandParentFolderId { get; set; }
        public List<Breadcrumb> Breadcrumbs { get; set; }
        public bool IsSearching { get; set; } // Nueva propiedad para determinar si se está realizando una búsqueda
    }
    public class Breadcrumb
    {
        public int FolderId { get; set; }
        public string FolderName { get; set; }
    }
}