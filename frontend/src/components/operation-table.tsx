import Stack from "@mui/material/Stack";
import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import { type Action, ActionButton } from "@/components/action-button";
import { StatusBadge } from "@/components/status-badge";
import type { ProductionOrder } from "@/lib/api/types";
import {
  canCancelOperation,
  canCompleteOperation,
  canStartOperation,
} from "@/lib/domain/transitions";
import { formatQuantity } from "@/lib/format";

export function OperationTable({
  order,
  transition,
}: {
  order: ProductionOrder;
  transition: Action;
}) {
  if (order.operations.length === 0) {
    return (
      <Typography color="text.secondary" variant="body2">
        No operations yet. An order needs at least one before it can be released.
      </Typography>
    );
  }

  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell>Seq</TableCell>
          <TableCell>Code</TableCell>
          <TableCell>Description</TableCell>
          <TableCell>Quantity</TableCell>
          <TableCell>Cycle time</TableCell>
          <TableCell>Status</TableCell>
          <TableCell align="right">Actions</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {order.operations.map((operation) => (
          <TableRow key={operation.id}>
            <TableCell sx={{ fontFamily: "monospace" }}>
              {operation.sequence}
            </TableCell>
            <TableCell sx={{ fontFamily: "monospace" }}>
              {operation.code}
            </TableCell>
            <TableCell>{operation.description}</TableCell>
            <TableCell>{formatQuantity(operation.plannedQuantity)}</TableCell>
            <TableCell>
              {operation.targetCycleTimeSeconds === null
                ? "—"
                : `${operation.targetCycleTimeSeconds}s`}
            </TableCell>
            <TableCell>
              <StatusBadge status={operation.status} />
            </TableCell>
            <TableCell align="right">
              <Stack direction="row" spacing={1} sx={{ justifyContent: "flex-end" }}>
                <ActionButton
                  action={transition}
                  label="Start"
                  disabled={!canStartOperation(order, operation)}
                  fields={{
                    orderId: order.id,
                    operationId: operation.id,
                    transition: "start",
                  }}
                />
                <ActionButton
                  action={transition}
                  label="Complete"
                  disabled={!canCompleteOperation(operation)}
                  fields={{
                    orderId: order.id,
                    operationId: operation.id,
                    transition: "complete",
                  }}
                />
                <ActionButton
                  action={transition}
                  label="Cancel"
                  tone="danger"
                  disabled={!canCancelOperation(operation)}
                  fields={{
                    orderId: order.id,
                    operationId: operation.id,
                    transition: "cancel",
                  }}
                />
              </Stack>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
