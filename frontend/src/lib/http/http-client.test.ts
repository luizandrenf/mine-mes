import { afterEach, describe, expect, it } from "vitest";
import { ApiError, FetchHttpClient } from "@/lib/http/http-client";

interface RecordedRequest {
  url: string;
  init: RequestInit;
}

const realFetch = globalThis.fetch;

/** Hand-written stand-in for `fetch`; no mock library involved. */
function fakeFetch(response: Response) {
  const requests: RecordedRequest[] = [];

  globalThis.fetch = ((url: string, init: RequestInit) => {
    requests.push({ url, init });
    return Promise.resolve(response);
  }) as typeof fetch;

  return requests;
}

function problem(status: number, body: unknown) {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/problem+json" },
  });
}

afterEach(() => {
  globalThis.fetch = realFetch;
});

describe("FetchHttpClient", () => {
  const http = new FetchHttpClient("http://api:8080");

  it("Get_prefixes_the_base_url_and_never_caches", async () => {
    const requests = fakeFetch(Response.json([{ id: "1" }]));

    await http.get("/api/products");

    expect(requests[0].url).toBe("http://api:8080/api/products");
    expect(requests[0].init.method).toBe("GET");
    expect(requests[0].init.cache).toBe("no-store");
    expect(requests[0].init.body).toBeUndefined();
  });

  it("Post_serializes_the_body_as_json", async () => {
    const requests = fakeFetch(Response.json({ id: "1" }));

    await http.post("/api/products", { code: "P-1" });

    expect(requests[0].init.body).toBe('{"code":"P-1"}');
    expect(requests[0].init.headers).toEqual({
      "Content-Type": "application/json",
    });
  });

  it("Transition_without_a_body_sends_no_content_type", async () => {
    const requests = fakeFetch(new Response(null, { status: 204 }));

    await http.postNoContent("/api/production-orders/1/release");

    expect(requests[0].init.headers).toEqual({});
  });

  it("NoContent_response_is_not_parsed_as_json", async () => {
    fakeFetch(new Response(null, { status: 204 }));

    await expect(
      http.patchNoContent("/api/products/1/activate"),
    ).resolves.toBeUndefined();
  });

  it.each([404, 409, 422])(
    "Status_%i_throws_an_ApiError_carrying_the_problem_detail",
    async (status) => {
      fakeFetch(
        problem(status, {
          status,
          title: "Business rule violated",
          detail: "Only a draft order can be released. Current status: Released.",
        }),
      );

      const error = await http
        .postNoContent("/api/production-orders/1/release")
        .catch((thrown: unknown) => thrown);

      expect(error).toBeInstanceOf(ApiError);
      expect((error as ApiError).status).toBe(status);
      expect((error as ApiError).message).toBe(
        "Only a draft order can be released. Current status: Released.",
      );
    },
  );

  it("Validation_errors_are_flattened_into_one_line", async () => {
    fakeFetch(
      problem(400, {
        status: 400,
        title: "One or more validation errors occurred.",
        errors: {
          Code: ["The Code field is required."],
          Name: ["The Name field is required."],
        },
      }),
    );

    const error = await http
      .post("/api/products", {})
      .catch((thrown: unknown) => thrown);

    expect((error as ApiError).message).toBe(
      "Code: The Code field is required. · Name: The Name field is required.",
    );
  });

  it("Unparseable_error_body_falls_back_to_the_status", async () => {
    fakeFetch(new Response("<html>502</html>", { status: 502 }));

    const error = await http
      .get("/api/products")
      .catch((thrown: unknown) => thrown);

    expect((error as ApiError).status).toBe(502);
    expect((error as ApiError).message).toContain("502");
  });
});
