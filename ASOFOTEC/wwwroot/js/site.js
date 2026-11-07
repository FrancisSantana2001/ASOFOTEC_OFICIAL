// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.querySelectorAll(".btn-like")
    .forEach(button => {
        button.addEventListener("click", async function () {
            const solucionId = this.dataset.solucionId;

            const response = await fetch('/Like/Toggle', {
                method: 'POST',
                credentials: 'same-origin',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded'
                },
                body: `solucionId=${solucionId}`
            });

            if (response.status === 401) {
                Ux.irALogin();
                return;
            }

            const data = await response.json();

            if (data.success) {
                // ✅ camelCase correcto + selector con punto
                this.querySelector(".like-count").innerText = data.totalLikes;

                // Botón verde: contorno cuando no has dado like, relleno cuando sí
                this.classList.toggle("btn-success", data.liked);
                this.classList.toggle("btn-outline-success", !data.liked);

                const icono = this.querySelector("i");
                if (icono) {
                    icono.classList.toggle("bi-hand-thumbs-up-fill", data.liked);
                    icono.classList.toggle("bi-hand-thumbs-up", !data.liked);
                }
            }
        });
    });

