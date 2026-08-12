using AliceInCradleHack.command;
using AliceInCradleHack.module;
using System;

namespace LeetName
{
    /// <summary>
    /// Console command to control the animated leet name without touching the config file.
    /// </summary>
    public sealed class LeetNameCommand : Command
    {
        public override string Name => "leetname";

        public override string Description => "Controls the animated leet/split multiplayer name.";

        public override string Usage =>
            "leetname\n" +
            "leetname <name>              - set the base name to animate (e.g. ASM / asm)\n" +
            "leetname interval <ms>       - set milliseconds per animation frame\n" +
            "leetname hold <ms>           - how long the full name stays visible (0 = no hold)\n" +
            "leetname leet <on|off>       - toggle leet forms for middle characters";

        public override void Execute(string[] args)
        {
            var module = ModuleManager.Instance.GetModuleByName("LeetName") as LeetNameModule;
            if (module == null)
            {
                Console.WriteLine("LeetName module not loaded.");
                return;
            }

            if (args.Length == 0)
            {
                Console.WriteLine($"Name: {module.DisplayName.Get()} | FrameMs: {module.FrameMs.Get()} | HoldMs: {module.HoldMs.Get()} | Leet: {module.UseLeet.Get()} | Enabled: {module.IsEnabled}");
                return;
            }

            if (args[0].Equals("interval", StringComparison.OrdinalIgnoreCase) && args.Length >= 2)
            {
                if (int.TryParse(args[1], out int ms))
                {
                    module.FrameMs.Set(Math.Max(10, ms));
                    ModuleManager.Instance.StoreModuleConfig(module);
                    Console.WriteLine($"FrameMs set to {module.FrameMs.Get()}.");
                }
                else
                {
                    Console.WriteLine("Invalid interval, expected a number of milliseconds.");
                }
                return;
            }

            if (args[0].Equals("hold", StringComparison.OrdinalIgnoreCase) && args.Length >= 2)
            {
                if (int.TryParse(args[1], out int ms))
                {
                    module.HoldMs.Set(Math.Max(0, ms));
                    ModuleManager.Instance.StoreModuleConfig(module);
                    Console.WriteLine($"HoldMs set to {module.HoldMs.Get()}.");
                }
                else
                {
                    Console.WriteLine("Invalid hold, expected a number of milliseconds.");
                }
                return;
            }

            if (args[0].Equals("leet", StringComparison.OrdinalIgnoreCase) && args.Length >= 2)
            {
                bool value = args[1].Equals("on", StringComparison.OrdinalIgnoreCase)
                             || args[1].Equals("1", StringComparison.OrdinalIgnoreCase)
                             || args[1].Equals("true", StringComparison.OrdinalIgnoreCase);
                module.UseLeet.Set(value);
                ModuleManager.Instance.StoreModuleConfig(module);
                Console.WriteLine($"UseLeet set to {module.UseLeet.Get()}.");
                return;
            }

            string name = string.Join(" ", args).Trim();
            module.DisplayName.Set(name);
            ModuleManager.Instance.StoreModuleConfig(module);
            Console.WriteLine($"Animated name set to '{name}'.");
        }
    }
}
