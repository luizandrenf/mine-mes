"use server";

import { revalidatePath } from "next/cache";
import type { ActionState } from "@/components/action-button";
import { requiredText, runAction } from "@/lib/actions";
import { productService } from "@/lib/services";

export async function createProduct(
  _previous: ActionState,
  formData: FormData,
): Promise<ActionState> {
  return runAction(async () => {
    await productService.create({
      code: requiredText(formData, "code"),
      name: requiredText(formData, "name"),
    });

    revalidatePath("/products");
  });
}

export async function toggleProductActive(
  _previous: ActionState,
  formData: FormData,
): Promise<ActionState> {
  return runAction(async () => {
    await productService.setActive(
      requiredText(formData, "productId"),
      requiredText(formData, "active") === "true",
    );

    revalidatePath("/products");
  });
}
