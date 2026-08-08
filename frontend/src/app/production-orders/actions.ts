"use server";

import { revalidatePath } from "next/cache";
import type { ActionState } from "@/components/action-button";
import {
  optionalNumber,
  optionalText,
  requiredText,
  runAction,
} from "@/lib/actions";
import { productionOrderService } from "@/lib/services";
import type {
  OperationTransition,
  OrderTransition,
} from "@/lib/services/production-order.service";

export async function createOrder(
  _previous: ActionState,
  formData: FormData,
): Promise<ActionState> {
  return runAction(async () => {
    await productionOrderService.create({
      orderNumber: requiredText(formData, "orderNumber"),
      productId: requiredText(formData, "productId"),
      plannedQuantity: Number(requiredText(formData, "plannedQuantity")),
      priority: optionalNumber(formData, "priority") ?? 0,
      plannedStartAt: toUtc(optionalText(formData, "plannedStartAt")),
      plannedEndAt: toUtc(optionalText(formData, "plannedEndAt")),
    });

    revalidatePath("/production-orders");
  });
}

export async function transitionOrder(
  _previous: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const orderId = requiredText(formData, "orderId");

  return runAction(async () => {
    await productionOrderService.transition(
      orderId,
      requiredText(formData, "transition") as OrderTransition,
    );

    revalidatePath("/production-orders");
    revalidatePath(`/production-orders/${orderId}`);
  });
}

export async function addOperation(
  _previous: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const orderId = requiredText(formData, "orderId");

  return runAction(async () => {
    await productionOrderService.addOperation(orderId, {
      sequence: Number(requiredText(formData, "sequence")),
      code: requiredText(formData, "code"),
      description: requiredText(formData, "description"),
      workCenterId: requiredText(formData, "workCenterId"),
      plannedQuantity: Number(requiredText(formData, "plannedQuantity")),
      targetCycleTimeSeconds: optionalNumber(
        formData,
        "targetCycleTimeSeconds",
      ),
    });

    revalidatePath(`/production-orders/${orderId}`);
  });
}

export async function transitionOperation(
  _previous: ActionState,
  formData: FormData,
): Promise<ActionState> {
  const orderId = requiredText(formData, "orderId");

  return runAction(async () => {
    await productionOrderService.transitionOperation(
      orderId,
      requiredText(formData, "operationId"),
      requiredText(formData, "transition") as OperationTransition,
    );

    revalidatePath(`/production-orders/${orderId}`);
  });
}

/** `datetime-local` has no zone; the API documents these fields as UTC. */
function toUtc(value: string | null): string | null {
  return value === null ? null : `${value}:00Z`;
}
