using System;
using System.ComponentModel.DataAnnotations;

namespace Microfichas_App.Models
{
    public class File
    {
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public int FolderId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Folder Folder { get; set; }

        // Nuevas propiedades
        public string Server { get; set; } // URL del servidor o blob donde se almacena el archivo.
        public string ContainerPath { get; set; } // Ruta del contenedor donde se guarda el archivo.
        public string FullFileName { get; set; } // Nombre completo del archivo con la extensión.

        // Propiedades para la integración con Azure
        public string? AzureToken { get; set; } // Token de acceso para la API de Azure.
        public string? AzureDocumentId { get; set; } // ID del documento almacenado en Azure.
    }

}
