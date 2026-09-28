# LibSqlLite

A small, reusable C# class library that lets any app persist plain C# classes to a local SQLite database **without hand-writing table definitions or column lists**. The C# class is the schema. Tables are created from classes via reflection, new properties become new columns automatically, and CRUD operations are generic.

This library is shared infrastructure for multiple personal apps. Keep it small, predictable, and dependency-light.

---

## Tech stack

- **Target:** .NET 10 (`net10.0`), C# latest, nullable reference types enabled, implicit usings enabled
- **Packages:**
  - `Microsoft.Data.Sqlite`: SQLite driver
  - `Dapper`: object mapping for queries
- **Tests:** xUnit, using a temp-file SQLite database per test (see `SqliteStoreTestBase`)
- No other runtime dependencies without asking first.

## Commands

```
dotnet build                                  # build the whole solution
dotnet test                                   # run all tests
dotnet test --filter "FullyQualifiedName~RoundTripTests"   # run one test class
dotnet test --filter "FullyQualifiedName~RoundTripTests.RoundTrip_AllSupportedTypes_WithValues"  # run one test
```

Run these from the repo root (`LibSqlLite.slnx` lives there). Both `dotnet build` and `dotnet test` must pass before considering a task done.

## Solution layout

The library and its tests are **sibling directories** (not nested under `src/`/`tests/`), tied together by `LibSqlLite.slnx`:

```
LibSqlLite/                        (repo root; also the library project directory)
    LibSqlLite.slnx
    LibSqlLite.csproj
    SqliteStore.cs             // public entry point
    Schema/TableBuilder.cs     // CREATE TABLE / ALTER TABLE generation
    Schema/TypeMap.cs          // CLR type -> SQLite type affinity
    Schema/EntityInfo.cs       // cached reflection metadata per type, pre-built SQL
    Attributes/                // TableAttribute, IgnoreAttribute, PrimaryKeyAttribute, UniqueAttribute
    TypeHandlers/               // Dapper handlers (Guid, DateTime, DateTimeOffset, decimal)
../LibSqlLite.Tests/            // sibling directory, referenced by the .slnx
    LibSqlLite.Tests.csproj
```

There is no `src/` nesting — the class library's files live directly in the repo root alongside the `.csproj`.

## Implementation notes specific to this codebase

- **Dapper's built-in type map shadows custom handlers.** Dapper has hardcoded `DbType` mappings for `Guid`, `DateTime`, `DateTimeOffset`, and `decimal` that are consulted *before* the `ITypeHandler` registry, so registering handlers for these types via `SqlMapper.AddTypeHandler` alone is silently ignored. `TypeHandlerRegistration` calls `SqlMapper.RemoveTypeMap(typeof(T))` (and `typeof(T?)`) for each of these four types before adding the handler — keep that call, and apply the same pattern if another built-in-mapped type ever needs a custom handler.
- **Ambient transaction is `AsyncLocal<SqliteTransaction?>`, not a plain field.** `InTransaction`/`InTransactionAsync` set it so nested calls on the same `SqliteStore` instance enlist in the same transaction instead of opening a new one. It must stay `AsyncLocal` (not `[ThreadStatic]`) because `ThreadStatic` doesn't flow across `await` continuations that resume on a different thread, and it must stay an instance field (not `static`) so two `SqliteStore` instances on the same thread don't see each other's transaction.
- **Boxing a key value for reflection `SetValue` needs an explicit `(object)` cast on every branch of a ternary.** `cond ? (int)x : longVal` unifies both branches to `long` before boxing, so `SetValue` on an `int`-typed key property throws `ArgumentException`. `SqliteStore.SetKeyValue` casts each branch to `(object)` separately to avoid this.
- Boxed key parameters (`Get<T>(object id)`, `Delete<T>(object id)`) are passed to Dapper via `DynamicParameters`, not an anonymous object — an anonymous object's property would be typed `object` at compile time, which loses the runtime type Dapper needs to pick the right `ITypeHandler` (e.g. for `Guid` keys).
- **Sync and async CRUD calls share one `SemaphoreSlim`, not separate lock primitives.** `SemaphoreSlim` supports both `Wait()` and `WaitAsync()` on the same instance, which is exactly what's needed here: a `Monitor`/`lock` for sync plus a separate semaphore for async would let a sync call and an async call run against the single shared `SqliteConnection` at the same time, since neither primitive knows about the other.
- **`EnsureTable` running inside `InTransaction`/`InTransactionAsync` must not mark the table "ensured" until that transaction actually commits.** If the surrounding transaction rolls back, the `CREATE TABLE` it ran is undone — but the in-memory `_ensuredTables` cache doesn't know that. Marking it ensured immediately means every later call on that type silently skips table creation and fails with "no such table" forever after a rollback. `SqliteStore` stages ensured-types in a per-transaction `AsyncLocal<HashSet<Type>>` (`_pendingEnsuredTables`) and only merges it into `_ensuredTables` after `tx.Commit()`/`CommitAsync()` succeeds.
- **The connection string sets `Pooling=False`.** A `SqliteStore` owns exactly one connection for its whole lifetime, so ADO.NET connection pooling buys nothing — but left on, it keeps the native SQLite file handle open (in the pool, for reuse) even after `Dispose()`, which surprises callers who expect to delete or move the `.db` file right after disposing the store.

---

## Conventions (how a class maps to a table)

- **Table name** = class name. Overridable with `[Table("Name")]`.
- **Columns** = every public instance property with both a public getter and setter.
- **Excluded:** properties marked `[Ignore]`, indexers, and properties whose type is not a supported scalar (see type map). Unsupported types should be skipped silently in schema generation, **not** throw. Log nothing; this is a library.
- **Primary key:**
  - A property named `Id` (case-insensitive), or one marked `[PrimaryKey]` (attribute wins if both exist).
  - If the key is `int` or `long`: `INTEGER PRIMARY KEY AUTOINCREMENT`. On insert, omit it from the INSERT and write the generated value back onto the object.
  - If the key is `Guid` or `string`: stored as `TEXT PRIMARY KEY`, value supplied by the caller. If a `Guid` key is `Guid.Empty` on insert, generate a new one and assign it.
  - A class with no key is an error when calling `EnsureTable<T>` (throw `InvalidOperationException` with a clear message naming the type).
- **Identifiers** are always quoted with square brackets: `[Table]`, `[Column]`.
- **Values** are always passed as parameters. Never concatenate values into SQL.

## Type map

| CLR type (and its nullable form) | SQLite type | Stored as |
|---|---|---|
| `int`, `long`, `short`, `byte`, `bool`, enums | `INTEGER` | enums as underlying integer |
| `double`, `float` | `REAL` | |
| `decimal` | `TEXT` | invariant-culture string, to preserve precision |
| `string` | `TEXT` | |
| `Guid` | `TEXT` | lowercase `D` format |
| `DateTime` | `TEXT` | ISO 8601 round-trip (`"O"`); convert to UTC on write |
| `DateTimeOffset` | `TEXT` | ISO 8601 round-trip (`"O"`) |
| `byte[]` | `BLOB` | |

Non-nullable CLR value types get `NOT NULL`; reference types and `Nullable<T>` are nullable columns. Columns added later via `ALTER TABLE` must be nullable or have a `DEFAULT` so existing rows stay valid. For non-nullable value types, use the type's default (`0`, `''`, etc.).

Register Dapper `SqlMapper.TypeHandler`s for `Guid`, `DateTime`, `DateTimeOffset`, and `decimal` so values round-trip exactly. Register them once, in a thread-safe static initializer.

## Schema evolution

`EnsureTable<T>()`:

1. `CREATE TABLE IF NOT EXISTS` from the class.
2. Read existing columns with `PRAGMA table_info([Table])`.
3. `ALTER TABLE ... ADD COLUMN` for each property with no matching column (case-insensitive match).

Rules:

- **Additive only.** Never drop, rename, or change the type of a column. Extra columns in the table that no longer exist on the class are left alone and ignored on read.
- Run steps 1–3 inside a transaction.
- Cache which types have been ensured in this store instance so repeated calls are cheap.
- CRUD methods call `EnsureTable<T>()` automatically on first use of a type. Callers can also call it explicitly at startup.

## Public API

Single entry point, `SqliteStore`, implementing `IDisposable`.

```csharp
public sealed class SqliteStore : IDisposable
{
    // Path to a .db file. Creates the directory and file if missing.
    public SqliteStore(string databasePath);

    // Convenience: %AppData%/{appName}/{fileName}
    public static SqliteStore ForApp(string appName, string fileName = "data.db");

    void EnsureTable<T>();

    void Insert<T>(T item);                       // assigns generated Id back to item
    void InsertMany<T>(IEnumerable<T> items);     // single transaction
    bool Update<T>(T item);                       // by key; returns false if no row matched
    void Upsert<T>(T item);                       // INSERT ... ON CONFLICT(key) DO UPDATE
    bool Delete<T>(object id);
    int  DeleteAll<T>();

    T? Get<T>(object id);
    IReadOnlyList<T> All<T>();

    // Escape hatch: raw SQL with parameters, results mapped by Dapper
    IReadOnlyList<T> Query<T>(string sql, object? parameters = null);
    int Execute(string sql, object? parameters = null);

    // Run several operations atomically
    void InTransaction(Action<SqliteStore> work);
}
```

Also provide async counterparts (`InsertAsync`, `GetAsync`, `AllAsync`, `QueryAsync`, etc.) with `CancellationToken` parameters.

Implementation notes:

- Keep one open `SqliteConnection` per store instance. Enable `PRAGMA journal_mode=WAL;` and `PRAGMA foreign_keys=ON;` on open.
- Guard the connection with a lock (or `SemaphoreSlim` for async) so a single store is safe to use from multiple threads.
- `InTransaction` must make nested store calls enlist in the same transaction rather than opening a new one.
- Build and cache per-type metadata (`EntityInfo`: table name, key property, column list, and pre-built INSERT/UPDATE/UPSERT/SELECT SQL) in a `ConcurrentDictionary<Type, EntityInfo>`. Do reflection once per type.

## Attributes

All in namespace `LibSqlLite`:

- `[Table("Name")]`: class-level, overrides table name
- `[Ignore]`: property-level, excludes from storage
- `[PrimaryKey]`: property-level, marks the key when it isn't named `Id`
- `[Unique]` / `[Unique(IgnoreCase = true)]`: property-level, creates `UX_{Table}_{Column}` via `CREATE UNIQUE INDEX IF NOT EXISTS` (with `COLLATE NOCASE` when `IgnoreCase`). Ignored on the key. Created in `EnsureTable` after columns are added, so it also applies to existing tables; it's never dropped or altered afterwards.

Keep attributes as the only customization mechanism in v1. No fluent configuration.

## Tests (required)

Cover at least:

- Table creation from a class with every supported type; verify column types via `PRAGMA table_info`
- Adding a property to a class (use two classes sharing a `[Table]` name) adds the column and preserves existing rows
- Round-trip of every supported type, including `null` for nullable ones, `decimal` precision, `DateTime` UTC, `Guid`, enums
- `int`, `long`, `Guid`, and `string` keys; generated Id is written back on insert
- `[Ignore]`, `[Table]`, `[PrimaryKey]`
- `Update` / `Delete` return `false` when no row matches
- `Upsert` inserts then updates
- `InTransaction` rolls back everything when the action throws
- A class with no key throws a clear error
- Values containing quotes and SQL-looking text are stored verbatim (parameterization check)

## Non-goals for v1

Do not build these unless asked: relationships/navigation properties, LINQ-to-SQL expression translation, column renames/drops or versioned migrations, encryption, indexes (beyond the primary key and `[Unique]`), non-SQLite databases.

## Coding standards

- Public API has XML doc comments.
- Throw meaningful exceptions with the type/property name in the message; don't swallow exceptions.
- No static mutable state except the Dapper type-handler registration and the metadata cache.
- Keep files small and focused; prefer clarity over cleverness.
- Run `dotnet build` and `dotnet test` and ensure both pass before considering a task done.
