import { ChangeDetectorRef, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { OrganizerEventService } from '../services/organizer-event.service';
import { CheckInResponse } from '../models/attendee.models';
import { ToastService } from '../../../../shared/services/toast.service';

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
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  ticketControl = new FormControl('', {
    nonNullable: true,
    validators: [Validators.required, Validators.minLength(10)],
  });

  state = signal<PageState>('form');
  result: CheckInResponse | null = null;
  errorMessage = '';
  submitted = false;

  get ticketCode(): string {
    return this.ticketControl.value.trim();
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.ticketControl.invalid) return;

    const code = this.ticketCode;
    if (!code) return;

    this.state.set('loading');
    this.errorMessage = '';
    this.result = null;
    this.cdr.detectChanges();

    this.eventService.checkIn(code).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.result = res.data;
          this.state.set('success');
        } else {
          this.errorMessage = res.message || 'Check-in failed.';
          this.state.set('error');
        }
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.errorMessage =
          err.error?.message || err.message || 'An unexpected error occurred. Please try again.';
        this.state.set('error');
        this.cdr.detectChanges();
      },
    });
  }

  reset(): void {
    this.ticketControl.reset();
    this.submitted = false;
    this.result = null;
    this.errorMessage = '';
    this.state.set('form');
    this.cdr.detectChanges();
  }

  getFieldError(): string {
    const ctrl = this.ticketControl;
    if (!ctrl.errors || !this.submitted) return '';

    if (ctrl.hasError('required')) return 'Ticket code is required.';
    if (ctrl.hasError('minlength'))
      return `Ticket code must be at least ${ctrl.errors['minlength'].requiredLength} characters.`;
    return 'Invalid ticket code.';
  }
}
