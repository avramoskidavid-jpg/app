import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts";
import type { MonthlyConsumption } from "../types";

export function ConsumptionChart({ data }: { data: MonthlyConsumption[] }) {
  return (
    <ResponsiveContainer width="100%" height={280}>
      <BarChart data={data} margin={{ top: 8, right: 8, left: 0, bottom: 0 }}>
        <CartesianGrid strokeDasharray="3 3" stroke="#e2e6ea" />
        <XAxis dataKey="month" tick={{ fontSize: 12 }} />
        <YAxis tick={{ fontSize: 12 }} />
        <Tooltip
          formatter={(value, name) => {
            const v = Number(value);
            return [
              name === "consumption" ? `${v.toFixed(1)} units` : `$${v.toFixed(2)}`,
              name === "consumption" ? "Consumption" : "Cost",
            ];
          }}
        />
        <Bar dataKey="consumption" fill="#2f6fed" radius={[4, 4, 0, 0]} />
      </BarChart>
    </ResponsiveContainer>
  );
}
