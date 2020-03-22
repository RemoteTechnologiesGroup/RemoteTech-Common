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
        private static readonly Texture2D L0MarkTexture = UiUtils.LoadTexture("GroundStationL0Mark");
        private static readonly Texture2D L1MarkTexture = UiUtils.LoadTexture("GroundStationL1Mark");
        private static readonly Texture2D L2MarkTexture = UiUtils.LoadTexture("GroundStationL2Mark");
        private static readonly Texture2D L3MarkTexture = UiUtils.LoadTexture("GroundStationL3Mark");
        private static GUIStyle groundStationHeadline;

        private bool loadCompleted = false;

        private string stationInfoString = "";
        private Texture2D stationTexture;

        [Persistent] public string ID;
        [Persistent] public Color Color = new Color(1.0f, 0.0f, 0.0f, 1f); //RemoteTechCommNetScenario.Instance.GroundStationDotColor; //issue: RemoteTechCommNetScenario.Instance not initialised yet
        [Persistent] protected string OptionalName = "";
        [Persistent] public short TechLevel = 0;

        public double altitude { get { return this.alt; } }
        public double latitude { get { return this.lat; } }
        public double longitude { get { return this.lon; } }
        public CommNode commNode { get { return this.comm; } }
        public string stationName
        {
            get { return (this.OptionalName.Length == 0) ? this.displaynodeName : this.OptionalName; }
            set { this.OptionalName = value; }
        }

        /// <summary>
        /// Call the stock OnNetworkInitialized() to be added to CommNetNetwork as node
        /// </summary>
        protected override void OnNetworkInitialized()
        {
            base.OnNetworkInitialized();
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
                normal = { textColor = Color.yellow },
                alignment = TextAnchor.MiddleCenter
            };

            loadCompleted = true;
        }

        protected override void CreateNode()
        {
            base.CreateNode();

            if (this.isKSC)
            {
                this.TechLevel = (short)((2 * ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.TrackingStation)) + 1);
            }

            this.comm.antennaRelay.Update(GetDSNRange(this.TechLevel), GameVariables.Instance.GetDSNRangeCurve(), false);
        }

        protected override void Start()
        {
            base.Start();
            this.refresh();
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

            if (HighLogic.CurrentGame.Parameters.CustomParams<RemoteTechCommonParams>().HideGroundStationsBehindBody && IsOccluded(nodeTransform.transform.position, this.body))
                return;

            if (HighLogic.CurrentGame.Parameters.CustomParams<RemoteTechCommonParams>().DistanceToHideGroundStations > 0.0f && !IsOccluded(nodeTransform.transform.position, this.body) && this.IsCamDistanceToWide(nodeTransform.transform.position))
                return;

            //maths calculations
            var screenPosition = PlanetariumCamera.Camera.WorldToScreenPoint(worldPos);
            var centerPosition = new Vector3(screenPosition.x - 8, (Screen.height - screenPosition.y) - 8);
            var groundStationRect = new Rect(centerPosition.x, centerPosition.y, 16, 16);

            //draw the dot
            var previousColor = GUI.color;
            GUI.color = RemoteTechCommNetScenario.Instance.GroundStationDotColor;
            GUI.DrawTexture(groundStationRect, stationTexture, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;

            //draw the headline below the dot
            if (HighLogic.CurrentGame.Parameters.CustomParams<RemoteTechCommonParams>().ShowMouseOverInfoGroundStations && UiUtils.ContainsMouse(groundStationRect))
            {
                //Ground Station Name
                var headlineRect = new Rect();
                var nameDim = RemoteTechCommNetHome.groundStationHeadline.CalcSize(new GUIContent(this.nodeName));
                headlineRect.x = centerPosition.x - nameDim.x / 2;
                headlineRect.y = centerPosition.y - nameDim.y;
                headlineRect.width = nameDim.x;
                headlineRect.height = nameDim.y;
                GUI.Label(headlineRect, this.nodeName, RemoteTechCommNetHome.groundStationHeadline);

                //Ground Station Range Info
                var rangeDim = RemoteTechCommNetHome.groundStationHeadline.CalcSize(new GUIContent(stationInfoString));
                var rangeRect = new Rect();
                rangeRect.x = centerPosition.x - rangeDim.x / 2;
                rangeRect.y = centerPosition.y + 25; //move out of mouse cursor
                rangeRect.width = rangeDim.x;
                rangeRect.height = rangeDim.y;
                GUI.Label(rangeRect, stationInfoString, RemoteTechCommNetHome.groundStationHeadline);
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

        /// <summary>
        /// Increment Tech Level Ground Station to max 3 and refresh the ground station
        /// </summary>
        public void incrementTechLevel()
        {
            if(this.TechLevel < 3 && !this.isKSC)
            {
                this.TechLevel++;
                refresh();
            }
        }

        /// <summary>
        /// Apply the changes from persistent.sfs
        /// </summary>
        public void applySavedChanges(RemoteTechCommNetHome stationSnapshot)
        {
            this.Color = stationSnapshot.Color;
            this.OptionalName = stationSnapshot.OptionalName;
            this.TechLevel = stationSnapshot.TechLevel;
        }

        /// <summary>
        /// Update relevant details based on Tech Level
        /// </summary>
        protected void refresh()
        {
            // Obtain Tech Level of Tracking Station in KCS
            //if (this.isKSC)
            //{
            //    this.TechLevel = (short)((2 * ScenarioUpgradeableFacilities.GetFacilityLevel(SpaceCenterFacility.TrackingStation)) + 1);
            //}

            // Update power of ground station
            if(this.comm != null)
            {
                this.comm.antennaRelay.Update(GetDSNRange(this.TechLevel), GameVariables.Instance.GetDSNRangeCurve(), false);
            }

            // Generate ground station information
            stationInfoString = (this.TechLevel == 0) ? "Build a ground station" :
                                                    string.Format("DSN Power: {1}\nBeamwidth: {2:0.00}°\nTech Level: {0}",
                                                    this.TechLevel,
                                                    UiUtils.RoundToNearestMetricFactor(this.comm.antennaRelay.power, 2),
                                                    90.0);

            // Generate visual ground station mark
            switch (this.TechLevel)
            {
                case 0:
                    stationTexture = L0MarkTexture;
                    break;
                case 1:
                    stationTexture = L1MarkTexture;
                    break;
                case 2:
                    stationTexture = L2MarkTexture;
                    break;
                case 3:
                default:
                    stationTexture = L3MarkTexture;
                    break;
            }
        }

        /// <summary>
        /// Custom DSN ranges instead of stock GameVariables.Instance.GetDSNRange
        /// </summary>
        /// Comment: Subclassing GameVariables.Instance.GetDSNRange to just change the ranges is too excessive at this point.
        public double GetDSNRange(short level)
        {
            var ps = HighLogic.CurrentGame.Parameters.CustomParams<RemoteTechCommonParams>();
            double power;
            if(this.isKSC)
            {
                power = ps.KSCStationPowers[level - 1];
            }
            else
            {
                if (level == 0)
                {
                    power = 0;
                }
                else
                {
                    power = ps.GroundStationUpgradeablePowers[level - 1];
                }
            }
            
            return power * ((double)HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().DSNModifier);
        }
    }
}
