import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BookService } from '../../services/book.service';
import { Book } from '../../models/book.model';

@Component({
  selector: 'app-book-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './book-list.component.html',
  styleUrls: ['./book-list.component.scss']
})
export class BookListComponent implements OnInit {
  books: Book[] = [];
  loading = false;
  editingBookId: number | null = null;
  editingBook: Book | null = null;
  isAddingNew = false;
  newBook: Omit<Book, 'id'> = {
    title: '',
    author: '',
    isbn: '',
    price: 0,
    publishedDate: '',
    genre: '',
    inStock: true
  };

  constructor(private bookService: BookService) {}

  ngOnInit(): void {
    this.bookService.loading$.subscribe(l => this.loading = l);
    this.bookService.getBooks().subscribe(books => this.books = books);
  }

  startEdit(book: Book): void {
    this.editingBookId = book.id;
    this.editingBook = { ...book };
    this.isAddingNew = false;
  }

  cancelEdit(): void {
    this.editingBookId = null;
    this.editingBook = null;
    this.isAddingNew = false;
    this.resetNewBook();
  }

  saveEdit(): void {
    if (this.editingBook) {
      this.bookService.updateBook(this.editingBook).subscribe(() => this.cancelEdit());
    }
  }

  deleteBook(id: number): void {
    if (confirm('Are you sure you want to delete this book?')) {
      this.bookService.deleteBook(id).subscribe();
    }
  }

  startAddNew(): void {
    this.isAddingNew = true;
    this.editingBookId = null;
    this.editingBook = null;
  }

  saveNewBook(): void {
    if (this.validateNewBook()) {
      this.bookService.addBook(this.newBook).subscribe(() => this.cancelEdit());
    }
  }

  validateNewBook(): boolean {
    return this.newBook.title.trim() !== '' && 
           this.newBook.author.trim() !== '' && 
           this.newBook.isbn.trim() !== '' &&
           this.newBook.price > 0;
  }

  private resetNewBook(): void {
    this.newBook = {
      title: '',
      author: '',
      isbn: '',
      price: 0,
      publishedDate: '',
      genre: '',
      inStock: true
    };
  }

  isEditing(bookId: number): boolean {
    return this.editingBookId === bookId;
  }
}