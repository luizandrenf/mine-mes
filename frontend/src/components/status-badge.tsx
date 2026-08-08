import Chip from "@mui/material/Chip";
import type {
  ProductionOperationStatus,
  ProductionOrderStatus,
} from "@/lib/api/types";

type Status = ProductionOrderStatus | ProductionOperationStatus;

const colors: Record<Status, "default" | "info" | "warning" | "success" | "error"> = {
  Draft: "default",
  Pending: "default",
  Released: "info",
  InProgress: "warning",
  Completed: "success",
  Cancelled: "error",
};

export function StatusBadge({ status }: { status: Status }) {
  return (
    <Chip
      size="small"
      variant="outlined"
      color={colors[status]}
      label={status === "InProgress" ? "In progress" : status}
    />
  );
}
