import { Component, Input, Self, Optional, Output, EventEmitter } from '@angular/core';
import { ControlValueAccessor, NgControl, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule, MatSelectChange } from '@angular/material/select';
import {
  MatDatepickerInputEvent,
  MatDatepickerModule,
} from '@angular/material/datepicker';
import { MatButtonModule } from '@angular/material/button';
import { MatNativeDateModule } from '@angular/material/core';
import { SecondaryButton } from '../buttons/secondary-button/secondary-button.component';
import { PrimaryButton } from '../buttons/primary-button/primary-button.component';

type InputValue = string | number | Date | null;

@Component({
  selector: 'app-inputs',
  standalone: true,
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatNativeDateModule,
    MatSelectModule,
    SecondaryButton,
    PrimaryButton,
  ],
  templateUrl: './inputs.components.html',
})
export class InputsComponent implements ControlValueAccessor {
  @Input() label: string = '';
  @Input() type:
    | 'text'
    | 'email'
    | 'password'
    | 'date'
    | 'textarea'
    | 'select'
    | 'mobile'
    | 'percent'
    | 'duration'
    | 'currency'
    | 'number' = 'text';
  @Input() placeholder?: string = '';
  @Input() errorMessages: { [key: string]: string } = {};
  @Input() options: { label: string; value: any }[] = [];
  @Input() labelMode: 'floating' | 'static' = 'static';
  @Input() tag?: string = '';
  @Input() hidePassword = true;
  @Input() isRequired = false;

  @Output() openedChange = new EventEmitter<boolean>();

  value: InputValue = null;
  disabled = false;
  isFocused = false;

  pendingDateValue: Date | null = null;

  private applyClicked = false;

  onChange: (value: InputValue) => void = () => {};
  onTouched = () => {};

  private readonly defaultErrorMessages: { [key: string]: string } = {
    required: 'This field is required.',
    email: 'Please enter a valid email address.',
    pattern: 'Invalid format.',
    startsWithNumber: 'Must not start with a number.',
    invalidChars: 'Only letters, numbers and spaces are allowed.',
    onlyNumber: 'Name must contain at least one alphabet.',
    maxlength: 'Input is too long.',
    minlength: 'Input is too short.',
  };

  constructor(@Self() @Optional() public ngControl: NgControl) {
    if (this.ngControl) {
      this.ngControl.valueAccessor = this;
    }
  }

  writeValue(value: InputValue): void {
    this.value = value;
  }

  registerOnChange(fn: (value: InputValue) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = () => {
      fn();
      if (this.type === 'email') {
        this.validateEmail();
      }
    };
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  get matDateValue(): Date | null {
    if (this.value instanceof Date) return this.value;
    if (typeof this.value === 'string' && this.value) return new Date(this.value);
    return null;
  }

  handleSelectChange(event: MatSelectChange): void {
    this.value = event.value ?? null;
    this.onChange(this.value);
    this.onTouched();
  }

  onSelectOpenedChange(isOpen: boolean): void {
    if (!isOpen) this.onTouched();
  }

  handleInput(event: Event): void {
    const target = event.target as HTMLInputElement | HTMLTextAreaElement;
    const rawValue = target.value;
    const control = this.ngControl?.control;

    if (rawValue === '') {
      this.value = null;
      if (
        (control && this.type === 'text' && control.errors?.['required'] !== undefined) ||
        this.isRequired
      ) {
        control?.setErrors({ required: true });
        control?.markAsTouched();
      }
    } else {
      switch (this.type) {
        case 'mobile': {
          const cleaned = rawValue.replace(/[^0-9]/g, '');
          this.value = cleaned;

          if (control) {
            const errors: { [key: string]: boolean } = {};
            if (!cleaned) {
              errors['required'] = true;
            } else if (cleaned.length !== 10) {
              errors['pattern'] = true;
            }
            control.setErrors(Object.keys(errors).length ? errors : null);
          }
          break;
        }

        case 'email': {
          this.value = rawValue;
          this.validateEmail();
          break;
        }

        case 'percent':
        case 'duration':
        case 'currency':
        case 'number':
          this.value = this.parseNumber(rawValue);
          break;

        case 'date':
          this.value = this.parseDate(rawValue);
          break;

        default: {
          this.value = String(rawValue);

          if (
            (control && this.type === 'text' && control.errors?.['required'] !== undefined) ||
            this.isRequired
          ) {
            const errors: { [key: string]: boolean } = {};
            const trimmed = String(rawValue).trim();

            if (!trimmed) {
              errors['required'] = true;
            } else if (!/^[a-zA-Z0-9 ]*$/.test(trimmed)) {
              errors['invalidChars'] = true;
            } else if (!/[a-zA-Z]/.test(trimmed)) {
              errors['onlyNumber'] = true;
            }

            control?.setErrors(Object.keys(errors).length ? errors : null);
            control?.markAsTouched();
          }
          break;
        }
      }
    }

    this.onChange(this.value);
  }

  onTextBlur(): void {
    const control = this.ngControl?.control;
    control?.markAsTouched();

    if (
      (this.type === 'text' && control && control.errors?.['required'] !== undefined) ||
      this.isRequired
    ) {
      const trimmed = typeof this.value === 'string' ? this.value.trim() : '';
      const errors: { [key: string]: boolean } = {};

      if (!trimmed) {
        errors['required'] = true;
      } else if (!/^[a-zA-Z0-9 ]*$/.test(trimmed)) {
        errors['invalidChars'] = true;
      } else if (/^[0-9]/.test(trimmed)) {
        errors['startsWithNumber'] = true;
      } else if (!/[a-zA-Z]/.test(trimmed)) {
        errors['onlyNumber'] = true;
      }

      control?.setErrors(Object.keys(errors).length ? errors : null);
    } else if (control) {
      control.setErrors(null); // clear any stale errors on optional fields
    }

    this.onTouched();
  }

  private parseNumber(value: string): number | null {
    const parsed = Number(value);
    return isNaN(parsed) ? null : parsed;
  }

  private parseDate(value: string): Date | null {
    const parsed = new Date(value);
    return isNaN(parsed.getTime()) ? null : parsed;
  }

  get hasValue(): boolean {
    return this.value !== null && this.value !== '';
  }

  get showError(): boolean {
    return !!(this.ngControl?.control?.invalid && this.ngControl?.control?.touched);
  }

  togglePassword(): void {
    this.hidePassword = !this.hidePassword;
  }

  handleDateChange(event: MatDatepickerInputEvent<Date>): void {
    this.pendingDateValue = event.value
      ? new Date(event.value.getFullYear(), event.value.getMonth(), event.value.getDate())
      : null;
  }

  onPickerClosed(): void {
    if (this.applyClicked && this.pendingDateValue !== null) {
      this.value = this.pendingDateValue;
      this.onChange(this.value);
    }
    this.pendingDateValue = null;
    this.applyClicked = false;
    this.onTouched();
  }
  onFocus(): void {
    this.isFocused = true;
  }

  onBlur(): void {
    setTimeout(() => {
      this.isFocused = false;
    }, 100);
  }

  get displayDateValue(): Date | null {
    if (this.pendingDateValue !== null) return this.pendingDateValue;
    return this.matDateValue;
  }

  onApplyClicked(): void {
    this.applyClicked = true;
  }

  clearDateOnly(): void {
    this.pendingDateValue = null;
    this.value = null;
    this.onChange(null);
    this.onTouched();
  }
  get displayLabel(): string {
    return this.isRequired ? `${this.label}*` : this.label;
  }

  get firstErrorKey(): string | null {
    const errors = this.ngControl?.control?.errors;
    if (!errors) return null;
    if (errors['invalidChars']) return 'invalidChars';
    if (errors['startsWithNumber']) return 'startsWithNumber';
    if (errors['onlyNumber']) return 'onlyNumber';
    if (errors['pattern']) return 'pattern';
    if (errors['email']) return 'email';
    if (errors['required']) return 'required';
    return Object.keys(errors)[0];
  }

  get errorMessage(): string {
    const key = this.firstErrorKey;
    if (!key) return '';
    return this.errorMessages[key] || this.defaultErrorMessages[key] || 'Invalid field.';
  }

  onMobileKeyPress(event: KeyboardEvent): void {
    if (!/[0-9]/.test(event.key)) {
      event.preventDefault();
    }
  }

  onTextKeyPress(event: KeyboardEvent): void {
    if (!/[a-zA-Z0-9 ]/.test(event.key)) {
      event.preventDefault();
    }
  }

  validateEmail(): void {
    const control = this.ngControl?.control;
    const value = typeof this.value === 'string' ? this.value : '';
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9-]+\.[a-zA-Z]{2,}$/;

    if (!control) return;

    if (!value) {
      control.setErrors({ required: true });
    } else if (!emailRegex.test(value)) {
      control.setErrors({ email: true });
    } else {
      control.setErrors(null);
    }

    control.markAsTouched();
  }
}
