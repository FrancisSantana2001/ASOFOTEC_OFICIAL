// Registro: calcula la edad exacta a partir de la fecha de nacimiento y bloquea a menores de 15 años.
(function () {
    'use strict';

    const EDAD_MINIMA = 15;
    const EDAD_MAXIMA = 120;

    const campo = document.getElementById('FechaNacimiento');
    const mensaje = document.getElementById('edad-calculada');
    if (!campo || !mensaje) return;

    const formulario = campo.closest('form');

    // "yyyy-mm-dd" -> Date local (evita desfases de zona horaria)
    function leerFecha(valor) {
        const partes = valor.split('-').map(Number);
        if (partes.length !== 3 || partes.some(isNaN)) return null;
        return new Date(partes[0], partes[1] - 1, partes[2]);
    }

    // Años cumplidos a hoy: si todavía no llegó el cumpleaños, no suma el año
    function calcularEdad(nacimiento) {
        const hoy = new Date();
        let edad = hoy.getFullYear() - nacimiento.getFullYear();
        const mes = hoy.getMonth() - nacimiento.getMonth();
        if (mes < 0 || (mes === 0 && hoy.getDate() < nacimiento.getDate())) edad--;
        return edad;
    }

    function actualizar() {
        const fecha = campo.value ? leerFecha(campo.value) : null;

        if (!fecha) {
            mensaje.textContent = 'Debes tener al menos ' + EDAD_MINIMA + ' años.';
            mensaje.className = 'edad-calculada text-muted mt-1';
            campo.setCustomValidity('');
            return true;
        }

        const edad = calcularEdad(fecha);

        if (fecha > new Date() || edad < EDAD_MINIMA) {
            mensaje.textContent = 'Debes tener al menos ' + EDAD_MINIMA + ' años para crear una cuenta.';
            mensaje.className = 'edad-calculada text-danger mt-1';
            campo.setCustomValidity('Edad mínima: ' + EDAD_MINIMA + ' años.');
            return false;
        }

        if (edad > EDAD_MAXIMA) {
            mensaje.textContent = 'La fecha de nacimiento no es válida.';
            mensaje.className = 'edad-calculada text-danger mt-1';
            campo.setCustomValidity('Fecha no válida.');
            return false;
        }

        mensaje.textContent = 'Edad: ' + edad + (edad === 1 ? ' año' : ' años');
        mensaje.className = 'edad-calculada text-success mt-1';
        campo.setCustomValidity('');
        return true;
    }

    campo.addEventListener('input', actualizar);
    campo.addEventListener('change', actualizar);

    if (formulario) {
        formulario.addEventListener('submit', function (e) {
            if (!actualizar()) {
                e.preventDefault();
                campo.focus();
            }
        });
    }

    actualizar();
})();
