import { Navigate, Outlet } from "react-router-dom";
import { useAuthStore } from "@/store/authStore";

/// Wrap any <Route> that needs a logged-in user in this. Unauthenticated visitors
/// are bounced to /login instead of seeing the protected page render (even briefly).
export default function ProtectedRoute() {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
  return isAuthenticated ? <Outlet /> : <Navigate to="/login" replace />;
}
