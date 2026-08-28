import type { AlertSeverity, AlertStatus } from "../types";

export function SeverityBadge({ severity }: { severity: AlertSeverity }) {
  const className =
    severity === "High" ? "badge badge-high" : severity === "Medium" ? "badge badge-warning" : "badge badge-role";
  return <span className={className}>{severity}</span>;
}

export function AlertStatusBadge({ status }: { status: AlertStatus }) {
  const className = status === "Open" ? "badge badge-high" : "badge badge-normal";
  return <span className={className}>{status}</span>;
}
