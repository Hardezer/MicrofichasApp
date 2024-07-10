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
        if (currentRequest) {
            currentRequest.abort();
        }

        currentRequest = $.ajax({
            url: '/Folders/Search',
            type: 'GET',
            data: { searchQuery: query },
            success: function (data) {
                if (query.length > 0) {
                    $('#defaultHeader').hide();
                    $('#searchHeader').show();
                } else {
                    $('#defaultHeader').show();
                    $('#searchHeader').hide();
                }

                $('#tableBody').html($(data).find('#tableBody').html());
            },
            error: function (xhr, status, error) {
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
