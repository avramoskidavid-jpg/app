import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { getDashboardSummary } from "../api/dashboard";
import { createBuilding, deleteBuilding, getBuildings, updateBuilding } from "../api/buildings";
import type { Building, BuildingInput, DashboardSummary } from "../types";
import { SummaryCard } from "../components/SummaryCard";
import { StatusBadge } from "../components/StatusBadge";
import { BuildingForm } from "../components/BuildingForm";

export function DashboardPage() {
  const { hasRole } = useAuth();
  const navigate = useNavigate();
  const canEdit = hasRole("Admin", "Manager");

  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [buildings, setBuildings] = useState<Building[]>([]);
  const [loading, setLoading] = useState(true);
  const [showAddForm, setShowAddForm] = useState(false);
  const [editingBuilding, setEditingBuilding] = useState<Building | null>(null);

  const loadData = async () => {
    setLoading(true);
    const [summaryData, buildingsData] = await Promise.all([getDashboardSummary(), getBuildings()]);
    setSummary(summaryData);
    setBuildings(buildingsData);
    setLoading(false);
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleCreate = async (input: BuildingInput) => {
    await createBuilding(input);
    await loadData();
  };

  const handleUpdate = async (input: BuildingInput) => {
    if (!editingBuilding) return;
    await updateBuilding(editingBuilding.id, input);
    await loadData();
  };

  const handleDelete = async (building: Building) => {
    if (!confirm(`Delete "${building.name}"? This also removes its meters and readings.`)) return;
    await deleteBuilding(building.id);
    await loadData();
  };

  if (loading) {
    return <div className="loading-state">Loading dashboard...</div>;
  }

  return (
    <div>
      <div className="page-header">
        <div>
          <h2>Dashboard</h2>
          <p>Overview of all buildings and current energy consumption.</p>
        </div>
        {canEdit && (
          <button className="btn" onClick={() => setShowAddForm(true)}>
            + Add Building
          </button>
        )}
      </div>

      <div className="summary-grid">
        <SummaryCard label="Total Buildings" value={String(summary?.totalBuildings ?? 0)} />
        <SummaryCard
          label="Total Consumption (this month)"
          value={`${(summary?.totalConsumption ?? 0).toLocaleString(undefined, { maximumFractionDigits: 0 })} units`}
        />
        <SummaryCard
          label="Total Cost (this month)"
          value={`$${(summary?.totalCost ?? 0).toLocaleString(undefined, { maximumFractionDigits: 2 })}`}
        />
        <SummaryCard label="Active Alerts" value={String(summary?.activeAlerts ?? 0)} to="/alerts" />
      </div>

      <div className="card">
        <div className="card-header">
          <h3>Buildings</h3>
        </div>
        {buildings.length === 0 ? (
          <div className="empty-state">No buildings yet.</div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Address</th>
                <th>Type</th>
                <th>Meters</th>
                <th>Consumption (mo.)</th>
                <th>Cost (mo.)</th>
                <th>Status</th>
                {canEdit && <th></th>}
              </tr>
            </thead>
            <tbody>
              {buildings.map((b) => (
                <tr key={b.id} className="clickable-row" onClick={() => navigate(`/buildings/${b.id}`)}>
                  <td>{b.name}</td>
                  <td>{b.address}</td>
                  <td>{b.type}</td>
                  <td>{b.meterCount}</td>
                  <td>{b.currentMonthConsumption.toLocaleString(undefined, { maximumFractionDigits: 1 })}</td>
                  <td>${b.currentMonthCost.toLocaleString(undefined, { maximumFractionDigits: 2 })}</td>
                  <td>
                    <StatusBadge status={b.status} />
                  </td>
                  {canEdit && (
                    <td onClick={(e) => e.stopPropagation()}>
                      <div className="btn-row">
                        <button className="btn secondary" onClick={() => setEditingBuilding(b)}>
                          Edit
                        </button>
                        <button className="btn danger" onClick={() => handleDelete(b)}>
                          Delete
                        </button>
                      </div>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {showAddForm && <BuildingForm onSubmit={handleCreate} onClose={() => setShowAddForm(false)} />}
      {editingBuilding && (
        <BuildingForm
          initial={editingBuilding}
          onSubmit={handleUpdate}
          onClose={() => setEditingBuilding(null)}
        />
      )}
    </div>
  );
}
