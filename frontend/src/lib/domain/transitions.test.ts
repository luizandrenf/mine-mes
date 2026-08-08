import { describe, expect, it } from "vitest";
import type { ProductionOrderStatus } from "@/lib/api/types";
import {
  canAddOperation,
  canCancelOperation,
  canCancelOrder,
  canCompleteOperation,
  canCompleteOrder,
  canReleaseOrder,
  canStartOperation,
  canStartOrder,
} from "@/lib/domain/transitions";
import { anOperation, anOrder } from "@/lib/test/factories";

describe("order transitions", () => {
  it("Release_requires_a_draft_order_with_at_least_one_operation", () => {
    expect(canReleaseOrder(anOrder({ status: "Draft" }))).toBe(false);
    expect(
      canReleaseOrder(
        anOrder({ status: "Draft", operations: [anOperation()] }),
      ),
    ).toBe(true);
  });

  it.each<ProductionOrderStatus>([
    "Released",
    "InProgress",
    "Completed",
    "Cancelled",
  ])("Release_is_unavailable_when_status_is_%s", (status) => {
    expect(
      canReleaseOrder(anOrder({ status, operations: [anOperation()] })),
    ).toBe(false);
  });

  it("Start_requires_a_released_order", () => {
    expect(canStartOrder(anOrder({ status: "Released" }))).toBe(true);
    expect(canStartOrder(anOrder({ status: "Draft" }))).toBe(false);
    expect(canStartOrder(anOrder({ status: "InProgress" }))).toBe(false);
  });

  it("Complete_requires_an_in_progress_order", () => {
    expect(canCompleteOrder(anOrder({ status: "Released" }))).toBe(false);
    expect(canCompleteOrder(anOrder({ status: "InProgress" }))).toBe(true);
  });

  it("Complete_is_blocked_by_a_pending_or_in_progress_operation", () => {
    const order = anOrder({
      status: "InProgress",
      operations: [
        anOperation({ sequence: 10, status: "Completed" }),
        anOperation({ sequence: 20, status: "Pending" }),
      ],
    });

    expect(canCompleteOrder(order)).toBe(false);
  });

  it("Complete_is_not_blocked_by_a_cancelled_operation", () => {
    const order = anOrder({
      status: "InProgress",
      operations: [
        anOperation({ sequence: 10, status: "Completed" }),
        anOperation({ sequence: 20, status: "Cancelled" }),
      ],
    });

    expect(canCompleteOrder(order)).toBe(true);
  });

  it.each<[ProductionOrderStatus, boolean]>([
    ["Draft", true],
    ["Released", true],
    ["InProgress", false],
    ["Completed", false],
    ["Cancelled", false],
  ])("Cancel_from_%s_is_%s", (status, expected) => {
    expect(canCancelOrder(anOrder({ status }))).toBe(expected);
  });

  it.each<[ProductionOrderStatus, boolean]>([
    ["Draft", true],
    ["Released", false],
    ["InProgress", false],
    ["Completed", false],
    ["Cancelled", false],
  ])("AddOperation_from_%s_is_%s", (status, expected) => {
    expect(canAddOperation(anOrder({ status }))).toBe(expected);
  });
});

describe("operation transitions", () => {
  const inProgressOrder = (
    ...operations: ReturnType<typeof anOperation>[]
  ) => anOrder({ status: "InProgress", operations });

  it("Start_requires_an_in_progress_order", () => {
    const operation = anOperation({ sequence: 10, status: "Pending" });

    expect(
      canStartOperation(
        anOrder({ status: "Released", operations: [operation] }),
        operation,
      ),
    ).toBe(false);
    expect(canStartOperation(inProgressOrder(operation), operation)).toBe(true);
  });

  it("Start_requires_a_pending_operation", () => {
    const operation = anOperation({ sequence: 10, status: "InProgress" });

    expect(canStartOperation(inProgressOrder(operation), operation)).toBe(false);
  });

  it("Start_is_blocked_while_a_previous_operation_is_not_finished", () => {
    const first = anOperation({ id: "a", sequence: 10, status: "InProgress" });
    const second = anOperation({ id: "b", sequence: 20, status: "Pending" });

    expect(canStartOperation(inProgressOrder(first, second), second)).toBe(
      false,
    );
  });

  it.each<["Completed" | "Cancelled"]>([["Completed"], ["Cancelled"]])(
    "Start_is_allowed_when_the_previous_operation_is_%s",
    (status) => {
      const first = anOperation({ id: "a", sequence: 10, status });
      const second = anOperation({ id: "b", sequence: 20, status: "Pending" });

      expect(canStartOperation(inProgressOrder(first, second), second)).toBe(
        true,
      );
    },
  );

  it("Complete_requires_an_in_progress_operation", () => {
    expect(canCompleteOperation(anOperation({ status: "InProgress" }))).toBe(
      true,
    );
    expect(canCompleteOperation(anOperation({ status: "Pending" }))).toBe(false);
    expect(canCompleteOperation(anOperation({ status: "Completed" }))).toBe(
      false,
    );
  });

  it("Cancel_is_refused_only_for_a_completed_operation", () => {
    expect(canCancelOperation(anOperation({ status: "Pending" }))).toBe(true);
    expect(canCancelOperation(anOperation({ status: "InProgress" }))).toBe(true);
    expect(canCancelOperation(anOperation({ status: "Cancelled" }))).toBe(true);
    expect(canCancelOperation(anOperation({ status: "Completed" }))).toBe(false);
  });
});
