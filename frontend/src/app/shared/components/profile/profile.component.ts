import { ChangeDetectorRef, Component, Input, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ProfileService, UserDto, UpdateUserRequest } from './profile.service';

@Component({
  selector: 'shared-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
  host: { '[class.pink]': 'themeColor === "pink"', '[class.purple]': 'themeColor === "purple"' },
})
export class ProfileComponent implements OnInit {
  private profileService = inject(ProfileService);
  private cdr = inject(ChangeDetectorRef);

  @Input({ required: true }) backLink!: string;
  @Input() themeColor: 'pink' | 'purple' = 'pink';

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
    this.isSaving = true;

    this.profileService
      .updateProfile({
        name: this.editForm.name.trim(),
        phone: this.editForm.phone?.trim() || null,
      })
      .subscribe({
        next: (data) => {
          this.profile = data;
          this.isEditing = false;
          this.isSaving = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.isSaving = false;
          this.cdr.detectChanges();
        },
      });
  }
}
