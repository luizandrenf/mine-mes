import type { ActionState } from "@/components/action-button";
import { ApiError } from "@/lib/http/http-client";

/**
 * Turns an ApiError into the state the form renders. Only the API's own refusals (404/409/422 and
 * validation) are shown; anything else is a real fault and must keep bubbling to the error boundary.
 */
export async function runAction(
  work: () => Promise<void>,
): Promise<ActionState> {
  try {
    await work();
    return { error: null };
  } catch (error) {
    if (error instanceof ApiError && error.status < 500) {
      return { error: error.message };
    }

    throw error;
  }
}

export function requiredText(formData: FormData, field: string): string {
  const value = formData.get(field);

  if (typeof value !== "string" || value.trim() === "") {
    throw new Error(`Missing form field '${field}'.`);
  }

  return value;
}

export function optionalNumber(
  formData: FormData,
  field: string,
): number | null {
  const value = formData.get(field);
  return typeof value === "string" && value.trim() !== "" ? Number(value) : null;
}

export function optionalText(formData: FormData, field: string): string | null {
  const value = formData.get(field);
  return typeof value === "string" && value.trim() !== "" ? value : null;
}
