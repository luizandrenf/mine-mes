import { FetchHttpClient } from "@/lib/http/http-client";
import { ProductService } from "@/lib/services/product.service";
import { ProductionOrderService } from "@/lib/services/production-order.service";

// The Angular `providedIn: 'root'` equivalent: one instance per process, wired once. Server-side
// only — the base URL never reaches the browser, so the API needs no CORS policy.
const http = new FetchHttpClient(
  process.env.API_BASE_URL ?? "http://localhost:5033",
);

export const productService = new ProductService(http);
export const productionOrderService = new ProductionOrderService(http);
