import { Component } from '@angular/core';
import { ThemeService } from './services/theme.service';
import { ClarityIcons, userIcon, homeIcon, archiveIcon, truckIcon, tagsIcon, usersIcon, storeIcon, sunIcon, moonIcon, bundleIcon, mapMarkerIcon, exclamationCircleIcon, mobileIcon, envelopeIcon } from '@cds/core/icon';

ClarityIcons.addIcons(userIcon, homeIcon, archiveIcon, truckIcon, tagsIcon, usersIcon, storeIcon, sunIcon, moonIcon, bundleIcon, mapMarkerIcon, exclamationCircleIcon, mobileIcon, envelopeIcon);

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  constructor(public themeService: ThemeService) {
    ClarityIcons.addIcons(userIcon, homeIcon, archiveIcon, truckIcon, tagsIcon, usersIcon, storeIcon, sunIcon, moonIcon, bundleIcon, mapMarkerIcon, exclamationCircleIcon, mobileIcon, envelopeIcon);
    this.themeService.initTheme();
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }
}