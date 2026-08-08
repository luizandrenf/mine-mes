import type { ProductionOperation, ProductionOrder } from "@/lib/api/types";

/**
 * Mirrors the invariants of ProductionOrder / ProductionOperation so the UI can grey out a button
 * instead of firing a request that comes back 422. The domain stays the authority: whatever slips
 * through is still refused by the API, and the ProblemDetails detail is shown as-is.
 */

export function canReleaseOrder(order: ProductionOrder): boolean {
  return order.status === "Draft" && order.operations.length > 0;
}

export function canStartOrder(order: ProductionOrder): boolean {
  return order.status === "Released";
}

export function canCompleteOrder(order: ProductionOrder): boolean {
  // A cancelled operation does not block completion — same reading as the entity.
  return (
    order.status === "InProgress" &&
    order.operations.every(
      (operation) =>
        operation.status === "Completed" || operation.status === "Cancelled",
    )
  );
}

export function canCancelOrder(order: ProductionOrder): boolean {
  return order.status === "Draft" || order.status === "Released";
}

export function canAddOperation(order: ProductionOrder): boolean {
  return order.status === "Draft";
}

export function canStartOperation(
  order: ProductionOrder,
  operation: ProductionOperation,
): boolean {
  return (
    order.status === "InProgress" &&
    operation.status === "Pending" &&
    order.operations
      .filter((other) => other.sequence < operation.sequence)
      .every(
        (other) => other.status === "Completed" || other.status === "Cancelled",
      )
  );
}

export function canCompleteOperation(operation: ProductionOperation): boolean {
  return operation.status === "InProgress";
}

export function canCancelOperation(operation: ProductionOperation): boolean {
  // Cancelling an already cancelled operation is allowed by the entity, and idempotent.
  return operation.status !== "Completed";
}
