export interface ApiResponse<T = any> {
  data?: T;
  statusCode?: number;
  errorMessage?: string;
}
