function toggleChat() {
    const content = document.getElementById("chatContent");
    const icon = document.getElementById("chatIcon");

    // ✅ Si esta página no tiene chat, no hacemos nada (evita el error)
    if (!content || !icon) return;

    const estaOculto = (content.style.display === "none" || content.style.display === "");

    if (estaOculto) {
        content.style.display = "block";
        icon.classList.replace("fa-plus", "fa-minus");
    } else {
        content.style.display = "none";
        icon.classList.replace("fa-minus", "fa-plus");
    }
}

document.addEventListener("DOMContentLoaded", function () {
    // ✅ Solo ejecuta toggleChat si existen los elementos
    const content = document.getElementById("chatContent");
    const icon = document.getElementById("chatIcon");
    if (content && icon) {
        // Si querés que por defecto arranque CERRADO, aseguramos display none:
        if (content.style.display === "") content.style.display = "none";
        // No lo abras automáticamente; solo deja listo el estado del ícono si querés
        // toggleChat(); // <-- dejalo comentado para no abrir/cerrar solo al cargar
    }
});