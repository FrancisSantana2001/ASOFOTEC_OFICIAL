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
                window.location.href = '/Auth/Login';
                return;
            }

            const data = await response.json();

            if (data.success) {
                // ✅ camelCase correcto + selector con punto
                this.querySelector(".like-count").innerText = data.totalLikes;

                if (data.liked) {
                    this.classList.remove("btn-outline-primary");
                    this.classList.add("btn-primary");
                } else {
                    this.classList.remove("btn-primary");
                    this.classList.add("btn-outline-primary");
                }
            }
        });
    });

