import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { idle } from "@/components/action-button";
import { ProductTable } from "@/components/product-table";
import { aProduct } from "@/lib/test/factories";

const noop = async () => idle;

describe("ProductTable", () => {
  it("Renders_an_empty_message_without_products", () => {
    render(<ProductTable products={[]} toggleActive={noop} />);

    expect(screen.getByText("No products yet.")).toBeInTheDocument();
  });

  it("Renders_one_row_per_product", () => {
    render(
      <ProductTable
        products={[
          aProduct({ id: "1", code: "P-0001", name: "Motor" }),
          aProduct({ id: "2", code: "P-0002", name: "Gearbox" }),
        ]}
        toggleActive={noop}
      />,
    );

    expect(screen.getByText("P-0001")).toBeInTheDocument();
    expect(screen.getByText("Gearbox")).toBeInTheDocument();
  });

  it("Offers_Deactivate_for_an_active_product_and_Activate_for_an_inactive_one", () => {
    render(
      <ProductTable
        products={[
          aProduct({ id: "1", isActive: true }),
          aProduct({ id: "2", isActive: false }),
        ]}
        toggleActive={noop}
      />,
    );

    expect(
      screen.getByRole("button", { name: "Deactivate" }),
    ).toBeInTheDocument();
    expect(screen.getByRole("button", { name: "Activate" })).toBeInTheDocument();
  });
});
