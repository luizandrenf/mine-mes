import type { CreateProductRequest, Product } from "@/lib/api/types";
import type { HttpClient } from "@/lib/http/http-client";

export class ProductService {
  constructor(private readonly http: HttpClient) {}

  getAll(): Promise<Product[]> {
    return this.http.get<Product[]>("/api/products");
  }

  getById(id: string): Promise<Product> {
    return this.http.get<Product>(`/api/products/${id}`);
  }

  create(request: CreateProductRequest): Promise<Product> {
    return this.http.post<Product>("/api/products", request);
  }

  setActive(id: string, active: boolean): Promise<void> {
    return this.http.patchNoContent(
      `/api/products/${id}/${active ? "activate" : "deactivate"}`,
    );
  }
}
