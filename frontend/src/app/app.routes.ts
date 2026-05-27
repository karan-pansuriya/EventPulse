import { Routes } from '@angular/router';
import { authGuard } from './auth/guards/auth.guard';
import { APP_ROUTES } from './shared/constants/app-routes.constants';
import { RoleId } from './auth/models/auth.models';

export const routes: Routes = [
  // ── Auth
  {
    path: APP_ROUTES.AUTH.LOGIN,
    loadComponent: () =>
      import('./auth/features/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: APP_ROUTES.AUTH.REGISTER,
    loadComponent: () =>
      import('./auth/features/register/register.component').then((m) => m.RegisterComponent),
  },

  // ── Admin
  {
    path: APP_ROUTES.ADMIN.ROOT,
    canActivate: [authGuard],
    data: { roleIds: [RoleId.Admin] },
    loadComponent: () =>
      import('./core/features/admin/layout/admin-layout/admin-layout.component').then(
        (m) => m.AdminLayoutComponent,
      ),
    children: [
      {
        path: APP_ROUTES.ADMIN.DASHBOARD,
        loadComponent: () =>
          import('./core/features/admin/dashboard/dashboard.component').then(
            (m) => m.AdminDashboard,
          ),
      },
      {
        path: APP_ROUTES.ADMIN.USERS,
        loadComponent: () =>
          import('./core/features/admin/users/users.component').then((m) => m.UsersComponent),
      },
      {
        path: APP_ROUTES.ADMIN.CATEGORIES,
        loadComponent: () =>
          import('./core/features/admin/categories/categories.component').then(
            (m) => m.CategoriesComponent,
          ),
      },
      {
        path: APP_ROUTES.ADMIN.BOOKINGS,
        loadComponent: () =>
          import('./core/features/admin/bookings/bookings.component').then(
            (m) => m.BookingsComponent,
          ),
      },
      {
        path: APP_ROUTES.ADMIN.PROFILE,
        loadComponent: () =>
          import('./core/features/admin/profile/profile.component').then((m) => m.ProfileComponent),
      },
      { path: '', redirectTo: APP_ROUTES.ADMIN.DASHBOARD, pathMatch: 'full' },
    ],
  },

  // ── Organizer
  {
    path: APP_ROUTES.ORGANIZER.ROOT,
    canActivate: [authGuard],
    data: { roleIds: [RoleId.Organizer] },
    loadComponent: () =>
      import('./core/features/organizer/layout/organizer-layout/organizer-layout.component').then(
        (m) => m.OrganizerLayoutComponent,
      ),
    children: [
      {
        path: APP_ROUTES.ORGANIZER.DASHBOARD,
        loadComponent: () =>
          import('./core/features/organizer/dashboard/dashboard.component').then(
            (m) => m.OrganizerDashboard,
          ),
      },
      {
        path: APP_ROUTES.ORGANIZER.EVENTS,
        loadComponent: () =>
          import('./core/features/organizer/events/events.component').then(
            (m) => m.EventsComponent,
          ),
      },
      {
        path: APP_ROUTES.ORGANIZER.CREATE_EVENT,
        loadComponent: () =>
          import('./core/features/organizer/create-event/create-event.component').then(
            (m) => m.CreateEventComponent,
          ),
      },
      {
        path: APP_ROUTES.ORGANIZER.ATTENDEES,
        loadComponent: () =>
          import('./core/features/organizer/attendees/attendees.component').then(
            (m) => m.AttendeesComponent,
          ),
      },
      {
        path: APP_ROUTES.ORGANIZER.CHECK_IN,
        loadComponent: () =>
          import('./core/features/organizer/check-in/check-in.component').then(
            (m) => m.CheckInComponent,
          ),
      },
      {
        path: APP_ROUTES.ORGANIZER.PROFILE,
        loadComponent: () =>
          import('./core/features/organizer/profile/profile.component').then(
            (m) => m.ProfileComponent,
          ),
      },
      { path: '', redirectTo: APP_ROUTES.ORGANIZER.DASHBOARD, pathMatch: 'full' },
    ],
  },

  // ── Attendee
  {
    path: APP_ROUTES.ATTENDEE.ROOT,
    canActivate: [authGuard],
    data: { roleIds: [RoleId.Customer] },
    loadComponent: () =>
      import('./core/features/attendee/layout/attendee-layout/attendee-layout.component').then(
        (m) => m.AttendeeLayoutComponent,
      ),
    children: [
      {
        path: APP_ROUTES.ATTENDEE.HOME,
        loadComponent: () =>
          import('./core/features/attendee/home/home.component').then((m) => m.HomeComponent),
      },
      {
        path: APP_ROUTES.ATTENDEE.EVENT_DETAILS,
        loadComponent: () =>
          import('./core/features/attendee/home/event_details/event_details.component').then(
            (m) => m.EventDetailsComponent
          ),
      },
      {
        path: APP_ROUTES.ATTENDEE.MY_TICKETS,
        loadComponent: () =>
          import('./core/features/attendee/my-tickets/my-tickets.component').then(
            (m) => m.MyTicketsComponent,
          ),
      },
      {
        path: APP_ROUTES.ATTENDEE.BOOKINGS,
        loadComponent: () =>
          import('./core/features/attendee/bookings/bookings.component').then(
            (m) => m.BookingsComponent,
          ),
      },
      {
        path: APP_ROUTES.ATTENDEE.CHECKOUT,
        loadComponent: () =>
          import('./core/features/attendee/checkout/checkout.component').then(
            (m) => m.CheckoutComponent,
          ),
      },
      {
        path: APP_ROUTES.ATTENDEE.PAYMENT_SUCCESS,
        loadComponent: () =>
          import('./core/features/attendee/payment-success/payment-success.component').then(
            (m) => m.PaymentSuccessComponent,
          ),
      },
      {
        path: APP_ROUTES.ATTENDEE.PROFILE,
        loadComponent: () =>
          import('./core/features/attendee/profile/profile.component').then(
            (m) => m.ProfileComponent,
          ),
      },
      { path: '', redirectTo: APP_ROUTES.ATTENDEE.HOME, pathMatch: 'full' },
    ],
  },

  // ── Fallback
  {
    path: '',
    redirectTo: `/${APP_ROUTES.AUTH.LOGIN}`,
    pathMatch: 'full',
  },
];
