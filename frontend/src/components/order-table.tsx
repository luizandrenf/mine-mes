import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import { AppLink } from "@/components/nav";
import { StatusBadge } from "@/components/status-badge";
import type { Product, ProductionOrder } from "@/lib/api/types";
import { formatDateTime, formatQuantity } from "@/lib/format";

export function OrderTable({
  orders,
  productsById,
}: {
  orders: ProductionOrder[];
  /** The API carries only productId — the join happens here. */
  productsById: Map<string, Product>;
}) {
  if (orders.length === 0) {
    return (
      <Typography color="text.secondary" variant="body2">
        No production orders yet.
      </Typography>
    );
  }

  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell>Order</TableCell>
          <TableCell>Product</TableCell>
          <TableCell>Quantity</TableCell>
          <TableCell>Status</TableCell>
          <TableCell>Priority</TableCell>
          <TableCell>Operations</TableCell>
          <TableCell>Created</TableCell>
        </TableRow>
      </TableHead>
      <TableBody>
        {orders.map((order) => (
          <TableRow key={order.id} hover>
            <TableCell>
              <AppLink
                href={`/production-orders/${order.id}`}
                sx={{ fontFamily: "monospace" }}
              >
                {order.orderNumber}
              </AppLink>
            </TableCell>
            <TableCell>
              {productsById.get(order.productId)?.code ?? "unknown"}
            </TableCell>
            <TableCell>{formatQuantity(order.plannedQuantity)}</TableCell>
            <TableCell>
              <StatusBadge status={order.status} />
            </TableCell>
            <TableCell>{order.priority}</TableCell>
            <TableCell>{order.operations.length}</TableCell>
            <TableCell>
              <Typography variant="body2" color="text.secondary">
                {formatDateTime(order.createdAt)}
              </Typography>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
