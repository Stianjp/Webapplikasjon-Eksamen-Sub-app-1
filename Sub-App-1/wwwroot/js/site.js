// Highlight the active sidebar link
document.addEventListener("DOMContentLoaded", function () {
    const currentUrl = window.location.pathname;
    const navLinks = document.querySelectorAll(".nav-link");

    navLinks.forEach(function (link) {
        if (link.getAttribute("href") === currentUrl) {
            link.classList.add("active");
            link.style.backgroundColor = 'var(--primary-green)'; // Highlight active link
            link.style.color = 'var(--text-black)';
        }
    });
});

document.addEventListener("DOMContentLoaded", function () {
    const navbarToggler = document.querySelector(".navbar-toggler");
    const navbarMenu = document.getElementById("navbarMenu");

    // Ensure the toggle button works natively
    navbarToggler.addEventListener("click", function () {
        const isExpanded = navbarToggler.getAttribute("aria-expanded") === "true";
        navbarToggler.setAttribute("aria-expanded", !isExpanded);
    });

    // Close the menu when a link is clicked
    const navLinks = navbarMenu.querySelectorAll(".nav-link");
    navLinks.forEach(link => {
        link.addEventListener("click", function () {
            const isExpanded = navbarMenu.classList.contains("show");
            if (isExpanded) {
                navbarToggler.click(); // Trigger the toggle button to close
            }
        });
    });
});

// Search functionality for filtering table rows
document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('keyup', function () {
            const filter = this.value.toUpperCase();
            const table = document.querySelector('table');
            const trs = table.getElementsByTagName('tr');

            for (let i = 1; i < trs.length; i++) { // Start from 1 to skip header row
                const tds = trs[i].getElementsByTagName('td');
                let found = false;

                for (let j = 0; j < tds.length; j++) {
                    if (tds[j]) {
                        const txtValue = tds[j].textContent || tds[j].innerText;
                        if (txtValue.toUpperCase().indexOf(filter) > -1) {
                            found = true;
                            break;
                        }
                    }
                }
                trs[i].style.display = found ? '' : 'none';
            }
        });
    }
});