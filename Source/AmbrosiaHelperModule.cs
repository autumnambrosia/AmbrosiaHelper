using System;
using Celeste.Mod.AmbrosiaHelper.Entities;

namespace Celeste.Mod.AmbrosiaHelper;

public class AmbrosiaHelperModule : EverestModule {
    public static AmbrosiaHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(AmbrosiaHelperModuleSettings);
    public static AmbrosiaHelperModuleSettings Settings => (AmbrosiaHelperModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(AmbrosiaHelperModuleSession);
    public static AmbrosiaHelperModuleSession Session => (AmbrosiaHelperModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(AmbrosiaHelperModuleSaveData);
    public static AmbrosiaHelperModuleSaveData SaveData => (AmbrosiaHelperModuleSaveData) Instance._SaveData;

    public AmbrosiaHelperModule() {
        Instance = this;
#if DEBUG
        Logger.SetLogLevel("AmbrosiaHelper", LogLevel.Verbose);
#else
        Logger.SetLogLevel("AmbrosiaHelper", LogLevel.Info);
#endif
    }

    public override void Load() {
        //ForceMoveField.Load();
        BoostHoney.Load();
        ReformIndicator.Load();
    }

    public override void Unload() {
        //ForceMoveField.Unload();
        BoostHoney.Unload();
        ReformIndicator.Unload();
    }
}