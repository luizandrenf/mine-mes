import Table from "@mui/material/Table";
import TableBody from "@mui/material/TableBody";
import TableCell from "@mui/material/TableCell";
import TableHead from "@mui/material/TableHead";
import TableRow from "@mui/material/TableRow";
import Typography from "@mui/material/Typography";
import { type Action, ActionButton } from "@/components/action-button";
import type { Product } from "@/lib/api/types";
import { formatDateTime } from "@/lib/format";

export function ProductTable({
  products,
  toggleActive,
}: {
  products: Product[];
  toggleActive: Action;
}) {
  if (products.length === 0) {
    return (
      <Typography color="text.secondary" variant="body2">
        No products yet.
      </Typography>
    );
  }

  return (
    <Table size="small">
      <TableHead>
        <TableRow>
          <TableCell>Code</TableCell>
          <TableCell>Name</TableCell>
          <TableCell>Status</TableCell>
          <TableCell>Created</TableCell>
          <TableCell />
        </TableRow>
      </TableHead>
      <TableBody>
        {products.map((product) => (
          <TableRow key={product.id} hover>
            <TableCell sx={{ fontFamily: "monospace" }}>
              {product.code}
            </TableCell>
            <TableCell>{product.name}</TableCell>
            <TableCell>{product.isActive ? "Active" : "Inactive"}</TableCell>
            <TableCell>
              <Typography variant="body2" color="text.secondary">
                {formatDateTime(product.createdAt)}
              </Typography>
            </TableCell>
            <TableCell align="right">
              <ActionButton
                action={toggleActive}
                label={product.isActive ? "Deactivate" : "Activate"}
                tone={product.isActive ? "danger" : "neutral"}
                fields={{
                  productId: product.id,
                  active: String(!product.isActive),
                }}
              />
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
