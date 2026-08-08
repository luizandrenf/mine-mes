import type {
  AddProductionOperationRequest,
  CreateProductionOrderRequest,
  ProductionOperation,
  ProductionOrder,
} from "@/lib/api/types";
import type { HttpClient } from "@/lib/http/http-client";

export type OrderTransition = "release" | "start" | "complete" | "cancel";
export type OperationTransition = "start" | "complete" | "cancel";

export class ProductionOrderService {
  constructor(private readonly http: HttpClient) {}

  getAll(): Promise<ProductionOrder[]> {
    return this.http.get<ProductionOrder[]>("/api/production-orders");
  }

  getById(id: string): Promise<ProductionOrder> {
    return this.http.get<ProductionOrder>(`/api/production-orders/${id}`);
  }

  create(request: CreateProductionOrderRequest): Promise<ProductionOrder> {
    return this.http.post<ProductionOrder>("/api/production-orders", request);
  }

  /** Every transition answers 204, so the caller must re-read the order to see the new status. */
  transition(id: string, transition: OrderTransition): Promise<void> {
    return this.http.postNoContent(`/api/production-orders/${id}/${transition}`);
  }

  addOperation(
    orderId: string,
    request: AddProductionOperationRequest,
  ): Promise<ProductionOperation> {
    return this.http.post<ProductionOperation>(
      `/api/production-orders/${orderId}/operations`,
      request,
    );
  }

  transitionOperation(
    orderId: string,
    operationId: string,
    transition: OperationTransition,
  ): Promise<void> {
    return this.http.postNoContent(
      `/api/production-orders/${orderId}/operations/${operationId}/${transition}`,
    );
  }
}
