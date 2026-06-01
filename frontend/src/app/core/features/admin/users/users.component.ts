import { Component, OnInit, OnDestroy, inject, ChangeDetectorRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject, takeUntil } from 'rxjs';
import { GridComponent, GridColumn, GridActionItem } from '../../../../shared/components/grid/grid.component';
import { AdminUserService, UserListResponse } from '../services/admin-user.service';

interface RoleTab {
  label: string;
  value: string | null;
}

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, GridComponent],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css',
})
export class UsersComponent implements OnInit, OnDestroy {
  private adminUserService = inject(AdminUserService);
  private cdr = inject(ChangeDetectorRef);
  private zone = inject(NgZone);
  private destroy$ = new Subject<void>();

  roleTabs: RoleTab[] = [
    { label: 'All', value: null },
    { label: 'Admin', value: 'Admin' },
    { label: 'Organizer', value: 'Organizer' },
    { label: 'Customer', value: 'Customer' },
  ];

  activeTab: string | null = null;
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
        return roles.map(r => `<span class="role-badge role-${r.toLowerCase()}">${r}</span>`).join(' ');
      },
    },
    {
      header: 'Status',
      field: 'isActive',
      formatter: (value: unknown) => {
        const active = value as boolean;
        return active
          ? '<span class="badge badge-active">Active</span>'
          : '<span class="badge badge-inactive">Inactive</span>';
      },
    },
    { header: 'Joined', field: 'createdAt', type: 'date' },
    { header: 'Actions', field: 'id', type: 'action' },
  ];

  getRowActionItems: (row: UserListResponse) => GridActionItem[] = () => [
    { label: 'Delete', icon: 'bi-trash', emit: 'delete' },
  ];

  ngOnInit(): void {
    this.loadUsers();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  selectTab(tab: RoleTab): void {
    this.activeTab = tab.value;
    this.currentPage = 1;
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.error = null;

    this.adminUserService
      .getAllUsers(this.currentPage, this.pageSize, this.activeTab ?? undefined)
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
    if (!confirm(`Delete user "${user.name}"? This action cannot be undone.`)) return;

    this.adminUserService
      .deleteUser(user.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.zone.run(() => {
          this.loadUsers();
          this.cdr.detectChanges();
        });
      });
  }
}
