import { Component } from '@angular/core';
import { ProfileComponent as SharedProfile } from '../../../../shared/components/profile/profile.component';

@Component({
  selector: 'app-admin-profile',
  standalone: true,
  imports: [SharedProfile],
  template: `<shared-profile backLink="/admin/dashboard" themeColor="pink" />`,
})
export class ProfileComponent {}
