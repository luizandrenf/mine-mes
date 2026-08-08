import type { HttpClient } from "@/lib/http/http-client";

export interface RecordedCall {
  method: string;
  path: string;
  body?: unknown;
}

/**
 * Hand-written fake, the counterpart of FakeProductionOrderRepository on the backend: it records
 * what the service asked for and replays whatever the test queued.
 */
export class FakeHttpClient implements HttpClient {
  readonly calls: RecordedCall[] = [];

  private responses: unknown[] = [];

  /** Queues the payloads returned by the next `get`/`post` calls, in order. */
  returns(...responses: unknown[]): this {
    this.responses.push(...responses);
    return this;
  }

  get<T>(path: string): Promise<T> {
    this.calls.push({ method: "GET", path });
    return Promise.resolve(this.next<T>());
  }

  post<T>(path: string, body?: unknown): Promise<T> {
    this.calls.push({ method: "POST", path, body });
    return Promise.resolve(this.next<T>());
  }

  postNoContent(path: string, body?: unknown): Promise<void> {
    this.calls.push({ method: "POST", path, body });
    return Promise.resolve();
  }

  patchNoContent(path: string): Promise<void> {
    this.calls.push({ method: "PATCH", path });
    return Promise.resolve();
  }

  get lastCall(): RecordedCall {
    const call = this.calls.at(-1);

    if (!call) {
      throw new Error("No call was recorded.");
    }

    return call;
  }

  private next<T>(): T {
    if (this.responses.length === 0) {
      throw new Error("FakeHttpClient has no queued response.");
    }

    return this.responses.shift() as T;
  }
}
