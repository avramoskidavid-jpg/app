import { apiClient } from "./client";
import type { AuthResponse, Role } from "../types";

export function login(email: string, password: string) {
  return apiClient
    .post<AuthResponse>("/auth/login", { email, password })
    .then((r) => r.data);
}

export function register(fullName: string, email: string, password: string, role: Role) {
  return apiClient
    .post<AuthResponse>("/auth/register", { fullName, email, password, role })
    .then((r) => r.data);
}
