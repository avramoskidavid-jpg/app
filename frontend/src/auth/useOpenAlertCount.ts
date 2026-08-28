import { useEffect, useState } from "react";
import { getOpenAlertCount } from "../api/alerts";
import { useAuth } from "./AuthContext";

const POLL_INTERVAL_MS = 30_000;

export function useOpenAlertCount() {
  const { isAuthenticated } = useAuth();
  const [count, setCount] = useState(0);

  useEffect(() => {
    if (!isAuthenticated) {
      setCount(0);
      return;
    }

    let cancelled = false;

    const load = () => {
      getOpenAlertCount()
        .then((c) => {
          if (!cancelled) setCount(c);
        })
        .catch(() => {
          /* ignore transient polling errors */
        });
    };

    load();
    const interval = setInterval(load, POLL_INTERVAL_MS);
    return () => {
      cancelled = true;
      clearInterval(interval);
    };
  }, [isAuthenticated]);

  return count;
}
