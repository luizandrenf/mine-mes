import { render, screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { idle } from "@/components/action-button";
import { OperationTable } from "@/components/operation-table";
import { anOperation, anOrder } from "@/lib/test/factories";

const noop = async () => idle;

/** The row of a given operation code, so the assertions stay per-operation. */
function row(code: string) {
  return screen.getByText(code).closest("tr") as HTMLElement;
}

describe("OperationTable", () => {
  it("Renders_an_empty_message_without_operations", () => {
    render(<OperationTable order={anOrder()} transition={noop} />);

    expect(screen.getByText(/No operations yet/)).toBeInTheDocument();
  });

  it("Renders_a_dash_when_there_is_no_target_cycle_time", () => {
    render(
      <OperationTable
        order={anOrder({
          operations: [anOperation({ targetCycleTimeSeconds: null })],
        })}
        transition={noop}
      />,
    );

    expect(screen.getByText("—")).toBeInTheDocument();
  });

  it("Disables_every_action_of_a_pending_operation_while_the_order_is_draft", () => {
    render(
      <OperationTable
        order={anOrder({ status: "Draft", operations: [anOperation()] })}
        transition={noop}
      />,
    );

    const cells = within(row("CUT"));
    expect(cells.getByRole("button", { name: "Start" })).toBeDisabled();
    expect(cells.getByRole("button", { name: "Complete" })).toBeDisabled();
    // Cancel stays open: the entity refuses it only for a completed operation.
    expect(cells.getByRole("button", { name: "Cancel" })).toBeEnabled();
  });

  it("Enables_Start_only_for_the_first_unfinished_operation", () => {
    render(
      <OperationTable
        order={anOrder({
          status: "InProgress",
          operations: [
            anOperation({ id: "a", sequence: 10, code: "CUT" }),
            anOperation({ id: "b", sequence: 20, code: "WELD" }),
          ],
        })}
        transition={noop}
      />,
    );

    expect(
      within(row("CUT")).getByRole("button", { name: "Start" }),
    ).toBeEnabled();
    expect(
      within(row("WELD")).getByRole("button", { name: "Start" }),
    ).toBeDisabled();
  });

  it("Enables_Complete_and_disables_Cancel_along_the_operation_lifecycle", () => {
    render(
      <OperationTable
        order={anOrder({
          status: "InProgress",
          operations: [
            anOperation({ id: "a", sequence: 10, code: "CUT", status: "InProgress" }),
            anOperation({ id: "b", sequence: 20, code: "WELD", status: "Completed" }),
          ],
        })}
        transition={noop}
      />,
    );

    expect(
      within(row("CUT")).getByRole("button", { name: "Complete" }),
    ).toBeEnabled();
    expect(
      within(row("WELD")).getByRole("button", { name: "Cancel" }),
    ).toBeDisabled();
  });
});
