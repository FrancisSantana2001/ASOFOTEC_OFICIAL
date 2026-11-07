// Tema claro / oscuro. El tema inicial lo aplica un script corto en el <head> (sin parpadeo);
// este archivo solo maneja el botón y recuerda la elección de la persona.
(function () {
    'use strict';

    const CLAVE = 'as-tema';
    const raiz = document.documentElement;

    function actual() {
        return raiz.getAttribute('data-bs-theme') === 'dark' ? 'dark' : 'light';
    }

    function actualizarBotones() {
        const oscuro = actual() === 'dark';

        document.querySelectorAll('.btn-tema').forEach(function (boton) {
            const icono = boton.querySelector('i');
            if (icono) icono.className = 'bi ' + (oscuro ? 'bi-sun' : 'bi-moon-stars');

            const texto = oscuro ? 'Cambiar a tema claro' : 'Cambiar a tema oscuro';
            boton.setAttribute('aria-label', texto);
            boton.title = texto;
        });
    }

    function aplicar(tema, guardar) {
        raiz.setAttribute('data-bs-theme', tema);
        if (guardar) {
            try { localStorage.setItem(CLAVE, tema); } catch (e) { /* sin almacenamiento */ }
        }
        actualizarBotones();
    }

    document.addEventListener('click', function (e) {
        if (!e.target.closest('.btn-tema')) return;
        aplicar(actual() === 'dark' ? 'light' : 'dark', true);
    });

    // Mientras la persona no haya elegido, se sigue el cambio de tema del sistema
    if (window.matchMedia) {
        const consulta = window.matchMedia('(prefers-color-scheme: dark)');
        const alCambiar = function (e) {
            let guardado = null;
            try { guardado = localStorage.getItem(CLAVE); } catch (err) { /* nada */ }
            if (!guardado) aplicar(e.matches ? 'dark' : 'light', false);
        };

        if (consulta.addEventListener) consulta.addEventListener('change', alCambiar);
        else if (consulta.addListener) consulta.addListener(alCambiar);
    }

    actualizarBotones();
})();
