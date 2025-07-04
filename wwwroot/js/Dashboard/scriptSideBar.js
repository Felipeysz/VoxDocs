  document.addEventListener('DOMContentLoaded', () => {
    const sidebar = document.getElementById('sidebar');
    const toggle  = document.getElementById('sidebarToggle');

    if (sidebar && toggle) {
      const icon = toggle.querySelector('.material-symbols-outlined');

      toggle.addEventListener('click', () => {
        const collapsed = sidebar.classList.toggle('collapsed');
        icon.textContent = collapsed ? 'menu' : 'menu_open';
      });
    }
  });