using CommNet;
using RemoteTech.Common.Utils;
using UnityEngine;

namespace RemoteTech.Common.RemoteTechCommNet
{
    public class RemoteTechCommNetHome : CommNetHome
    {
        private static readonly Texture2D markTexture = UiUtils.LoadTexture("groundStationMark");

        public void copyOf(CommNetHome stockHome)
        {
            this.nodeName = stockHome.nodeName;
            this.nodeTransform = stockHome.nodeTransform;
            this.isKSC = stockHome.isKSC;
            //this.comm = stockHome.GetComponentInChildren<CommNode>(); // maybe too early as it is null at beginning
            //this.body = stockHome.GetComponentInChildren<CelestialBody>(); // maybe too early as it is null at beginning
        }

        public void OnGUI()
        {
            if (GameUtil.IsGameScenario)
                return;

            if (!(HighLogic.LoadedScene == GameScenes.FLIGHT || HighLogic.LoadedScene == GameScenes.TRACKSTATION))
                return;

            if ((!HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().enableGroundStations && !this.isKSC) || !MapView.MapIsEnabled || MapView.MapCamera == null)
                return;

            var worldPos = ScaledSpace.LocalToScaledSpace(nodeTransform.transform.position);
            if (MapView.MapCamera.transform.InverseTransformPoint(worldPos).z < 0f) return;
            var pos = PlanetariumCamera.Camera.WorldToScreenPoint(worldPos);
            var screenRect = new Rect((pos.x - 8), (Screen.height - pos.y) - 8, 16, 16);

            if (IsOccluded(nodeTransform.transform.position, this.body))
                return;

            if (!IsOccluded(nodeTransform.transform.position, this.body) && this.IsCamDistanceToWide(nodeTransform.transform.position))
                return;

            var previousColor = GUI.color;
            GUI.color = Color.red; // TODO: switch to customised colors when RTSetting goes live
            GUI.DrawTexture(screenRect, markTexture, ScaleMode.ScaleToFit, true);
            GUI.color = previousColor;
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

            if (distance >= 30000000) // TODO: replace this when RTSetting goes live
                return true;
            return false;
        }
    }
}
