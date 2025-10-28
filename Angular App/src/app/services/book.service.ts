import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { delay, finalize, tap } from 'rxjs/operators';
import { Book } from '../models/book.model';

@Injectable({
  providedIn: 'root'
})
export class BookService {
  private booksSubject = new BehaviorSubject<Book[]>(this.getInitialBooks());
  public books$ = this.booksSubject.asObservable();
  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  constructor() { }

  private getInitialBooks(): Book[] {
    return [
      {
        id: 1,
        title: 'The Great Gatsby',
        author: 'F. Scott Fitzgerald',
        isbn: '978-0-7432-7356-5',
        price: 12.99,
        publishedDate: '1925-04-10',
        genre: 'Fiction',
        inStock: true
      },
      {
        id: 2,
        title: 'To Kill a Mockingbird',
        author: 'Harper Lee',
        isbn: '978-0-06-112008-4',
        price: 14.99,
        publishedDate: '1960-07-11',
        genre: 'Fiction',
        inStock: true
      },
      {
        id: 3,
        title: '1984',
        author: 'George Orwell',
        isbn: '978-0-452-28423-4',
        price: 13.99,
        publishedDate: '1949-06-08',
        genre: 'Dystopian Fiction',
        inStock: false
      },
      {
        id: 4,
        title: 'Pride and Prejudice',
        author: 'Jane Austen',
        isbn: '978-0-14-143951-8',
        price: 11.99,
        publishedDate: '1813-01-28',
        genre: 'Romance',
        inStock: true
      },
      {
        id: 5,
        title: 'The Catcher in the Rye',
        author: 'J.D. Salinger',
        isbn: '978-0-316-76948-0',
        price: 15.99,
        publishedDate: '1951-07-16',
        genre: 'Fiction',
        inStock: true
      }
    ];
  }

  getBooks(): Observable<Book[]> {
    // Simulate latency for demo only
    this.setLoading(true);
    return of(this.booksSubject.value).pipe(
      delay(400),
      finalize(() => this.setLoading(false))
    );
  }

  addBook(book: Omit<Book, 'id'>): Observable<Book> {
    this.setLoading(true);
    const books = this.booksSubject.value;
    const newId = books.length ? Math.max(...books.map(b => b.id)) + 1 : 1;
    const newBook: Book = { ...book, id: newId };
    return of(newBook).pipe(
      delay(400),
      tap(created => this.booksSubject.next([...books, created])),
      finalize(() => this.setLoading(false))
    );
  }

  updateBook(updatedBook: Book): Observable<Book> {
    this.setLoading(true);
    return of(updatedBook).pipe(
      delay(400),
      tap(u => {
        const books = this.booksSubject.value;
        const index = books.findIndex(b => b.id === u.id);
        if (index !== -1) {
          const copy = [...books];
          copy[index] = { ...u };
          this.booksSubject.next(copy);
        }
      }),
      finalize(() => this.setLoading(false))
    );
  }

  deleteBook(id: number): Observable<number> {
    this.setLoading(true);
    return of(id).pipe(
      delay(400),
      tap(delId => {
        const books = this.booksSubject.value;
        this.booksSubject.next(books.filter(b => b.id !== delId));
      }),
      finalize(() => this.setLoading(false))
    );
  }

  private setLoading(isLoading: boolean) {
    this.loadingSubject.next(isLoading);
  }
}