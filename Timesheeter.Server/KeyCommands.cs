namespace Timesheeter.Server
{
    /// <summary>
    /// Managing API keys from the command line, instead of starting the server:
    /// <code>
    /// Timesheeter.Server keys list
    /// Timesheeter.Server keys add &lt;name&gt;
    /// Timesheeter.Server keys revoke &lt;name&gt;
    /// </code>
    /// </summary>
    internal static class KeyCommands
    {
        /// <summary>True if <paramref name="args"/> was a keys command (it has then been run).</summary>
        public static bool TryRun(string[] args, ApiKeys keys)
        {
            if (args.Length == 0 || !args[0].Equals("keys", StringComparison.OrdinalIgnoreCase)) return false;

            string command = args.Length > 1 ? args[1].ToLowerInvariant() : "list";
            string name = string.Join(' ', args.Skip(2)).Trim();

            switch (command)
            {
                case "list":
                    var all = keys.All();
                    if (all.Count == 0) Console.WriteLine("No API keys yet.");
                    foreach (var key in all.OrderBy(k => k.Created))
                    {
                        string state = key.Revoked is { } revoked ? $"revoked {revoked:yyyy-MM-dd HH:mm} UTC" : "active";
                        Console.WriteLine($"  {key.Name,-20} created {key.Created:yyyy-MM-dd HH:mm} UTC   {state}");
                    }
                    break;

                case "add" when name.Length > 0:
                    if (keys.All().Any(k => string.Equals(k.Name, name, StringComparison.OrdinalIgnoreCase)))
                    {
                        Console.Error.WriteLine($"A key named '{name}' already exists.");
                        Environment.ExitCode = 1;
                        break;
                    }
                    PrintNewKey(name, keys.Create(name));
                    break;

                case "revoke" when name.Length > 0:
                    if (keys.Revoke(name)) Console.WriteLine($"Revoked '{name}'. It stops working immediately (restart not needed).");
                    else
                    {
                        Console.Error.WriteLine($"No active key named '{name}'.");
                        Environment.ExitCode = 1;
                    }
                    break;

                default:
                    Console.WriteLine("Usage: Timesheeter.Server keys [list | add <name> | revoke <name>]");
                    Environment.ExitCode = 1;
                    break;
            }
            return true;
        }

        public static void PrintNewKey(string name, string key)
        {
            Console.WriteLine();
            Console.WriteLine($"API key '{name}' (shown only once; the server keeps just a hash of it):");
            Console.WriteLine();
            Console.WriteLine($"    {key}");
            Console.WriteLine();
            Console.WriteLine($"Send it with every request in the {ApiKeys.HeaderName} header, or paste it into");
            Console.WriteLine("Timesheeter's Settings page. Lost it? Revoke it and add a new one.");
            Console.WriteLine();
        }
    }
}
