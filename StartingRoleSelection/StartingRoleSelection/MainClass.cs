using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LabApi.Events.CustomHandlers;
using LabApi.Features;
using LabApi.Loader;
using LabApi.Loader.Features.Plugins;

namespace StartingRoleSelection
{
    public class MainClass : Plugin<Config>
    {
        public override void LoadConfigs()
        {
            pluginTranslation = this.LoadConfig<Translation>("translation.yml");
            base.LoadConfigs();
        }

        public override void Enable()
        {
            Instance = this;
            pluginConfig = Config;
            Events = new();
            CustomHandlersManager.RegisterEventsHandler(Events);
        }

        public override void Disable()
        {
            CustomHandlersManager.UnregisterEventsHandler(Events);
            Events = null;
            pluginConfig = null;
            Instance = null;
        }

        public Config pluginConfig;
        public Translation pluginTranslation;

        public EventHandler Events { get; private set; }
        public static MainClass Instance { get; private set; }

        public override string Name { get; } = "StartingRoleSelection";
        public override string Description { get; } = null;
        public override string Author { get; } = "Phineapple18";
        public override Version Version { get; } = new(1, 0, 0);
        public override Version RequiredApiVersion { get; } = new(LabApiProperties.CompiledVersion);
    }
}
