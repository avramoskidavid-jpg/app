import { useEffect, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { getAlerts, resolveAlert, scanForAnomalies } from "../api/alerts";
import { getBuildings } from "../api/buildings";
import type { Alert, AlertSeverity, AlertStatus, Building } from "../types";
import { SeverityBadge, AlertStatusBadge } from "../components/AlertBadges";

export function AlertsPage() {
  const { hasRole } = useAuth();
  const canManage = hasRole("Admin", "Manager");

  const [alerts, setAlerts] = useState<Alert[]>([]);
  const [buildings, setBuildings] = useState<Building[]>([]);
  const [loading, setLoading] = useState(true);
  const [scanning, setScanning] = useState(false);
  const [scanMessage, setScanMessage] = useState("");

  const [severityFilter, setSeverityFilter] = useState<AlertSeverity | "">("");
  const [buildingFilter, setBuildingFilter] = useState<number | "">("");
  const [statusFilter, setStatusFilter] = useState<AlertStatus | "">("Open");

  const loadAlerts = async () => {
    setLoading(true);
    const data = await getAlerts({
      severity: severityFilter || undefined,
      buildingId: buildingFilter || undefined,
      status: statusFilter || undefined,
    });
    setAlerts(data);
    setLoading(false);
  };

  useEffect(() => {
    getBuildings().then(setBuildings);
  }, []);

  useEffect(() => {
    loadAlerts();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [severityFilter, buildingFilter, statusFilter]);

  const handleResolve = async (alert: Alert) => {
    await resolveAlert(alert.id);
    await loadAlerts();
  };

  const handleScan = async () => {
    setScanning(true);
    setScanMessage("");
    try {
      const result = await scanForAnomalies();
      setScanMessage(
        result.alertsCreated > 0
          ? `${result.alertsCreated} new alert(s) created.`
          : "No new anomalies detected."
      );
      await loadAlerts();
    } finally {
      setScanning(false);
    }
  };

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>Alerts</h2>
          <p>Consumption anomalies detected across all buildings.</p>
        </div>
        {canManage && (
          <button className="btn" onClick={handleScan} disabled={scanning}>
            {scanning ? "Scanning..." : "Scan for anomalies"}
          </button>
        )}
      </div>

      {scanMessage && <div className="error-banner" style={{ background: "#eef4ff", color: "#3538cd" }}>{scanMessage}</div>}

      <div className="card">
        <div className="card-header">
          <h3>Filters</h3>
        </div>
        <div className="form-grid" style={{ gridTemplateColumns: "repeat(3, 1fr)" }}>
          <div className="field">
            <label>Status</label>
            <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value as AlertStatus | "")}>
              <option value="Open">Open</option>
              <option value="Resolved">Resolved</option>
              <option value="">All</option>
            </select>
          </div>
          <div className="field">
            <label>Severity</label>
            <select value={severityFilter} onChange={(e) => setSeverityFilter(e.target.value as AlertSeverity | "")}>
              <option value="">All</option>
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
            </select>
          </div>
          <div className="field">
            <label>Building</label>
            <select
              value={buildingFilter}
              onChange={(e) => setBuildingFilter(e.target.value ? Number(e.target.value) : "")}
            >
              <option value="">All buildings</option>
              {buildings.map((b) => (
                <option key={b.id} value={b.id}>
                  {b.name}
                </option>
              ))}
            </select>
          </div>
        </div>
      </div>

      <div className="card">
        <div className="card-header">
          <h3>{alerts.length} alert{alerts.length === 1 ? "" : "s"}</h3>
        </div>
        {loading ? (
          <div className="loading-state">Loading alerts...</div>
        ) : alerts.length === 0 ? (
          <div className="empty-state">No alerts match these filters.</div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Building</th>
                <th>Message</th>
                <th>Severity</th>
                <th>Status</th>
                <th>Created</th>
                {canManage && <th></th>}
              </tr>
            </thead>
            <tbody>
              {alerts.map((a) => (
                <tr key={a.id}>
                  <td>{a.buildingName}</td>
                  <td>{a.message}</td>
                  <td>
                    <SeverityBadge severity={a.severity} />
                  </td>
                  <td>
                    <AlertStatusBadge status={a.status} />
                  </td>
                  <td>{new Date(a.createdAt).toLocaleString()}</td>
                  {canManage && (
                    <td>
                      {a.status === "Open" && (
                        <button className="btn secondary" onClick={() => handleResolve(a)}>
                          Mark Resolved
                        </button>
                      )}
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
