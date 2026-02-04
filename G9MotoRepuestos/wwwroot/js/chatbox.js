/**
 * Lógica para el Chatbox de Moto Repuestos Rojas
 */
function toggleChat() {
    const chatContent = document.getElementById("chatContent");
    const chatIcon = document.getElementById("chatIcon");

    if (!chatContent || !chatIcon) return;

    if (chatContent.style.display === "none") {
        chatContent.style.display = "block";
        chatIcon.classList.remove("fa-plus");
        chatIcon.classList.add("fa-minus");
    } else {
        chatContent.style.display = "none";
        chatIcon.classList.remove("fa-minus");
        chatIcon.classList.add("fa-plus");
    }
}

// Inicialización
document.addEventListener("DOMContentLoaded", function () {
    console.log("Chatbox Rojas cargado correctamente.");
});