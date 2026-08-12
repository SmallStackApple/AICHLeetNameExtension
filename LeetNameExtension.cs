using AliceInCradleHack.command;
using AliceInCradleHack.extension;
using AliceInCradleHack.module;
using AliceInCradleHack.utils.client;

namespace LeetName
{
    /// <summary>
    /// Extension entry point. Loaded by <see cref="ExtensionManager"/> from its own
    /// <c>&lt;mainFolder&gt;\Extensions\LeetName</c> folder. Registers the <see cref="LeetNameModule"/>
    /// and the <c>leetname</c> command, and re-applies the module's persisted enabled state.
    /// </summary>
    public sealed class LeetNameExtension : Extension
    {
        private readonly LeetNameModule _module = new();
        private readonly LeetNameCommand _command = new();

        public override string Name => "LeetName";

        public override string Description => "Animated leet/split name for the Kaleidoscopic multiplayer display.";

        public override void Initialize()
        {
            Log.Info($"[{Name}] initializing...");

            ModuleManager.Instance.RegisterModule(_module);
            if (ModuleManager.Instance.GetModuleByName(_module.Name) == null)
            {
                Log.Error($"[{Name}] failed to register module '{_module.Name}'.");
                return;
            }

            CommandManager.Instance.RegisterCommand(_command);

            // Re-apply the persisted enabled state for this late-registered module
            // (built-in modules are re-applied by ModuleManager during its own Initialize).
            if (!_module.IsEnabled)
                ModuleManager.Instance.EnableModule(_module.Name);

            Log.Info($"[{Name}] registered module '{_module.Name}' and command '{_command.Name}'.");
        }

        public override void Dispose()
        {
            CommandManager.Instance.UnregisterCommand(_command.Name);
            ModuleManager.Instance.UnregisterModule(_module.Name);
            Log.Info($"[{Name}] unregistered command '{_command.Name}' and module '{_module.Name}'.");
        }
    }
}
