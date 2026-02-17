function toggleChat() {
    var content = document.getElementById("chatContent");
    var icon = document.getElementById("chatIcon");

    // Si está oculto, lo muestra y cambia el icono a menos (-)
    if (content.style.display === "none") {
        content.style.display = "block";
        icon.classList.remove("fa-plus");
        icon.classList.add("fa-minus");
    }
    // Si está visible, lo oculta y cambia el icono a más (+)
    else {
        content.style.display = "none";
        icon.classList.remove("fa-minus");
        icon.classList.add("fa-plus");
    }
}