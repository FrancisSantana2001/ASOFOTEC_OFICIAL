// Chat en tiempo real (SignalR): mensajes, "escribiendo...", visto / no visto y bloqueo de personas.
// Se carga en todas las páginas con sesión iniciada para mantener al día el contador de mensajes.
(function () {
    'use strict';

    const MSG_ERROR = 'No se pudo completar la acción. Inténtalo de nuevo.';

    function obtenerToken() {
        const input = document.querySelector('#antiforgery-form input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

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

    async function obtenerJson(url) {
        const respuesta = await fetch(url, {
            credentials: 'same-origin',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });
        if (respuesta.status === 401) { Ux.irALogin(); return null; }
        return respuesta.ok ? respuesta.json() : null;
    }

    // ============================================================ Bloquear / desbloquear
    // Funciona aunque SignalR no haya cargado

    document.addEventListener('click', async function (e) {
        const boton = e.target.closest('.btn-bloquear');
        if (!boton) return;

        const estabaBloqueado = boton.dataset.bloqueado === '1';
        const nombre = boton.dataset.nombre || 'esta persona';

        const acepto = await Ux.confirmar({
            titulo: estabaBloqueado ? 'Desbloquear' : 'Bloquear persona',
            mensaje: estabaBloqueado
                ? '¿Quieres desbloquear a ' + nombre + '?'
                : '¿Bloquear a ' + nombre + '? Dejarán de seguirse y no podrán escribirse.',
            textoAceptar: estabaBloqueado ? 'Desbloquear' : 'Bloquear',
            peligro: !estabaBloqueado
        });
        if (!acepto) return;

        try {
            const respuesta = await post('/Bloqueo/Alternar', { id: boton.dataset.usuarioId });
            if (!respuesta) return;
            if (!respuesta.ok) {
                Ux.toast(MSG_ERROR, 'danger');
                return;
            }
            window.location.reload();
        } catch (err) {
            Ux.toast(MSG_ERROR, 'danger');
        }
    });

    // ============================================================ Contador de mensajes sin leer

    function actualizarContador(total) {
        document.querySelectorAll('.msg-badge').forEach(function (badge) {
            badge.textContent = total > 99 ? '99+' : String(total);
            badge.classList.toggle('d-none', total === 0);
        });
    }

    const hayIcono = document.querySelector('.msg-btn') !== null;
    const app = document.getElementById('chat-app');

    if (hayIcono) {
        // Carga inicial (no depende de SignalR)
        obtenerJson('/Chat/NoLeidos').then(function (d) { if (d) actualizarContador(d.noLeidos); }).catch(function () { });
    }

    if (!hayIcono && !app) return;   // sin sesión: no hay chat

    if (typeof signalR === 'undefined') {
        if (app) {
            Ux.toast('No se pudo cargar el chat en tiempo real. Instala la librería SignalR (LibMan: Restore Client-Side Libraries).', 'danger');
        }
        return;
    }

    const iconoMensajes = document.querySelector('.msg-btn');
    const miId = app ? Number(app.dataset.miId) : Number(iconoMensajes ? iconoMensajes.dataset.miId : 0);

    const conexion = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/chat')
        .withAutomaticReconnect([0, 2000, 5000, 10000, 20000])
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    function conectado() {
        return conexion.state === signalR.HubConnectionState.Connected;
    }

    conexion.on('NoLeidos', actualizarContador);

    // ============================================================ Fuera de la pantalla de chat

    if (!app) {
        conexion.on('MensajeRecibido', function (m) {
            if (m.remitenteId !== miId) {
                Ux.toast('Nuevo mensaje de ' + m.remitenteNombre, 'info');
            }
        });

        conexion.start().catch(function () { /* sin conexión: el contador inicial ya se cargó */ });
        return;
    }

    // ============================================================ Pantalla de chat

    const convId = Number(app.dataset.conversacionId || 0);
    const otroId = Number(app.dataset.otroId || 0);
    const nombreOtro = app.dataset.otroNombre || '';
    const listaEl = document.getElementById('chat-lista');

    // ---------- Formato de fechas (siempre en la hora local de quien mira)

    const mismaFecha = function (a, b) {
        return a.getFullYear() === b.getFullYear() && a.getMonth() === b.getMonth() && a.getDate() === b.getDate();
    };

    function formatoHora(fecha) {
        return fecha.toLocaleTimeString('es-DO', { hour: '2-digit', minute: '2-digit' });
    }

    function etiquetaDia(fecha) {
        const hoy = new Date();
        const ayer = new Date();
        ayer.setDate(hoy.getDate() - 1);

        if (mismaFecha(fecha, hoy)) return 'Hoy';
        if (mismaFecha(fecha, ayer)) return 'Ayer';
        return fecha.toLocaleDateString('es-DO', { weekday: 'long', day: 'numeric', month: 'long' });
    }

    // Hora corta de la lista: hora si es de hoy, "Ayer" o día/mes
    function formatearTiemposLista() {
        document.querySelectorAll('#chat-lista time[data-iso]').forEach(function (el) {
            const fecha = new Date(el.dataset.iso);
            if (isNaN(fecha)) return;

            const hoy = new Date();
            const ayer = new Date();
            ayer.setDate(hoy.getDate() - 1);

            if (mismaFecha(fecha, hoy)) el.textContent = formatoHora(fecha);
            else if (mismaFecha(fecha, ayer)) el.textContent = 'Ayer';
            else el.textContent = fecha.toLocaleDateString('es-DO', { day: '2-digit', month: '2-digit' });
        });
    }

    let refrescandoLista = false;
    async function refrescarLista() {
        if (refrescandoLista || !listaEl) return;
        refrescandoLista = true;
        try {
            const respuesta = await fetch('/Chat/Lista?activa=' + convId, {
                credentials: 'same-origin',
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (respuesta.ok) {
                // HTML generado por el servidor (textos ya codificados)
                listaEl.innerHTML = await respuesta.text();
                formatearTiemposLista();
            }
        } catch (err) { /* se reintentará con el siguiente mensaje */ }
        refrescandoLista = false;
    }

    formatearTiemposLista();

    // ---------- Sin conversación abierta: solo se atienden los mensajes que llegan

    if (!convId) {
        conexion.on('MensajeRecibido', function (m) {
            if (m.remitenteId !== miId) Ux.toast('Nuevo mensaje de ' + m.remitenteNombre, 'info');
            refrescarLista();
        });
        conexion.on('EstadoChatCambiado', refrescarLista);
        conexion.start().catch(function () {
            Ux.toast('No se pudo conectar al chat. Recarga la página.', 'danger');
        });
        return;
    }

    // ---------- Conversación abierta

    const contenedor = document.getElementById('chat-mensajes');
    const flujo = document.getElementById('chat-flujo');
    const cargarMasCaja = document.getElementById('chat-cargar-mas-caja');
    const cargarMasBoton = document.getElementById('chat-cargar-mas');
    const indicador = document.getElementById('chat-escribiendo');
    const formulario = document.getElementById('chat-form');
    const entrada = document.getElementById('chat-input');
    const botonEnviar = document.getElementById('chat-enviar');

    let mensajes = [];
    let hayMas = false;

    try {
        const datos = JSON.parse(document.getElementById('chat-datos').textContent);
        mensajes = datos.mensajes || [];
        hayMas = !!datos.hayMas;
    } catch (err) { /* sin mensajes iniciales */ }

    function cercaDelFinal() {
        return contenedor.scrollHeight - contenedor.scrollTop - contenedor.clientHeight < 120;
    }

    function crearElemento(m, fecha) {
        const mio = m.remitenteId === miId;

        const fila = document.createElement('div');
        fila.className = 'chat-msg ' + (mio ? 'mio' : 'otro');
        fila.dataset.id = m.id;

        const burbuja = document.createElement('div');
        burbuja.className = 'chat-burbuja';

        const texto = document.createElement('div');
        texto.className = 'chat-texto';
        texto.textContent = m.texto;               // textContent: nunca se interpreta como HTML

        const meta = document.createElement('div');
        meta.className = 'chat-meta';

        const hora = document.createElement('span');
        hora.textContent = formatoHora(fecha);
        meta.appendChild(hora);

        // Solo en mis mensajes: visto (doble marca azul) o enviado (una marca)
        if (mio) {
            const marca = document.createElement('i');
            marca.className = 'bi chat-visto ' + (m.leido ? 'bi-check2-all visto' : 'bi-check2');
            marca.title = m.leido ? 'Visto' : 'Enviado';
            meta.appendChild(marca);
        }

        burbuja.appendChild(texto);
        burbuja.appendChild(meta);
        fila.appendChild(burbuja);
        return fila;
    }

    function crearSeparador(fecha) {
        const separador = document.createElement('div');
        separador.className = 'chat-dia';
        const etiqueta = document.createElement('span');
        etiqueta.textContent = etiquetaDia(fecha);
        separador.appendChild(etiqueta);
        return separador;
    }

    // Dibuja de nuevo todos los mensajes (con sus separadores de día)
    // opciones.abajo: baja al final | opciones.anteriores: mantiene la posición al agregar arriba
    function dibujar(opciones) {
        opciones = opciones || {};
        const alturaAntes = contenedor.scrollHeight;
        const posicionAntes = contenedor.scrollTop;

        const fragmento = document.createDocumentFragment();
        let diaActual = null;

        mensajes.forEach(function (m) {
            const fecha = new Date(m.fecha);
            if (diaActual === null || !mismaFecha(diaActual, fecha)) {
                diaActual = fecha;
                fragmento.appendChild(crearSeparador(fecha));
            }
            fragmento.appendChild(crearElemento(m, fecha));
        });

        flujo.replaceChildren(fragmento);
        cargarMasCaja.classList.toggle('d-none', !hayMas);

        if (opciones.abajo) {
            contenedor.scrollTop = contenedor.scrollHeight;
        } else if (opciones.anteriores) {
            contenedor.scrollTop = contenedor.scrollHeight - alturaAntes + posicionAntes;
        } else {
            contenedor.scrollTop = posicionAntes;
        }
    }

    dibujar({ abajo: true });

    // ---------- Visto

    function hayPendientesPorLeer() {
        return mensajes.some(function (m) { return m.remitenteId !== miId && !m.leido; });
    }

    async function marcarLeidos() {
        if (document.visibilityState !== 'visible' || !conectado()) return;

        try {
            await conexion.invoke('MarcarLeidos', convId);
            mensajes.forEach(function (m) { if (m.remitenteId !== miId) m.leido = true; });
        } catch (err) { /* se reintenta al recibir el siguiente mensaje */ }
    }

    document.addEventListener('visibilitychange', function () {
        if (document.visibilityState === 'visible') marcarLeidos();
    });
    window.addEventListener('focus', marcarLeidos);

    // ---------- Eventos del servidor

    conexion.on('MensajeRecibido', function (m) {
        if (m.conversacionId !== convId) {
            if (m.remitenteId !== miId) Ux.toast('Nuevo mensaje de ' + m.remitenteNombre, 'info');
            refrescarLista();
            return;
        }

        if (mensajes.some(function (x) { return x.id === m.id; })) return;

        const eraMio = m.remitenteId === miId;
        const bajar = eraMio || cercaDelFinal();

        mensajes.push(m);
        ocultarEscribiendo();
        dibujar({ abajo: bajar });

        if (!eraMio) marcarLeidos();
        refrescarLista();
    });

    conexion.on('MensajesLeidos', function (id) {
        if (id !== convId) return;
        mensajes.forEach(function (m) { if (m.remitenteId === miId) m.leido = true; });
        dibujar();
    });

    let temporizadorIndicador = null;

    function ocultarEscribiendo() {
        clearTimeout(temporizadorIndicador);
        indicador.classList.add('d-none');
    }

    conexion.on('Escribiendo', function (id, escribiendo) {
        if (id !== convId) return;

        if (!escribiendo) {
            ocultarEscribiendo();
            return;
        }

        const primerNombre = nombreOtro.split(' ')[0] || 'La otra persona';
        indicador.textContent = primerNombre + ' está escribiendo...';
        indicador.classList.remove('d-none');

        // Si deja de llegar la señal, el aviso desaparece solo
        clearTimeout(temporizadorIndicador);
        temporizadorIndicador = setTimeout(ocultarEscribiendo, 5000);

        if (cercaDelFinal()) contenedor.scrollTop = contenedor.scrollHeight;
    });

    // Bloqueo o cambio de amistad: se recarga para mostrar el estado correcto
    conexion.on('EstadoChatCambiado', function (id) {
        if (id === otroId) window.location.reload();
        else refrescarLista();
    });

    // ---------- Cargar mensajes anteriores

    cargarMasBoton.addEventListener('click', async function () {
        if (!mensajes.length) return;
        cargarMasBoton.disabled = true;

        try {
            const datos = await obtenerJson('/Chat/Mensajes?conversacionId=' + convId + '&antesDeId=' + mensajes[0].id);
            if (!datos) {
                Ux.toast(MSG_ERROR, 'danger');
                return;
            }
            mensajes = datos.mensajes.concat(mensajes);
            hayMas = !!datos.hayMas;
            dibujar({ anteriores: true });
        } catch (err) {
            Ux.toast(MSG_ERROR, 'danger');
        } finally {
            cargarMasBoton.disabled = false;
        }
    });

    // ---------- Enviar y "escribiendo..."

    function autoajustar() {
        entrada.style.height = 'auto';
        entrada.style.height = Math.min(entrada.scrollHeight, 120) + 'px';
    }

    let ultimoAviso = 0;
    let temporizadorFin = null;

    function avisarEscribiendo(valor) {
        if (conectado()) conexion.invoke('Escribiendo', convId, valor).catch(function () { });
    }

    if (formulario && entrada) {
        entrada.addEventListener('input', function () {
            autoajustar();

            // Se avisa como mucho cada 2,5 s y se apaga si se deja de escribir
            const ahora = Date.now();
            if (ahora - ultimoAviso > 2500) {
                ultimoAviso = ahora;
                avisarEscribiendo(true);
            }
            clearTimeout(temporizadorFin);
            temporizadorFin = setTimeout(function () {
                ultimoAviso = 0;
                avisarEscribiendo(false);
            }, 3000);
        });

        // Enter envía; Shift+Enter hace salto de línea
        entrada.addEventListener('keydown', function (e) {
            if (e.key === 'Enter' && !e.shiftKey && !e.isComposing) {
                e.preventDefault();
                formulario.requestSubmit();
            }
        });

        formulario.addEventListener('submit', async function (e) {
            e.preventDefault();

            const texto = entrada.value.trim();
            if (!texto) return;

            if (!conectado()) {
                Ux.toast('Sin conexión. Reconectando...', 'danger');
                return;
            }

            botonEnviar.disabled = true;
            try {
                const error = await conexion.invoke('EnviarMensaje', convId, texto);
                if (error) {
                    Ux.toast(error, 'danger');
                } else {
                    entrada.value = '';
                    autoajustar();
                    clearTimeout(temporizadorFin);
                    ultimoAviso = 0;
                    avisarEscribiendo(false);
                }
            } catch (err) {
                Ux.toast(MSG_ERROR, 'danger');
            } finally {
                botonEnviar.disabled = false;
                entrada.focus();
            }
        });
    }

    // ---------- Reconexión: se recuperan los mensajes que llegaron mientras no había conexión

    conexion.onreconnected(async function () {
        try {
            const ultimo = mensajes.length ? mensajes[mensajes.length - 1].id : 0;
            const datos = await obtenerJson('/Chat/Mensajes?conversacionId=' + convId + '&despuesDeId=' + ultimo);
            if (datos && datos.mensajes.length) {
                datos.mensajes.forEach(function (m) {
                    if (!mensajes.some(function (x) { return x.id === m.id; })) mensajes.push(m);
                });
                dibujar({ abajo: true });
            }
        } catch (err) { /* se verá al recargar */ }

        marcarLeidos();
        refrescarLista();
    });

    conexion.onclose(function () {
        Ux.toast('Se perdió la conexión con el chat. Recarga la página.', 'danger');
    });

    conexion.start()
        .then(function () {
            if (hayPendientesPorLeer() || mensajes.length) marcarLeidos();
            if (entrada) entrada.focus();
        })
        .catch(function () {
            Ux.toast('No se pudo conectar al chat. Recarga la página.', 'danger');
        });
})();
