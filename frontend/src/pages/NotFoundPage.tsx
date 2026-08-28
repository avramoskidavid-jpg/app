import { Link } from "react-router-dom";

export function NotFoundPage() {
  return (
    <div className="empty-state">
      <p>Page not found.</p>
      <Link to="/dashboard">Go to Dashboard</Link>
    </div>
  );
}
