"use client";

import Alert from "@mui/material/Alert";
import AlertTitle from "@mui/material/AlertTitle";
import Button from "@mui/material/Button";
import Stack from "@mui/material/Stack";

/**
 * Catches what `runAction` deliberately rethrows: anything the API answers with 5xx, plus errors
 * of a client-side navigation. A *first* page load with the API down never reaches here — Next
 * answers those with its own 500 document. The message is never shown: a stack trace is not an
 * answer for whoever is on the shop floor.
 */
export default function Error({ retry }: { retry: () => void }) {
  return (
    <Stack spacing={2} sx={{ alignItems: "flex-start" }}>
      <Alert severity="error" sx={{ width: "100%" }}>
        <AlertTitle>Something broke</AlertTitle>
        The Production API is unreachable or answered with an error. Check that
        it is running, then try again.
      </Alert>
      <Button variant="contained" onClick={retry}>
        Try again
      </Button>
    </Stack>
  );
}
