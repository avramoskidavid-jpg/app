import type { ConsumptionStatus } from "../types";

export function StatusBadge({ status }: { status: ConsumptionStatus }) {
  const className =
    status === "High" ? "badge badge-high" : status === "Warning" ? "badge badge-warning" : "badge badge-normal";
  return <span className={className}>{status}</span>;
}
