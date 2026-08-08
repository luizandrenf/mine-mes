// Mirrors the DTOs and request contracts of MiniMes.Production. Serialized camelCase by the
// ASP.NET default; enum members travel as their name, never as a number.

export type ProductionOrderStatus =
  | "Draft"
  | "Released"
  | "InProgress"
  | "Completed"
  | "Cancelled";

export type ProductionOperationStatus =
  | "Pending"
  | "InProgress"
  | "Completed"
  | "Cancelled";

export interface Product {
  id: string;
  code: string;
  name: string;
  isActive: boolean;
  createdAt: string;
}

export interface ProductionOperation {
  id: string;
  productionOrderId: string;
  sequence: number;
  code: string;
  description: string;
  workCenterId: string;
  // ponytail: decimal on the wire, number here — fine up to 2^53; revisit if quantities ever
  // need exact fractional arithmetic on the client.
  plannedQuantity: number;
  status: ProductionOperationStatus;
  targetCycleTimeSeconds: number | null;
}

export interface ProductionOrder {
  id: string;
  orderNumber: string;
  productId: string;
  plannedQuantity: number;
  status: ProductionOrderStatus;
  priority: number;
  plannedStartAt: string | null;
  plannedEndAt: string | null;
  createdAt: string;
  releasedAt: string | null;
  /** Already sorted by sequence by the API. */
  operations: ProductionOperation[];
}

export interface CreateProductRequest {
  code: string;
  name: string;
}

export interface CreateProductionOrderRequest {
  orderNumber: string;
  productId: string;
  plannedQuantity: number;
  priority: number;
  plannedStartAt: string | null;
  plannedEndAt: string | null;
}

export interface AddProductionOperationRequest {
  sequence: number;
  code: string;
  description: string;
  workCenterId: string;
  plannedQuantity: number;
  targetCycleTimeSeconds: number | null;
}

/** RFC 7807, as written by DomainExceptionHandler. `errors` only appears on a 400. */
export interface ProblemDetails {
  status?: number;
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}
