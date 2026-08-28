import { apiClient } from "./client";
import type { Reading, ReadingInput } from "../types";

export function getReadingsByMeter(meterId: number) {
  return apiClient
    .get<Reading[]>("/readings", { params: { meterId } })
    .then((r) => r.data);
}

export function createReading(input: ReadingInput) {
  return apiClient.post<Reading>("/readings", input).then((r) => r.data);
}

export function updateReading(id: number, input: Omit<ReadingInput, "meterId">) {
  return apiClient.put<Reading>(`/readings/${id}`, input).then((r) => r.data);
}

export function deleteReading(id: number) {
  return apiClient.delete(`/readings/${id}`);
}
