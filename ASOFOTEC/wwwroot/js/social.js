// Funciones sociales: seguir personas, compartir enlaces y campana de notificaciones.
(function () {
    'use strict';

    const MSG_ERROR = 'No se pudo completar la acción. Inténtalo de nuevo.';
    const CADA_MS = 30000;   // cada cuánto se consulta si hay notificaciones nuevas

    function obtenerToken() {
        const input = document.querySelector('#antiforgery-form input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    // POST con antiforgery; si el servidor responde 401 manda al login recordando la página
    async function post(url, datos) {
        const body = new URLSearchParams(datos || {});
        body.append('__RequestVerificationToken', obtenerToken());

        const respuesta = await fetch(url, {
            method: 'POST',
            credentials: 'same-origin',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: body
        });

        if (respuesta.status === 401) {
            Ux.irALogin();
            return null;
        }
        return respuesta;
    }

    // ============================================================ Seguir

    function aplicarEstadoSeguir(boton, siguiendo) {
        boton.classList.toggle('btn-as-outline', siguiendo);
        boton.classList.toggle('btn-as', !siguiendo);
        boton.textContent = siguiendo
            ? 'Siguiendo'
            : (boton.dataset.meSigue === '1' ? 'Seguir también' : 'Seguir');
    }

    document.addEventListener('click', async function (e) {
        const boton = e.target.closest('.btn-seguir');
        if (!boton) return;

        boton.disabled = true;
        try {
            const respuesta = await post('/Seguimiento/Alternar', { id: boton.dataset.usuarioId });
            if (!respuesta) return;

            if (!respuesta.ok) {
                Ux.toast(MSG_ERROR, 'danger');
                return;
            }

            const datos = await respuesta.json();
            aplicarEstadoSeguir(boton, datos.siguiendo);

            document.querySelectorAll('[data-seguidores-de="' + boton.dataset.usuarioId + '"]')
                .forEach(function (el) { el.textContent = datos.seguidores; });
        } catch (err) {
            Ux.toast(MSG_ERROR, 'danger');
        } finally {
            boton.disabled = false;
        }
    });

    // ============================================================ Compartir

    // El botón "Más opciones" (menú de compartir del celular) solo aparece si el navegador lo permite
    if (navigator.share) {
        document.documentElement.classList.add('soporta-share');
    }

    function copiarTexto(texto) {
        if (navigator.clipboard && window.isSecureContext) {
            return navigator.clipboard.writeText(texto);
        }

        // Respaldo para páginas sin HTTPS o navegadores antiguos
        return new Promise(function (resolver, rechazar) {
            const area = document.createElement('textarea');
            area.value = texto;
            area.setAttribute('readonly', '');
            area.style.position = 'fixed';
            area.style.opacity = '0';
            document.body.appendChild(area);
            area.select();
            try {
                document.execCommand('copy') ? resolver() : rechazar();
            } catch (err) {
                rechazar(err);
            } finally {
                area.remove();
            }
        });
    }

    document.addEventListener('click', async function (e) {
        const copiar = e.target.closest('.btn-copiar-enlace');
        if (copiar) {
            try {
                await copiarTexto(copiar.dataset.url);
                Ux.toast('Enlace copiado al portapapeles.');
            } catch (err) {
                Ux.toast('No se pudo copiar. Copia la dirección desde la barra del navegador.', 'danger');
            }
            return;
        }

        const nativo = e.target.closest('.btn-compartir-nativo');
        if (nativo) {
            try {
                await navigator.share({ title: nativo.dataset.titulo, url: nativo.dataset.url });
            } catch (err) {
                // La persona cerró el menú de compartir: no es un error
            }
        }
    });

    // ============================================================ Campana de notificaciones

    function actualizarCampana(total) {
        document.querySelectorAll('.notif-badge').forEach(function (badge) {
            badge.textContent = total > 9 ? '9+' : String(total);
            badge.classList.toggle('d-none', total === 0);
        });

        document.querySelectorAll('.notif-btn i').forEach(function (icono) {
            icono.classList.toggle('bi-bell-fill', total > 0);
            icono.classList.toggle('bi-bell', total === 0);
        });
    }

    async function consultarNoLeidas() {
        if (!document.querySelector('.notif-badge')) return;     // no hay sesión: no hay campana
        if (document.visibilityState !== 'visible') return;      // pestaña en segundo plano

        try {
            const respuesta = await fetch('/Notificaciones/Resumen', {
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (!respuesta.ok) return;

            const datos = await respuesta.json();
            actualizarCampana(datos.noLeidas);
        } catch (err) {
            // Sin conexión: se vuelve a intentar en el siguiente ciclo
        }
    }

    async function cargarLista(contenedor) {
        const lista = contenedor.querySelector('.notif-lista');
        try {
            const respuesta = await fetch('/Notificaciones/Recientes', {
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });

            if (respuesta.status === 401) { Ux.irALogin(); return; }

            // El HTML lo genera el servidor con los textos ya codificados
            lista.innerHTML = respuesta.ok
                ? await respuesta.text()
                : '<div class="text-center text-muted small py-4">No se pudieron cargar las notificaciones.</div>';
        } catch (err) {
            lista.innerHTML = '<div class="text-center text-muted small py-4">Sin conexión.</div>';
        }
    }

    // Al abrir la campana se carga la lista
    document.addEventListener('show.bs.dropdown', function (e) {
        const contenedor = e.target.closest ? e.target.closest('.notif-dropdown') : null;
        if (contenedor) cargarLista(contenedor);
    });

    document.addEventListener('click', async function (e) {
        const boton = e.target.closest('.btn-marcar-todas');
        if (!boton) return;

        const contenedor = boton.closest('.notif-dropdown');
        try {
            const respuesta = await post('/Notificaciones/MarcarTodas');
            if (respuesta && respuesta.ok) {
                actualizarCampana(0);
                if (contenedor) cargarLista(contenedor);
            }
        } catch (err) {
            Ux.toast(MSG_ERROR, 'danger');
        }
    });

    if (document.querySelector('.notif-badge')) {
        consultarNoLeidas();
        setInterval(consultarNoLeidas, CADA_MS);
        document.addEventListener('visibilitychange', function () {
            if (document.visibilityState === 'visible') consultarNoLeidas();
        });
    }
})();
