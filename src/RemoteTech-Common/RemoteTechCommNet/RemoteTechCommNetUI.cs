using CommNet;
using KSP.Localization;
using KSP.UI.Screens.Mapview;
using System;
using System.ComponentModel;
using UnityEngine;

namespace RemoteTech.Common.RemoteTechCommNet
{
    /// <summary>
    /// CommNetUI is the view in the Model–view–controller sense. Everything a player is seeing goes through this class
    /// </summary>
    public class RemoteTechCommNetUI : CommNetUI
    {
        /// <summary>
        /// Add own display mode in replacement of stock display mode which cannot be extended easily
        /// </summary>
        public enum CustomDisplayMode
        {
            [Description("None")]
            None,
            [Description("First Hop")]
            FirstHop,
            [Description("Active Connection")]
            Path,
            [Description("Vessel Links")]
            VesselLinks,
            [Description("Network")]
            Network,
            [Description("All Active Connections")]
            MultiPaths
        }

        public enum RemoteTechMapFilter
        {
            None = 0,
            OmniLine = 1,
            DishLine = 2,
            DishCone = 4,
            VisualRange = 8
        }

        //New variables related to display mode
        public static CustomDisplayMode CustomMode = CustomDisplayMode.Path;
        public static CustomDisplayMode CustomModeTrackingStation = CustomDisplayMode.Network;
        public static CustomDisplayMode CustomModeFlightMap = CustomDisplayMode.Path;
        private static int CustomModeCount = Enum.GetValues(typeof(CustomDisplayMode)).Length;
        public static RemoteTechMapFilter RTMapFilter = RemoteTechMapFilter.OmniLine | RemoteTechMapFilter.DishLine;
        private static float Line3DWidth = 1f;
        private static float Line2DWidth = 1f;

        public static new RemoteTechCommNetUI Instance
        {
            get;
            protected set;
        }

        protected override void Start()
        {
            base.Start();
            if (Versioning.version_major == 1)
            {
                switch (Versioning.version_minor)
                {
                    case 3:
                        Line3DWidth = 2f; //4f is too thick in 1.3
                        Line2DWidth = 3f;
                        break;
                    default:
                        Line3DWidth = Line2DWidth = 1f;
                        break;
                }
            }
        }

        /// <summary>
        /// Activate things when the player enter a scene that uses CommNet UI
        /// </summary>
        public override void Show()
        {
            this.lineWidth3D = Line3DWidth;
            this.lineWidth2D = Line2DWidth;
            registerMapNodeIconCallbacks();
            base.Show();
        }

        /// <summary>
        /// Clean up things when the player exits a scene that uses CommNet UI
        /// </summary>
        public override void Hide()
        {
            deregisterMapNodeIconCallbacks();
            base.Hide();
        }

        /// <summary>
        /// Run own display updates
        /// </summary>
        protected override void UpdateDisplay()
        {
            if (CommNetNetwork.Instance == null)
            {
                return;
            }
            else
            {
                updateCustomisedView();
            }
        }

        /// <summary>
        /// Register own callbacks
        /// </summary>
        protected void registerMapNodeIconCallbacks()
        {
            var vessels = FlightGlobals.fetch.vessels;

            for (int i = 0; i < vessels.Count; i++)
            {
                MapObject mapObj = vessels[i].mapObject;

                if (mapObj.type == MapObject.ObjectType.Vessel)
                    mapObj.uiNode.OnUpdateVisible += new Callback<MapNode, MapNode.IconData>(this.OnMapNodeUpdateVisible);
            }
        }

        /// <summary>
        /// Remove own callbacks
        /// </summary>
        protected void deregisterMapNodeIconCallbacks()
        {
            var vessels = FlightGlobals.fetch.vessels;

            for (int i = 0; i < vessels.Count; i++)
            {
                MapObject mapObj = vessels[i].mapObject;
                mapObj.uiNode.OnUpdateVisible -= new Callback<MapNode, MapNode.IconData>(this.OnMapNodeUpdateVisible);
            }
        }

        /// <summary>
        /// Update the MapNode object of each CommNet vessel
        /// </summary>
        private void OnMapNodeUpdateVisible(MapNode node, MapNode.IconData iconData)
        {
            var thisVessel = node.mapObject.vessel.connection;

            //do some thing on this vessel
        }

        /// <summary>
        /// Overrode ResetMode to use custom display mode
        /// </summary>
        public override void ResetMode()
        {
            RemoteTechCommNetUI.CustomMode = RemoteTechCommNetUI.CustomDisplayMode.None;

            if (FlightGlobals.ActiveVessel == null)
            {
                RemoteTechCommNetUI.CustomModeTrackingStation = RemoteTechCommNetUI.CustomMode;
            }
            else
            {
                RemoteTechCommNetUI.CustomModeFlightMap = RemoteTechCommNetUI.CustomMode;
            }

            this.points.Clear();
            /*
            //distraction
            ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_118264", new string[]
            {
                Localizer.Format(RemoteTechCommNetUI.CustomMode.displayDescription())
            }), 5f);
            */
        }

        /// <summary>
        /// Overrode SwitchMode to use custom display mode
        /// </summary>
        public override void SwitchMode(int step)
        {
            var modeIndex = (((int)RemoteTechCommNetUI.CustomMode) + step + RemoteTechCommNetUI.CustomModeCount) % RemoteTechCommNetUI.CustomModeCount;
            RemoteTechCommNetUI.CustomDisplayMode newMode = (RemoteTechCommNetUI.CustomDisplayMode)modeIndex;

            if (this.useTSBehavior)
            {
                this.ClampAndSetMode(ref RemoteTechCommNetUI.CustomModeTrackingStation, newMode);
            }
            else
            {
                this.ClampAndSetMode(ref RemoteTechCommNetUI.CustomModeFlightMap, newMode);
            }

            this.points.Clear();
            /*
            //distraction
            ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_118530", new string[]
            {
                Localizer.Format(RemoteTechCommNetUI.CustomMode.displayDescription())
            }), 5f);
            */
        }

        /// <summary>
        /// Add new ClampAndSetMode for custom display mode
        /// </summary>
        public void ClampAndSetMode(ref RemoteTechCommNetUI.CustomDisplayMode curMode, RemoteTechCommNetUI.CustomDisplayMode newMode)
        {
            if (this.vessel == null || this.vessel.connection == null || this.vessel.connection.Comm.Net == null)
            {
                if (newMode != RemoteTechCommNetUI.CustomDisplayMode.None &&
                    newMode != RemoteTechCommNetUI.CustomDisplayMode.Network &&
                    newMode != RemoteTechCommNetUI.CustomDisplayMode.MultiPaths)
                {
                    newMode = ((curMode != RemoteTechCommNetUI.CustomDisplayMode.None) ? RemoteTechCommNetUI.CustomDisplayMode.None : RemoteTechCommNetUI.CustomDisplayMode.Network);
                }
            }

            RemoteTechCommNetUI.CustomMode = (curMode = newMode);
        }

        /// <summary>
        /// Overrode UpdateDisplay() fully and add own customisations
        /// </summary>
        private void updateCustomisedView()
        {
            if (FlightGlobals.ActiveVessel == null)
            {
                this.useTSBehavior = true;
            }
            else
            {
                this.useTSBehavior = false;
                this.vessel = FlightGlobals.ActiveVessel;
            }

            if (this.vessel == null || this.vessel.connection == null || this.vessel.connection.Comm.Net == null) //revert to default display mode if saved mode is inconsistent in current situation
            {
                this.useTSBehavior = true;
                if (CustomModeTrackingStation != CustomDisplayMode.None)
                {
                    if (CustomModeTrackingStation != CustomDisplayMode.Network && CustomModeTrackingStation != CustomDisplayMode.MultiPaths)
                    {
                        CustomModeTrackingStation = CustomDisplayMode.Network;
                        /*
                        //distraction
                        ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_118264", new string[]
                        {
                            Localizer.Format(CustomModeTrackingStation.displayDescription())
                        }), 5f);
                        */
                    }
                }
            }

            if (this.useTSBehavior)
            {
                RemoteTechCommNetUI.CustomMode = RemoteTechCommNetUI.CustomModeTrackingStation;
            }
            else
            {
                RemoteTechCommNetUI.CustomMode = RemoteTechCommNetUI.CustomModeFlightMap;
            }

            CommNetwork net = CommNetNetwork.Instance.CommNet;
            CommNetVessel cnvessel = null;
            CommNode node = null;
            CommPath path = null;

            if (this.vessel != null && this.vessel.connection != null && this.vessel.connection.Comm.Net != null)
            {
                cnvessel = this.vessel.connection;
                node = cnvessel.Comm;
                path = cnvessel.ControlPath;
            }

            //work out which links to display
            var count = this.points.Count;//save previous value
            var numLinks = 0;
            var pathLinkExist = false;
            switch (RemoteTechCommNetUI.CustomMode)
            {
                case RemoteTechCommNetUI.CustomDisplayMode.None:
                    numLinks = 0;
                    break;

                case RemoteTechCommNetUI.CustomDisplayMode.FirstHop:
                case RemoteTechCommNetUI.CustomDisplayMode.Path:
                    if (cnvessel.ControlState == VesselControlState.Probe || cnvessel.ControlState == VesselControlState.Kerbal ||
                        path.Count == 0)
                    {
                        numLinks = 0;
                    }
                    else
                    {
                        if (RemoteTechCommNetUI.CustomMode == RemoteTechCommNetUI.CustomDisplayMode.FirstHop)
                        {
                            path.First.GetPoints(this.points);
                            numLinks = 1;
                        }
                        else
                        {
                            path.GetPoints(this.points, true);
                            numLinks = path.Count;
                        }
                    }
                    break;

                case RemoteTechCommNetUI.CustomDisplayMode.VesselLinks:
                    numLinks = node.Count;
                    node.GetLinkPoints(this.points);
                    break;

                case RemoteTechCommNetUI.CustomDisplayMode.Network:
                    if (net.Links.Count == 0)
                    {
                        numLinks = 0;
                    }
                    else
                    {
                        numLinks = net.Links.Count;
                        net.GetLinkPoints(this.points);
                    }
                    break;
                case RemoteTechCommNetUI.CustomDisplayMode.MultiPaths:
                    if (net.Links.Count == 0)
                    {
                        numLinks = 0;
                    }
                    else
                    {
                        path = new CommPath();
                        path.Capacity = net.Links.Count;

                        var vessels = FlightGlobals.fetch.vessels;
                        for (int i = 0; i < vessels.Count; i++)
                        {
                            var CNvessel = vessels[i].connection as RemoteTechCommNetVessel;
                            if(CNvessel == null)
                            {
                                continue;
                            }

                            CNvessel.computeUnloadedUpdate();//network update is done only once for unloaded vessels so need to manually re-trigger every time

                            if (!(CNvessel.ControlState == VesselControlState.Probe || CNvessel.ControlState == VesselControlState.Kerbal ||
                                CNvessel.ControlPath == null || CNvessel.ControlPath.Count == 0))
                            {
                                //add each link in control path to overall path
                                for (int controlpathIndex = 0; controlpathIndex < CNvessel.ControlPath.Count; controlpathIndex++)
                                {
                                    pathLinkExist = false;
                                    for (int overallpathIndex = 0; overallpathIndex < path.Count; overallpathIndex++)//check if overall path has this link already
                                    {
                                        if (RemoteTechCommNetwork.AreSame(path[overallpathIndex].a, CNvessel.ControlPath[controlpathIndex].a) &&
                                            RemoteTechCommNetwork.AreSame(path[overallpathIndex].b, CNvessel.ControlPath[controlpathIndex].b))
                                        {
                                            pathLinkExist = true;
                                            break;
                                        }
                                    }
                                    if (!pathLinkExist)
                                    {
                                        path.Add(CNvessel.ControlPath[controlpathIndex]); //laziness wins
                                        //KSP techincally does not care if path is consisted of non-continuous links or not
                                    }
                                }
                            }
                        }

                        path.GetPoints(this.points, true);
                        numLinks = path.Count;
                    }
                    break;
            }// end of switch

            //check if nothing to display
            if (numLinks == 0)
            {
                if (this.line != null)
                    this.line.active = false;

                this.points.Clear();
                return;
            }

            if (this.line != null)
            {
                this.line.active = true;
            }
            else
            {
                this.refreshLines = true;
            }

            ScaledSpace.LocalToScaledSpace(this.points); //seem very important

            if (this.refreshLines || MapView.Draw3DLines != this.draw3dLines || count != this.points.Count || this.line == null)
            {
                this.CreateLine(ref this.line, this.points);//seems it is multiple separate lines not single continuous line
                this.draw3dLines = MapView.Draw3DLines;
                this.refreshLines = false;
            }

            //paint the links
            switch (RemoteTechCommNetUI.CustomMode)
            {
                case RemoteTechCommNetUI.CustomDisplayMode.FirstHop:
                    {
                            this.line.SetColor(colorBlending(this.colorHigh,
                                                             this.colorLow,
                                                             Mathf.Pow((float)path.First.signalStrength, this.colorLerpPower)),
                                                             0);
                            break;
                    }
                case RemoteTechCommNetUI.CustomDisplayMode.Path:
                case RemoteTechCommNetUI.CustomDisplayMode.MultiPaths:
                    {
                            for (int i = numLinks - 1; i >= 0; i--)
                            {
                                this.line.SetColor(colorBlending(this.colorHigh,
                                                                 this.colorLow,
                                                                 Mathf.Pow((float)path[i].signalStrength, this.colorLerpPower)),
                                                                 i);
                            }
                            break;
                    }
                case RemoteTechCommNetUI.CustomDisplayMode.VesselLinks:
                    {
                            CommLink[] links = new CommLink[node.Count];
                            node.Values.CopyTo(links, 0);
                            for (int i = 0; i < links.Length; i++)
                            {
                                this.line.SetColor(colorBlending(this.colorHigh,
                                                                 this.colorLow,
                                                                 Mathf.Pow((float)links[i].GetSignalStrength(links[i].a != node, links[i].b != node), this.colorLerpPower)),
                                                                 i);
                            }
                            break;
                    }
                case RemoteTechCommNetUI.CustomDisplayMode.Network:
                    {
                            for (int i = numLinks - 1; i >= 0; i--)
                            {
                                this.line.SetColor(colorBlending(this.colorHigh,
                                                                 this.colorLow,
                                                                 Mathf.Pow((float)net.Links[i].GetBestSignal(), this.colorLerpPower)),
                                                                 i);
                            }
                            break;
                    }
            } // end of switch

            if (this.draw3dLines)
            {
                this.line.SetWidth(this.lineWidth3D);
                this.line.Draw3D();
            }
            else
            {
                this.line.SetWidth(this.lineWidth2D);
                this.line.Draw();
            }
        }

        /// <summary>
        /// Compute final color based on inputs
        /// </summary>
        private Color colorBlending(Color colorHigh, Color colorLow, float colorLevel)
        {
            if (colorHigh == Color.clear)
            {
                return colorHigh;
            }
            else if (this.swapHighLow)
            {
                return Color.Lerp(colorHigh, colorLow, colorLevel);
            }
            else
            {
                return Color.Lerp(colorLow, colorHigh, colorLevel);
            }
        }
    }
}
