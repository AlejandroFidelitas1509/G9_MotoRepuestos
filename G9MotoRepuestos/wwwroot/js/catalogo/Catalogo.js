// Función para simular añadir al carrito y abrir el offcanvas
function agregarAlCarrito(nombre, precio) {
    // Aquí podrías integrar tu lógica real de C# después
    console.log("Añadido: " + nombre);

    // Abrir el carrito automáticamente para mostrar dinamismo
    var myOffcanvas = document.getElementById('offcanvasCart');
    var bsOffcanvas = new bootstrap.Offcanvas(myOffcanvas);
    bsOffcanvas.show();

    // Feedback visual en el botón (opcional)
    // Se puede implementar con SweetAlert2 para que se vea más pro
}

// Escuchar clics en los botones de "Añadir"
document.querySelectorAll('.btn-outline-danger').forEach(boton => {
    boton.addEventListener('click', function (e) {
        const card = this.closest('.card-body');
        const nombre = card.querySelector('h6').innerText;
        agregarAlCarrito(nombre, "");
    });

    document.addEventListener("DOMContentLoaded", () => {
        const input = document.getElementById("categoriaIdInput");
        const items = document.querySelectorAll(".categoria-item");

        items.forEach(btn => {
            btn.addEventListener("click", () => {
                const id = btn.getAttribute("data-id");
                input.value = id;

                // UI: marcar activo (opcional)
                items.forEach(x => x.classList.remove("fw-bold", "text-danger"));
                btn.classList.add("fw-bold", "text-danger");
            });
        });
    });
});