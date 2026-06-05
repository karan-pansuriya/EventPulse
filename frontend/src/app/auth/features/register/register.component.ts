import { ChangeDetectorRef, Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize, take } from 'rxjs';
import { ToastService } from '../../../shared/services/toast.service';
import { AuthService } from '../../services/auth.service';
import { RegisterRequest, RoleResponse, RoleId } from '../../models/auth.models';

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
export class RegisterComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  private readonly emailPattern = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
  private readonly passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$/;

  form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.maxLength(255), Validators.pattern(this.emailPattern)]],
    phoneNumber: ['', [Validators.required, Validators.pattern('^[0-9]{10}$')]],
    password: ['', [Validators.required, Validators.pattern(this.passwordPattern)]],
    confirmPassword: ['', [Validators.required]],
    roleId: [3 as number, Validators.required],
  }, { validators: passwordsMatchValidator });

  loading = false;
  showPassword = false;
  showConfirm = false;
  serverError = '';
  roleOptions: RoleResponse[] = [];
  rolesLoading = true;

  ngOnInit(): void {
    this.authService.getRoles().pipe(take(1)).subscribe({
      next: (roles) => {
        this.roleOptions = roles.filter(r => r.id !== RoleId.Admin);
        this.rolesLoading = false;
        if (this.roleOptions.length > 0) {
          this.form.get('roleId')?.setValue(this.roleOptions[0].id);
        }
        this.cdr.markForCheck();
      },
      error: () => {
        this.rolesLoading = false;
        this.cdr.markForCheck();
      },
    });
  }

  get passwordsMatch(): boolean {
    return this.form.value.password === this.form.value.confirmPassword;
  }

  onPhoneInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const digits = input.value.replace(/\D/g, '');
    input.value = digits;
    this.form.get('phoneNumber')?.setValue(digits, { emitEvent: false });
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) {
      this.toast.error('Please fix the highlighted fields.', 'Invalid form');
      return;
    }

    this.loading = true;
    this.serverError = '';

    const raw = this.form.getRawValue();
    const request: RegisterRequest = {
      name: raw.fullName,
      email: raw.email,
      password: raw.password,
      phone: raw.phoneNumber,
      roleId: raw.roleId,
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
          this.router.navigate(['/login']);
        },
        error: (err) => {
          const message = (err as { error?: { message?: string } })?.error?.message;
          this.serverError = message || 'Registration failed. Please try again.';
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
  }
}
