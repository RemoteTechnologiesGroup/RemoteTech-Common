using CommNet;
using KSP.Localization;
using KSP.UI.Screens.Mapview;
using System;
using System.Collections.Generic;
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
            [Description("Working Connection")]
            Path,
            [Description("Vessel Links")]
            VesselLinks,
            [Description("Network")]
            Network,
            [Description("All Working Connections")]
            MultiPaths
        }

        //New variables related to display mode
        public static CustomDisplayMode CustomMode = CustomDisplayMode.Path;
        public static CustomDisplayMode CustomModeTrackingStation = CustomDisplayMode.Network;
        public static CustomDisplayMode CustomModeFlightMap = CustomDisplayMode.Path;
        private static int CustomModeCount = Enum.GetValues(typeof(CustomDisplayMode)).Length;

        public static new RemoteTechCommNetUI Instance
        {
            get;
            protected set;
        }

        /// <summary>
        /// Activate things when the player enter a scene that uses CommNet UI
        /// </summary>
        public override void Show()
        {
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
            ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_118264", new string[]
            {
                Localizer.Format(RemoteTechCommNetUI.CustomMode.displayDescription())
            }), 5f);
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
            ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_118530", new string[]
            {
                Localizer.Format(RemoteTechCommNetUI.CustomMode.displayDescription())
            }), 5f);
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
                        ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_118264", new string[]
                        {
                            Localizer.Format(CustomModeTrackingStation.displayDescription())
                        }), 5f);
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
                        var newPath = new CommPath();
                        var nodes = net;
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
                                for (int pathIndex = 0; pathIndex < CNvessel.ControlPath.Count; pathIndex++)
                                {
                                    var link = CNvessel.ControlPath[pathIndex];
                                    if (newPath.Find(x => (RemoteTechCommNetwork.AreSame(x.a, link.a) && RemoteTechCommNetwork.AreSame(x.b, link.b))) == null)//not found in list of links to be displayed
                                    {
                                        newPath.Add(link); //laziness wins
                                        //KSP techincally does not care if path is consisted of non-continuous links or not
                                    }
                                }
                            }
                        }

                        path = newPath;
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
                        float lvl = Mathf.Pow((float)path.First.signalStrength, this.colorLerpPower);
                        if (this.swapHighLow)
                            this.line.SetColor(Color.Lerp(this.colorHigh, this.colorLow, lvl), 0);
                        else
                            this.line.SetColor(Color.Lerp(this.colorLow, this.colorHigh, lvl), 0);
                        break;
                    }
                case RemoteTechCommNetUI.CustomDisplayMode.Path:
                case RemoteTechCommNetUI.CustomDisplayMode.MultiPaths:
                    {
                        int linkIndex = numLinks;
                        for (int i = linkIndex - 1; i >= 0; i--)
                        {
                            float lvl = Mathf.Pow((float)path[i].signalStrength, this.colorLerpPower);
                            if (this.swapHighLow)
                                this.line.SetColor(Color.Lerp(this.colorHigh, this.colorLow, lvl), i);
                            else
                                this.line.SetColor(Color.Lerp(this.colorLow, this.colorHigh, lvl), i);
                        }
                        break;
                    }
                case RemoteTechCommNetUI.CustomDisplayMode.VesselLinks:
                    {
                        var itr = node.Values.GetEnumerator();
                        int linkIndex = 0;
                        while (itr.MoveNext())
                        {
                            CommLink link = itr.Current;
                            float lvl = Mathf.Pow((float)link.GetSignalStrength(link.a != node, link.b != node), this.colorLerpPower);
                            if (this.swapHighLow)
                                this.line.SetColor(Color.Lerp(this.colorHigh, this.colorLow, lvl), linkIndex++);
                            else
                                this.line.SetColor(Color.Lerp(this.colorLow, this.colorHigh, lvl), linkIndex++);
                        }
                        break;
                    }
                case RemoteTechCommNetUI.CustomDisplayMode.Network:
                    {
                        for (int i = numLinks - 1; i >= 0; i--)
                        {
                            CommLink commLink = net.Links[i];
                            float lvl = Mathf.Pow((float)net.Links[i].GetBestSignal(), this.colorLerpPower);
                            if (this.swapHighLow)
                                this.line.SetColor(Color.Lerp(this.colorHigh, this.colorLow, lvl), i);
                            else
                                this.line.SetColor(Color.Lerp(this.colorLow, this.colorHigh, lvl), i);
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
    }
}
