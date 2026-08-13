using AliceInCradleHack.extension;
using AliceInCradleHack.module;
using AliceInCradleHack.utils.client;

namespace AnimatedPlayerName
{
    /// <summary>
    /// Extension entry point. Loaded by <see cref="ExtensionManager"/> from its own
    /// <c>&lt;mainFolder&gt;\Extensions\AnimatedPlayerName</c> folder. Registers the
    /// <see cref="AnimatedPlayerNameModule"/> and re-applies its persisted enabled state.
    /// </summary>
    public sealed class AnimatedPlayerNameExtension : Extension
    {
        private readonly AnimatedPlayerNameModule _module = new();

        public override string Name => "AnimatedPlayerName";

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

            // Re-apply the persisted enabled state for this late-registered module
            // (built-in modules are re-applied by ModuleManager during its own Initialize).
            if (!_module.IsEnabled)
                ModuleManager.Instance.EnableModule(_module.Name);
        }

        public override void Dispose()
        {
            ModuleManager.Instance.UnregisterModule(_module.Name);
        }
    }
}
