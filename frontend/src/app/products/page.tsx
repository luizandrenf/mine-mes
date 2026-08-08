import Paper from "@mui/material/Paper";
import Stack from "@mui/material/Stack";
import Typography from "@mui/material/Typography";
import { toggleProductActive } from "@/app/products/actions";
import { ProductForm } from "@/app/products/product-form";
import { ProductTable } from "@/components/product-table";
import { productService } from "@/lib/services";

export default async function ProductsPage() {
  const products = await productService.getAll();

  return (
    <Stack spacing={3}>
      <div>
        <Typography variant="h5" component="h1">
          Products
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Only an active product can be used in a new production order.
        </Typography>
      </div>

      <ProductForm />

      <Paper variant="outlined" sx={{ p: 2 }}>
        <ProductTable products={products} toggleActive={toggleProductActive} />
      </Paper>
    </Stack>
  );
}
