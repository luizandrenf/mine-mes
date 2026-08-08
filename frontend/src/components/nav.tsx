"use client";

import Button from "@mui/material/Button";
import MuiLink from "@mui/material/Link";
import type { LinkProps } from "@mui/material/Link";
import NextLink from "next/link";

/**
 * MUI takes the element to render as a `component` prop, and a Server Component cannot hand a
 * component reference across the boundary. These wrappers sit on the client side of it, so pages
 * keep `next/link` routing without turning themselves into Client Components.
 */

export function AppLink({
  href,
  children,
  ...props
}: { href: string } & Omit<LinkProps, "href" | "component">) {
  return (
    <MuiLink component={NextLink} href={href} {...props}>
      {children}
    </MuiLink>
  );
}

export function NavButton({
  href,
  children,
}: {
  href: string;
  children: React.ReactNode;
}) {
  return (
    <Button component={NextLink} href={href} color="inherit">
      {children}
    </Button>
  );
}
