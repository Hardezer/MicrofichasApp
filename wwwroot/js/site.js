// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).on('click', '.edit-btn', function () {
    var url = $(this).data('url');
    $('#editModal .modal-body').load(url, function () {
        $('#editModal').modal({ show: true });
    });
});

$(document).on('click', '.delete-btn', function () {
    var url = $(this).data('url');
    $('#deleteModal .modal-body').load(url, function () {
        $('#deleteModal').modal({ show: true });
    });
});
