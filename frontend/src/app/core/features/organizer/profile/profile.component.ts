import { Component } from '@angular/core';
import { ProfileComponent as SharedProfile } from '../../../../shared/components/profile/profile.component';

@Component({
  selector: 'app-organizer-profile',
  standalone: true,
  imports: [SharedProfile],
  template: `<shared-profile backLink="/organizer/dashboard" themeColor="pink" />`,
})
export class ProfileComponent {}
