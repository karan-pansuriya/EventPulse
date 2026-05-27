import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-payment-success',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './payment-success.component.html',
  styleUrl: './payment-success.component.css',
})
export class PaymentSuccessComponent implements OnInit {
  private route = inject(ActivatedRoute);
  bookingCode: string | null = null;

  ngOnInit(): void {
    this.bookingCode = this.route.snapshot.queryParamMap.get('code');
  }
}
