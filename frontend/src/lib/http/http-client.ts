import type { ProblemDetails } from "@/lib/api/types";

/**
 * The transport the services depend on — the frontend counterpart of a repository. Services take
 * it by constructor, so a test swaps in a fake without touching `fetch`.
 */
export interface HttpClient {
  get<T>(path: string): Promise<T>;
  post<T>(path: string, body?: unknown): Promise<T>;
  postNoContent(path: string, body?: unknown): Promise<void>;
  patchNoContent(path: string): Promise<void>;
}

export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly problem: ProblemDetails,
    message: string,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

/** Turns a ProblemDetails into one presentable line. The API already speaks English. */
function messageFrom(status: number, problem: ProblemDetails): string {
  if (problem.errors) {
    const fields = Object.entries(problem.errors)
      .map(([field, messages]) => `${field}: ${messages.join(" ")}`)
      .join(" · ");

    if (fields) {
      return fields;
    }
  }

  // `||`, not `??`: an empty detail or title is as useless as a missing one.
  return problem.detail || problem.title || `Request failed with status ${status}.`;
}

export class FetchHttpClient implements HttpClient {
  constructor(private readonly baseUrl: string) {}

  get<T>(path: string): Promise<T> {
    return this.send<T>("GET", path);
  }

  post<T>(path: string, body?: unknown): Promise<T> {
    return this.send<T>("POST", path, body);
  }

  async postNoContent(path: string, body?: unknown): Promise<void> {
    await this.send<void>("POST", path, body);
  }

  async patchNoContent(path: string): Promise<void> {
    await this.send<void>("PATCH", path);
  }

  private async send<T>(
    method: string,
    path: string,
    body?: unknown,
  ): Promise<T> {
    const response = await fetch(`${this.baseUrl}${path}`, {
      method,
      headers: body === undefined ? {} : { "Content-Type": "application/json" },
      body: body === undefined ? undefined : JSON.stringify(body),
      // Every read must reflect the last mutation; nothing here is worth caching.
      cache: "no-store",
    });

    if (!response.ok) {
      const problem = await readProblem(response);
      throw new ApiError(
        response.status,
        problem,
        messageFrom(response.status, problem),
      );
    }

    if (response.status === 204) {
      return undefined as T;
    }

    return (await response.json()) as T;
  }
}

async function readProblem(response: Response): Promise<ProblemDetails> {
  try {
    return (await response.json()) as ProblemDetails;
  } catch {
    // A 500 behind a proxy, or an empty body — there is nothing to parse.
    return { status: response.status, title: response.statusText };
  }
}
