import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize, take } from 'rxjs';
import { ToastService } from '../../../shared/services/toast.service';
import { AuthService } from '../../services/auth.service';
import { RoleResponse } from '../../models/auth.models';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['../../shared/auth-styles.css', './login.component.css'],
})
export class LoginComponent implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  private readonly emailPattern = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
  private readonly passwordPattern = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$/;

  loginRoleOptions: RoleResponse[] = [];
  rolesLoading = true;

  ngOnInit(): void {
    this.authService.getRoles().pipe(take(1)).subscribe({
      next: (roles) => {
        this.loginRoleOptions = roles;
        this.rolesLoading = false;
        if (roles.length > 0) {
          this.form.get('role')?.setValue(roles[0].name);
        }
        this.cdr.markForCheck();
      },
      error: () => {
        this.rolesLoading = false;
        this.cdr.markForCheck();
      },
    });
  }

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.maxLength(255), Validators.pattern(this.emailPattern)]],
    password: ['', [Validators.required, Validators.pattern(this.passwordPattern)]],
    role: ['Customer', Validators.required],
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
        next: () => this.navigateByRole(this.form.get('role')!.value),
        error: (err) => {
          const message = (err as { error?: { message?: string } })?.error?.message;
          this.serverError = message || 'Invalid email or password.';
          this.setInvalidCredentials();
          this.loading = false;
          this.cdr.markForCheck();
        },
      });
  }

  private navigateByRole(role: string): void {
    if (role === 'Admin') this.router.navigate(['/admin/dashboard']);
    else if (role === 'Organizer') this.router.navigate(['/organizer/dashboard']);
    else this.router.navigate(['/attendee/home']);
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
