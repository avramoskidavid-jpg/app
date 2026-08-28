import { Navigate, Outlet } from "react-router-dom";
import type { Role } from "../types";
import { useAuth } from "./AuthContext";

export function ProtectedRoute({ roles }: { roles?: Role[] }) {
  const { isAuthenticated, hasRole } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (roles && roles.length > 0 && !hasRole(...roles)) {
    return <Navigate to="/dashboard" replace />;
  }

  return <Outlet />;
}
