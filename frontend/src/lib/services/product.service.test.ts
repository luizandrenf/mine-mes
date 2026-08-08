import { describe, expect, it } from "vitest";
import { ProductService } from "@/lib/services/product.service";
import { aProduct } from "@/lib/test/factories";
import { FakeHttpClient } from "@/lib/test/fake-http-client";

function service() {
  const http = new FakeHttpClient();
  return { http, service: new ProductService(http) };
}

describe("ProductService", () => {
  it("GetAll_reads_the_collection_route", async () => {
    const { http, service: subject } = service();
    http.returns([aProduct()]);

    const products = await subject.getAll();

    expect(http.lastCall).toEqual({ method: "GET", path: "/api/products" });
    expect(products).toHaveLength(1);
  });

  it("GetById_reads_the_item_route", async () => {
    const { http, service: subject } = service();
    http.returns(aProduct({ id: "abc" }));

    const product = await subject.getById("abc");

    expect(http.lastCall.path).toBe("/api/products/abc");
    expect(product.id).toBe("abc");
  });

  it("Create_posts_the_request_and_returns_the_normalized_product", async () => {
    const { http, service: subject } = service();
    // The API upper-cases the code, so the echo does not match the input.
    http.returns(aProduct({ code: "P-0001" }));

    const product = await subject.create({ code: " p-0001 ", name: "Motor" });

    expect(http.lastCall).toEqual({
      method: "POST",
      path: "/api/products",
      body: { code: " p-0001 ", name: "Motor" },
    });
    expect(product.code).toBe("P-0001");
  });

  it.each([
    [true, "/api/products/abc/activate"],
    [false, "/api/products/abc/deactivate"],
  ])("SetActive_%s_patches_%s", async (active, path) => {
    const { http, service: subject } = service();

    await subject.setActive("abc", active);

    expect(http.lastCall).toEqual({ method: "PATCH", path });
  });
});
