import { Component, forwardRef, HostListener, Input, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe } from '@angular/common';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-time-picker',
  standalone: true,
  imports: [CommonModule, DecimalPipe, MatFormFieldModule, MatInputModule, ReactiveFormsModule],
  templateUrl: './time-picker.component.html',
  styleUrls: ['./time-picker.component.css'],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => TimePickerComponent),
      multi: true,
    },
  ],
})
export class TimePickerComponent implements ControlValueAccessor, OnInit {
  @Input() label: string = 'Time';
  @Input() disabled: boolean = false;

  isOpen = false;
  value = '';

  tempHour = 12;
  tempMin = 0;
  tempAmPm: 'AM' | 'PM' = 'AM';

  hours: number[] = [];
  minutes: number[] = [];

  private onChange: (v: string) => void = () => {};
  onTouched: () => void = () => {};

  get showError(): boolean {
    return false;
  }

  ngOnInit(): void {
    this.hours = Array.from({ length: 12 }, (_, i) => i + 1);
    this.minutes = Array.from({ length: 60 }, (_, i) => i);
  }

  writeValue(val: string): void {
    if (!val) {
      this.value = '';
      return;
    }
    const [hStr, mStr] = val.split(':');
    let h = parseInt(hStr, 10);
    const m = parseInt(mStr, 10);
    const ampm: 'AM' | 'PM' = h >= 12 ? 'PM' : 'AM';
    h = h % 12 || 12;
    this.tempHour = h;
    this.tempMin = m;
    this.tempAmPm = ampm;
    this.value = this.formatDisplay(h, m, ampm);
  }

  registerOnChange(fn: (v: string) => void): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  toggle(): void {
    this.isOpen ? this.cancel() : this.open();
  }

  open(): void {
    if (this.disabled) return;
    if (this.value) {
      const parts = this.value.match(/^(\d{1,2}):(\d{2})\s*(AM|PM)$/i);
      if (parts) {
        this.tempHour = parseInt(parts[1], 10);
        this.tempMin = parseInt(parts[2], 10);
        this.tempAmPm = parts[3].toUpperCase() as 'AM' | 'PM';
      }
    }
    this.isOpen = true;
    this.onTouched();
  }

  cancel(): void {
    this.isOpen = false;
  }

  apply(): void {
    this.value = this.formatDisplay(this.tempHour, this.tempMin, this.tempAmPm);
    this.onChange(this.to24h(this.tempHour, this.tempMin, this.tempAmPm));
    this.isOpen = false;
  }

  selectHour(h: number): void {
    this.tempHour = h;
  }
  selectMin(m: number): void {
    this.tempMin = m;
  }
  selectAmPm(ap: 'AM' | 'PM'): void {
    this.tempAmPm = ap;
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    this.isOpen = false;
  }

  private formatDisplay(h: number, m: number, ampm: 'AM' | 'PM'): string {
    return `${h.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')} ${ampm}`;
  }

  private to24h(h: number, m: number, ampm: 'AM' | 'PM'): string {
    let hour24 = ampm === 'AM' ? (h === 12 ? 0 : h) : h === 12 ? 12 : h + 12;
    return `${hour24.toString().padStart(2, '0')}:${m.toString().padStart(2, '0')}`;
  }
}
