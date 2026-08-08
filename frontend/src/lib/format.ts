// Fixed locale and time zone so the server render and the client hydration agree.
const dateTime = new Intl.DateTimeFormat("en-GB", {
  dateStyle: "short",
  timeStyle: "short",
  timeZone: "UTC",
});

export function formatDateTime(value: string | null): string {
  return value ? dateTime.format(new Date(value)) : "—";
}

export function formatQuantity(value: number): string {
  return new Intl.NumberFormat("en-GB", { maximumFractionDigits: 3 }).format(
    value,
  );
}
