document.addEventListener("DOMContentLoaded", function () {
    const dz = document.getElementById('dropZone');
    const fi = document.getElementById('fotoArchivo');
    const pr = document.getElementById('imgPreview');

    if (dz && fi) {
        dz.addEventListener('click', () => fi.click());

        fi.addEventListener('change', function () {
            const file = this.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = (e) => {
                    pr.src = e.target.result;
                };
                reader.readAsDataURL(file);
            }
        });
    }
});