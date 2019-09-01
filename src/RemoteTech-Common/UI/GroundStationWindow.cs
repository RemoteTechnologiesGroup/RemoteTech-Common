using CommNet;
using RemoteTech.Common.RemoteTechCommNet;
using RemoteTech.Common.UI.DialogGUI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteTech.Common.UI
{
    public class GroundStationWindow : AbstractDialog
    {
        RemoteTechCommonParams ps = HighLogic.CurrentGame.Parameters.CustomParams<RemoteTechCommonParams>();

        public GroundStationWindow() : base("groundstationwin",
                                            "Ground Stations",
                                            0.5f,
                                            0.5f,
                                            450,
                                            300,
                                            new DialogOptions[] { })
        {
        }

        protected override List<DialogGUIBase> Draw()
        {
            List<DialogGUIBase> componments = new List<DialogGUIBase>();

            List<DialogGUIHorizontalLayout> newRows = new List<DialogGUIHorizontalLayout>();
            List<RemoteTechCommNetHome> stations = RemoteTechCommNetScenario.Instance.groundStations;

            if (HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().enableGroundStations)
            {
                for (int i = 0; i < stations.Count; i++)
                {
                    newRows.Add(createGroundStationRow(stations[i]));
                }
            }
            else
            {
                newRows.Add(createGroundStationRow(stations.Find(x => x.isKSC)));
            }

            DialogGUIVerticalLayout contentLayout = new DialogGUIVerticalLayout(new DialogGUIBase[] { new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true) });
            contentLayout.AddChildren(newRows.ToArray());
            componments.Add(new CustomDialogGUIScrollList(new Vector2(430, 280), false, true, contentLayout));

            return componments;
        }

        private DialogGUIHorizontalLayout createGroundStationRow(RemoteTechCommNetHome thisStation)
        {
            int money = thisStation.TechLevel >= 3 ? 0 : ps.GroundStationUpgradeableCosts[thisStation.TechLevel];

            DialogGUILabel label = new DialogGUILabel(string.Format("Name: {0}, Tech Level: {1}, Upgrade Cost: {2} Funding", thisStation.stationName, thisStation.TechLevel, money));
            DialogGUIButton upgradeButton = new DialogGUIButton("+", delegate { thisStation.incrementTechLevel(); }, false);
            DialogGUIHorizontalLayout groundStationGroup = new DialogGUIHorizontalLayout(new DialogGUIBase[] { label, upgradeButton});
            return groundStationGroup;
        }
    }
}
