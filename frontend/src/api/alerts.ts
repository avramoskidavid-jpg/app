import { apiClient } from "./client";
import type { Alert, AlertSeverity, AlertStatus } from "../types";

export function getAlerts(filters?: { buildingId?: number; severity?: AlertSeverity; status?: AlertStatus }) {
  return apiClient.get<Alert[]>("/alerts", { params: filters }).then((r) => r.data);
}

export function getOpenAlertCount() {
  return apiClient.get<{ count: number }>("/alerts/count").then((r) => r.data.count);
}

export function resolveAlert(id: number) {
  return apiClient.put<Alert>(`/alerts/${id}/resolve`).then((r) => r.data);
}

export function scanForAnomalies() {
  return apiClient.post<{ alertsCreated: number }>("/alerts/scan").then((r) => r.data);
}
