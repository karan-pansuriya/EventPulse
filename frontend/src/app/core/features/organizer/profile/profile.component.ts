import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ProfileService, UserDto, UpdateUserRequest } from '../../attendee/profile/profile.service';

@Component({
  selector: 'app-organizer-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
})
export class ProfileComponent implements OnInit {
  private profileService = inject(ProfileService);
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
      },
      error: () => {
        this.isSaving = false;
        this.cdr.detectChanges();
      },
    });
  }
}
