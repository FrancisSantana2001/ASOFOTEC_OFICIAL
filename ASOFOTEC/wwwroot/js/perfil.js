// Mi perfil: al elegir una foto se valida el tamaño y se sube automáticamente.
(function () {
    'use strict';

    document.addEventListener('click', function (e) {
        const boton = e.target.closest('.btn-elegir-foto');
        if (!boton) return;

        const entrada = boton.closest('form').querySelector('input[type="file"]');
        entrada.click();
    });

    document.addEventListener('change', function (e) {
        const entrada = e.target;
        if (!(entrada instanceof HTMLInputElement) || entrada.type !== 'file') return;

        const formulario = entrada.closest('.form-foto');
        if (!formulario || !entrada.files.length) return;

        const archivo = entrada.files[0];
        const maximoMb = Number(formulario.dataset.maxMb || 3);
        const permitidos = ['image/jpeg', 'image/png', 'image/webp'];

        if (permitidos.indexOf(archivo.type) === -1) {
            Ux.toast('Usa una imagen JPG, PNG o WEBP.', 'danger');
            entrada.value = '';
            return;
        }

        if (archivo.size > maximoMb * 1024 * 1024) {
            Ux.toast('La imagen supera el máximo de ' + maximoMb + ' MB.', 'danger');
            entrada.value = '';
            return;
        }

        formulario.submit();
    });
})();
