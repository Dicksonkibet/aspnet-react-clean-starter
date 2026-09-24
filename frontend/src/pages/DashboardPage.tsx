import { useEffect, useState } from "react";
import { apiFetch, HttpError } from "@/lib/api/http";

interface TodoItem {
  id: string;
  title: string;
  isDone: boolean;
}

// Sample authenticated page — proves the full slice works: JWT attached
// automatically by apiFetch, hits the sample TodoItems feature on the API.
export default function DashboardPage() {
  const [items, setItems] = useState<TodoItem[]>([]);
  const [title, setTitle] = useState("");
  const [error, setError] = useState<string | null>(null);

  async function load() {
    try {
      setItems(await apiFetch<TodoItem[]>("/api/todo-items"));
    } catch (err) {
      setError(err instanceof HttpError ? err.message : "Failed to load.");
    }
  }

  useEffect(() => {
    load();
  }, []);

  async function addItem(e: React.FormEvent) {
    e.preventDefault();
    if (!title.trim()) return;
    await apiFetch("/api/todo-items", { method: "POST", body: { title } });
    setTitle("");
    load();
  }

  async function toggleDone(item: TodoItem) {
    await apiFetch(`/api/todo-items/${item.id}/done`, { method: "PATCH", body: !item.isDone });
    load();
  }

  async function remove(id: string) {
    await apiFetch(`/api/todo-items/${id}`, { method: "DELETE" });
    load();
  }

  return (
    <div className="max-w-md space-y-4">
      <h1 className="text-xl font-semibold">Dashboard</h1>
      <p className="text-sm text-slate-600">
        This list is a sample MediatR + EF Core vertical slice — copy the pattern for
        your real features and delete this once you don't need it.
      </p>
      <form onSubmit={addItem} className="flex gap-2">
        <input
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="New item..."
          className="flex-1 rounded-md border border-slate-300 px-3 py-2 text-sm"
        />
        <button className="rounded-md bg-slate-900 px-3 py-2 text-sm text-white hover:bg-slate-800">
          Add
        </button>
      </form>
      {error && <p className="text-sm text-red-600">{error}</p>}
      <ul className="divide-y divide-slate-200 rounded-md border border-slate-200 bg-white">
        {items.map((item) => (
          <li key={item.id} className="flex items-center gap-3 px-3 py-2">
            <input type="checkbox" checked={item.isDone} onChange={() => toggleDone(item)} />
            <span className={`flex-1 text-sm ${item.isDone ? "text-slate-400 line-through" : ""}`}>
              {item.title}
            </span>
            <button onClick={() => remove(item.id)} className="text-xs text-slate-400 hover:text-red-600">
              Remove
            </button>
          </li>
        ))}
        {items.length === 0 && <li className="px-3 py-4 text-sm text-slate-400">No items yet.</li>}
      </ul>
    </div>
  );
}
