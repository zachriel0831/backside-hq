// 選單和側邊欄功能
(function() {
  'use strict';

  // 檢查菜單權限並控制顯示（含重試機制）
  function checkMenuPermissions(retryCount) {
    if (typeof retryCount === 'undefined') {
      retryCount = 0;
    }
    
    // 檢查 DOM 是否已有菜單項目
    var allMenuItems = document.querySelectorAll('[data-menu-code]');
    if (allMenuItems.length === 0 && retryCount < 10) {
      // 菜單尚未載入，延遲 100ms 後重試
      setTimeout(function() {
        checkMenuPermissions(retryCount + 1);
      }, 100);
      return;
    }
    
    if (allMenuItems.length === 0) {
      console.warn('[菜單權限] 找不到菜單項目，跳過權限檢查');
      return;
    }
    
    fetch('/Menu/GetUserMenuPermissions', {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json'
      }
    })
    .then(function(response) {
      if (!response.ok) {
        throw new Error('權限查詢失敗');
      }
      // 先取得原始文字內容檢查
      return response.text().then(function(text) {
        console.log('[菜單權限] Response Text:', text);
        console.log('[菜單權限] Response Text Length:', text.length);
        if (!text || text.length === 0) {
          throw new Error('Response 為空');
        }
        
        text = text.replace(/,"Data"\s*:\s*null\s*}/g, '}');
        text = text.replace(/,"data"\s*:\s*null\s*}/g, '}');
        console.log('[菜單權限] Fixed Text:', text);
        
        return JSON.parse(text);
      });
    })
    .then(function(result) {
      // BaseController.Json() 使用 CamelCasePropertyNamesContractResolver
      console.log('[菜單權限] API 回傳:', result);
      if (result.Code === 1 && result.Data && result.Data.MenuCodes) {
        var allowedMenuCodes = result.Data.MenuCodes;
        
        // 重新取得所有菜單項目（確保最新）
        var currentMenuItems = document.querySelectorAll('[data-menu-code]');
        
        currentMenuItems.forEach(function(menuItem) {
          var menuCode = menuItem.getAttribute('data-menu-code');
          
          // 如果該 menu_code 不在允許清單中，隱藏該菜單
          if (allowedMenuCodes.indexOf(menuCode) === -1) {
            menuItem.style.display = 'none';
          } else {
            menuItem.style.display = '';
          }
        });
        
        console.log('[菜單權限] 已套用權限控管，允許的菜單:', allowedMenuCodes);
      }
    })
    .catch(function(error) {
      console.warn('[菜單權限] 權限查詢失敗，保持預設顯示:', error);
      // 查詢失敗時不隱藏任何菜單，保持原狀
    });
  }

  // 初始化函數
  function initMenu() {
    // 檢查並套用菜單權限
    checkMenuPermissions();
    
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
