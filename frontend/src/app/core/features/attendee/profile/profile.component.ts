import { Component } from '@angular/core';
import { ProfileComponent as SharedProfile } from '../../../../shared/components/profile/profile.component';

@Component({
  selector: 'app-attendee-profile',
  standalone: true,
  imports: [SharedProfile],
  template: `<shared-profile backLink="/attendee/home" themeColor="pink" />`,
})
export class ProfileComponent {}
