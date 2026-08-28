import { NavLink, Outlet } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { useOpenAlertCount } from "../auth/useOpenAlertCount";

export function Layout() {
  const { user, logout } = useAuth();
  const openAlertCount = useOpenAlertCount();

  return (
    <div className="app-shell">
      <aside className="navbar">
        <h1>Energy Mgmt</h1>
        <nav>
          <NavLink to="/dashboard" className={({ isActive }) => (isActive ? "active" : "")}>
            Dashboard
          </NavLink>
          <NavLink to="/alerts" className={({ isActive }) => (isActive ? "active" : "")}>
            Alerts
            {openAlertCount > 0 && <span className="nav-badge">{openAlertCount}</span>}
          </NavLink>
        </nav>
        <div className="user-info">
          <strong>{user?.fullName}</strong>
          <div>{user?.email}</div>
          <span className="role-pill">{user?.role}</span>
          <button className="logout-btn" onClick={logout}>
            Log out
          </button>
        </div>
      </aside>
      <main className="page-content">
        <Outlet />
      </main>
    </div>
  );
}
