import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-primary-button',
  templateUrl: './primary-button.component.html',
  styleUrls: ['./primary-button.component.css'],
  imports: [CommonModule],
})
export class PrimaryButton {
  @Input() fullWidth: boolean = false;
  @Input() rounded: boolean = false;
  @Output() onButtonClick: EventEmitter<any> = new EventEmitter<any>();
  @Input() imageAtBack: boolean = false;
  @Input() src: string = '';
}
