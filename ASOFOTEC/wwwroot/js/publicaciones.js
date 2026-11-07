// Compositor de publicaciones: adjuntar una foto o un video con vista previa.
(function () {
    'use strict';

    const form = document.getElementById('form-publicacion');
    if (!form) return;

    const input = document.getElementById('archivo-input');
    const preview = document.getElementById('adjunto-preview');
    const contenido = document.getElementById('adjunto-contenido');
    const botonQuitar = document.getElementById('adjunto-quitar');
    const botonPublicar = document.getElementById('btn-publicar');
    const textarea = form.querySelector('textarea[name="Contenido"]');

    // Mismos límites que valida el servidor
    const MAX_FOTO = 5 * 1024 * 1024;     // 5 MB
    const MAX_VIDEO = 50 * 1024 * 1024;   // 50 MB

    let urlTemporal = null;

    function limpiar() {
        if (urlTemporal) {
            URL.revokeObjectURL(urlTemporal);
            urlTemporal = null;
        }
        contenido.innerHTML = '';
        preview.classList.add('d-none');
        input.value = '';
    }

    // Los botones "Foto" y "Video" abren el selector con el tipo correspondiente
    form.querySelectorAll('.btn-adjuntar').forEach(function (boton) {
        boton.addEventListener('click', function () {
            input.accept = boton.dataset.tipo === 'video'
                ? 'video/mp4,video/webm,video/quicktime,.mp4,.webm,.mov'
                : 'image/jpeg,image/png,image/gif,image/webp,.jpg,.jpeg,.png,.gif,.webp';
            input.click();
        });
    });

    input.addEventListener('change', function () {
        const archivo = input.files[0];
        if (!archivo) {
            limpiar();
            return;
        }

        const esVideo = archivo.type.startsWith('video/');
        const esFoto = archivo.type.startsWith('image/');

        if (!esVideo && !esFoto) {
            Ux.toast('Selecciona una foto (JPG, PNG, GIF, WEBP) o un video (MP4, WEBM, MOV).', 'danger');
            limpiar();
            return;
        }

        if (esFoto && archivo.size > MAX_FOTO) {
            Ux.toast('La foto supera el máximo de 5 MB.', 'danger');
            limpiar();
            return;
        }

        if (esVideo && archivo.size > MAX_VIDEO) {
            Ux.toast('El video supera el máximo de 50 MB.', 'danger');
            limpiar();
            return;
        }

        if (urlTemporal) URL.revokeObjectURL(urlTemporal);
        urlTemporal = URL.createObjectURL(archivo);

        contenido.innerHTML = '';
        let elemento;
        if (esVideo) {
            elemento = document.createElement('video');
            elemento.controls = true;
            elemento.preload = 'metadata';
        } else {
            elemento = document.createElement('img');
            elemento.alt = 'Vista previa';
        }
        elemento.src = urlTemporal;
        elemento.className = 'adjunto-vista';
        contenido.appendChild(elemento);

        preview.classList.remove('d-none');
    });

    botonQuitar.addEventListener('click', limpiar);

    form.addEventListener('submit', function (e) {
        const hayTexto = textarea.value.trim().length > 0;
        const hayArchivo = input.files.length > 0;

        if (!hayTexto && !hayArchivo) {
            e.preventDefault();
            Ux.toast('Escribe algo o adjunta una foto o un video.', 'danger');
            return;
        }

        // Evita publicar dos veces mientras se sube el archivo
        botonPublicar.disabled = true;
        botonPublicar.textContent = hayArchivo ? 'Subiendo...' : 'Publicando...';
    });
})();
