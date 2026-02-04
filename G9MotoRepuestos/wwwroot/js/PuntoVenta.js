function toggleChat() {
    const content = document.getElementById("chatContent");
    const icon = document.getElementById("chatIcon");

    if (content.style.display === "none") {
        content.style.display = "block";
        icon.classList.replace("fa-plus", "fa-minus");
    } else {
        content.style.display = "none";
        icon.classList.replace("fa-minus", "fa-plus");
    }
}

document.addEventListener("DOMContentLoaded", function () {
     toggleChat(); 
});