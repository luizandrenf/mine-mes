import { AppRouterCacheProvider } from "@mui/material-nextjs/v16-appRouter";
import AppBar from "@mui/material/AppBar";
import Container from "@mui/material/Container";
import CssBaseline from "@mui/material/CssBaseline";
import Toolbar from "@mui/material/Toolbar";
import Typography from "@mui/material/Typography";
import type { Metadata } from "next";
import { NavButton } from "@/components/nav";

export const metadata: Metadata = {
  title: "MiniMES",
  description: "Shop floor control for the MiniMES Production service",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="en">
      <body>
        {/* Collects the emotion CSS on the server so it lands in <head>, not mid-body. */}
        <AppRouterCacheProvider>
          <CssBaseline />
          <AppBar position="static" color="default" elevation={1}>
            <Toolbar variant="dense" sx={{ gap: 2 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                MiniMES
              </Typography>
              <NavButton href="/production-orders">Production orders</NavButton>
              <NavButton href="/products">Products</NavButton>
            </Toolbar>
          </AppBar>
          <Container component="main" maxWidth="lg" sx={{ py: 4 }}>
            {children}
          </Container>
        </AppRouterCacheProvider>
      </body>
    </html>
  );
}
