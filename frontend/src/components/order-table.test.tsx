import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { OrderTable } from "@/components/order-table";
import type { Product } from "@/lib/api/types";
import { anOperation, anOrder, aProduct } from "@/lib/test/factories";

const productsById = (...products: Product[]) =>
  new Map(products.map((product) => [product.id, product]));

describe("OrderTable", () => {
  it("Renders_an_empty_message_without_orders", () => {
    render(<OrderTable orders={[]} productsById={productsById()} />);

    expect(screen.getByText("No production orders yet.")).toBeInTheDocument();
  });

  it("Joins_the_product_code_by_id", () => {
    render(
      <OrderTable
        orders={[anOrder({ productId: "p1" })]}
        productsById={productsById(aProduct({ id: "p1", code: "P-0001" }))}
      />,
    );

    expect(screen.getByText("P-0001")).toBeInTheDocument();
  });

  it("Falls_back_when_the_product_is_missing", () => {
    render(
      <OrderTable
        orders={[anOrder({ productId: "gone" })]}
        productsById={productsById()}
      />,
    );

    expect(screen.getByText("unknown")).toBeInTheDocument();
  });

  it("Links_to_the_order_detail_and_counts_its_operations", () => {
    render(
      <OrderTable
        orders={[
          anOrder({
            id: "abc",
            orderNumber: "OP-0001",
            operations: [
              anOperation({ id: "a", sequence: 10 }),
              anOperation({ id: "b", sequence: 20 }),
            ],
          }),
        ]}
        productsById={productsById(aProduct())}
      />,
    );

    expect(screen.getByRole("link", { name: "OP-0001" })).toHaveAttribute(
      "href",
      "/production-orders/abc",
    );
    expect(screen.getByText("2")).toBeInTheDocument();
  });
});
