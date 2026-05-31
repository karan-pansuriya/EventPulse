import { Component, ElementRef, EventEmitter, HostListener, Input, Output, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../auth/services/auth.service';
import { RoleId } from '../../../auth/models/auth.models';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './header.component.html',
  styleUrl: './header.component.css',
})
export class HeaderComponent {
  private authService = inject(AuthService);
  private router = inject(Router);
  private elementRef = inject(ElementRef);

  @Input() mode: 'customer' | 'admin' = 'customer';
  @Output() toggleSidebar = new EventEmitter<void>();

  dropdownOpen = false;

  get userName(): string {
    return this.authService.user()?.name || 'User';
  }

  get initials(): string {
    const name = this.userName;
    const parts = name.split(' ');
    return parts.length > 1
      ? (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
      : name.substring(0, 2).toUpperCase();
  }

  get isCustomer(): boolean {
    const roleIds = this.authService.user()?.roleIds || [];
    return roleIds.includes(RoleId.Customer);
  }

  get profileRoute(): string {
    const roleIds = this.authService.user()?.roleIds || [];
    if (roleIds.includes(RoleId.Admin)) return '/admin/profile';
    if (roleIds.includes(RoleId.Organizer)) return '/organizer/profile';
    return '/attendee/profile';
  }

  onToggleSidebar(): void {
    this.toggleSidebar.emit();
  }

  toggleDropdown(): void {
    this.dropdownOpen = !this.dropdownOpen;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as Node | null;
    if (target && !this.elementRef.nativeElement.contains(target)) {
      this.dropdownOpen = false;
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
