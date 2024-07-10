$(document).ready(function () {
    // Manejar formulario de creación de carpeta
    $('#createFolderForm').on('submit', function (e) {
        e.preventDefault();
        var formData = $(this).serialize();
        console.log("Datos del formulario:", formData);

        $.post('/Folders/Create', formData, function (response) {
            console.log("Respuesta recibida:", response);
            if (response.success) {
                $('#createFolderModal').modal('hide');
                window.location.reload();
            } else {
                alert(response.message);
            }
        }).fail(function (error) {
            console.error('Error al crear la carpeta:', error);
            alert('No se pudo crear la carpeta. Por favor, intenta nuevamente.');
        });
    });


    // Manejar evento cuando se muestra el modal de editar carpeta
    $('#editFolderModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var folderId = button.data('folder-id');
        var folderName = button.data('folder-name');

        var modal = $(this);
        modal.find('#editFolderId').val(folderId);
        modal.find('#editFolderName').val(folderName);
    });

    // Manejar formulario de edición de carpeta
    $('#editFolderForm').submit(function (event) {
        event.preventDefault();

        var folder = {
            FolderId: $('#editFolderId').val(),
            FolderName: $('#editFolderName').val(),
            ModifiedDate: new Date().toISOString()
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

    
    let nameSortOrder = 'asc';
    let dateSortOrder = 'asc';

    //sort por nombre
    document.getElementById('name-sort-icon').addEventListener('click', function () {
        sortTable('tableBody', 0, nameSortOrder, 'string');
        nameSortOrder = (nameSortOrder === 'asc') ? 'desc' : 'asc';
        updateSortIcon('name-sort-icon', nameSortOrder);
    });
    //sort por nombre
    document.getElementById('date-sort-icon').addEventListener('click', function () {
        sortTable('tableBody', 2, dateSortOrder, 'date');
        dateSortOrder = (dateSortOrder === 'asc') ? 'desc' : 'asc';
        updateSortIcon('date-sort-icon', dateSortOrder);
    });

    //cambia la tabla
    function sortTable(tableId, column, order, type) {
        let table = document.getElementById(tableId);
        let rows = Array.from(table.rows);
        rows.sort((a, b) => {
            let aText = a.cells[column].innerText.trim();
            let bText = b.cells[column].innerText.trim();

            if (type === 'date') {
                aText = aText ? new Date(aText.split('/').reverse().join('-')) : new Date(0);
                bText = bText ? new Date(bText.split('/').reverse().join('-')) : new Date(0);
                return (order === 'asc') ? aText - bText : bText - aText;
            } else {
                return (order === 'asc') ? aText.localeCompare(bText) : bText.localeCompare(aText);
            }
        });
        rows.forEach(row => table.appendChild(row));
    }

    // cambia el icono de orden
    function updateSortIcon(iconId, order) {
        let icon = document.getElementById(iconId);
        if (order === 'asc') {
            icon.classList.add('arrow-up');
            icon.classList.remove('arrow-down');
        } else {
            icon.classList.add('arrow-down');
            icon.classList.remove('arrow-up');
        }
    }

  

});
