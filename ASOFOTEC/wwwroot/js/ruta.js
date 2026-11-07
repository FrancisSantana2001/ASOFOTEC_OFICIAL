// Ruta del problema: botones "Ir al paso" y paso activo visible en el stepper.
(function () {
    'use strict';

    function irAlPaso(numero) {
        const pestana = document.querySelector('[data-bs-target="#paso-' + numero + '"]');
        if (!pestana) return;
        bootstrap.Tab.getOrCreateInstance(pestana).show();
        pestana.scrollIntoView({ behavior: 'smooth', inline: 'center', block: 'nearest' });
    }

    document.addEventListener('click', function (e) {
        const boton = e.target.closest('.btn-ir-paso');
        if (!boton) return;
        irAlPaso(boton.dataset.paso);
    });

    // En pantallas pequeñas el stepper se desplaza: se centra el paso que está abierto
    const activo = document.querySelector('.ruta-paso.active');
    if (activo) {
        const contenedor = activo.closest('.ruta-stepper');
        if (contenedor) {
            contenedor.scrollLeft = activo.offsetLeft - (contenedor.clientWidth - activo.clientWidth) / 2;
        }
    }
})();
