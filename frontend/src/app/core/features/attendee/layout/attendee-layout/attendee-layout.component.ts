import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from '../../../../../shared/components/header/header.component';

@Component({
  selector: 'app-attendee-layout',
  standalone: true,
  imports: [RouterOutlet, HeaderComponent],
  templateUrl: './attendee-layout.component.html',
  styleUrl: './attendee-layout.component.css',
})
export class AttendeeLayoutComponent {}
