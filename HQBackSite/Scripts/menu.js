// 選單和側邊欄功能
(function() {
  'use strict';

  // 初始化函數
  function initMenu() {
    // 側邊欄切換功能
    const toggleSidebarBtn = document.getElementById('toggleSidebarBtn');
    const sidebar = document.getElementById('sidebar');
    const sidebarOverlay = document.getElementById('sidebarOverlay');

    if (toggleSidebarBtn && sidebar && sidebarOverlay) {
      // 檢查是否已經綁定過事件
      if (!toggleSidebarBtn.hasAttribute('data-listener-bound')) {
        toggleSidebarBtn.setAttribute('data-listener-bound', 'true');
        toggleSidebarBtn.addEventListener('click', function() {
          sidebar.classList.toggle('sidebar-open');
          sidebarOverlay.style.display = sidebar.classList.contains('sidebar-open') ? 'block' : 'none';
        });
      }

      if (!sidebarOverlay.hasAttribute('data-listener-bound')) {
        sidebarOverlay.setAttribute('data-listener-bound', 'true');
        sidebarOverlay.addEventListener('click', function() {
          sidebar.classList.remove('sidebar-open');
          this.style.display = 'none';
        });
      }
    }

    // 選單群組展開/收合功能
    const menuList = document.getElementById('menuList');
    if (menuList) {
      const groupHeaders = menuList.querySelectorAll('.nav-group-header');
      
      groupHeaders.forEach(header => {
        // 檢查是否已經綁定過事件
        if (header.hasAttribute('data-listener-bound')) {
          return;
        }
        header.setAttribute('data-listener-bound', 'true');
        
        header.addEventListener('click', function() {
          const submenu = this.nextElementSibling;
          const icon = this.querySelector('i');
          
          if (submenu && submenu.classList.contains('nav-submenu')) {
            // 檢查當前狀態：如果 display 是 none 或空字串，則視為收合
            const currentDisplay = submenu.style.display || window.getComputedStyle(submenu).display;
            const isExpanded = currentDisplay !== 'none';
            
            if (isExpanded) {
              submenu.style.display = 'none';
              if (icon) {
                icon.className = 'fas fa-chevron-down';
              }
            } else {
              submenu.style.display = 'block';
              if (icon) {
                icon.className = 'fas fa-chevron-up';
              }
            }
          }
        });
      });

      // 在行動裝置上，點擊選單項目後關閉側邊欄
      const navLinks = menuList.querySelectorAll('.nav-link[data-module-id]');
      navLinks.forEach(link => {
        if (link.hasAttribute('data-link-bound')) {
          return;
        }
        link.setAttribute('data-link-bound', 'true');
        
        link.addEventListener('click', function() {
          if (window.innerWidth < 768 && sidebar && sidebarOverlay) {
            sidebar.classList.remove('sidebar-open');
            sidebarOverlay.style.display = 'none';
          }
        });
      });
    }
  }

  // DOM 載入完成後初始化
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initMenu);
  } else {
    initMenu();
  }
})();
