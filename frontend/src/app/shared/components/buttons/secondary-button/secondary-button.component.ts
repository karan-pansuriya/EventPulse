import { CommonModule } from '@angular/common';
import { Component, Input, Output, EventEmitter, HostListener } from '@angular/core';

@Component({
  selector: 'app-secondary-button',
  imports: [CommonModule],
  templateUrl: './secondary-button.component.html',
  styleUrl: './secondary-button.component.css',
})
export class SecondaryButton {
  @Input() fullWidth: boolean = false;
  @Input() rounded: boolean = false;
  @Output() onButtonClick: EventEmitter<any> = new EventEmitter<any>();
  @Input() imageAtBack: boolean = false;
  @Input() src: string = '';
}
