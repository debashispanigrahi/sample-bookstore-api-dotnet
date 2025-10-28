# Book Store App (Angular 19 Standalone)

An Angular 19 sample Book Store application using the standalone component approach and Bootstrap for styling. It demonstrates an editable table of books backed by a simple RxJS `BehaviorSubject` service with in-memory static data.

## Features
* Standalone Angular components (no NgModules)
* Bootstrap styling (imported via global `styles.scss`)
* Editable table cells (click to edit inline)
* Add new book row with validation
* Save / Cancel actions while editing or adding
* Delete existing book entries
* Reactive data updates via `BookService` using `BehaviorSubject`

## Tech Stack
* Angular 19 (standalone API)
* TypeScript strict mode
* RxJS for state management (`BehaviorSubject`)
* SCSS + Bootstrap 5

## Project Structure (Key Parts)
```
src/app/
	models/book.model.ts          # Book interface definition
	services/book.service.ts      # In-memory CRUD service
	components/book-list/         # Standalone component with editable table
	app.component.ts / .html      # Root component wiring the book list
styles.scss                     # Global styles + Bootstrap import
```

## Run the App

```powershell
npm install
npm start
```

Navigate to: http://localhost:4200/

### Alternate (explicit) dev serve
```powershell
ng serve --configuration development
```

## Editing & Adding Books
* Click any non-action cell to begin editing that row (row is highlighted).
* Use the "Add New Book" button to insert a new temporary row at the top.
* Save buttons are disabled until required fields are filled (`title`, `author`, `isbn`, `price > 0`).
* Delete prompts for confirmation.

## Build (Production)
```powershell
ng build --configuration production
```
Artifacts output to `dist/book-store-app`.

## Testing
Basic unit tests are scaffolded by the CLI (none custom yet). Run:
```powershell
ng test
```
Potential future tests:
* BookService CRUD behavior
* Inline editing component state transitions

## Future Enhancements (Ideas)
* Persist data to backend or localStorage
* Pagination & sorting
* Filter/search bar
* Form-based add/edit dialog instead of inline
* Unit test coverage for editing workflow
* Dark mode theme

## Resources
* Angular Docs: https://angular.dev
* CLI Reference: https://angular.dev/tools/cli
* Bootstrap Docs: https://getbootstrap.com

---
Generated with Angular CLI 19.x – Standalone component demo.
