export type Role = "Admin" | "Manager" | "Viewer";

export interface User {
  id: number;
  fullName: string;
  email: string;
  role: Role;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: User;
}

export type ConsumptionStatus = "Normal" | "Warning" | "High";

export interface Building {
  id: number;
  name: string;
  address: string;
  type: string;
  areaSqm: number;
  warningThreshold: number;
  highThreshold: number;
  energyTarget: number;
  meterCount: number;
  currentMonthConsumption: number;
  currentMonthCost: number;
  status: ConsumptionStatus;
}

export type BuildingInput = {
  name: string;
  address: string;
  type: string;
  areaSqm: number;
  warningThreshold: number;
  highThreshold: number;
  energyTarget: number;
};

export type MeterType = "Electricity" | "Water" | "Gas";

export interface Meter {
  id: number;
  buildingId: number;
  serialNumber: string;
  type: MeterType;
  unit: string;
  costPerUnit: number;
  latestReadingValue: number | null;
  latestReadingTimestamp: string | null;
}

export type MeterInput = {
  buildingId: number;
  serialNumber: string;
  type: MeterType;
  unit: string;
  costPerUnit: number;
};

export interface Reading {
  id: number;
  meterId: number;
  timestamp: string;
  value: number;
  cost: number;
  notes: string | null;
}

export type ReadingInput = {
  meterId: number;
  timestamp: string;
  value: number;
  notes?: string | null;
};

export interface MonthlyConsumption {
  month: string;
  consumption: number;
  cost: number;
}

export interface BuildingDetails {
  building: Building;
  meters: Meter[];
  monthlyConsumption: MonthlyConsumption[];
}

export interface DashboardSummary {
  totalBuildings: number;
  totalConsumption: number;
  totalCost: number;
  activeAlerts: number;
}

export type AlertSeverity = "Low" | "Medium" | "High";
export type AlertStatus = "Open" | "Resolved";

export interface Alert {
  id: number;
  buildingId: number;
  buildingName: string;
  message: string;
  severity: AlertSeverity;
  status: AlertStatus;
  createdAt: string;
  resolvedAt: string | null;
}

export interface EnergyTargetResult {
  exceeded: boolean;
  currentMonthConsumption: number;
  energyTarget: number;
  percentOfTarget: number;
}
