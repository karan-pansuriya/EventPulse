import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ProfileService, UserDto, UpdateUserRequest } from '../../attendee/profile/profile.service';
import { ToastService } from '../../../../shared/services/toast.service';

@Component({
  selector: 'app-admin-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="profile-page">
      <div class="container py-3 py-md-4">

        <a routerLink="/admin/dashboard" class="btn btn-sm btn-outline-secondary mb-3">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="me-1">
            <polyline points="15 18 9 12 15 6" />
          </svg>
          Back
        </a>

        <div class="d-flex align-items-center gap-3 mb-4">
          <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="#7c3aed" stroke-width="1.5">
            <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" />
            <circle cx="12" cy="7" r="4" />
          </svg>
          <h2 class="fw-bold mb-0">My Profile</h2>
        </div>

        @if (isLoading) {
          <div class="text-center py-5">
            <div class="spinner-border text-primary" role="status">
              <span class="visually-hidden">Loading...</span>
            </div>
            <p class="text-muted mt-2">Loading profile…</p>
          </div>
        }

        @if (!isLoading && profile) {
          <div class="profile-card">

            <div class="text-center pt-4 pb-3">
              <div class="profile-avatar mx-auto mb-2">{{ getInitials(profile.name) }}</div>
              <h4 class="fw-bold mb-0">{{ profile.name }}</h4>
              <small class="text-muted">{{ profile.email }}</small>
            </div>

            <hr class="my-0" />

            @if (!isEditing) {
              <div class="p-4">
                <div class="info-row">
                  <span class="info-label">Name</span>
                  <span class="info-value">{{ profile.name }}</span>
                </div>
                <div class="info-row">
                  <span class="info-label">Email</span>
                  <span class="info-value">{{ profile.email }}</span>
                </div>
                <div class="info-row">
                  <span class="info-label">Phone</span>
                  <span class="info-value">{{ profile.phone || 'Not set' }}</span>
                </div>
                <div class="info-row">
                  <span class="info-label">Member since</span>
                  <span class="info-value">{{ profile.createdAt | date:'longDate' }}</span>
                </div>

                <button class="btn btn-primary w-100 mt-4" (click)="startEditing()">
                  <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="me-1">
                    <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
                    <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
                  </svg>
                  Edit Profile
                </button>
              </div>
            }

            @if (isEditing) {
              <div class="p-4">
                <div class="mb-3">
                  <label class="form-label fw-semibold">Name</label>
                  <input type="text" class="form-control" [(ngModel)]="editForm.name"
                         placeholder="Your name" required />
                </div>
                <div class="mb-3">
                  <label class="form-label fw-semibold">Email</label>
                  <input type="email" class="form-control" [value]="profile.email"
                         disabled readonly />
                  <small class="text-muted">Email cannot be changed.</small>
                </div>
                <div class="mb-3">
                  <label class="form-label fw-semibold">Phone</label>
                  <input type="tel" class="form-control" [(ngModel)]="editForm.phone"
                         placeholder="Phone number (optional)" />
                </div>

                <div class="d-flex gap-2 mt-4">
                  <button class="btn btn-primary flex-fill" (click)="saveProfile()"
                          [disabled]="isSaving || !editForm.name.trim()">
                    @if (isSaving) {
                      <span class="spinner-border spinner-border-sm me-1"></span>
                    }
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="me-1">
                      <polyline points="20 6 9 17 4 12" />
                    </svg>
                    Save
                  </button>
                  <button class="btn btn-outline-secondary flex-fill" (click)="cancelEditing()">Cancel</button>
                </div>
              </div>
            }

          </div>
        }

      </div>
    </div>
  `,
  styles: [`
    .profile-page {
      max-width: 520px;
      margin: 0 auto;
    }
    .profile-card {
      background: #ffffff;
      border: 1px solid #e5e7eb;
      border-radius: 14px;
      overflow: hidden;
      box-shadow: 0 1px 6px rgba(0, 0, 0, 0.06);
    }
    .profile-avatar {
      width: 72px;
      height: 72px;
      border-radius: 50%;
      background: #7c3aed;
      color: #fff;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.5rem;
      font-weight: 700;
    }
    .info-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 10px 0;
      border-bottom: 1px solid #f3f4f6;
    }
    .info-row:last-of-type {
      border-bottom: none;
    }
    .info-label {
      font-size: 0.85rem;
      color: #6b7280;
      font-weight: 500;
    }
    .info-value {
      font-size: 0.9rem;
      color: #111827;
      font-weight: 600;
      text-align: right;
      max-width: 60%;
      word-break: break-word;
    }
    .form-control {
      border-radius: 8px;
      border: 1px solid #d1d5db;
      padding: 10px 14px;
      font-size: 0.9rem;
    }
    .form-control:focus {
      border-color: #7c3aed;
      box-shadow: 0 0 0 3px rgba(124, 58, 237, 0.15);
    }
    .form-control:disabled {
      background: #f9fafb;
      color: #6b7280;
    }
    .btn-primary {
      background: linear-gradient(135deg, #7c3aed 0%, #6d28d9 100%);
      border: none;
      border-radius: 10px;
      padding: 12px 28px;
    }
    .btn-primary:hover:not(:disabled) {
      opacity: 0.92;
      transform: translateY(-1px);
    }
    .btn-primary:disabled {
      opacity: 0.6;
    }
  `],
})
export class ProfileComponent implements OnInit {
  private profileService = inject(ProfileService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  profile: UserDto | null = null;
  isEditing = false;
  isLoading = true;
  isSaving = false;

  editForm: UpdateUserRequest = { name: '', phone: '' };

  ngOnInit(): void {
    this.loadProfile();
  }

  private loadProfile(): void {
    this.profileService.getProfile().subscribe({
      next: (data) => {
        this.profile = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
        this.toast.error('Failed to load profile.', 'Error');
      },
    });
  }

  startEditing(): void {
    if (!this.profile) return;
    this.editForm = { name: this.profile.name, phone: this.profile.phone ?? '' };
    this.isEditing = true;
  }

  cancelEditing(): void {
    this.isEditing = false;
  }

  getInitials(name: string): string {
    const parts = name.split(' ');
    return parts.length > 1
      ? (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
      : name.substring(0, 2).toUpperCase();
  }

  saveProfile(): void {
    if (!this.editForm.name.trim()) return;
    this.isSaving = true;
    this.profileService.updateProfile(this.editForm).subscribe({
      next: (data) => {
        this.profile = data;
        this.isEditing = false;
        this.isSaving = false;
        this.cdr.detectChanges();
        this.toast.success('Profile updated successfully.', 'Saved');
      },
      error: () => {
        this.isSaving = false;
        this.cdr.detectChanges();
        this.toast.error('Failed to update profile.', 'Error');
      },
    });
  }
}
