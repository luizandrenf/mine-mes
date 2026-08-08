import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { StatusBadge } from "@/components/status-badge";
import type { ProductionOrderStatus } from "@/lib/api/types";

describe("StatusBadge", () => {
  it.each<ProductionOrderStatus>([
    "Draft",
    "Released",
    "Completed",
    "Cancelled",
  ])("Renders_%s_verbatim", (status) => {
    render(<StatusBadge status={status} />);

    expect(screen.getByText(status)).toBeInTheDocument();
  });

  it("Renders_InProgress_as_two_words", () => {
    render(<StatusBadge status="InProgress" />);

    expect(screen.getByText("In progress")).toBeInTheDocument();
  });
});
