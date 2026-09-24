import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { apiFetch, HttpError } from "@/lib/api/http";
import { useAuthStore } from "@/store/authStore";

interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  email: string;
  fullName: string;
}

export default function LoginPage() {
  const navigate = useNavigate();
  const login = useAuthStore((s) => s.login);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const res = await apiFetch<AuthResponse>("/api/auth/login", {
        method: "POST",
        body: { email, password },
      });
      login({ email: res.email, fullName: res.fullName }, res.token);
      navigate("/dashboard");
    } catch (err) {
      setError(err instanceof HttpError ? err.message : "Something went wrong.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="mx-auto max-w-sm space-y-4">
      <h1 className="text-xl font-semibold">Log in</h1>
      <form onSubmit={handleSubmit} className="space-y-3">
        <input
          type="email"
          required
          placeholder="Email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
        />
        <input
          type="password"
          required
          placeholder="Password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          className="w-full rounded-md border border-slate-300 px-3 py-2 text-sm"
        />
        {error && <p className="text-sm text-red-600">{error}</p>}
        <button
          type="submit"
          disabled={loading}
          className="w-full rounded-md bg-slate-900 px-3 py-2 text-sm font-medium text-white hover:bg-slate-800 disabled:opacity-50"
        >
          {loading ? "Logging in..." : "Log in"}
        </button>
      </form>
      <p className="text-sm text-slate-600">
        No account?{" "}
        <Link to="/register" className="underline">
          Sign up
        </Link>
      </p>
    </div>
  );
}
