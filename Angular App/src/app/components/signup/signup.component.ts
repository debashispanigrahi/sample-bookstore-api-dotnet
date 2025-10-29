import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.scss'],
})
export class SignupComponent {
  username = '';
  email = '';
  password = '';
  role = 'User';
  error = '';
  success = '';

  constructor(public auth: AuthService, private router: Router) {
    // Ensure non-admin visitors cannot set role to Admin
    try {
      if (!this.auth.hasRole('Admin')) {
        this.role = 'User';
      }
    } catch {
      this.role = 'User';
    }
  }

  submit() {
    this.error = '';
    this.success = '';
    const sendRole = this.auth.hasRole('Admin') ? this.role : 'User';
    this.auth
      .register(this.username, this.email, this.password, sendRole)
      .subscribe({
        next: (res) => {
          this.success = 'Registration successful. Please login.';
          // navigate to login after short delay
          setTimeout(() => this.router.navigate(['/login']), 1200);
        },
        error: (err) =>
          (this.error = (err && err.message) || 'Registration failed'),
      });
  }
}
