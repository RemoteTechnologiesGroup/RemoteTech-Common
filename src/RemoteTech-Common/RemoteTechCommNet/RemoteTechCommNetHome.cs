using CommNet;
using KSP.Localization;
using RemoteTech.Common.Utils;
using System;
using UnityEngine;

namespace RemoteTech.Common.RemoteTechCommNet
{
    /// <summary>
    /// Customise the home nodes
    /// </summary>
    public class RemoteTechCommNetHome : CommNetHome, IComparable<RemoteTechCommNetHome>
    {
        private static readonly Texture2D markTexture = UiUtils.LoadTexture("groundStationMark");
        private static GUIStyle groundStationHeadline;
        private bool loadCompleted = false;

        [Persistent] public string ID;
        [Persistent] public Color Color = RemoteTechCommNetScenario.Instance.GroundStationDotColor;//future expansion of GS
        [Persistent] protected string OptionalName = "";

        public double altitude { get { return this.alt; } }
        public double latitude { get { return this.lat; } }
        public double longitude { get { return this.lon; } }
        public CommNode commNode { get { return this.comm; } }
        public string stationName
        {
            get { return (this.OptionalName.Length == 0) ? this.displaynodeName : this.OptionalName; }
            set { this.OptionalName = value; }
        }

        public void copyOf(CommNetHome stockHome)
        {
            Logging.Info("CommNet Home '{0}' added", stockHome.nodeName);

            this.ID = stockHome.nodeName;
            this.nodeName = stockHome.nodeName;
            this.displaynodeName = Localizer.Format(stockHome.displaynodeName);
            this.nodeTransform = stockHome.nodeTransform;
            this.isKSC = stockHome.isKSC;
            this.body = stockHome.GetComponentInParent<CelestialBody>();

            //comm, lat, alt, lon are initialised by CreateNode() later

            groundStationHeadline = new GUIStyle(HighLogic.Skin.label)
            {
                fontSize = 12,
                normal = { textColor = Color.yellow }
            };

            loadCompleted = true;
        }

        /// <summary>
        /// Draw graphic components on screen like RemoteTech's ground-station marks
        /// </summary>
        public void OnGUI()
        {
            if (GameUtil.IsGameScenario || !loadCompleted)
                return;

            if (!(HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.TRACKSTATION))
                return;

            if ((!HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().enableGroundStations && !this.isKSC) || !MapView.MapIsEnabled || MapView.MapCamera == null)
                return;

            var worldPos = ScaledSpace.LocalToScaledSpace(nodeTransform.transform.position);

            if (MapView.MapCamera.transform.InverseTransformPoint(worldPos).z < 0f || HighLogic.CurrentGame.Parameters.CustomParams<RemoteTechCommonParams>().HideGroundStationsFully)
                return;

            var position = PlanetariumCamera.Camera.WorldToScreenPoint(worldPos);
            var groundStationRect = new Rect((position.x - 8), (Screen.height - position.y) - 8, 16, 16);

            if (HighLogic.CurrentGame.Parameters.CustomParams<RemoteTechCommonParams>().HideGroundStationsBehindBody && IsOccluded(nodeTransform.transform.position, this.body))
                return;

            if (HighLogic.CurrentGame.Parameters.CustomParams<RemoteTechCommonParams>().DistanceToHideGroundStations > 0.1f && !IsOccluded(nodeTransform.transform.position, this.body) && this.IsCamDistanceToWide(nodeTransform.transform.position))
                return;

            //draw the dot
            var previousColor = GUI.color;
            GUI.color = RemoteTechCommNetScenario.Instance.GroundStationDotColor;
            GUI.DrawTexture(groundStationRect, markTexture, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;

            //draw the headline below the dot
            if (HighLogic.CurrentGame.Parameters.CustomParams<RemoteTechCommonParams>().ShowMouseOverInfoGroundStations && UiUtils.ContainsMouse(groundStationRect))
            {
                var headlineRect = groundStationRect;
                var nameDim = RemoteTechCommNetHome.groundStationHeadline.CalcSize(new GUIContent(this.nodeName));
                headlineRect.x -= nameDim.x / 2 - 5;
                headlineRect.y -= nameDim.y + 5;
                headlineRect.width = nameDim.x;
                headlineRect.height = nameDim.y;
                GUI.Label(headlineRect, this.nodeName, RemoteTechCommNetHome.groundStationHeadline);

                /*
                //TODO: display antenna info
                // loop antennas
                String antennaRanges = String.Empty;
                foreach (var antenna in s.Antennas)
                {
                    if(antenna.Omni > 0)
                    {
                        antennaRanges += "Omni: "+ RTUtil.FormatSI(antenna.Omni,"m") + Environment.NewLine;
                    }
                    if (antenna.Dish > 0)
                    {
                        antennaRanges += "Dish: " + RTUtil.FormatSI(antenna.Dish, "m") + Environment.NewLine;
                    }
                }

                if(!antennaRanges.Equals(String.Empty))
                {
                    Rect antennas = screenRect;
                    GUIContent content = new GUIContent(antennaRanges);

                    Vector2 antennaDim = this.smallStationText.CalcSize(content);
                    float maxHeight = this.smallStationText.CalcHeight(content, antennaDim.x);

                    antennas.y += headline.height - 3;
                    antennas.x -= antennaDim.x + 10;
                    antennas.width = antennaDim.x;
                    antennas.height = maxHeight;

                    // draw antenna infos of the station
                    GUI.Label(antennas, antennaRanges, this.smallStationText);
                }
                */
            }
        }

        /// <summary>
        /// Checks whether the location is behind the body
        /// Original code by regex from https://github.com/NathanKell/RealSolarSystem/blob/master/Source/KSCSwitcher.cs
        /// </summary>
        private bool IsOccluded(Vector3d loc, CelestialBody body)
        {
            var camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);

            if (Vector3d.Angle(camPos - loc, body.position - loc) > 90)
                return false;
            return true;
        }
        
        /// <summary>
        /// Calculates the distance between the camera position and the ground station, and
        /// returns true if the distance is >= DistanceToHideGroundStations from the settings file.
        /// </summary>
        /// <param name="loc">Position of the ground station</param>
        /// <returns>True if the distance is to wide, otherwise false</returns>
        private bool IsCamDistanceToWide(Vector3d loc)
        {
            var camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);
            float distance = Vector3.Distance(camPos, loc);

            if (distance >= HighLogic.CurrentGame.Parameters.CustomParams<RemoteTechCommonParams>().DistanceToHideGroundStations)
                return true;
            return false;
        }

        /// <summary>
        /// Allow to be sorted easily
        /// </summary>
        public int CompareTo(RemoteTechCommNetHome other)
        {
            return this.stationName.CompareTo(other.stationName);
        }
    }
}
