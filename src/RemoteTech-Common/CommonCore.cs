using KSP.UI.Screens;
using RemoteTech.Common.UI;
using RemoteTech.Common.Utils;
using UnityEngine;

namespace RemoteTech.Common
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class CommonCoreMainMenu : CommonCore
    {
        public new void Start()
        {
            base.Start();
        }
    }

    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class CommonCoreSpaceCenter : CommonCore
    {
        public new void Start()
        {
            base.Start();
            this.launcherWindow = new LauncherWindow();
            LauncherButton = ApplicationLauncher.Instance.AddModApplication(
                launcherWindow.launch, launcherWindow.dismiss, null, null, null, null,
                ApplicationLauncher.AppScenes.SPACECENTER,
                AppLauncherTexture);
        }
    }

    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class CommonCoreFlight : CommonCore
    {
        public new void Start()
        {
            base.Start();
            this.launcherWindow = new LauncherWindow();
            LauncherButton = ApplicationLauncher.Instance.AddModApplication(
                launcherWindow.launch, launcherWindow.dismiss, null, null, null, null,
                ApplicationLauncher.AppScenes.FLIGHT,
                AppLauncherTexture);
        }
    }

    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class CommonCoreTrackingStation : CommonCore
    {
        public new void Start()
        {
            base.Start();
            this.launcherWindow = new LauncherWindow();
            LauncherButton = ApplicationLauncher.Instance.AddModApplication(
                launcherWindow.launch, launcherWindow.dismiss, null, null, null, null,
                ApplicationLauncher.AppScenes.TRACKSTATION,
                AppLauncherTexture);
        }
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class CommonCoreEditor : CommonCore
    {
        public new void Start()
        {
            base.Start();
        }
    }

    public abstract class CommonCore: MonoBehaviour
    {
        /// <summary>Button for KSP Stock Tool bar</summary>
        public static ApplicationLauncherButton LauncherButton = null;
        /// <summary>Texture for the KSP Stock Tool-bar Button</summary>
        protected static readonly Texture2D AppLauncherTexture = UiUtils.LoadTexture("RTLauncher");
        /// <summary></summary>
        protected LauncherWindow launcherWindow;

        public void Start()
        {
            Logging.Debug($"RemoteTech-Common Starting. Scene: {HighLogic.LoadedScene}");
        }

        public void OnDestroy()
        {
            if (LauncherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(LauncherButton);
            }
        }
    }
}
