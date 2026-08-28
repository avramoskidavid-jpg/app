import { apiClient } from "./client";
import type { Meter, MeterInput } from "../types";

export function getMetersByBuilding(buildingId: number) {
  return apiClient
    .get<Meter[]>("/meters", { params: { buildingId } })
    .then((r) => r.data);
}

export function createMeter(input: MeterInput) {
  return apiClient.post<Meter>("/meters", input).then((r) => r.data);
}

export function updateMeter(id: number, input: Omit<MeterInput, "buildingId">) {
  return apiClient.put<Meter>(`/meters/${id}`, input).then((r) => r.data);
}

export function deleteMeter(id: number) {
  return apiClient.delete(`/meters/${id}`);
}
