import { useState } from "react";
import type { Reading, ReadingInput } from "../types";
import { Modal } from "./Modal";

function toDateInputValue(iso?: string) {
  if (!iso) return new Date().toISOString().slice(0, 10);
  return iso.slice(0, 10);
}

export function ReadingForm({
  meterId,
  initial,
  onSubmit,
  onClose,
}: {
  meterId: number;
  initial?: Reading;
  onSubmit: (input: ReadingInput) => Promise<void>;
  onClose: () => void;
}) {
  const [date, setDate] = useState(toDateInputValue(initial?.timestamp));
  const [value, setValue] = useState(initial?.value ?? 0);
  const [notes, setNotes] = useState(initial?.notes ?? "");
  const [error, setError] = useState("");
  const [saving, setSaving] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError("");
    setSaving(true);
    try {
      await onSubmit({
        meterId,
        timestamp: new Date(date + "T00:00:00Z").toISOString(),
        value,
        notes: notes || null,
      });
      onClose();
    } catch (err: any) {
      setError(err?.response?.data?.message ?? "Something went wrong.");
    } finally {
      setSaving(false);
    }
  };

  return (
    <Modal title={initial ? "Edit Reading" : "Add Reading"} onClose={onClose}>
      <form onSubmit={handleSubmit}>
        {error && <div className="error-banner">{error}</div>}
        <div className="form-grid">
          <div className="field">
            <label>Date</label>
            <input type="date" required value={date} onChange={(e) => setDate(e.target.value)} />
          </div>
          <div className="field">
            <label>Consumption value</label>
            <input
              type="number"
              step="0.1"
              min={0}
              required
              value={value}
              onChange={(e) => setValue(Number(e.target.value))}
            />
          </div>
        </div>
        <div className="field">
          <label>Notes (optional)</label>
          <textarea rows={2} value={notes} onChange={(e) => setNotes(e.target.value)} />
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
