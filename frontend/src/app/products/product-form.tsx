"use client";

import Alert from "@mui/material/Alert";
import Button from "@mui/material/Button";
import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import TextField from "@mui/material/TextField";
import { useActionState } from "react";
import { createProduct } from "@/app/products/actions";
import { idle } from "@/components/action-button";

export function ProductForm() {
  const [state, formAction, pending] = useActionState(createProduct, idle);

  return (
    <Paper
      component="form"
      action={formAction}
      variant="outlined"
      sx={{ p: 2 }}
    >
      <Stack direction="row" sx={{ flexWrap: "wrap", alignItems: "flex-start", gap: 2 }}>
        <TextField
          name="code"
          label="Code"
          size="small"
          required
          placeholder="P-0001"
          slotProps={{ htmlInput: { maxLength: 50 } }}
        />
        <TextField
          name="name"
          label="Name"
          size="small"
          required
          placeholder="Motor"
          sx={{ width: 260 }}
          slotProps={{ htmlInput: { maxLength: 200 } }}
        />
        <Button type="submit" variant="contained" loading={pending}>
          Create product
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
