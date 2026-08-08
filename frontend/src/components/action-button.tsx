"use client";

import Box from "@mui/material/Box";
import Button from "@mui/material/Button";
import Typography from "@mui/material/Typography";
import { useActionState } from "react";

export interface ActionState {
  error: string | null;
}

export const idle: ActionState = { error: null };

export type Action = (
  previous: ActionState,
  formData: FormData,
) => Promise<ActionState>;

/**
 * One transition = one tiny form. The id travels in hidden inputs so every action keeps the same
 * `(previous, formData)` signature, and the ProblemDetails detail is rendered where it happened.
 */
export function ActionButton({
  action,
  label,
  fields = {},
  disabled = false,
  title,
  tone = "neutral",
}: {
  action: Action;
  label: string;
  fields?: Record<string, string>;
  disabled?: boolean;
  title?: string;
  tone?: "neutral" | "primary" | "danger";
}) {
  const [state, formAction, pending] = useActionState(action, idle);

  return (
    <Box component="form" action={formAction} sx={{ display: "inline-block" }}>
      {Object.entries(fields).map(([name, value]) => (
        <input key={name} type="hidden" name={name} value={value} />
      ))}
      <Button
        type="submit"
        size="small"
        title={title}
        loading={pending}
        disabled={disabled}
        variant={tone === "primary" ? "contained" : "outlined"}
        color={tone === "danger" ? "error" : "primary"}
      >
        {label}
      </Button>
      {state.error ? (
        <Typography
          role="alert"
          variant="caption"
          color="error"
          sx={{ display: "block", maxWidth: 280 }}
        >
          {state.error}
        </Typography>
      ) : null}
    </Box>
  );
}
