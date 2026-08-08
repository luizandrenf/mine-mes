import type {
  Product,
  ProductionOperation,
  ProductionOrder,
} from "@/lib/api/types";

// Counterpart of the private static helpers at the top of the xUnit test classes.

export function aProduct(overrides: Partial<Product> = {}): Product {
  return {
    id: "11111111-1111-1111-1111-111111111111",
    code: "P-0001",
    name: "Motor",
    isActive: true,
    createdAt: "2026-08-08T12:00:00Z",
    ...overrides,
  };
}

export function anOperation(
  overrides: Partial<ProductionOperation> = {},
): ProductionOperation {
  return {
    id: "33333333-3333-3333-3333-333333333333",
    productionOrderId: "22222222-2222-2222-2222-222222222222",
    sequence: 10,
    code: "CUT",
    description: "Cut the raw bar",
    workCenterId: "44444444-4444-4444-4444-444444444444",
    plannedQuantity: 100,
    status: "Pending",
    targetCycleTimeSeconds: 30,
    ...overrides,
  };
}

export function anOrder(
  overrides: Partial<ProductionOrder> = {},
): ProductionOrder {
  return {
    id: "22222222-2222-2222-2222-222222222222",
    orderNumber: "OP-0001",
    productId: "11111111-1111-1111-1111-111111111111",
    plannedQuantity: 100,
    status: "Draft",
    priority: 1,
    plannedStartAt: null,
    plannedEndAt: null,
    createdAt: "2026-08-08T12:00:00Z",
    releasedAt: null,
    operations: [],
    ...overrides,
  };
}
