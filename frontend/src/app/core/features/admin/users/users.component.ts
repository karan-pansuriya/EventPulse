import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, takeUntil } from 'rxjs';
import { GridComponent, GridColumn } from '../../../../shared/components/grid/grid.component';
import { AdminUserService } from '../layout/admin-layout/services/admin-user.service';
import { UserListResponse } from '../layout/admin-layout/models/adminuser.model';
import { RoleCheckbox } from '../layout/admin-layout/models/roles-tab.models';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule, GridComponent],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css',
})
export class UsersComponent implements OnInit, OnDestroy {
  private adminUserService = inject(AdminUserService);
  private destroy$ = new Subject<void>();
  private cdr = inject(ChangeDetectorRef);
  private zone = inject(NgZone);

  selectedRole: string = '';
  users: UserListResponse[] = [];
  totalRecords = 0;
  currentPage = 1;
  pageSize = 10;
  loading = false;
  error: string | null = null;

  columns: GridColumn[] = [
    { header: 'ID', field: 'id', width: '60px' },
    { header: 'Name', field: 'name' },
    { header: 'Email', field: 'email' },
    { header: 'Phone', field: 'phone' },
    {
      header: 'Roles',
      field: 'roles',
      formatter: (value: unknown) => {
        const roles = value as string[];
        return roles
          .map((r) => `<span class="role-badge role-${r.toLowerCase()}">${r}</span>`)
          .join(' ');
      },
    },
    { header: 'Joined', field: 'createdAt', type: 'date' },
    { header: 'Delete', field: 'id', type: 'delete', width: '40px' },
  ];

  /* --- Role-selection modal --- */
  selectedUser: UserListResponse | null = null;
  roleCheckboxes: RoleCheckbox[] = [];
  isRemoving = false;

  ngOnInit(): void {
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onRoleFilterChange(): void {
    this.currentPage = 1;
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.error = null;

    this.adminUserService
      .getAllUsers(this.currentPage, this.pageSize, this.selectedRole || undefined)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (res) => {
          this.zone.run(() => {
            if (res.data) {
              this.users = res.data.items;
              this.totalRecords = res.data.totalCount;
            }
            this.loading = false;
            this.cdr.detectChanges();
          });
        },
        error: () => {
          this.zone.run(() => {
            this.error = 'Failed to load users.';
            this.loading = false;
            this.cdr.detectChanges();
          });
        },
      });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadUsers();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
    this.loadUsers();
  }

  onDelete(user: UserListResponse): void {
    this.selectedUser = user;
    this.roleCheckboxes = user.roleIds.map((id, i) => ({
      id,
      name: user.roles[i] || `Role ${id}`,
      checked: true,
    }));
    this.isRemoving = false;
  }

  toggleRole(id: number): void {
    const cb = this.roleCheckboxes.find((r) => r.id === id);
    if (cb) cb.checked = !cb.checked;
  }

  selectAllRoles(): void {
    this.roleCheckboxes.forEach((r) => (r.checked = true));
  }

  deselectAllRoles(): void {
    this.roleCheckboxes.forEach((r) => (r.checked = false));
  }

  closeModal(): void {
    this.selectedUser = null;
    this.roleCheckboxes = [];
  }

  confirmRemoveRoles(): void {
    if (!this.selectedUser) return;

    const toRemove = this.roleCheckboxes.filter((r) => r.checked).map((r) => r.id);
    if (toRemove.length === 0) return;

    this.isRemoving = true;

    this.adminUserService
      .removeUserRoles(this.selectedUser.id, toRemove)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.closeModal();
          this.loadUsers();
          this.cdr.detectChanges();
        },
        error: () => {
          this.isRemoving = false;
          this.cdr.detectChanges();
        },
      });
  }

  get selectedCount(): number {
    return this.roleCheckboxes.filter((r) => r.checked).length;
  }

  get willSoftDelete(): boolean {
    return this.selectedCount >= this.roleCheckboxes.length;
  }
}
