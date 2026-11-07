// Utilidades de interfaz compartidas: avisos (toasts) y confirmaciones con modal.
// Reemplazan a alert() y confirm() del navegador.
(function () {
    'use strict';

    const ICONOS = {
        success: 'bi-check-circle-fill',
        danger: 'bi-exclamation-triangle-fill',
        info: 'bi-info-circle-fill'
    };

    function contenedorToasts() {
        return document.getElementById('toast-container');
    }

    // Muestra un aviso temporal. tipo: 'success' | 'danger' | 'info'
    function toast(mensaje, tipo) {
        tipo = tipo || 'success';
        const contenedor = contenedorToasts();
        if (!contenedor || !mensaje) return;

        const el = document.createElement('div');
        el.className = 'toast align-items-center text-bg-' + tipo + ' border-0';
        el.setAttribute('role', tipo === 'danger' ? 'alert' : 'status');
        el.setAttribute('aria-live', tipo === 'danger' ? 'assertive' : 'polite');
        el.setAttribute('aria-atomic', 'true');

        const cuerpo = document.createElement('div');
        cuerpo.className = 'd-flex';

        const texto = document.createElement('div');
        texto.className = 'toast-body';
        const icono = document.createElement('i');
        icono.className = 'bi ' + (ICONOS[tipo] || ICONOS.info) + ' me-2';
        texto.appendChild(icono);
        texto.appendChild(document.createTextNode(mensaje)); // textContent: sin riesgo de XSS

        const cerrar = document.createElement('button');
        cerrar.type = 'button';
        cerrar.className = 'btn-close btn-close-white me-2 m-auto';
        cerrar.setAttribute('data-bs-dismiss', 'toast');
        cerrar.setAttribute('aria-label', 'Cerrar');

        cuerpo.appendChild(texto);
        cuerpo.appendChild(cerrar);
        el.appendChild(cuerpo);
        contenedor.appendChild(el);

        el.addEventListener('hidden.bs.toast', function () { el.remove(); });
        bootstrap.Toast.getOrCreateInstance(el, { delay: tipo === 'danger' ? 7000 : 4000 }).show();
    }

    // Pide confirmación en un modal. Devuelve una promesa con true (aceptó) o false (canceló)
    function confirmar(opciones) {
        opciones = opciones || {};
        const modalEl = document.getElementById('ux-confirm');
        if (!modalEl) return Promise.resolve(window.confirm(opciones.mensaje || '¿Estás seguro?'));

        const peligro = opciones.peligro !== false;
        modalEl.querySelector('#ux-confirm-titulo').textContent = opciones.titulo || '¿Estás seguro?';
        modalEl.querySelector('#ux-confirm-mensaje').textContent = opciones.mensaje || '';

        const aceptar = modalEl.querySelector('#ux-confirm-aceptar');
        aceptar.textContent = opciones.textoAceptar || 'Eliminar';
        aceptar.className = 'btn ' + (peligro ? 'btn-danger' : 'btn-success');

        const icono = modalEl.querySelector('#ux-confirm-icono');
        icono.className = 'ux-confirm-icono mb-2 ' + (peligro ? 'text-danger' : 'text-success');
        icono.firstElementChild.className = 'bi ' + (peligro ? 'bi-exclamation-triangle' : 'bi-question-circle');

        const modal = bootstrap.Modal.getOrCreateInstance(modalEl);

        return new Promise(function (resolve) {
            let decidido = false;

            function terminar(valor) {
                if (decidido) return;
                decidido = true;
                aceptar.removeEventListener('click', alAceptar);
                modalEl.removeEventListener('hidden.bs.modal', alCerrar);
                resolve(valor);
            }
            function alAceptar() { terminar(true); modal.hide(); }
            function alCerrar() { terminar(false); }

            aceptar.addEventListener('click', alAceptar);
            modalEl.addEventListener('hidden.bs.modal', alCerrar);
            modal.show();
        });
    }

    // Lleva al login recordando la página actual
    function irALogin() {
        window.location.href = '/Auth/Login?returnUrl=' +
            encodeURIComponent(window.location.pathname + window.location.search);
    }

    window.Ux = { toast: toast, confirmar: confirmar, irALogin: irALogin };

    // Formularios con data-confirm="mensaje": piden confirmación antes de enviarse
    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (!(form instanceof HTMLFormElement) || !form.hasAttribute('data-confirm')) return;

        if (form.dataset.confirmado === '1') {
            form.dataset.confirmado = '';
            return; // ya confirmado: se envía normalmente
        }

        e.preventDefault();
        const enviador = e.submitter;

        confirmar({
            titulo: form.dataset.confirmTitulo,
            mensaje: form.getAttribute('data-confirm'),
            textoAceptar: form.dataset.confirmOk,
            peligro: form.dataset.confirmPeligro !== 'false'
        }).then(function (ok) {
            if (!ok) return;
            form.dataset.confirmado = '1';
            form.requestSubmit(enviador || undefined);
        });
    }, true);

    // Avisos que envía el servidor (TempData) al cargar la página
    document.addEventListener('DOMContentLoaded', function () {
        const mensajes = document.getElementById('ux-mensajes');
        if (!mensajes) return;
        if (mensajes.dataset.success) toast(mensajes.dataset.success, 'success');
        if (mensajes.dataset.error) toast(mensajes.dataset.error, 'danger');
    });
})();
