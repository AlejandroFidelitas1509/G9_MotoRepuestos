$(document).ready(function () {
    // Tabla general de citas
    $('#tablaCitas').DataTable({
        paging: true,
        searching: true,
        ordering: true,
        pageLength: 10,
        lengthMenu: [5, 10, 25, 50, 100],
        language: {
            url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json"
        }
    });

    // Tabla de MisCitas
    $('#tablaMisCitas').DataTable({
        paging: true,
        searching: true,
        ordering: true,
        pageLength: 5, // puedes darle otro valor inicial
        lengthMenu: [5, 10, 25, 50],
        language: {
            url: "//cdn.datatables.net/plug-ins/1.13.6/i18n/es-ES.json"
        }
    });
});