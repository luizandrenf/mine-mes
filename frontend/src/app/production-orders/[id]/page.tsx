import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { notFound } from "next/navigation";
import { OperationForm } from "@/app/production-orders/[id]/operation-form";
import {
  transitionOperation,
  transitionOrder,
} from "@/app/production-orders/actions";
import { ActionButton } from "@/components/action-button";
import { AppLink } from "@/components/nav";
import { OperationTable } from "@/components/operation-table";
import { StatusBadge } from "@/components/status-badge";
import type { ProductionOrder } from "@/lib/api/types";
import {
  canAddOperation,
  canCancelOrder,
  canCompleteOrder,
  canReleaseOrder,
  canStartOrder,
} from "@/lib/domain/transitions";
import { formatDateTime, formatQuantity } from "@/lib/format";
import { ApiError } from "@/lib/http/http-client";
import { productionOrderService, productService } from "@/lib/services";

export default async function ProductionOrderPage({
  params,
}: PageProps<"/production-orders/[id]">) {
  const { id } = await params;

  const order = await productionOrderService.getById(id).catch((error) => {
    if (error instanceof ApiError && error.status === 404) {
      notFound();
    }
    throw error;
  });

  const product = await productService
    .getById(order.productId)
    .catch(() => null);

  return (
    <Stack spacing={3}>
      <Stack
        direction="row"
        sx={{
          flexWrap: "wrap",
          justifyContent: "space-between",
          alignItems: "center",
          gap: 2,
        }}
      >
        <div>
          <AppLink
            href="/production-orders"
            variant="caption"
            color="text.secondary"
          >
            ← All orders
          </AppLink>
          <Typography
            variant="h5"
            component="h1"
            sx={{ display: "flex", alignItems: "center", gap: 1.5 }}
          >
            <span style={{ fontFamily: "monospace" }}>{order.orderNumber}</span>
            <StatusBadge status={order.status} />
          </Typography>
          <Typography variant="body2" color="text.secondary">
            {product ? `${product.code} — ${product.name}` : "Unknown product"} ·{" "}
            {formatQuantity(order.plannedQuantity)} units · priority{" "}
            {order.priority}
          </Typography>
        </div>

        <Stack direction="row" sx={{ flexWrap: "wrap", gap: 1 }}>
          <ActionButton
            action={transitionOrder}
            label="Release"
            tone="primary"
            disabled={!canReleaseOrder(order)}
            title={releaseHint(order)}
            fields={{ orderId: order.id, transition: "release" }}
          />
          <ActionButton
            action={transitionOrder}
            label="Start"
            tone="primary"
            disabled={!canStartOrder(order)}
            fields={{ orderId: order.id, transition: "start" }}
          />
          <ActionButton
            action={transitionOrder}
            label="Complete"
            tone="primary"
            disabled={!canCompleteOrder(order)}
            title={completeHint(order)}
            fields={{ orderId: order.id, transition: "complete" }}
          />
          <ActionButton
            action={transitionOrder}
            label="Cancel"
            tone="danger"
            disabled={!canCancelOrder(order)}
            fields={{ orderId: order.id, transition: "cancel" }}
          />
        </Stack>
      </Stack>

      <Paper
        variant="outlined"
        component="dl"
        sx={{
          p: 2,
          m: 0,
          display: "grid",
          gap: 2,
          gridTemplateColumns: { xs: "repeat(2, 1fr)", sm: "repeat(4, 1fr)" },
        }}
      >
        <Fact label="Created" value={formatDateTime(order.createdAt)} />
        <Fact label="Released" value={formatDateTime(order.releasedAt)} />
        <Fact label="Planned start" value={formatDateTime(order.plannedStartAt)} />
        <Fact label="Planned end" value={formatDateTime(order.plannedEndAt)} />
      </Paper>

      <Stack spacing={1.5}>
        <Typography variant="overline" color="text.secondary" component="h2">
          Operations
        </Typography>

        {canAddOperation(order) ? (
          <OperationForm
            orderId={order.id}
            nextSequence={nextSequence(order)}
            defaultQuantity={order.plannedQuantity}
          />
        ) : null}

        <Paper variant="outlined" sx={{ p: 2 }}>
          <OperationTable order={order} transition={transitionOperation} />
        </Paper>
      </Stack>
    </Stack>
  );
}

function Fact({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <Typography component="dt" variant="caption" color="text.secondary">
        {label}
      </Typography>
      <Typography component="dd" variant="body2" sx={{ m: 0 }}>
        {value}
      </Typography>
    </div>
  );
}

function nextSequence(order: ProductionOrder): number {
  return Math.max(0, ...order.operations.map((o) => o.sequence)) + 10;
}

function releaseHint(order: ProductionOrder): string | undefined {
  if (order.status === "Draft" && order.operations.length === 0) {
    return "Add at least one operation first.";
  }
  return undefined;
}

function completeHint(order: ProductionOrder): string | undefined {
  if (order.status === "InProgress" && !canCompleteOrder(order)) {
    return "Every operation must be completed or cancelled first.";
  }
  return undefined;
}
