const toggle = document.querySelector(".nav-toggle");
const nav = document.querySelector(".site-nav");

if (toggle && nav) {
  toggle.addEventListener("click", () => {
    const isOpen = nav.classList.toggle("open");
    toggle.setAttribute("aria-expanded", String(isOpen));
  });

  const menus = nav.querySelectorAll(".nav-menu");

  menus.forEach((menu) => {
    menu.addEventListener("toggle", () => {
      if (!menu.open) return;

      menus.forEach((otherMenu) => {
        if (otherMenu !== menu) otherMenu.removeAttribute("open");
      });
    });
  });

  nav.querySelectorAll("a").forEach((link) => {
    link.addEventListener("click", () => {
      nav.classList.remove("open");
      toggle.setAttribute("aria-expanded", "false");
    });
  });

  document.addEventListener("click", (event) => {
    if (nav.contains(event.target) || toggle.contains(event.target)) return;
    menus.forEach((menu) => menu.removeAttribute("open"));
  });
}
