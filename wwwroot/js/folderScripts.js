// wwwroot/js/folderScripts.js
$(document).ready(function () {
    // Manejar evento cuando se muestra el modal de editar carpeta
    $('#editFolderModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var folderId = button.data('folder-id');
        var folderName = button.data('folder-name');

        var modal = $(this);
        modal.find('#editFolderId').val(folderId);
        modal.find('#editFolderName').val(folderName);
    });

    // Manejar evento cuando se envía el formulario de editar carpeta
    $('#editFolderForm').submit(function (event) {
        event.preventDefault();

        var folder = {
            FolderId: $('#editFolderId').val(),
            FolderName: $('#editFolderName').val()
        };

        $.ajax({
            url: '/Folders/EditFolder',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(folder),
            success: function (response) {
                if (response.success) {
                    location.reload();
                } else {
                    alert('Error: ' + response.message);
                }
            },
            error: function (xhr, status, error) {
                alert('Request failed: ' + error);
            }
        });
    });

    // Manejar evento cuando se muestra el modal de eliminar carpeta
    $('#deleteFolderModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var folderId = button.data('folder-id');
        var folderName = button.data('folder-name');

        var modal = $(this);
        modal.find('#deleteFolderId').val(folderId);
        modal.find('#deleteFolderName').text(folderName);
    });

    // Manejar evento cuando se confirma la eliminación de carpeta
    $('#confirmDeleteFolder').click(function () {
        var folderId = $('#deleteFolderId').val();

        $.ajax({
            url: '/Folders/DeleteFolder',
            type: 'POST',
            data: { id: folderId },
            success: function (response) {
                if (response.success) {
                    location.reload();
                } else {
                    alert('Error: ' + response.message);
                }
            },
            error: function (xhr, status, error) {
                alert('Request failed: ' + error);
            }
        });
    });

    // Manejar evento cuando se muestra el modal de editar archivo
    $('#editFileModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var fileId = button.data('file-id');
        var fileName = button.data('file-name');

        var modal = $(this);
        modal.find('#editFileId').val(fileId);
        modal.find('#editFileName').val(fileName);
    });

    // Manejar evento cuando se envía el formulario de editar archivo
    $('#editFileForm').submit(function (event) {
        event.preventDefault();

        var file = {
            FileId: $('#editFileId').val(),
            FileName: $('#editFileName').val()
        };

        $.ajax({
            url: '/Files/EditFile',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(file),
            success: function (response) {
                if (response.success) {
                    location.reload();
                } else {
                    alert('Error: ' + response.message);
                }
            },
            error: function (xhr, status, error) {
                alert('Request failed: ' + error);
            }
        });
    });

    // Manejar evento cuando se muestra el modal de eliminar archivo
    $('#deleteFileModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var fileId = button.data('file-id');
        var fileName = button.data('file-name');

        var modal = $(this);
        modal.find('#deleteFileId').val(fileId);
        modal.find('#deleteFileName').text(fileName);
    });

    // Manejar evento cuando se confirma la eliminación de archivo
    $('#confirmDeleteFile').click(function () {
        var fileId = $('#deleteFileId').val();

        $.ajax({
            url: '/Files/DeleteFile',
            type: 'POST',
            data: { id: fileId },
            success: function (response) {
                if (response.success) {
                    location.reload();
                } else {
                    alert('Error: ' + response.message);
                }
            },
            error: function (xhr, status, error) {
                alert('Request failed: ' + error);
            }
        });
    });

    $('#searchBox').on('input', function () {
        var query = $(this).val();
        $.ajax({
            url: '/Folders/Search',
            type: 'GET',
            data: { searchQuery: query },
            success: function (data) {
                // Actualizar la vista con los resultados obtenidos
                $('tbody').html(data);
            },
            error: function (xhr, status, error) {
                console.error('Error en la búsqueda: ' + error);
            }
        });
    });
});
