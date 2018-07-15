using RemoteTech.Common.Interfaces;
using RemoteTech.Common.RemoteTechCommNet;
using RemoteTech.Common.Utils;
using UnityEngine;
using static RemoteTech.Common.RemoteTechCommNet.RemoteTechCommNetUI;

namespace RemoteTech.Common.UI
{
    /// <summary>
    /// Class used for RemoteTech buttons overlay in Tracking Station scene.
    /// Draws and handles buttons on the bottom left of the scene.
    /// </summary>
    public class FilterOverlay: IUnityWindow
    {
        private bool mShowOverlay = true;
        private bool onTrackingStation { get { return (HighLogic.LoadedScene == GameScenes.TRACKSTATION); } }
        private static UnityEngine.UI.Image mTrackingSatVesselSideImg = null;

        private static GUIStyle BackgroundButtonStyle;

        private readonly short numButtons = 4;

        public FilterOverlay()
        {
            //loading styles
            BackgroundButtonStyle = GUITextureButtonFactory.CreateFromFilename("BackgroundButton");

            GameEvents.onPlanetariumTargetChanged.Add(OnChangeTarget);
            GameEvents.onHideUI.Add(OnHideUI);
            GameEvents.onShowUI.Add(OnShowUI);
            MapView.OnEnterMapView += OnEnterMapView;
            MapView.OnExitMapView += OnExitMapView;

            // Add the on mouse over event
            //mAntennaFragment.onMouseOverListEntry += showTargetInfo;

            // Create a new Targetinfo window with a fixed position to the antenna fragment
            //mTargetInfos = new TargetInfoWindow(PositionAntenna, targetInfoAlign);
        }

        private Rect Position
        {
            get
            {
                //default at bottom right corner
                float posX = Screen.width - BackgroundButtonStyle.normal.background.width * numButtons * GameSettings.UI_SCALE;
                float posY = Screen.height - BackgroundButtonStyle.normal.background.height * GameSettings.UI_SCALE;

                if (onTrackingStation)
                {
                    if (mTrackingSatVesselSideImg == null)
                    {
                        mTrackingSatVesselSideImg = GameObject.Find("Side Bar").GetChild("bg (stretch)").GetComponent<UnityEngine.UI.Image>();
                    }

                    //move to bottom left corner
                    posX = mTrackingSatVesselSideImg.rectTransform.rect.width * GameSettings.UI_SCALE;
                }

                return new Rect(posX, 
                                posY, 
                                BackgroundButtonStyle.normal.background.width * numButtons * GameSettings.UI_SCALE, 
                                BackgroundButtonStyle.normal.background.height * GameSettings.UI_SCALE);
            }
        }

        private Texture2D DishReachTexture
        {
            get
            {
                RemoteTechMapFilter mask = RemoteTechCommNetUI.RTMapFilter;
                return ((mask & RemoteTechMapFilter.DishCone) == RemoteTechMapFilter.DishCone) ? RemoteTechTelemetryUpdate.Textures.ConeTxt : RemoteTechTelemetryUpdate.Textures.TempTxt;
            }
        }

        private Texture2D DishTargetTexture
        {
            get
            {
                return RemoteTechTelemetryUpdate.Textures.DishTargetTxt;
            }
        }

        private Texture2D LineTypeTexture
        {
            get
            {
                RemoteTechMapFilter mask = RemoteTechCommNetUI.RTMapFilter;
                if ((mask & (RemoteTechMapFilter.OmniLine | RemoteTechMapFilter.DishLine)) == (RemoteTechMapFilter.OmniLine | RemoteTechMapFilter.DishLine))
                {
                    return RemoteTechTelemetryUpdate.Textures.OmniDishTxt;
                }
                else if ((mask & RemoteTechMapFilter.OmniLine) == RemoteTechMapFilter.OmniLine)
                {
                    return RemoteTechTelemetryUpdate.Textures.TempTxt;
                }
                else if ((mask & RemoteTechMapFilter.DishLine) == RemoteTechMapFilter.DishLine)
                {
                    return RemoteTechTelemetryUpdate.Textures.TempTxt;
                }
                else //off
                {
                    return RemoteTechTelemetryUpdate.Textures.TempTxt;
                }
            }
        }

        private Texture2D VisualRangeTexture
        {
            get
            {
                RemoteTechMapFilter mask = RemoteTechCommNetUI.RTMapFilter;
                return ((mask & RemoteTechMapFilter.VisualRange) == RemoteTechMapFilter.VisualRange) ? RemoteTechTelemetryUpdate.Textures.VisualRangeTxt : RemoteTechTelemetryUpdate.Textures.TempTxt;
            }
        }

        private void OnHideUI()
        {
            mShowOverlay = false;
        }

        private void OnShowUI()
        {
            mShowOverlay = true;
        }

        public void Dispose()
        {
            // Remove the on mouse over event
            //mAntennaFragment.onMouseOverListEntry -= showTargetInfo;

            GameEvents.onPlanetariumTargetChanged.Remove(OnChangeTarget);
            GameEvents.onHideUI.Remove(OnHideUI);
            GameEvents.onShowUI.Remove(OnShowUI);
            MapView.OnEnterMapView -= OnEnterMapView;
            MapView.OnExitMapView -= OnExitMapView;

            mShowOverlay = false;

            //mAntennaFragment.Dispose();
        }

        public void OnEnterMapView()
        {
            CommonCore.Instance.OnUnityGUIUpdate += Draw;
            CommonCore.Instance.OnFrameUpdate += Update;
        }

        public void OnExitMapView()
        {
            CommonCore.Instance.OnUnityGUIUpdate -= Draw;
            CommonCore.Instance.OnFrameUpdate -= Update;
        }

        private void OnChangeTarget(MapObject mo)
        {

        }

        public void Update()
        {
            
        }

        public void Draw()
        {
            if (!mShowOverlay) { return; }
            GUI.depth = 0;
            GUI.skin = HighLogic.Skin;

            // Draw Toolbar
            GUILayout.BeginArea(Position);
            {
                GUILayout.BeginHorizontal();
                {
                    if (this.onTrackingStation)
                    {
                        if (GUILayout.Button(DishTargetTexture, BackgroundButtonStyle, GUILayout.Width(RemoteTechTelemetryUpdate.Textures.DishTargetTxt.width * GameSettings.UI_SCALE), GUILayout.Height(RemoteTechTelemetryUpdate.Textures.DishTargetTxt.height * GameSettings.UI_SCALE)))
                        { RemoteTechTelemetryUpdate.OnClickDishTarget(); }
                        if (GUILayout.Button(LineTypeTexture, BackgroundButtonStyle, GUILayout.Width(RemoteTechTelemetryUpdate.Textures.OmniDishTxt.width * GameSettings.UI_SCALE), GUILayout.Height(RemoteTechTelemetryUpdate.Textures.OmniDishTxt.height * GameSettings.UI_SCALE)))
                        { RemoteTechTelemetryUpdate.OnClickLineType(); }
                        if (GUILayout.Button(DishReachTexture, BackgroundButtonStyle, GUILayout.Width(RemoteTechTelemetryUpdate.Textures.ConeTxt.width * GameSettings.UI_SCALE), GUILayout.Height(RemoteTechTelemetryUpdate.Textures.ConeTxt.height * GameSettings.UI_SCALE)))
                        { RemoteTechTelemetryUpdate.OnClickDishReach(); }
                        if (GUILayout.Button(VisualRangeTexture, BackgroundButtonStyle, GUILayout.Width(RemoteTechTelemetryUpdate.Textures.VisualRangeTxt.width * GameSettings.UI_SCALE), GUILayout.Height(RemoteTechTelemetryUpdate.Textures.VisualRangeTxt.height * GameSettings.UI_SCALE)))
                        { RemoteTechTelemetryUpdate.OnClickVisualRange(); }
                    }
                    else
                    {
                        /* 
                        //buttons are provided in CommNet telemetry bar
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(TextureDishReachButton, BackgroundButtonStyle, GUILayout.Width(Cone.width * GameSettings.UI_SCALE), GUILayout.Height(Cone.height * GameSettings.UI_SCALE)))
                            OnClickReach();
                        if (GUILayout.Button(TextureLineTypeButton, BackgroundButtonStyle, GUILayout.Width(OmniDish.width * GameSettings.UI_SCALE), GUILayout.Height(OmniDish.height * GameSettings.UI_SCALE)))
                            OnClickType();
                        if (GUILayout.Button(TextureLineTypeButton2, BackgroundButtonStyle, GUILayout.Width(DishTargetButton.width * GameSettings.UI_SCALE), GUILayout.Height(DishTargetButton.height * GameSettings.UI_SCALE)))
                            OnClickStatus();
                        */
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        } 
    }
}
