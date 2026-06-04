export interface PaginatedResponse<T> {
  data: T[];
  page: string;
  pageSize: string;
  totalItems: string;
  totalPages: string;
}

export interface ApiError {
  type: string | null;
  title: string | null;
  status: string;
  detail: string | null;
  instance: string | null;
}

export interface Pagination {
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}
