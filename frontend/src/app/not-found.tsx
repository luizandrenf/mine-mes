import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { AppLink } from "@/components/nav";

export default function NotFound() {
  return (
    <Stack spacing={1} sx={{ alignItems: "flex-start" }}>
      <Typography variant="h5" component="h1">
        Not found
      </Typography>
      <Typography variant="body2" color="text.secondary">
        This page does not exist, or the production order was removed.
      </Typography>
      <AppLink href="/production-orders">Back to production orders</AppLink>
    </Stack>
  );
}
