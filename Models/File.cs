using System;
using System.ComponentModel.DataAnnotations;

namespace Microfichas_App.Models
{
    public class File
    {
        public int FileId { get; set; }

        [Required]
        public string FileName { get; set; }

        public string FileType { get; set; }

        [Required]
        public int FolderId { get; set; }


        public string CreatedBy { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedDate { get; set; }

        public string ModifiedBy { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ModifiedDate { get; set; }

        public Folder Folder { get; set; }

        // Nuevas propiedades
        public string Server { get; set; } // Ejemplo: "file:///" o "https://public.blob.core.windows.net/"
        public string ContainerPath { get; set; } // Ejemplo: "C:/Users/ezel_/Desktop/Año 51/43-1951/"
        public string FullFileName { get; set; } // Ejemplo: "ScanPro_1_123886.jpg"

    }
    }
