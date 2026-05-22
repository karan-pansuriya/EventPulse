import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize, take } from 'rxjs';
import { ToastService } from '../../../shared/services/toast.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['../../shared/auth-styles.css', './login.component.css'],
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  private readonly emailPattern = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
  private readonly passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$/;

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.maxLength(255), Validators.pattern(this.emailPattern)]],
    password: ['', [Validators.required, Validators.pattern(this.passwordPattern)]],
  });

  loading = false;
  showPassword = false;
  serverError = '';

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.toast.error('Please fix the highlighted fields.', 'Invalid form');
      return;
    }
    this.loading = true;
    this.serverError = '';
    this.clearInvalidCredentials();

    this.authService.login(this.form.getRawValue())
      .pipe(
        take(1),
        finalize(() => {
          this.loading = false;
          this.cdr.markForCheck();
        }),
      )
      .subscribe({
        next: () => {
          const user = this.authService.user();
        if (user?.roles.includes('Admin')) this.router.navigate(['/admin/dashboard']);
        else if (user?.roles.includes('Organizer')) this.router.navigate(['/organizer/dashboard']);
        else this.router.navigate(['/attendee/home']);
        },
        error: (err) => {
          const message = (err as { error?: { message?: string } })?.error?.message;
          this.serverError = message || 'Invalid email or password.';
          this.setInvalidCredentials();
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
  }

  onAuthInput(): void {
    if (this.serverError) {
      this.serverError = '';
    }
    this.clearInvalidCredentials();
  }

  private setInvalidCredentials(): void {
    const password = this.form.get('password');
    if (!password) return;
    const currentErrors = password.errors || {};
    password.setErrors({ ...currentErrors, invalidCredentials: true });
  }

  private clearInvalidCredentials(): void {
    const password = this.form.get('password');
    if (!password?.errors?.['invalidCredentials']) return;
    const { invalidCredentials, ...rest } = password.errors;
    password.setErrors(Object.keys(rest).length ? rest : null);
  }
}
