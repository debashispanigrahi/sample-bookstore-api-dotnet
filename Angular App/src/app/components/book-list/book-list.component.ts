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
  styleUrls: ['./book-list.component.scss'],
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
    inStock: true,
  };

  constructor(private bookService: BookService) {}

  ngOnInit(): void {
    this.loadBooks();
  }

  private loadBooks(): void {
    this.loading = true;
    this.bookService.getBooks().subscribe({
      next: (books) => {
        this.books = books;
        this.loading = false;
      },
      error: () => (this.loading = false),
    });
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
    // Update endpoint not implemented in backend yet
    alert('Update is not implemented on the backend.');
    this.cancelEdit();
  }

  deleteBook(id: number): void {
    // Delete endpoint not implemented in backend yet
    alert('Delete is not implemented on the backend.');
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
    return (
      this.newBook.title.trim() !== '' &&
      this.newBook.author.trim() !== '' &&
      this.newBook.isbn.trim() !== '' &&
      this.newBook.price > 0
    );
  }

  private resetNewBook(): void {
    this.newBook = {
      title: '',
      author: '',
      isbn: '',
      price: 0,
      publishedDate: '',
      genre: '',
      inStock: true,
    };
  }

  isEditing(bookId: number): boolean {
    return this.editingBookId === bookId;
  }
}
