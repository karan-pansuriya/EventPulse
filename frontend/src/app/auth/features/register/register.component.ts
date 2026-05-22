import { ChangeDetectorRef, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize, take } from 'rxjs';
import { ToastService } from '../../../shared/services/toast.service';
import { AuthService } from '../../services/auth.service';
import { RegisterRequest } from '../../models/auth.models';

const passwordsMatchValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const password = control.get('password');
  const confirm = control.get('confirmPassword');
  return password && confirm && password.value !== confirm.value
    ? { passwordsMismatch: true }
    : null;
};

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['../../shared/auth-styles.css', './register.component.css'],
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  private readonly emailPattern = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
  private readonly passwordPattern = /^(?=.*[a-z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$/;

  form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.maxLength(255), Validators.pattern(this.emailPattern)]],
    phoneNumber: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
    password: ['', [Validators.required, Validators.pattern(this.passwordPattern)]],
    confirmPassword: ['', [Validators.required]],
    role: ['Customer' as 'Customer' | 'Organizer', Validators.required],
  }, { validators: passwordsMatchValidator });

  loading = false;
  showPassword = false;
  showConfirm = false;

  roleOptions = [
    { label: 'Customer', value: 'Customer' as const },
    { label: 'Organizer', value: 'Organizer' as const },
  ];

  get passwordsMatch(): boolean {
    return this.form.value.password === this.form.value.confirmPassword;
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.toast.error('Please fix the highlighted fields.', 'Invalid form');
      return;
    }

    this.loading = true;

    const raw = this.form.getRawValue();
    const request: RegisterRequest = {
      name: raw.fullName,
      email: raw.email,
      password: raw.password,
      phone: raw.phoneNumber,
      role: raw.role,
    };

    this.authService.register(request)
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
        error: () => {
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
  }
}
