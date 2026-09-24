import { Link, Outlet } from "react-router-dom";
import { useAuthStore } from "@/store/authStore";

export default function Layout() {
  const { user, isAuthenticated, logout } = useAuthStore();

  return (
    <div className="min-h-screen bg-slate-50 text-slate-900">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-4xl items-center justify-between px-4 py-3">
          <Link to="/" className="font-semibold tracking-tight">
            CleanStart
          </Link>
          <nav className="flex items-center gap-4 text-sm">
            {isAuthenticated ? (
              <>
                <Link to="/dashboard" className="hover:underline">
                  Dashboard
                </Link>
                <span className="text-slate-500">{user?.fullName}</span>
                <button onClick={logout} className="text-slate-500 hover:text-slate-900">
                  Log out
                </button>
              </>
            ) : (
              <>
                <Link to="/login" className="hover:underline">
                  Log in
                </Link>
                <Link
                  to="/register"
                  className="rounded-md bg-slate-900 px-3 py-1.5 text-white hover:bg-slate-800"
                >
                  Sign up
                </Link>
              </>
            )}
          </nav>
        </div>
      </header>
      <main className="mx-auto max-w-4xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  );
}
