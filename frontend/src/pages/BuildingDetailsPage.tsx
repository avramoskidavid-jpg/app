import { useEffect, useState } from "react";
import { Link, useParams } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { getBuildingDetails, getEnergyTarget } from "../api/buildings";
import { createMeter, deleteMeter, updateMeter } from "../api/meters";
import { createReading, deleteReading, getReadingsByMeter, updateReading } from "../api/readings";
import type { BuildingDetails, EnergyTargetResult, Meter, MeterInput, Reading, ReadingInput } from "../types";
import { StatusBadge } from "../components/StatusBadge";
import { ConsumptionChart } from "../components/ConsumptionChart";
import { MeterForm } from "../components/MeterForm";
import { ReadingForm } from "../components/ReadingForm";

export function BuildingDetailsPage() {
  const { id } = useParams();
  const buildingId = Number(id);
  const { hasRole } = useAuth();
  const canEdit = hasRole("Admin", "Manager");

  const [details, setDetails] = useState<BuildingDetails | null>(null);
  const [energyTarget, setEnergyTarget] = useState<EnergyTargetResult | null>(null);
  const [loading, setLoading] = useState(true);
  const [notFound, setNotFound] = useState(false);

  const [showAddMeter, setShowAddMeter] = useState(false);
  const [editingMeter, setEditingMeter] = useState<Meter | null>(null);

  const [selectedMeterId, setSelectedMeterId] = useState<number | null>(null);
  const [readings, setReadings] = useState<Reading[]>([]);
  const [readingsLoading, setReadingsLoading] = useState(false);
  const [showAddReading, setShowAddReading] = useState(false);
  const [editingReading, setEditingReading] = useState<Reading | null>(null);

  const loadDetails = async () => {
    setLoading(true);
    try {
      const [data, target] = await Promise.all([getBuildingDetails(buildingId), getEnergyTarget(buildingId)]);
      setDetails(data);
      setEnergyTarget(target);
      if (data.meters.length > 0 && selectedMeterId === null) {
        setSelectedMeterId(data.meters[0].id);
      }
    } catch (err: any) {
      if (err?.response?.status === 404) setNotFound(true);
    } finally {
      setLoading(false);
    }
  };

  const loadReadings = async (meterId: number) => {
    setReadingsLoading(true);
    const data = await getReadingsByMeter(meterId);
    setReadings(data);
    setReadingsLoading(false);
  };

  useEffect(() => {
    loadDetails();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [buildingId]);

  useEffect(() => {
    if (selectedMeterId !== null) {
      loadReadings(selectedMeterId);
    } else {
      setReadings([]);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [selectedMeterId]);

  const handleCreateMeter = async (input: MeterInput) => {
    await createMeter(input);
    await loadDetails();
  };

  const handleUpdateMeter = async (input: MeterInput) => {
    if (!editingMeter) return;
    await updateMeter(editingMeter.id, input);
    await loadDetails();
  };

  const handleDeleteMeter = async (meter: Meter) => {
    if (!confirm(`Delete meter "${meter.serialNumber}"? This also removes its readings.`)) return;
    await deleteMeter(meter.id);
    if (selectedMeterId === meter.id) setSelectedMeterId(null);
    await loadDetails();
  };

  const handleCreateReading = async (input: ReadingInput) => {
    await createReading(input);
    if (selectedMeterId) await loadReadings(selectedMeterId);
    await loadDetails();
  };

  const handleUpdateReading = async (input: ReadingInput) => {
    if (!editingReading) return;
    await updateReading(editingReading.id, input);
    if (selectedMeterId) await loadReadings(selectedMeterId);
    await loadDetails();
  };

  const handleDeleteReading = async (reading: Reading) => {
    if (!confirm("Delete this reading?")) return;
    await deleteReading(reading.id);
    if (selectedMeterId) await loadReadings(selectedMeterId);
    await loadDetails();
  };

  if (loading) return <div className="loading-state">Loading building...</div>;
  if (notFound || !details) return <div className="empty-state">Building not found.</div>;

  const { building, meters, monthlyConsumption } = details;
  const selectedMeter = meters.find((m) => m.id === selectedMeterId) ?? null;

  return (
    <div>
      <div className="breadcrumb">
        <Link to="/dashboard">← Back to Dashboard</Link>
      </div>
      <div className="page-header">
        <div>
          <h2>{building.name}</h2>
          <p>{building.address}</p>
        </div>
        <StatusBadge status={building.status} />
      </div>

      <div className="card">
        <div className="card-header">
          <h3>Building Info</h3>
        </div>
        <div className="info-grid">
          <div>
            <div className="label">Type</div>
            <div className="value">{building.type}</div>
          </div>
          <div>
            <div className="label">Area</div>
            <div className="value">{building.areaSqm.toLocaleString()} sqm</div>
          </div>
          <div>
            <div className="label">Consumption (this mo.)</div>
            <div className="value">{building.currentMonthConsumption.toFixed(1)} units</div>
          </div>
          <div>
            <div className="label">Cost (this mo.)</div>
            <div className="value">${building.currentMonthCost.toFixed(2)}</div>
          </div>
          <div>
            <div className="label">Warning threshold</div>
            <div className="value">{building.warningThreshold.toLocaleString()}</div>
          </div>
          <div>
            <div className="label">High threshold</div>
            <div className="value">{building.highThreshold.toLocaleString()}</div>
          </div>
          <div>
            <div className="label">Energy target</div>
            <div className="value">
              {building.energyTarget.toLocaleString()}
              {energyTarget && (
                <span
                  className={`badge ${energyTarget.exceeded ? "badge-high" : "badge-normal"}`}
                  style={{ marginLeft: 8 }}
                >
                  {energyTarget.exceeded ? "Exceeded" : "Within Target"}
                </span>
              )}
            </div>
          </div>
        </div>
      </div>

      <div className="card">
        <div className="card-header">
          <h3>Monthly Consumption</h3>
        </div>
        <ConsumptionChart data={monthlyConsumption} />
      </div>

      <div className="card">
        <div className="card-header">
          <h3>Meters</h3>
          {canEdit && (
            <button className="btn" onClick={() => setShowAddMeter(true)}>
              + Add Meter
            </button>
          )}
        </div>
        {meters.length === 0 ? (
          <div className="empty-state">No meters yet.</div>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Serial</th>
                <th>Type</th>
                <th>Unit</th>
                <th>Cost/unit</th>
                <th>Latest reading</th>
                {canEdit && <th></th>}
              </tr>
            </thead>
            <tbody>
              {meters.map((m) => (
                <tr
                  key={m.id}
                  className="clickable-row"
                  style={{ background: m.id === selectedMeterId ? "#f0f5ff" : undefined }}
                  onClick={() => setSelectedMeterId(m.id)}
                >
                  <td>{m.serialNumber}</td>
                  <td>{m.type}</td>
                  <td>{m.unit}</td>
                  <td>${m.costPerUnit.toFixed(2)}</td>
                  <td>
                    {m.latestReadingValue !== null
                      ? `${m.latestReadingValue} ${m.unit} (${new Date(m.latestReadingTimestamp!).toLocaleDateString()})`
                      : "—"}
                  </td>
                  {canEdit && (
                    <td onClick={(e) => e.stopPropagation()}>
                      <div className="btn-row">
                        <button className="btn secondary" onClick={() => setEditingMeter(m)}>
                          Edit
                        </button>
                        <button className="btn danger" onClick={() => handleDeleteMeter(m)}>
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

      {selectedMeter && (
        <div className="card">
          <div className="card-header">
            <h3>Readings — {selectedMeter.serialNumber}</h3>
            {canEdit && (
              <button className="btn" onClick={() => setShowAddReading(true)}>
                + Add Reading
              </button>
            )}
          </div>
          {readingsLoading ? (
            <div className="loading-state">Loading readings...</div>
          ) : readings.length === 0 ? (
            <div className="empty-state">No readings yet for this meter.</div>
          ) : (
            <table>
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Value</th>
                  <th>Cost</th>
                  <th>Notes</th>
                  {canEdit && <th></th>}
                </tr>
              </thead>
              <tbody>
                {readings.map((r) => (
                  <tr key={r.id}>
                    <td>{new Date(r.timestamp).toLocaleDateString()}</td>
                    <td>
                      {r.value} {selectedMeter.unit}
                    </td>
                    <td>${r.cost.toFixed(2)}</td>
                    <td>{r.notes ?? "—"}</td>
                    {canEdit && (
                      <td>
                        <div className="btn-row">
                          <button className="btn secondary" onClick={() => setEditingReading(r)}>
                            Edit
                          </button>
                          <button className="btn danger" onClick={() => handleDeleteReading(r)}>
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
      )}

      {showAddMeter && (
        <MeterForm buildingId={buildingId} onSubmit={handleCreateMeter} onClose={() => setShowAddMeter(false)} />
      )}
      {editingMeter && (
        <MeterForm
          buildingId={buildingId}
          initial={editingMeter}
          onSubmit={handleUpdateMeter}
          onClose={() => setEditingMeter(null)}
        />
      )}
      {showAddReading && selectedMeter && (
        <ReadingForm
          meterId={selectedMeter.id}
          onSubmit={handleCreateReading}
          onClose={() => setShowAddReading(false)}
        />
      )}
      {editingReading && selectedMeter && (
        <ReadingForm
          meterId={selectedMeter.id}
          initial={editingReading}
          onSubmit={handleUpdateReading}
          onClose={() => setEditingReading(null)}
        />
      )}
    </div>
  );
}
