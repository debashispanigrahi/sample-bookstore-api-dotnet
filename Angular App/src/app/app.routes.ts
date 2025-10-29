import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { BookListComponent } from './components/book-list/book-list.component';
import { UserListComponent } from './components/user-list/user-list.component';
import { authGuard } from './guards/auth.guard';
import { SignupComponent } from './components/signup/signup.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'signup', component: SignupComponent },
  { path: 'users', component: UserListComponent, canActivate: [authGuard] },
  { path: '', component: BookListComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: '' },
];
