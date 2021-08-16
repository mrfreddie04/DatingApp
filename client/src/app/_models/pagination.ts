export interface Pagination {
  itemsPerPage: number;
  currentPage: number;
  totalPages: number;
  totalItems: number;
}

export class PaginatedResult<T> {
  pagination: Pagination;
  result: T;
}