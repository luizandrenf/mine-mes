"use client";

import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import { useActionState } from "react";
import { addOperation } from "@/app/production-orders/actions";
import { idle } from "@/components/action-button";

export function OperationForm({
  orderId,
  nextSequence,
  defaultQuantity,
}: {
  orderId: string;
  nextSequence: number;
  defaultQuantity: number;
}) {
  const [state, formAction, pending] = useActionState(addOperation, idle);

  return (
    <Paper component="form" action={formAction} variant="outlined" sx={{ p: 2 }}>
      <input type="hidden" name="orderId" value={orderId} />
      <Stack direction="row" sx={{ flexWrap: "wrap", alignItems: "flex-start", gap: 2 }}>
        <TextField
          name="sequence"
          label="Sequence"
          size="small"
          type="number"
          required
          defaultValue={nextSequence}
          sx={{ width: 110 }}
          slotProps={{ htmlInput: { min: 1 } }}
        />
        <TextField
          name="code"
          label="Code"
          size="small"
          required
          placeholder="CUT"
          sx={{ width: 130 }}
          slotProps={{ htmlInput: { maxLength: 50 } }}
        />
        <TextField
          name="description"
          label="Description"
          size="small"
          required
          placeholder="Cut the raw bar"
          sx={{ width: 260 }}
          slotProps={{ htmlInput: { maxLength: 200 } }}
        />
        {/* Owned by the Equipment service; Production stores it without validating. */}
        <TextField
          name="workCenterId"
          label="Work center"
          size="small"
          required
          placeholder="uuid"
          sx={{ width: 300 }}
        />
        <TextField
          name="plannedQuantity"
          label="Quantity"
          size="small"
          type="number"
          required
          defaultValue={defaultQuantity}
          sx={{ width: 120 }}
          slotProps={{ htmlInput: { min: 0.001, step: "any" } }}
        />
        <TextField
          name="targetCycleTimeSeconds"
          label="Cycle time (s)"
          size="small"
          type="number"
          sx={{ width: 130 }}
          slotProps={{ htmlInput: { min: 1 } }}
        />
        <Button type="submit" variant="contained" loading={pending}>
          Add operation
        </Button>
      </Stack>
      {state.error ? (
        <Alert role="alert" severity="error" sx={{ mt: 2 }}>
          {state.error}
        </Alert>
      ) : null}
    </Paper>
  );
}
