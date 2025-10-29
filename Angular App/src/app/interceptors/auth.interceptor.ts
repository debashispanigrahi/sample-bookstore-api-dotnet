import { Injectable } from '@angular/core';
import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { SpinnerService } from '../services/spinner.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private auth: AuthService, private spinner: SpinnerService) {}

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    const token = this.auth.getToken();
    let cloned = req;
    if (token) {
      cloned = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
    }

    this.spinner.show();
    return next.handle(cloned).pipe(finalize(() => this.spinner.hide()));
  }
}
