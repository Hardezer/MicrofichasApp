$(document).ready(function () {
    function debounce(func, wait) {
        let timeout;
        return function (...args) {
            clearTimeout(timeout);
            timeout = setTimeout(() => func.apply(this, args), wait);
        };
    }

    let currentRequest = null;

    const performSearch = debounce(function (query) {
        // Muestra un mensaje de carga mientras se realiza la búsqueda
        $('#tableBody').html('<tr><td colspan="4" class="text-center"><img src="../images/loading.gif" alt="Cargando..." style="width: 50px; height: 50px;" /><div style="font-size: 16px; margin-top: 10px;">Buscando...</div></td></tr>');


        $('#searchIcon').attr('src', '../images/loading.gif'); // Cambia el ícono a loading
        if (currentRequest) {
            currentRequest.abort();
        }

        currentRequest = $.ajax({
            url: '/Folders/Search',
            type: 'GET',
            data: { searchQuery: query },
            success: function (data) {
                $('#searchIcon').attr('src', '../images/search-icon.png'); // Restablece el ícono de búsqueda
                if (query.length > 0) {
                    $('#defaultHeader').hide();
                    $('#searchHeader').show();
                } else {
                    $('#defaultHeader').show();
                    $('#searchHeader').hide();
                }
                const newTableBody = $(data).find('#tableBody').html();
                $('#tableBody').html(newTableBody);

                if (!newTableBody.trim()) {
                    $('#tableBody').html('<tr><td colspan="4" class="text-center">No existen archivos con ese nombre.</td></tr>');
                }
            },
            error: function (xhr, status, error) {
                $('#searchIcon').attr('src', '../images/search-icon.png'); // Restablece el ícono de búsqueda en caso de error
                if (status !== 'abort') {
                    console.error('Error en la búsqueda: ' + error);
                }
            }
        });
    }, 300);

    $('#searchBox').on('input', function () {
        var query = $(this).val();
        performSearch(query);
    });
});
