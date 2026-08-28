import { useState } from "react";
import type { Meter, MeterInput, MeterType } from "../types";
import { Modal } from "./Modal";

const METER_TYPES: { type: MeterType; unit: string }[] = [
  { type: "Electricity", unit: "kWh" },
  { type: "Water", unit: "m3" },
  { type: "Gas", unit: "m3" },
];

export function MeterForm({
  buildingId,
  initial,
  onSubmit,
  onClose,
}: {
  buildingId: number;
  initial?: Meter;
  onSubmit: (input: MeterInput) => Promise<void>;
  onClose: () => void;
}) {
  const [form, setForm] = useState<MeterInput>({
    buildingId,
    serialNumber: initial?.serialNumber ?? "",
    type: initial?.type ?? "Electricity",
    unit: initial?.unit ?? "kWh",
    costPerUnit: initial?.costPerUnit ?? 0.2,
  });
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  const handleTypeChange = (type: MeterType) => {
    const preset = METER_TYPES.find((m) => m.type === type);
    setForm({ ...form, type, unit: preset?.unit ?? form.unit });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setSaving(true);
    try {
      await onSubmit(form);
      onClose();
    } catch (err: any) {
      setError(err?.response?.data?.message ?? "Something went wrong.");
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal title={initial ? "Edit Meter" : "Add Meter"} onClose={onClose}>
      <form onSubmit={handleSubmit}>
        {error && <div className="error-banner">{error}</div>}
        <div className="field">
          <label>Serial number</label>
          <input
            required
            value={form.serialNumber}
            onChange={(e) => setForm({ ...form, serialNumber: e.target.value })}
          />
        </div>
        <div className="form-grid">
          <div className="field">
            <label>Type</label>
            <select value={form.type} onChange={(e) => handleTypeChange(e.target.value as MeterType)}>
              {METER_TYPES.map((m) => (
                <option key={m.type} value={m.type}>
                  {m.type}
                </option>
              ))}
            </select>
          </div>
          <div className="field">
            <label>Unit</label>
            <input required value={form.unit} onChange={(e) => setForm({ ...form, unit: e.target.value })} />
          </div>
          <div className="field">
            <label>Cost per unit ($)</label>
            <input
              type="number"
              step="0.01"
              min={0}
              required
              value={form.costPerUnit}
              onChange={(e) => setForm({ ...form, costPerUnit: Number(e.target.value) })}
            />
          </div>
        </div>
        <div className="btn-row">
          <button type="submit" className="btn" disabled={saving}>
            {saving ? "Saving..." : "Save"}
          </button>
          <button type="button" className="btn secondary" onClick={onClose}>
            Cancel
          </button>
        </div>
      </form>
    </Modal>
  );
}
