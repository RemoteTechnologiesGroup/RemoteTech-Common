using KSP.UI.Screens;
using RemoteTech.Common.UI;
using RemoteTech.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RemoteTech.Common
{
    /// <summary>
    /// Abstract RemoteTech Core class to be implemented and executed across KSP scenes
    /// </summary>
    public abstract class CommonCore : MonoBehaviour
    {
        /// <summary>
        /// Main class instance.
        /// </summary>
        public static CommonCore Instance { get; protected set; }

        /*
         * Application Launcher
         */
        /// <summary>Application Launcher Window</summary>
        protected LauncherWindow launcherWindow;
        /// <summary>Button for KSP Stock Tool bar</summary>
        protected ApplicationLauncherButton LauncherButton;
        /// <summary>Texture for the KSP Stock Tool-bar Button</summary>
        protected static readonly Texture2D AppLauncherTexture = UiUtils.LoadTexture("RTLauncher");

        /*
         * Events
         */
        /// <summary>
        /// Methods can register to this event to be called during the Update() method of the Unity engine (Game Logic engine phase).
        /// </summary>
        public event Action OnFrameUpdate = delegate { };
        /// <summary>
        /// Methods can register to this event to be called during the FixedUpdate() method of the Unity engine (Physics engine phase).
        /// </summary>
        public event Action OnPhysicsUpdate = delegate { };
        /// <summary>
        /// Methods can register to this event to be called during the OnGUI() method of the Unity engine (GUI Rendering engine phase).
        /// </summary>
        public event Action OnUnityGUIUpdate = delegate { };

        /*
         * Misc
         */
        protected bool GUIVisible = true;

        /// <summary>
        /// Called by Unity engine during initialization phase.
        /// Only ever called once.
        /// </summary>
        public void Start()
        {
            // Destroy the Core instance if != null or if RemoteTech is disabled
            if (Instance != null)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            Logging.Debug($"RemoteTech-Common Starting. Scene: {HighLogic.LoadedScene}");

            // Handling new F2 GUI Hiding
            GameEvents.onShowUI.Add(UIOn);
            GameEvents.onHideUI.Add(UIOff);
        }

        /// <summary>
        /// Called by the Unity engine during the Decommissioning phase of the Engine.
        /// This is used to clean up everything before quiting.
        /// </summary>
        public void OnDestroy()
        {
            Logging.Debug($"RemoteTech-Common Destroying. Scene: {HighLogic.LoadedScene}");

            if (LauncherButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(LauncherButton);
            }

            // Release all RT locks currently engaged
            ReleaseLocks();

            // Remove GUI stuff
            GameEvents.onShowUI.Remove(UIOn);
            GameEvents.onHideUI.Remove(UIOff);

            Instance = null;
        }

        /// <summary>
        /// Called by the Unity engine during the GUI rendering phase.
        /// Note that OnGUI() is called multiple times per frame in response to GUI events.
        /// The Layout and Repaint events are processed first, followed by a Layout and keyboard/mouse event for each input event.
        /// </summary>
        public void OnGUI()
        {
            if(!GUIVisible) { return; }

            GUI.depth = 0;
            OnUnityGUIUpdate.Invoke();
        }

        /// <summary>
        /// Called by the Unity engine during the game logic phase.
        /// This function is called once per frame. It is the main workhorse function for frame updates.
        /// </summary>
        public void Update()
        {
            OnFrameUpdate.Invoke();
        }

        /// <summary>
        /// Called by the Unity engine during the Physics phase.
        /// Note that FixedUpdate() is called before the internal engine physics update. This function is often called more frequently than Update().
        /// </summary>
        public void FixedUpdate()
        {
            OnPhysicsUpdate.Invoke();
        }

        /// <summary>
        /// Prevent duplicate calls for the OnFrameUpdate event.
        /// </summary>
        /// <param name="action">The action to be added to the OnFrameUpdate event.</param>
        public void AddOnceOnFrameUpdate(Action action)
        {
            if (!Instance.OnFrameUpdate.GetInvocationList().Contains(action))
                Instance.OnFrameUpdate += action;
        }

        /// <summary>
        /// F2 GUI Hiding functionality; called when the UI must be displayed.
        /// </summary>
        public void UIOn()
        {
            GUIVisible = true;
        }

        /// <summary>
        /// F2 GUI Hiding functionality; called when the UI must be hidden.
        /// </summary>
        public void UIOff()
        {
            GUIVisible = false;
        }

        /// <summary>
        /// Release RemoteTech UI locks (enable usage of UI buttons).
        /// </summary>
        private static void ReleaseLocks()
        {
            InputLockManager.RemoveControlLock("RTLockStaging");
            InputLockManager.RemoveControlLock("RTLockSAS");
            InputLockManager.RemoveControlLock("RTLockRCS");
            InputLockManager.RemoveControlLock("RTLockActions");
        }

        /// <summary>
        /// Acquire RemoteTech UI locks (disable usage of UI buttons).
        /// </summary>
        private static void EngageLocks()
        {
            InputLockManager.SetControlLock(ControlTypes.STAGING, "RTLockStaging");
            InputLockManager.SetControlLock(ControlTypes.SAS, "RTLockSAS");
            InputLockManager.SetControlLock(ControlTypes.RCS, "RTLockRCS");
            InputLockManager.SetControlLock(ControlTypes.GROUPS_ALL, "RTLockActions");
        }

        // Monstrosity that should fix the kOS control locks without modifications on their end.
        private static IEnumerable<KSPActionGroup> GetActivatedGroup()
        {
            //TODO: Replace Linq with non-linq way
            if (GameSettings.LAUNCH_STAGES.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.STAGING) == ControlTypes.STAGING && !l.Key.Equals("RTLockStaging")))
                    yield return KSPActionGroup.Stage;
            if (GameSettings.AbortActionGroup.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.GROUP_ABORT) == ControlTypes.GROUP_ABORT && !l.Key.Equals("RTLockActions")))
                    yield return KSPActionGroup.Abort;
            if (GameSettings.RCS_TOGGLE.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.RCS) == ControlTypes.RCS && !l.Key.Equals("RTLockRCS")))
                    yield return KSPActionGroup.RCS;
            if (GameSettings.SAS_TOGGLE.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.SAS) == ControlTypes.SAS && !l.Key.Equals("RTLockSAS")))
                    yield return KSPActionGroup.SAS;
            if (GameSettings.SAS_HOLD.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.SAS) == ControlTypes.SAS && !l.Key.Equals("RTLockSAS")))
                    yield return KSPActionGroup.SAS;
            if (GameSettings.SAS_HOLD.GetKeyUp())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.SAS) == ControlTypes.SAS && !l.Key.Equals("RTLockSAS")))
                    yield return KSPActionGroup.SAS;
            if (GameSettings.BRAKES.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.GROUP_BRAKES) == ControlTypes.GROUP_BRAKES && !l.Key.Equals("RTLockActions")))
                    yield return KSPActionGroup.Brakes;
            if (GameSettings.LANDING_GEAR.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.GROUP_GEARS) == ControlTypes.GROUP_GEARS && !l.Key.Equals("RTLockActions")))
                    yield return KSPActionGroup.Gear;
            if (GameSettings.HEADLIGHT_TOGGLE.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.GROUP_LIGHTS) == ControlTypes.GROUP_LIGHTS && !l.Key.Equals("RTLockActions")))
                    yield return KSPActionGroup.Light;
            if (GameSettings.CustomActionGroup1.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.CUSTOM_ACTION_GROUPS) == ControlTypes.CUSTOM_ACTION_GROUPS && !l.Key.Equals("RTLockActions")))
                    yield return KSPActionGroup.Custom01;
            if (GameSettings.CustomActionGroup2.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.CUSTOM_ACTION_GROUPS) == ControlTypes.CUSTOM_ACTION_GROUPS && !l.Key.Equals("RTLockActions")))
                    yield return KSPActionGroup.Custom02;
            if (GameSettings.CustomActionGroup3.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.CUSTOM_ACTION_GROUPS) == ControlTypes.CUSTOM_ACTION_GROUPS && !l.Key.Equals("RTLockActions")))
                    yield return KSPActionGroup.Custom03;
            if (GameSettings.CustomActionGroup4.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.CUSTOM_ACTION_GROUPS) == ControlTypes.CUSTOM_ACTION_GROUPS && !l.Key.Equals("RTLockActions")))
                    yield return KSPActionGroup.Custom04;
            if (GameSettings.CustomActionGroup5.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.CUSTOM_ACTION_GROUPS) == ControlTypes.CUSTOM_ACTION_GROUPS && !l.Key.Equals("RTLockActions")))
                    yield return KSPActionGroup.Custom05;
            if (GameSettings.CustomActionGroup6.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.CUSTOM_ACTION_GROUPS) == ControlTypes.CUSTOM_ACTION_GROUPS && !l.Key.Equals("RTLockActions")))
                    yield return KSPActionGroup.Custom06;
            if (GameSettings.CustomActionGroup7.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.CUSTOM_ACTION_GROUPS) == ControlTypes.CUSTOM_ACTION_GROUPS && !l.Key.Equals("RTLockActions")))
                    yield return KSPActionGroup.Custom07;
            if (GameSettings.CustomActionGroup8.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.CUSTOM_ACTION_GROUPS) == ControlTypes.CUSTOM_ACTION_GROUPS && !l.Key.Equals("RTLockActions")))
                    yield return KSPActionGroup.Custom08;
            if (GameSettings.CustomActionGroup9.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.CUSTOM_ACTION_GROUPS) == ControlTypes.CUSTOM_ACTION_GROUPS && !l.Key.Equals("RTLockActions")))
                    yield return KSPActionGroup.Custom09;
            if (GameSettings.CustomActionGroup10.GetKeyDown())
                if (!InputLockManager.lockStack.Any(l => ((ControlTypes)l.Value & ControlTypes.CUSTOM_ACTION_GROUPS) == ControlTypes.CUSTOM_ACTION_GROUPS && !l.Key.Equals("RTLockActions")))
                    yield return KSPActionGroup.Custom10;
        }
    }

    /// -----------------------------------------------------
    /// Implementations of abstract CommonCore class
    /// -----------------------------------------------------
    
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class CommonCoreSpaceCenter : CommonCore
    {
        public new void Start()
        {
            base.Start();
            if (Instance == null) { return; }

            launcherWindow = new LauncherWindow();
            LauncherButton = ApplicationLauncher.Instance.AddModApplication(
                launcherWindow.launch, launcherWindow.Dismiss, null, null, null, null,
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
            if (Instance == null) { return; }

            launcherWindow = new LauncherWindow();
            LauncherButton = ApplicationLauncher.Instance.AddModApplication(
                launcherWindow.launch, launcherWindow.Dismiss, null, null, null, null,
                ApplicationLauncher.AppScenes.FLIGHT,
                AppLauncherTexture);
        }

        public new void OnDestroy()
        {
            if (Instance != null)
            {
            }

            base.OnDestroy();
        }
    }

    [KSPAddon(KSPAddon.Startup.TrackingStation, false)]
    public class CommonCoreTrackingStation : CommonCore
    {
        private FilterOverlay FilterBar;

        public new void Start()
        {
            base.Start();
            if (Instance == null) { return; }

            //Application Launcher
            launcherWindow = new LauncherWindow();
            LauncherButton = ApplicationLauncher.Instance.AddModApplication(
                launcherWindow.launch, launcherWindow.Dismiss, null, null, null, null,
                ApplicationLauncher.AppScenes.TRACKSTATION,
                AppLauncherTexture);

            //Filter Bar
            FilterBar = new FilterOverlay();
            FilterBar.OnEnterMapView();
        }

        public new void OnDestroy()
        {
            if (Instance != null)
            {
                if (FilterBar != null)
                {
                    FilterBar.OnExitMapView();
                    FilterBar.Dispose();
                }
            }

            base.OnDestroy();
        }
    }

    [KSPAddon(KSPAddon.Startup.EditorAny, false)]
    public class CommonCoreEditor : CommonCore
    {
    }
}
