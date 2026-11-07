// Interacciones del feed y de los detalles: comentarios (crear, editar, eliminar, responder)
// y likes de publicaciones y comentarios, todo sin recargar la página.
(function () {
    'use strict';

    const MSG_ERROR = 'No se pudo completar la acción. Inténtalo de nuevo.';

    function obtenerToken() {
        const input = document.querySelector('#antiforgery-form input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    // POST con antiforgery. Si el servidor responde 401 (sin sesión) manda al login.
    async function post(url, datos) {
        const body = new URLSearchParams(datos);
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

    // Recalcula el número de comentarios (incluye respuestas) y el mensaje "Aún no hay comentarios"
    function actualizarContador(panel) {
        const tarjeta = panel.closest('.card');
        const total = panel.querySelectorAll('.comentario-item').length;

        tarjeta.querySelectorAll('.contador-comentarios').forEach(function (el) {
            el.textContent = total;
        });

        const vacio = panel.querySelector('.sin-comentarios');
        if (vacio) vacio.classList.toggle('d-none', total > 0);
    }

    // El textarea crece con el texto (hasta un máximo) para no agrandar el contenedor
    function autoajustar(textarea) {
        textarea.style.height = 'auto';
        textarea.style.height = Math.min(textarea.scrollHeight, 120) + 'px';
    }

    // ---------- Crear comentario o respuesta ----------
    document.addEventListener('submit', async function (e) {
        const form = e.target.closest('.form-comentario, .form-respuesta');
        if (!form) return;

        e.preventDefault();

        const textarea = form.querySelector('textarea');
        const boton = form.querySelector('button[type="submit"]');
        const contenido = textarea.value.trim();
        if (!contenido) return;

        const panel = form.closest('.panel-comentarios');
        const esRespuesta = form.classList.contains('form-respuesta');
        const hilo = esRespuesta ? form.closest('.comentario-hilo') : null;

        const datos = { Contenido: contenido };
        if (esRespuesta) {
            datos.ComentarioPadreId = hilo.dataset.hiloId;
        } else {
            // SolucionId o PublicacionId, según la tarjeta
            form.querySelectorAll('input[type="hidden"]').forEach(function (input) {
                if (input.name && input.name !== '__RequestVerificationToken') {
                    datos[input.name] = input.value;
                }
            });
        }

        boton.disabled = true;

        try {
            const respuesta = await post(form.getAttribute('action') || '/Comentario/CrearComentario', datos);
            if (!respuesta) return;

            if (!respuesta.ok) {
                Ux.toast(MSG_ERROR, 'danger');
                return;
            }

            const html = await respuesta.text();

            if (esRespuesta) {
                hilo.querySelector('.lista-respuestas').insertAdjacentHTML('beforeend', html);
                form.remove();
            } else {
                panel.querySelector('.lista-comentarios').insertAdjacentHTML('afterbegin', html);
                textarea.value = '';
                autoajustar(textarea);
            }

            actualizarContador(panel);
        } catch (err) {
            Ux.toast(MSG_ERROR, 'danger');
        } finally {
            boton.disabled = false;
        }
    });

    // Enter envía; Shift+Enter hace salto de línea; Escape cierra el formulario de respuesta
    document.addEventListener('keydown', function (e) {
        const textarea = e.target.closest('.comentario-input');
        if (!textarea) return;

        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            textarea.closest('form').requestSubmit();
        } else if (e.key === 'Escape') {
            const respuesta = textarea.closest('.form-respuesta');
            if (respuesta) respuesta.remove();
        }
    });

    document.addEventListener('input', function (e) {
        if (e.target.classList.contains('comentario-input')) autoajustar(e.target);
    });

    // ---------- Formulario de respuesta ----------
    function mostrarFormularioRespuesta(item) {
        const cuerpo = item.querySelector('.comentario-cuerpo');

        const existente = cuerpo.querySelector('.form-respuesta');
        if (existente) {
            existente.querySelector('textarea').focus();
            return;
        }

        const nombre = item.querySelector('.comentario-nombre').textContent.trim();

        const form = document.createElement('form');
        form.className = 'form-respuesta d-flex align-items-end gap-2 mt-2';
        form.setAttribute('action', '/Comentario/CrearComentario');

        const textarea = document.createElement('textarea');
        textarea.className = 'form-control form-control-sm comentario-input';
        textarea.rows = 1;
        textarea.maxLength = 300;
        textarea.placeholder = 'Responder a ' + nombre + '...';

        // Al responder a una respuesta se menciona a la persona
        if (item.classList.contains('es-respuesta')) {
            textarea.value = '@' + nombre + ' ';
        }

        const boton = document.createElement('button');
        boton.type = 'submit';
        boton.className = 'btn btn-success btn-sm btn-enviar-comentario';
        boton.title = 'Responder';
        boton.innerHTML = '<i class="bi bi-send"></i>';

        form.appendChild(textarea);
        form.appendChild(boton);
        cuerpo.appendChild(form);

        textarea.focus();
        textarea.setSelectionRange(textarea.value.length, textarea.value.length);
    }

    // ---------- Clics: likes, responder, editar, eliminar ----------
    document.addEventListener('click', async function (e) {

        // Me gusta en una publicación
        const likePub = e.target.closest('.btn-like-pub');
        if (likePub) {
            likePub.disabled = true;
            try {
                const respuesta = await post('/Like/TogglePublicacion', { publicacionId: likePub.dataset.publicacionId });
                if (!respuesta || !respuesta.ok) { if (respuesta) Ux.toast(MSG_ERROR, 'danger'); return; }

                const datos = await respuesta.json();
                likePub.querySelector('.like-count').textContent = datos.totalLikes;
                likePub.classList.toggle('btn-success', datos.liked);
                likePub.classList.toggle('btn-outline-success', !datos.liked);

                const icono = likePub.querySelector('i');
                icono.classList.toggle('bi-hand-thumbs-up-fill', datos.liked);
                icono.classList.toggle('bi-hand-thumbs-up', !datos.liked);
            } catch (err) {
                Ux.toast(MSG_ERROR, 'danger');
            } finally {
                likePub.disabled = false;
            }
            return;
        }

        // Me gusta en un comentario o respuesta
        const likeCom = e.target.closest('.btn-like-comentario');
        if (likeCom) {
            const itemLike = likeCom.closest('.comentario-item');
            likeCom.disabled = true;
            try {
                const respuesta = await post('/Like/ToggleComentario', { comentarioId: likeCom.dataset.comentarioId });
                if (!respuesta || !respuesta.ok) { if (respuesta) Ux.toast(MSG_ERROR, 'danger'); return; }

                const datos = await respuesta.json();
                likeCom.classList.toggle('fw-bold', datos.liked);
                likeCom.classList.toggle('text-success', datos.liked);
                likeCom.classList.toggle('text-muted', !datos.liked);

                const contenedor = itemLike.querySelector('.likes-comentario');
                contenedor.querySelector('.contador-like-com').textContent = datos.totalLikes;
                contenedor.classList.toggle('d-none', datos.totalLikes === 0);
            } catch (err) {
                Ux.toast(MSG_ERROR, 'danger');
            } finally {
                likeCom.disabled = false;
            }
            return;
        }

        const item = e.target.closest('.comentario-item');
        if (!item) return;

        const id = item.dataset.comentarioId;
        const vista = item.querySelector('.comentario-vista');
        const edicion = item.querySelector('.comentario-edicion');
        const texto = item.querySelector('.comentario-texto');

        // Responder
        if (e.target.closest('.btn-responder-comentario')) {
            mostrarFormularioRespuesta(item);
            return;
        }

        // Editar: cambia la burbuja por una caja de edición
        if (e.target.closest('.btn-editar-comentario')) {
            const area = edicion.querySelector('textarea');
            area.value = texto.textContent;
            vista.classList.add('d-none');
            edicion.classList.remove('d-none');
            area.focus();
            return;
        }

        // Cancelar edición
        if (e.target.closest('.btn-cancelar-edicion')) {
            edicion.classList.add('d-none');
            vista.classList.remove('d-none');
            return;
        }

        // Guardar edición
        if (e.target.closest('.btn-guardar-edicion')) {
            const boton = e.target.closest('.btn-guardar-edicion');
            const nuevo = edicion.querySelector('textarea').value.trim();
            if (!nuevo) {
                Ux.toast('El comentario no puede estar vacío.', 'danger');
                return;
            }

            boton.disabled = true;
            try {
                const respuesta = await post('/Comentario/Editar', { Id: id, Contenido: nuevo });
                if (!respuesta) return;

                if (!respuesta.ok) {
                    Ux.toast(MSG_ERROR, 'danger');
                    return;
                }

                const datos = await respuesta.json();
                texto.textContent = datos.contenido;
                edicion.classList.add('d-none');
                vista.classList.remove('d-none');
            } catch (err) {
                Ux.toast(MSG_ERROR, 'danger');
            } finally {
                boton.disabled = false;
            }
            return;
        }

        // Eliminar (si es un comentario principal, el servidor también elimina sus respuestas)
        if (e.target.closest('.btn-eliminar-comentario')) {
            const esRespuesta = item.classList.contains('es-respuesta');
            const mensaje = esRespuesta
                ? '¿Deseas eliminar esta respuesta?'
                : '¿Deseas eliminar este comentario y sus respuestas?';
            const acepto = await Ux.confirmar({
                titulo: esRespuesta ? 'Eliminar respuesta' : 'Eliminar comentario',
                mensaje: mensaje,
                textoAceptar: 'Eliminar'
            });
            if (!acepto) return;

            const panel = item.closest('.panel-comentarios');
            try {
                const respuesta = await post('/Comentario/Eliminar', { id: id });
                if (!respuesta) return;

                if (!respuesta.ok) {
                    Ux.toast(MSG_ERROR, 'danger');
                    return;
                }

                if (esRespuesta) {
                    item.remove();
                } else {
                    item.closest('.comentario-hilo').remove();
                }
                actualizarContador(panel);
            } catch (err) {
                Ux.toast(MSG_ERROR, 'danger');
            }
        }
    });
})();
