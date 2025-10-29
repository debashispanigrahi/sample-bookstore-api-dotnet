import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { User } from '../../models/user.model';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.scss'],
})
export class UserListComponent implements OnInit {
  users: User[] = [];
  loading = false;

  constructor(private http: HttpClient, public auth: AuthService) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading = true;
    // There's no users controller in the API project; try /api/v1/users
    this.http.get<any>(`${environment.apiBaseUrl}/api/v1/users`).subscribe({
      next: (r) => {
        this.users = (r && r.data) || [];
        this.loading = false;
      },
      error: () => {
        this.users = [];
        this.loading = false;
      },
    });
  }
}
