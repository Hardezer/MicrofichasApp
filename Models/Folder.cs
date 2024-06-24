using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Microfichas_App.Models
{
    public class Folder
    {
        public int FolderId { get; set; }

        [Required]
        public string FolderName { get; set; }

        public int? ParentFolderId { get; set; }

        public string CreatedBy { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ModifiedDate { get; set; }

        public ICollection<File> Files { get; set; } = new List<File>();
    }
}
