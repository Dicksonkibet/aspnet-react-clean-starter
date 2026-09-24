export default function HomePage() {
  return (
    <div className="space-y-3">
      <h1 className="text-2xl font-semibold tracking-tight">CleanStart</h1>
      <p className="text-slate-600">
        A clean-architecture ASP.NET Core backend + React/TypeScript frontend starter.
        Sign up, log in, and try the sample to-do list on the dashboard to see the
        full slice (API auth → JWT → protected route → React) working end to end.
      </p>
    </div>
  );
}
