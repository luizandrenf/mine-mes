import { describe, expect, it } from "vitest";
import {
  type OperationTransition,
  type OrderTransition,
  ProductionOrderService,
} from "@/lib/services/production-order.service";
import { anOperation, anOrder } from "@/lib/test/factories";
import { FakeHttpClient } from "@/lib/test/fake-http-client";

function service() {
  const http = new FakeHttpClient();
  return { http, service: new ProductionOrderService(http) };
}

describe("ProductionOrderService", () => {
  it("GetAll_reads_the_collection_route", async () => {
    const { http, service: subject } = service();
    http.returns([anOrder()]);

    const orders = await subject.getAll();

    expect(http.lastCall).toEqual({
      method: "GET",
      path: "/api/production-orders",
    });
    expect(orders).toHaveLength(1);
  });

  it("GetById_reads_the_item_route_with_its_operations", async () => {
    const { http, service: subject } = service();
    http.returns(anOrder({ id: "abc", operations: [anOperation()] }));

    const order = await subject.getById("abc");

    expect(http.lastCall.path).toBe("/api/production-orders/abc");
    expect(order.operations).toHaveLength(1);
  });

  it("Create_posts_the_request_body", async () => {
    const { http, service: subject } = service();
    http.returns(anOrder());

    await subject.create({
      orderNumber: "OP-0001",
      productId: "p1",
      plannedQuantity: 100,
      priority: 1,
      plannedStartAt: null,
      plannedEndAt: null,
    });

    expect(http.lastCall.method).toBe("POST");
    expect(http.lastCall.path).toBe("/api/production-orders");
    expect(http.lastCall.body).toMatchObject({ orderNumber: "OP-0001" });
  });

  it.each<OrderTransition>(["release", "start", "complete", "cancel"])(
    "Transition_%s_posts_to_the_matching_route",
    async (transition) => {
      const { http, service: subject } = service();

      await subject.transition("abc", transition);

      expect(http.lastCall).toEqual({
        method: "POST",
        path: `/api/production-orders/abc/${transition}`,
        body: undefined,
      });
    },
  );

  it("AddOperation_posts_under_the_order", async () => {
    const { http, service: subject } = service();
    http.returns(anOperation());

    await subject.addOperation("abc", {
      sequence: 10,
      code: "CUT",
      description: "Cut the raw bar",
      workCenterId: "wc1",
      plannedQuantity: 100,
      targetCycleTimeSeconds: null,
    });

    expect(http.lastCall.path).toBe("/api/production-orders/abc/operations");
    expect(http.lastCall.body).toMatchObject({ sequence: 10, code: "CUT" });
  });

  it.each<OperationTransition>(["start", "complete", "cancel"])(
    "TransitionOperation_%s_posts_to_the_matching_route",
    async (transition) => {
      const { http, service: subject } = service();

      await subject.transitionOperation("abc", "op1", transition);

      expect(http.lastCall.path).toBe(
        `/api/production-orders/abc/operations/op1/${transition}`,
      );
    },
  );
});
