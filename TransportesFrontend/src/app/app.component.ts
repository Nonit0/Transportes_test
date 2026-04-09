import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { ThemeService } from './services/theme.service';
import { AuthService } from './services/auth.service';
import { ClarityIcons, userIcon, homeIcon, archiveIcon, truckIcon, tagsIcon, usersIcon, storeIcon, sunIcon, moonIcon, bundleIcon, mapMarkerIcon, exclamationCircleIcon, mobileIcon, envelopeIcon, loginIcon, logoutIcon } from '@cds/core/icon';

ClarityIcons.addIcons(userIcon, homeIcon, archiveIcon, truckIcon, tagsIcon, usersIcon, storeIcon, sunIcon, moonIcon, bundleIcon, mapMarkerIcon, exclamationCircleIcon, mobileIcon, envelopeIcon, loginIcon, logoutIcon);

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  constructor(
    public themeService: ThemeService,
    public authService: AuthService,
    private router: Router
  ) {
    this.themeService.initTheme();
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }

  hacerLogout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}