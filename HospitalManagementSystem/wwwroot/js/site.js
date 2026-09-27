// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {
	document.querySelectorAll("a[href]").forEach((link) => {
		link.addEventListener("click", (event) => {
			if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
				return;
			}

			const target = new URL(link.href, window.location.href);
			if (target.origin !== window.location.origin || target.hash || link.target === "_blank" || link.hasAttribute("download")) {
				return;
			}

			document.body.classList.add("is-leaving");
		});
	});
});

window.addEventListener("pageshow", () => {
	document.body.classList.remove("is-leaving");
});
