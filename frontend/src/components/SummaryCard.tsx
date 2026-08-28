import { useNavigate } from "react-router-dom";

export function SummaryCard({ label, value, to }: { label: string; value: string; to?: string }) {
  const navigate = useNavigate();
  return (
    <div
      className="summary-card"
      onClick={to ? () => navigate(to) : undefined}
      style={to ? { cursor: "pointer" } : undefined}
    >
      <div className="label">{label}</div>
      <div className="value">{value}</div>
    </div>
  );
}
