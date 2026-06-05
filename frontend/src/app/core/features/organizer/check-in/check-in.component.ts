import { ChangeDetectorRef, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { OrganizerEventService } from '../services/organizer-event.service';
import { CheckInResponse } from '../models/attendee.models';

type PageState = 'form' | 'loading' | 'success' | 'error';

@Component({
  selector: 'app-check-in',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './check-in.component.html',
  styleUrl: './check-in.component.css',
})
export class CheckInComponent {
  private eventService = inject(OrganizerEventService);

  checkInForm = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(10),Validators.maxLength(50)],
  });

  state = signal<PageState>('form');
  result: CheckInResponse | null = null;
  errorMessage = '';
  submitted = false;

  get ticketCode(): string {
    return this.checkInForm.value.trim();
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.checkInForm.invalid) return;

    const code = this.ticketCode;
    if (!code) return;

    this.state.set('loading');
    this.errorMessage = '';
    this.result = null;

    this.eventService.checkIn(code).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.result = res.data;
          this.state.set('success');
        } else {
          this.errorMessage = res.message || 'Check-in failed.';
          this.state.set('error');
        }
      },
      error: (err) => {
        this.errorMessage =
          err.error?.message || err.message || 'An unexpected error occurred. Please try again.';
        this.state.set('error');
      },
    });
  }

  reset(): void {
    this.checkInForm.reset();
    this.submitted = false;
    this.result = null;
    this.errorMessage = '';
    this.state.set('form');
  }

  getFieldError(): string {
    const ctrl = this.checkInForm;
    if (!ctrl.errors || !this.submitted) return '';

    if (ctrl.hasError('required')) return 'Ticket code is required.';
    if (ctrl.hasError('minlength'))
      return `Ticket code must be at least ${ctrl.errors['minlength'].requiredLength} characters.`;
    if (ctrl.hasError('maxlength'))
      return `Ticket code must be at most ${ctrl.errors['maxlength'].requiredLength} characters.`;
    return 'Invalid ticket code.';
  }
}
