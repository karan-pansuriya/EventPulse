import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { HeaderComponent } from '../../../../../shared/components/header/header.component';
@Component({
  selector: 'app-organizer-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, HeaderComponent],
  templateUrl: './organizer-layout.component.html',
  styleUrl: './organizer-layout.component.css',
})
export class OrganizerLayoutComponent {}
