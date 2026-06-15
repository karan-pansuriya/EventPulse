import { ChangeDetectorRef, Component, OnInit, OnDestroy, inject } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormGroup,
  FormControl,
  Validators,
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
import { OrganizerEventService } from '../../../core/features/organizer/layout/services/organizer-event.service';
import { CategoryService } from '../../../core/features/attendee/home/services/category.service';
import { LocationService } from '../../../core/features/organizer/layout/services/location.service';
import { Category } from '../../../core/features/attendee/home/models/category.models';
import {
  Country,
  State,
  City,
} from '../../../core/features/organizer/layout/models/location.models';
import { ToastService } from '../../services/toast.service';
import { AuthService } from '../../../auth/services/auth.service';
import { RoleId } from '../../../auth/models/auth.models';
import { AdminUserService } from '../../../core/features/admin/layout/admin-layout/services/admin-user.service';
import { OrganizerResponse } from '../../../core/features/admin/layout/admin-layout/models/adminuser.model';
import { environment } from '../../../../environments/environment';

function pastDateValidator(minDate: string): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    return control.value < minDate ? { pastDate: true } : null;
  };
}

function pastTimeValidator(formGroup: FormGroup): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value || !formGroup) return null;
    const eventDate = formGroup.get('eventDate')?.value;
    const today = new Date().toISOString().slice(0, 10);
    if (eventDate !== today) return null;
    const now = new Date();
    const currentMinutes = now.getHours() * 60 + now.getMinutes();
    const [h, m] = (control.value as string).split(':').map(Number);
    return h * 60 + m <= currentMinutes ? { pastTime: true } : null;
  };
}

@Component({
  selector: 'app-event-form',
  standalone: true,
  imports: [RouterLink, CommonModule, ReactiveFormsModule],
  templateUrl: './event-form.component.html',
  styleUrl: './event-form.component.css',
})
export class EventFormComponent implements OnInit, OnDestroy {
  private eventService = inject(OrganizerEventService);
  private categoryService = inject(CategoryService);
  private locationService = inject(LocationService);
  private toastService = inject(ToastService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  private authService = inject(AuthService);
  private adminUserService = inject(AdminUserService);

  private imageBaseUrl = environment.apiUrl.replace('/api', '');

  isEditMode = false;
  eventId = 0;

  categories: Category[] = [];
  countries: Country[] = [];
  states: State[] = [];
  cities: City[] = [];
  organizers: OrganizerResponse[] = [];

  isLoading = false;
  isSaving = false;
  isLoadingCategories = true;
  isLoadingCountries = true;
  isLoadingOrganizers = false;
  isLoadingStates = false;
  isLoadingCities = false;
  submitted = false;
  isAdmin = false;

  get backRoute(): string {
    return this.isAdmin ? '/admin/events' : '/organizer/events';
  }

  minDate = new Date().toISOString().slice(0, 10);

  get minTime(): string | null {
    const eventDate = this.eventForm?.get('eventDate')?.value;
    if (eventDate !== this.minDate) return null;
    const now = new Date();
    return `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`;
  }

  eventForm = new FormGroup({
    title: new FormControl('', [
      Validators.required,
      Validators.minLength(2),
      Validators.maxLength(200),
    ]),
    description: new FormControl('', Validators.maxLength(1000)),
    genre: new FormControl('', Validators.maxLength(100)),
    ageRestriction: new FormControl('', [
      Validators.maxLength(10),
      Validators.pattern(/^[0-9+]{0,10}$/),
    ]),
    performers: new FormControl('', Validators.maxLength(200)),
    durationMins: new FormControl<number | null>(null, [Validators.min(1), Validators.max(420)]),
    categoryId: new FormControl<number | null>(null),
    eventDate: new FormControl('', [Validators.required, pastDateValidator(this.minDate)]),
    startTime: new FormControl('', Validators.required),
    price: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(0),
      Validators.max(9999999999),
    ]),
    totalSeats: new FormControl<number | null>(null, [
      Validators.required,
      Validators.min(1),
      Validators.max(100000),
    ]),
    venueName: new FormControl('', [Validators.required, Validators.maxLength(150)]),
    venueAddress: new FormControl('', [Validators.required, Validators.maxLength(200)]),
    countryId: new FormControl<number | null>(null, Validators.required),
    stateId: new FormControl<number | null>(null, Validators.required),
    cityId: new FormControl<number | null>(null, Validators.required),
    organizerId: new FormControl<number | null>(null),
  });

  selectedFiles: File[] = [];
  newFilePreviews: string[] = [];
  existingPosterUrls: string[] = [];
  removedPosterUrls: string[] = [];

  ngOnInit(): void {
    this.eventId = Number(this.route.snapshot.paramMap.get('id'));
    this.isEditMode = !!this.eventId;

    this.isAdmin = this.authService.user()?.roleIds.includes(RoleId.Admin) ?? false;

    const startTimeControl = this.eventForm.get('startTime');
    startTimeControl?.addValidators(pastTimeValidator(this.eventForm));

    this.eventForm.get('eventDate')?.valueChanges.subscribe(() => {
      startTimeControl?.updateValueAndValidity();
    });

    if (!this.isEditMode && this.isAdmin) {
      this.eventForm.get('organizerId')?.addValidators(Validators.required);
      this.eventForm.get('organizerId')?.updateValueAndValidity();
    }

    this.locationService.getCountries().subscribe({
      next: (res) => {
        if (res.success && res.data) this.countries = res.data;
        this.isLoadingCountries = false;
        this.afterInitLoad();
      },
      error: () => {
        this.isLoadingCountries = false;
        this.afterInitLoad();
      },
    });

    this.categoryService.getAllCatagorys().subscribe({
      next: (res) => {
        if (res.success && res.data) this.categories = res.data;
        this.isLoadingCategories = false;
        this.afterInitLoad();
      },
      error: () => {
        this.isLoadingCategories = false;
        this.afterInitLoad();
      },
    });

    if (!this.isEditMode && this.isAdmin) {
      this.isLoadingOrganizers = true;
      this.adminUserService.getOrganizers().subscribe({
        next: (res) => {
          if (res.success && res.data) this.organizers = res.data;
          this.isLoadingOrganizers = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.isLoadingOrganizers = false;
          this.cdr.detectChanges();
        },
      });
    }
  }

  private afterInitLoad(): void {
    if (this.isLoadingCategories || this.isLoadingCountries) return;
    if (this.isEditMode) {
      this.loadEvent();
    }
  }

  private loadEvent(): void {
    this.isLoading = true;
    this.eventService.getEventById(this.eventId).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const e = res.data;
          this.eventForm.patchValue({
            title: e.title,
            description: e.description || '',
            genre: e.genre || '',
            ageRestriction: e.ageRestriction || '',
            performers: e.performers || '',
            durationMins: e.durationMins,
            categoryId: e.categoryId,
            eventDate: e.eventDate.split('T')[0],
            startTime: e.startTime.slice(0, 5),
            price: e.price,
            totalSeats: e.totalSeats,
            venueName: e.venueName || '',
            venueAddress: e.venueAddress || '',
          });
          this.existingPosterUrls = e.posterUrls || [];

          if (e.cityId) {
            this.eventForm.patchValue({ cityId: e.cityId });
            this.locationService.getCityById(e.cityId).subscribe({
              next: (cityRes) => {
                if (cityRes.success && cityRes.data) {
                  const city = cityRes.data;
                  this.eventForm.patchValue({ stateId: city.stateId, countryId: city.countryId });
                  this.loadStatesAndCities();
                }
                this.isLoading = false;
                this.cdr.detectChanges();
              },
              error: () => {
                this.isLoading = false;
                this.cdr.detectChanges();
              },
            });
          } else {
            this.isLoading = false;
            this.cdr.detectChanges();
          }
        } else {
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      },
      error: () => {
        this.toastService.error('Failed to load event details.');
        this.isLoading = false;
        this.router.navigate([this.backRoute]);
        this.cdr.detectChanges();
      },
    });
  }

  private loadStatesAndCities(): void {
    const countryId = this.eventForm.get('countryId')?.value;
    const stateId = this.eventForm.get('stateId')?.value;
    if (countryId) {
      this.locationService.getStates(countryId).subscribe({
        next: (res) => {
          if (res.success && res.data) this.states = res.data;
          this.cdr.detectChanges();
        },
        error: () => this.cdr.detectChanges(),
      });
    }
    if (stateId) {
      this.locationService.getCities(stateId).subscribe({
        next: (res) => {
          if (res.success && res.data) this.cities = res.data;
          this.cdr.detectChanges();
        },
        error: () => this.cdr.detectChanges(),
      });
    }
  }

  ngOnDestroy(): void {
    this.newFilePreviews.forEach((u) => URL.revokeObjectURL(u));
  }

  onCountryChange(): void {
    this.eventForm.patchValue({ stateId: null, cityId: null });
    this.states = [];
    this.cities = [];
    const countryId = this.eventForm.get('countryId')?.value;
    if (!countryId) return;

    this.isLoadingStates = true;
    this.locationService.getStates(countryId).subscribe({
      next: (res) => {
        if (res.success && res.data) this.states = res.data;
        this.isLoadingStates = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoadingStates = false;
        this.cdr.detectChanges();
      },
    });
  }

  onStateChange(): void {
    this.eventForm.patchValue({ cityId: null });
    this.cities = [];
    const stateId = this.eventForm.get('stateId')?.value;
    if (!stateId) return;

    this.isLoadingCities = true;
    this.locationService.getCities(stateId).subscribe({
      next: (res) => {
        if (res.success && res.data) this.cities = res.data;
        this.isLoadingCities = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoadingCities = false;
        this.cdr.detectChanges();
      },
    });
  }

  getPosterUrl(url: string): string {
    return url ? `${this.imageBaseUrl}/${url}` : '';
  }

  get currentPosters(): string[] {
    return this.existingPosterUrls.filter((u) => !this.removedPosterUrls.includes(u));
  }

  removePoster(url: string): void {
    this.removedPosterUrls.push(url);
  }

  undoRemovePoster(url: string): void {
    this.removedPosterUrls = this.removedPosterUrls.filter((u) => u !== url);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;

    const maxFiles = 5;
    const maxSize = 5 * 1024 * 1024;

    const files = Array.from(input.files);

    const allowedExtensions = ['jpg', 'jpeg', 'png'];
    const invalid = files.find((f) => {
      const ext = f.name.split('.').pop()?.toLowerCase();
      return !ext || !allowedExtensions.includes(ext);
    });
    if (invalid) {
      this.toastService.error(
        `${invalid.name} has an unsupported file type. Only JPG, JPEG, and PNG are allowed.`,
        'Invalid file',
      );
      input.value = '';
      return;
    }

    const oversized = files.find((f) => f.size > maxSize);
    if (oversized) {
      this.toastService.error(`${oversized.name} exceeds the 5 MB limit.`, 'File too large');
      input.value = '';
      return;
    }

    const totalAfterAdd = this.selectedFiles.length + files.length;
    if (totalAfterAdd > maxFiles) {
      this.toastService.error(
        `Maximum ${maxFiles} poster images allowed. You already have ${this.selectedFiles.length}.`,
        'Too many files',
      );
      input.value = '';
      return;
    }
    this.selectedFiles = [...this.selectedFiles, ...files];
    this.newFilePreviews = [...this.newFilePreviews, ...files.map((f) => URL.createObjectURL(f))];
  }

  removeNewFile(index: number): void {
    URL.revokeObjectURL(this.newFilePreviews[index]);
    this.selectedFiles = this.selectedFiles.filter((_, i) => i !== index);
    this.newFilePreviews = this.newFilePreviews.filter((_, i) => i !== index);
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.eventForm.get(fieldName);
    return !!control && control.invalid && (control.touched || control.dirty || this.submitted);
  }

  getFieldError(fieldName: string): string | null {
    const control = this.eventForm.get(fieldName);
    if (!control || !control.errors || !(control.touched || control.dirty || this.submitted))
      return null;

    const errors = control.errors;
    if (errors['required']) return 'This field is required.';
    if (errors['minlength'])
      return `Minimum ${errors['minlength'].requiredLength} characters required.`;
    if (errors['maxlength'])
      return `Maximum ${errors['maxlength'].requiredLength} characters allowed.`;
    if (errors['min']) return `Minimum value is ${errors['min'].min}.`;
    if (errors['max']) return `Maximum value is ${errors['max'].max}.`;
    if (errors['pattern']) return 'Invalid format.';
    if (errors['pastDate']) return 'Event date must be today or later.';
    if (errors['pastTime']) return 'Start time must be in the future.';
    return null;
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.eventForm.invalid) {
      Object.keys(this.eventForm.controls).forEach((key) => {
        this.eventForm.get(key)?.markAsTouched();
      });
      const firstError = Object.keys(this.eventForm.controls).find(
        (key) => this.eventForm.get(key)?.invalid,
      );
      if (firstError) {
        const msg = this.getFieldError(firstError) || 'Please fix the highlighted errors.';
        this.toastService.error(msg);
      }
      return;
    }

    this.isSaving = true;
    this.cdr.detectChanges();

    const formData = this.buildFormData();

    const request$ = this.isEditMode
      ? this.eventService.updateEvent(this.eventId, formData)
      : this.eventService.createEvent(formData);

    request$.subscribe({
      next: (res) => {
        if (res.success) {
          this.toastService.success(
            this.isEditMode ? 'Event updated successfully!' : 'Event created successfully!',
          );
          this.router.navigate([this.backRoute]);
        }
        this.isSaving = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.toastService.error(
          this.isEditMode
            ? 'Failed to update event. Please try again.'
            : 'Failed to create event. Please try again.',
        );
        this.isSaving = false;
        this.cdr.detectChanges();
      },
    });
  }

  private buildFormData(): FormData {
    const v = this.eventForm.value;
    const fd = new FormData();
    fd.append('Title', v.title ?? '');
    fd.append('EventDate', v.eventDate ?? '');
    fd.append('StartTime', (v.startTime ?? '') + ':00');
    fd.append('Price', String(v.price ?? 0));
    fd.append('TotalSeats', String(v.totalSeats ?? 0));
    fd.append('VenueName', v.venueName ?? '');
    fd.append('VenueAddress', v.venueAddress ?? '');
    fd.append('CityId', String(v.cityId ?? ''));

    if (v.description) fd.append('Description', v.description);
    if (v.genre) fd.append('Genre', v.genre);
    if (v.ageRestriction) fd.append('AgeRestriction', v.ageRestriction);
    if (v.performers) fd.append('Performers', v.performers);
    if (v.durationMins) fd.append('DurationMins', String(v.durationMins));
    if (v.categoryId) fd.append('CategoryId', String(v.categoryId));

    if (!this.isEditMode && v.organizerId) fd.append('OrganizerId', String(v.organizerId));

    if (this.isEditMode) {
      for (const url of this.removedPosterUrls) fd.append('RemovePosterUrls', url);
    }
    for (const file of this.selectedFiles) fd.append('posterImages', file, file.name);

    return fd;
  }
}
