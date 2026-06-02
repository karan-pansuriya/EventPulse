import { ChangeDetectorRef, Component, OnInit, OnDestroy, ViewChild, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { OrganizerEventService } from '../services/organizer-event.service';
import { CategoryService } from '../../attendee/home/services/category.service';
import { LocationService } from '../services/location.service';
import { Category } from '../../attendee/home/models/category.models';
import { Country, State, City } from '../models/location.models';
import { ToastService } from '../../../../shared/services/toast.service';
import { AuthService } from '../../../../auth/services/auth.service';
import { RoleId } from '../../../../auth/models/auth.models';
import { AdminUserService, OrganizerResponse } from '../../admin/services/admin-user.service';

@Component({
  selector: 'app-create-event',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule],
  templateUrl: './create-event.component.html',
  styleUrl: './create-event.component.css',
})
export class CreateEventComponent implements OnInit, OnDestroy {
  @ViewChild('eventForm') eventForm!: NgForm;

  private eventService = inject(OrganizerEventService);
  private categoryService = inject(CategoryService);
  private locationService = inject(LocationService);
  private toastService = inject(ToastService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  private authService = inject(AuthService);
  private adminUserService = inject(AdminUserService);

  categories: Category[] = [];
  countries: Country[] = [];
  states: State[] = [];
  cities: City[] = [];
  organizers: OrganizerResponse[] = [];

  isSaving = false;
  isLoadingCategories = true;
  isLoadingCountries = true;
  isLoadingOrganizers = false;
  isLoadingStates = false;
  isLoadingCities = false;
  submitted = false;
  isAdmin = false;

  minDate = new Date().toISOString().slice(0, 10);

  get minTime(): string | null {
    if (this.form.eventDate !== this.minDate) return null;
    const now = new Date();
    return `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`;
  }

  form = {
    title: '',
    description: '',
    genre: '',
    ageRestriction: '',
    performers: '',
    durationMins: null as number | null,
    categoryId: null as number | null,
    eventDate: '',
    startTime: '',
    price: null as number | null,
    totalSeats: null as number | null,
    venueName: '',
    venueAddress: '',
    countryId: null as number | null,
    stateId: null as number | null,
    cityId: null as number | null,
    organizerId: null as number | null,
  };

  selectedFiles: File[] = [];
  selectedFilePreviews: string[] = [];

  ngOnInit(): void {
    this.isAdmin = this.authService.user()?.roleIds.includes(RoleId.Admin) ?? false;

    this.categoryService.getAll().subscribe({
      next: (res) => {
        if (res.success && res.data) this.categories = res.data;
        this.isLoadingCategories = false;
        this.cdr.detectChanges();
      },
      error: () => { this.isLoadingCategories = false; this.cdr.detectChanges(); },
    });

    this.locationService.getCountries().subscribe({
      next: (res) => {
        if (res.success && res.data) this.countries = res.data;
        this.isLoadingCountries = false;
        this.cdr.detectChanges();
      },
      error: () => { this.isLoadingCountries = false; this.cdr.detectChanges(); },
    });

    if (this.isAdmin) {
      this.isLoadingOrganizers = true;
      this.adminUserService.getOrganizers().subscribe({
        next: (res) => {
          if (res.success && res.data) this.organizers = res.data;
          this.isLoadingOrganizers = false;
          this.cdr.detectChanges();
        },
        error: () => { this.isLoadingOrganizers = false; this.cdr.detectChanges(); },
      });
    }
  }

  ngOnDestroy(): void {
    this.selectedFilePreviews.forEach(u => URL.revokeObjectURL(u));
  }

  onCountryChange(): void {
    this.form.stateId = null;
    this.form.cityId = null;
    this.states = [];
    this.cities = [];
    if (!this.form.countryId) return;

    this.isLoadingStates = true;
    this.locationService.getStates(this.form.countryId).subscribe({
      next: (res) => {
        if (res.success && res.data) this.states = res.data;
        this.isLoadingStates = false;
        this.cdr.detectChanges();
      },
      error: () => { this.isLoadingStates = false; this.cdr.detectChanges(); },
    });
  }

  onStateChange(): void {
    this.form.cityId = null;
    this.cities = [];
    if (!this.form.stateId) return;

    this.isLoadingCities = true;
    this.locationService.getCities(this.form.stateId).subscribe({
      next: (res) => {
        if (res.success && res.data) this.cities = res.data;
        this.isLoadingCities = false;
        this.cdr.detectChanges();
      },
      error: () => { this.isLoadingCities = false; this.cdr.detectChanges(); },
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;

    const maxFiles = 5;
    const maxSize = 5 * 1024 * 1024;

    const files = Array.from(input.files);

    const allowedExtensions = ['jpg', 'jpeg', 'png'];
    const invalid = files.find(f => {
      const ext = f.name.split('.').pop()?.toLowerCase();
      return !ext || !allowedExtensions.includes(ext);
    });
    if (invalid) {
      this.toastService.error(`${invalid.name} has an unsupported file type. Only JPG, JPEG, and PNG are allowed.`, 'Invalid file');
      input.value = '';
      return;
    }
    const oversized = files.find(f => f.size > maxSize);
    if (oversized) {
      this.toastService.error(`${oversized.name} exceeds the 5 MB limit.`, 'File too large');
      input.value = '';
      return;
    }
    if (files.length > maxFiles) {
      this.toastService.error(`Maximum ${maxFiles} poster images allowed.`, 'Too many files');
      input.value = '';
      return;
    }
    this.selectedFiles = files;
    this.selectedFilePreviews = files.map(f => URL.createObjectURL(f));
  }

  removeFile(index: number): void {
    URL.revokeObjectURL(this.selectedFilePreviews[index]);
    this.selectedFiles = this.selectedFiles.filter((_, i) => i !== index);
    this.selectedFilePreviews = this.selectedFilePreviews.filter((_, i) => i !== index);
  }

  isFieldInvalid(fieldName: string): boolean {
    const control = this.eventForm?.controls[fieldName];
    return !!control && control.invalid && (control.touched || control.dirty || this.submitted);
  }

  onSubmit(): void {
    this.submitted = true;

    if (this.eventForm?.invalid) {
      Object.keys(this.eventForm.controls).forEach(key => {
        this.eventForm.controls[key].markAsTouched();
      });
      this.cdr.detectChanges();
      return;
    }

    const error = this.validate();
    if (error) {
      this.toastService.error(error);
      return;
    }

    this.isSaving = true;
    this.cdr.detectChanges();

    const formData = this.buildFormData();

    this.eventService.createEvent(formData).subscribe({
      next: (res) => {
        if (res.success) {
          this.toastService.success('Event created successfully!');
          this.router.navigate(['/organizer/events']);
        }
        this.isSaving = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.toastService.error('Failed to create event. Please try again.');
        this.isSaving = false;
        this.cdr.detectChanges();
      },
    });
  }

  private validate(): string | null {
    if (this.isAdmin && !this.form.organizerId) return 'Please select an organizer.';
    if (!this.form.title || this.form.title.length < 2) return 'Title must be at least 2 characters.';
    if (this.form.title.length > 200) return 'Title must not exceed 200 characters.';
    if (this.form.description && this.form.description.length > 1000) return 'Description must not exceed 1000 characters.';
    if (this.form.genre && this.form.genre.length > 100) return 'Genre must not exceed 100 characters.';
    if (this.form.ageRestriction && !/^[0-9+]{0,10}$/.test(this.form.ageRestriction)) return 'Age restriction must be numeric (e.g. 18+).';
    if (this.form.performers && this.form.performers.length > 200) return 'Performers must not exceed 200 characters.';
    if (this.form.durationMins !== null && (this.form.durationMins < 1 || this.form.durationMins > 420)) return 'Duration must be between 1 and 420 minutes.';
    if (!this.form.eventDate) return 'Event date is required.';
    if (this.form.eventDate < this.minDate) return 'Event date must be today or later.';
    if (this.form.eventDate === this.minDate && this.form.startTime && this.minTime && this.form.startTime < this.minTime) return 'Start time must be in the future.';
    if (!this.form.startTime) return 'Start time is required.';
    if (this.form.price === null || this.form.price < 0) return 'Price must be a valid amount (0 or more).';
    if (this.form.price > 9999999999) return 'Price must not exceed 10 digits.';
    if (this.form.totalSeats === null || this.form.totalSeats < 1) return 'Total seats must be at least 1.';
    if (this.form.totalSeats > 100000) return 'Total seats must not exceed 100,000.';
    if (!this.form.venueName) return 'Venue name is required.';
    if (this.form.venueName.length > 150) return 'Venue name must not exceed 150 characters.';
    if (!this.form.venueAddress) return 'Venue address is required.';
    if (this.form.venueAddress.length > 200) return 'Venue address must not exceed 200 characters.';
    if (!this.form.cityId) return 'Please select a city.';
    return null;
  }

  private buildFormData(): FormData {
    const fd = new FormData();
    fd.append('Title', this.form.title);
    fd.append('EventDate', this.form.eventDate);
    fd.append('StartTime', this.form.startTime + ':00');
    fd.append('Price', String(this.form.price ?? 0));
    fd.append('TotalSeats', String(this.form.totalSeats ?? 0));
    fd.append('VenueName', this.form.venueName);
    fd.append('VenueAddress', this.form.venueAddress);
    fd.append('CityId', String(this.form.cityId));

    if (this.form.description) fd.append('Description', this.form.description);
    if (this.form.genre) fd.append('Genre', this.form.genre);
    if (this.form.ageRestriction) fd.append('AgeRestriction', this.form.ageRestriction);
    if (this.form.performers) fd.append('Performers', this.form.performers);
    if (this.form.durationMins) fd.append('DurationMins', String(this.form.durationMins));
    if (this.form.categoryId) fd.append('CategoryId', String(this.form.categoryId));
    if (this.form.organizerId) fd.append('OrganizerId', String(this.form.organizerId));

    for (const file of this.selectedFiles) fd.append('posterImages', file, file.name);

    return fd;
  }
}
