// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

/* To make the sidebar green for active site */
document.addEventListener("DOMContentLoaded", function () {
    var currentUrl = window.location.pathname;
    var navLinks = document.querySelectorAll(".nav-link");

    navLinks.forEach(function (link) {
        if (link.getAttribute("href") === currentUrl) {
            link.classList.add("active");
            link.style.backgroundColor = 'var(--primary-green)'; // Using the css variabels and custom styles.
            link.style.color = 'var(--text-light)'; // Using the css variabels and custom styles.
        }
    });
});