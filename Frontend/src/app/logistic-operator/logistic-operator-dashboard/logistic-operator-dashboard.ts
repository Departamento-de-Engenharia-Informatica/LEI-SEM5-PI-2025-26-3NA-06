import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-logistic-operator-dashboard',
  imports: [CommonModule, RouterModule],
  templateUrl: './logistic-operator-dashboard.html',
  styleUrl: './logistic-operator-dashboard.css',
  standalone: true,
})
export class LogisticOperatorDashboard {}
