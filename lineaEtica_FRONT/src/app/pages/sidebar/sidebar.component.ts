import { Component, computed, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, IsActiveMatchOptions } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { LayoutStateService } from '../layout/layout-state.service';
import { ThemeService } from '../../core/theme.service';
import { MENU_ITEMS } from '../menu.config';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent {
  private auth = inject(AuthService);
  readonly layoutState = inject(LayoutStateService);
  readonly theme = inject(ThemeService);

  readonly userName = computed(() => this.auth.getCurrentUserName());
  readonly userRole = computed(() => this.auth.getCurrentUserRole());

  readonly menuItems = computed(() => {
    const role = this.userRole();
    return MENU_ITEMS.filter(item =>
      item.roles.length === 0 || item.roles.includes(role)
    );
  });

  readonly exactOptions: IsActiveMatchOptions = {
    paths: 'exact',
    queryParams: 'exact',
    fragment: 'ignored',
    matrixParams: 'ignored',
  };

  readonly defaultOptions: IsActiveMatchOptions = {
    paths: 'exact',
    queryParams: 'ignored',
    fragment: 'ignored',
    matrixParams: 'ignored',
  };

  onNavClick(): void {
    if (typeof window !== 'undefined' && window.innerWidth < 992)
      this.layoutState.close();
  }
}
