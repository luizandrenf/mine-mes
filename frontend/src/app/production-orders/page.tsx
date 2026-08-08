import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { OrderForm } from "@/app/production-orders/order-form";
import { OrderTable } from "@/components/order-table";
import { productionOrderService, productService } from "@/lib/services";

export default async function ProductionOrdersPage() {
  const [orders, products] = await Promise.all([
    productionOrderService.getAll(),
    productService.getAll(),
  ]);

  return (
    <Stack spacing={3}>
      <div>
        <Typography variant="h5" component="h1">
          Production orders
        </Typography>
        <Typography variant="body2" color="text.secondary">
          An order is born as a draft and needs at least one operation before it
          can be released.
        </Typography>
      </div>

      <OrderForm activeProducts={products.filter((product) => product.isActive)} />

      <Paper variant="outlined" sx={{ p: 2 }}>
        <OrderTable
          orders={orders}
          productsById={new Map(products.map((product) => [product.id, product]))}
        />
      </Paper>
    </Stack>
  );
}
