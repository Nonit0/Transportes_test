import { Injectable, Renderer2, RendererFactory2 } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private renderer: Renderer2;
  private themeKey = 'user-theme';

  constructor(rendererFactory: RendererFactory2) {
    this.renderer = rendererFactory.createRenderer(null, null);
  }

  /**
   * Inicializa el tema basándose en localStorage o en la preferencia del sistema.
   */
  initTheme() {
    const savedTheme = localStorage.getItem(this.themeKey);
    if (savedTheme) {
      this.applyTheme(savedTheme as 'light' | 'dark');
    } else {
      const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
      this.applyTheme(prefersDark ? 'dark' : 'light');
    }

    // Escuchar cambios en la preferencia del sistema por si el usuario la cambia
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', e => {
      if (!localStorage.getItem(this.themeKey)) {
        this.applyTheme(e.matches ? 'dark' : 'light');
      }
    });
  }

  toggleTheme() {
    const currentTheme = document.body.getAttribute('cds-theme') === 'dark' ? 'light' : 'dark';
    this.applyTheme(currentTheme);
    localStorage.setItem(this.themeKey, currentTheme);
  }

  private applyTheme(theme: 'light' | 'dark') {
    this.renderer.setAttribute(document.body, 'cds-theme', theme);
    // Para Clarity Classic, también solemos añadir una clase
    if (theme === 'dark') {
      this.renderer.addClass(document.body, 'dark-theme');
    } else {
      this.renderer.removeClass(document.body, 'dark-theme');
    }
  }

  isDark(): boolean {
    return document.body.getAttribute('cds-theme') === 'dark';
  }
}
