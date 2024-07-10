$(document).ready(function () {
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
        console.log(`Eliminando archivo con ID: ${fileId}`);

        $.ajax({
            url: '/Files/DeleteFile',
            type: 'POST',
            data: { id: fileId },
            success: function (response) {
                if (response.success) {
                    console.log('Archivo eliminado con éxito');
                    // Actualiza la vista para eliminar la fila del archivo eliminado
                    $(`[data-file-id="${fileId}"]`).closest('tr').remove();
                    $('#deleteFileModal').modal('hide');
                } else {
                    console.error('Error: ' + response.message);
                    alert('Error: ' + response.message);
                }
            },
            error: function (xhr, status, error) {
                console.error('Solicitud fallida: ' + error);
                alert('Solicitud fallida: ' + error);
            }
        });
    });
});
