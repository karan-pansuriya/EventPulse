import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-button-primary-outline',
  imports: [CommonModule],
  templateUrl: './button-primary-outline.component.html',
  styleUrl: './button-primary-outline.component.css',
})
export class ButtonPrimaryOutline {
  @Input() fullWidth: boolean = false;
  @Input() rounded: boolean = false;
  @Output() onButtonClick: EventEmitter<any> = new EventEmitter<any>();
  @Input() imageAtBack: boolean = false;
  @Input() src: string = '';
}
