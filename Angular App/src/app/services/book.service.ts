import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { Book } from '../models/book.model';
import { ApiResponse } from '../models/api-response.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class BookService {
  private base = environment.apiBaseUrl;

  constructor(private http: HttpClient) {}

  getBooks(): Observable<Book[]> {
    // Backend returns ApiResponse with Data containing books
    return this.http
      .get<ApiResponse<Book[]>>(`${this.base}/api/v1/books`)
      .pipe(map((r) => (r && (r as any).data) || []));
  }

  addBook(book: Omit<Book, 'id'>): Observable<number | null> {
    return this.http
      .post<ApiResponse<number>>(`${this.base}/api/v1/books`, book)
      .pipe(map((r) => (r && (r as any).data) || null));
  }

  // Update/Delete endpoints are not present in the current backend.
  // If you add update/delete server endpoints, implement them here.
}
