import { apiClient } from "./client";
import type { Building, BuildingDetails, BuildingInput, EnergyTargetResult } from "../types";

export function getBuildings() {
  return apiClient.get<Building[]>("/buildings").then((r) => r.data);
}

export function getEnergyTarget(id: number) {
  return apiClient.get<EnergyTargetResult>(`/buildings/${id}/energy-target`).then((r) => r.data);
}

export function getBuildingDetails(id: number) {
  return apiClient.get<BuildingDetails>(`/buildings/${id}`).then((r) => r.data);
}

export function createBuilding(input: BuildingInput) {
  return apiClient.post<Building>("/buildings", input).then((r) => r.data);
}

export function updateBuilding(id: number, input: BuildingInput) {
  return apiClient.put<Building>(`/buildings/${id}`, input).then((r) => r.data);
}

export function deleteBuilding(id: number) {
  return apiClient.delete(`/buildings/${id}`);
}
