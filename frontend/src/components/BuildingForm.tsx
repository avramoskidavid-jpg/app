import { useState } from "react";
import type { Building, BuildingInput } from "../types";
import { Modal } from "./Modal";

const BUILDING_TYPES = ["Office", "Residential", "Commercial", "Industrial"];

export function BuildingForm({
  initial,
  onSubmit,
  onClose,
}: {
  initial?: Building;
  onSubmit: (input: BuildingInput) => Promise<void>;
  onClose: () => void;
}) {
  const [form, setForm] = useState<BuildingInput>({
    name: initial?.name ?? "",
    address: initial?.address ?? "",
    type: initial?.type ?? "Office",
    areaSqm: initial?.areaSqm ?? 1000,
    warningThreshold: initial?.warningThreshold ?? 5000,
    highThreshold: initial?.highThreshold ?? 8000,
    energyTarget: initial?.energyTarget ?? 4500,
  });
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

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
    <Modal title={initial ? "Edit Building" : "Add Building"} onClose={onClose}>
      <form onSubmit={handleSubmit}>
        {error && <div className="error-banner">{error}</div>}
        <div className="field">
          <label>Name</label>
          <input
            required
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
          />
        </div>
        <div className="field">
          <label>Address</label>
          <input
            required
            value={form.address}
            onChange={(e) => setForm({ ...form, address: e.target.value })}
          />
        </div>
        <div className="form-grid">
          <div className="field">
            <label>Type</label>
            <select value={form.type} onChange={(e) => setForm({ ...form, type: e.target.value })}>
              {BUILDING_TYPES.map((t) => (
                <option key={t} value={t}>
                  {t}
                </option>
              ))}
            </select>
          </div>
          <div className="field">
            <label>Area (sqm)</label>
            <input
              type="number"
              min={0}
              required
              value={form.areaSqm}
              onChange={(e) => setForm({ ...form, areaSqm: Number(e.target.value) })}
            />
          </div>
          <div className="field">
            <label>Warning threshold</label>
            <input
              type="number"
              min={0}
              required
              value={form.warningThreshold}
              onChange={(e) => setForm({ ...form, warningThreshold: Number(e.target.value) })}
            />
          </div>
          <div className="field">
            <label>High threshold</label>
            <input
              type="number"
              min={0}
              required
              value={form.highThreshold}
              onChange={(e) => setForm({ ...form, highThreshold: Number(e.target.value) })}
            />
          </div>
          <div className="field">
            <label>Energy target</label>
            <input
              type="number"
              min={0}
              required
              value={form.energyTarget}
              onChange={(e) => setForm({ ...form, energyTarget: Number(e.target.value) })}
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
