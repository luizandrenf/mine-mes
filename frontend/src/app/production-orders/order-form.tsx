"use client";

import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import MenuItem from "@mui/material/MenuItem";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import { useActionState } from "react";
import { createOrder } from "@/app/production-orders/actions";
import { idle } from "@/components/action-button";
import type { Product } from "@/lib/api/types";

export function OrderForm({ activeProducts }: { activeProducts: Product[] }) {
  const [state, formAction, pending] = useActionState(createOrder, idle);
  const noProducts = activeProducts.length === 0;

  return (
    <Paper component="form" action={formAction} variant="outlined" sx={{ p: 2 }}>
      <Stack direction="row" sx={{ flexWrap: "wrap", alignItems: "flex-start", gap: 2 }}>
        <TextField
          name="orderNumber"
          label="Order number"
          size="small"
          required
          placeholder="OP-0001"
          disabled={noProducts}
          slotProps={{ htmlInput: { minLength: 2, maxLength: 50 } }}
        />
        <TextField
          name="productId"
          label="Product"
          size="small"
          select
          required
          disabled={noProducts}
          defaultValue={activeProducts[0]?.id ?? ""}
          sx={{ width: 260 }}
        >
          {activeProducts.map((product) => (
            <MenuItem key={product.id} value={product.id}>
              {product.code} — {product.name}
            </MenuItem>
          ))}
        </TextField>
        <TextField
          name="plannedQuantity"
          label="Quantity"
          size="small"
          type="number"
          required
          defaultValue={100}
          disabled={noProducts}
          sx={{ width: 120 }}
          slotProps={{ htmlInput: { min: 0.001, step: "any" } }}
        />
        <TextField
          name="priority"
          label="Priority"
          size="small"
          type="number"
          defaultValue={0}
          disabled={noProducts}
          sx={{ width: 110 }}
          slotProps={{ htmlInput: { min: 0 } }}
        />
        <TextField
          name="plannedStartAt"
          label="Planned start (UTC)"
          size="small"
          type="datetime-local"
          disabled={noProducts}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <TextField
          name="plannedEndAt"
          label="Planned end (UTC)"
          size="small"
          type="datetime-local"
          disabled={noProducts}
          slotProps={{ inputLabel: { shrink: true } }}
        />
        <Button
          type="submit"
          variant="contained"
          loading={pending}
          disabled={noProducts}
        >
          Create order
        </Button>
      </Stack>
      {noProducts ? (
        <Alert severity="info" sx={{ mt: 2 }}>
          Create and activate a product first.
        </Alert>
      ) : null}
      {state.error ? (
        <Alert role="alert" severity="error" sx={{ mt: 2 }}>
          {state.error}
        </Alert>
      ) : null}
    </Paper>
  );
}
