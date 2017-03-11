using RemoteTech.Common.RemoteTechCommNet;
using RemoteTech.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteTech.Common.AntennaSimulator
{
    public class RangeSection : SimulatorSection
    {
        private List<ModuleDataTransmitter> antennaModules = new List<ModuleDataTransmitter>();

        private enum TargetNode { CUSTOM, TARGET, DSN };
        private TargetNode currentRangeTargetNode = TargetNode.CUSTOM;

        private DialogGUIVerticalLayout targetNodePaneLayout;

        private double vesselAntennaComPower = 0.0;
        private double targetAntennaComPower = 0.0;
        private double vesselAntennaDrainPower = 0.0;
        private double currentConnectionStrength = 50.0;
        private double partialControlRangeMultipler = 0.2; //TODO: get from RT's CustomGameParams

        public RangeSection(AntennaSimulator simulator) : base(SimulationType.RANGE, simulator) { }

        public override DialogGUIBase[] draw(List<Part> parts)
        {
            List<DialogGUIBase> components = new List<DialogGUIBase>();            

            //reset information data
            antennaModules.Clear();
            vesselAntennaComPower = 0;
            vesselAntennaDrainPower = 0;
            targetAntennaComPower = 0;

            // CURRENT NODE PANE
            DialogGUIVerticalLayout currentNodeLayout = new DialogGUIVerticalLayout(true, false, 0, new RectOffset(5, 5, 5, 5), TextAnchor.MiddleLeft, new DialogGUIBase[] { });
            currentNodeLayout.AddChild(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            DialogGUILabel message = new DialogGUILabel("<b>Your vessel</b>\n<b>List of antennas detected:</b>", true, false);
            currentNodeLayout.AddChild(message);

            //list the antennas detected on the active vessel
            for (int i = 0; i < parts.Count; i++)
            {
                bool inUseState = true;
                Part thisPart = parts[i];
                ModuleDataTransmitter antennaModule;
                if ((antennaModule = thisPart.FindModuleImplementing<ModuleDataTransmitter>()) != null)
                {
                    ModuleDeployableAntenna delayModule;
                    if ((delayModule = thisPart.FindModuleImplementing<ModuleDeployableAntenna>()) != null)
                        inUseState = (delayModule.deployState == ModuleDeployablePart.DeployState.EXTENDED);

                    if (inUseState)
                    {
                        vesselAntennaComPower += antennaModule.CommPower;
                        vesselAntennaDrainPower += antennaModule.DataResourceCost;
                    }

                    int antennaIndex = antennaModules.Count; // antennaModules.Count doesn't work due to the compiler optimization

                    DialogGUIToggle toggleBtn = new DialogGUIToggle(inUseState, thisPart.partInfo.title, delegate (bool b) { antennaSelected(b, antennaIndex); }, 130, 24);
                    DialogGUILabel comPowerLabel = new DialogGUILabel("Power: " + UiUtils.RoundToNearestMetricFactor(antennaModule.CommPower));
                    DialogGUILabel powerConsLabel = new DialogGUILabel(string.Format("Drain: {0:0.00} charge/s", antennaModule.DataResourceCost));
                    currentNodeLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { toggleBtn, comPowerLabel, powerConsLabel }));

                    antennaModules.Add(antennaModule);
                }
            }

            DialogGUILabel combinablePower = new DialogGUILabel(getCombinablePowerMessage, true, false);
            currentNodeLayout.AddChild(combinablePower);

            // TARGET NODE PANE
            DialogGUIVerticalLayout targetNodeLayout = new DialogGUIVerticalLayout(true, false, 0, new RectOffset(5, 5, 5, 5), TextAnchor.MiddleLeft, new DialogGUIBase[] { });
            targetNodeLayout.AddChild(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            DialogGUILabel message2 = new DialogGUILabel("<b>Target</b>", true, false);
            targetNodeLayout.AddChild(message2);

            DialogGUIToggle commNodeToggleBtn = new DialogGUIToggle(true, "Custom node", delegate (bool b) { displayTargetNodeContent(TargetNode.CUSTOM); });
            DialogGUIToggle targetToggleBtn = new DialogGUIToggle(false, "Designated target", delegate (bool b) { displayTargetNodeContent(TargetNode.TARGET); });
            DialogGUIToggle DSNToggleBtn = new DialogGUIToggle(false, "Deep Space Network", delegate (bool b) { displayTargetNodeContent(TargetNode.DSN); });
            DialogGUIToggleGroup targetToggleGroup = new DialogGUIToggleGroup(new DialogGUIToggle[] { commNodeToggleBtn, targetToggleBtn, DSNToggleBtn });
            targetNodeLayout.AddChild(targetToggleGroup);

            DialogGUILabel message3 = new DialogGUILabel("Configuration below:", true, false);
            targetNodeLayout.AddChild(message3);

            targetNodePaneLayout = new DialogGUIVerticalLayout(10, 100, 0, new RectOffset(10, 25, 10, 10), TextAnchor.UpperLeft, new DialogGUIBase[] { });// empty initially
            drawCustomTargetNode();
            targetNodeLayout.AddChild(targetNodePaneLayout);

            //Top row
            DialogGUILabel nodeMessage = new DialogGUILabel("<b>Connection between your vessel and a target node</b>", true, false);
            contentPaneLayout.AddChild(nodeMessage);
            DialogGUIScrollList targetNodeScrollPane = new DialogGUIScrollList(new Vector2((dialogWidth - 60) / 2, (int)(dialogHeight * 0.45)), false, true, targetNodeLayout);
            DialogGUIScrollList currentNodeScrollPane = new DialogGUIScrollList(new Vector2((dialogWidth - 60) / 2, (int)(dialogHeight * 0.45)), false, true, currentNodeLayout);
            contentPaneLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { currentNodeScrollPane, new DialogGUISpace(5), targetNodeScrollPane }));

            //Bottom row
            DialogGUILabel resultMessage = new DialogGUILabel("<b>Estimated ranges and other predictions</b>", true, false);
            contentPaneLayout.AddChild(resultMessage);
            Texture2D graphImageTxt = new Texture2D(400, 100, TextureFormat.ARGB32, false);
            for (int y = 0; y < graphImageTxt.height; y++)
            {
                for (int x = 0; x < graphImageTxt.width; x++)
                    graphImageTxt.SetPixel(x, y, Color.grey);
            }
            graphImageTxt.Apply();
            DialogGUIImage graphImage = new DialogGUIImage(new Vector2(graphImageTxt.width, graphImageTxt.height), Vector2.zero, Color.white, graphImageTxt);
            contentPaneLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), graphImage, new DialogGUIFlexibleSpace() }));

            DialogGUILabel fullProbeActionRange = new DialogGUILabel(getFullActionRange, true, false);
            DialogGUILabel partialProbeActionRange = new DialogGUILabel(getPartialActionRange, true, false);
            DialogGUILabel powerWarning = new DialogGUILabel(getWarningPowerMessage, true, false);
            contentPaneLayout.AddChild(fullProbeActionRange);
            contentPaneLayout.AddChild(partialProbeActionRange);
            contentPaneLayout.AddChild(powerWarning);

            return components.ToArray();
        }

        private void drawCustomTargetNode()
        {
            DialogGUILabel commPowerLabel = new DialogGUILabel("Com power", 120, 24);
            DialogGUITextInput powerInput = new DialogGUITextInput("", false, 12, customTargetComPowerEntered, 24);

            targetNodePaneLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { commPowerLabel, powerInput }));
        }

        private void drawUserTargetNode()
        {
            DialogGUILabel massTargetLabel = new DialogGUILabel(getTargetInfo, 120, 24);

            targetNodePaneLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { massTargetLabel }));
        }

        private void drawDSNTargetNode()
        {
            //TrackingStationBuilding.
            List<DialogGUIToggle> DSNLevels = new List<DialogGUIToggle>();

            int numLevels = ScenarioUpgradeableFacilities.GetFacilityLevelCount(SpaceCenterFacility.TrackingStation) + 1; // why is GetFacilityLevelCount zero-index??
            for (int lvl = 0; lvl < numLevels; lvl++)
            {
                float normalizedLevel = (1f / numLevels) * lvl;
                double dsnPower = GameVariables.Instance.GetDSNRange(normalizedLevel);
                DialogGUIToggle DSNLevel = new DialogGUIToggle(false, string.Format("Level {0} - Max DSN Power: {1}", lvl + 1, UiUtils.RoundToNearestMetricFactor(dsnPower)), delegate (bool b) { DSNLevelSelected(b, dsnPower); });
                DSNLevels.Add(DSNLevel);
            }

            DialogGUIToggleGroup DSNLevelGroup = new DialogGUIToggleGroup(DSNLevels.ToArray());

            targetNodePaneLayout.AddChild(new DialogGUIVerticalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperLeft, new DialogGUIBase[] { DSNLevelGroup }));
        }

        private string getTargetInfo()
        {
            ITargetable target = FlightGlobals.fetch.VesselTarget;
            Vessel activeVessel = FlightGlobals.fetch.activeVessel;

            if (!HighLogic.LoadedSceneIsFlight)
            {
                targetAntennaComPower = 0;
                return "Only in flight";
            }
            else if (target == null)
            {
                targetAntennaComPower = 0;
                return "Please designate your target";
            }
            else if (!(target is CelestialBody) && !(target is Vessel))
            {
                targetAntennaComPower = 0;
                return "This target is neither vessel nor celestial body";
            }

            if (target is Vessel)
            {
                Vessel v = target as Vessel;
                if (v.connection.CanComm)
                    targetAntennaComPower = v.connection.Comm.antennaRelay.power;
                else
                    targetAntennaComPower = 0;
            }
            else // celestial body
            {
                targetAntennaComPower = 0;
            }

            string message = string.Format("Target: {0} ({1})\n", target.GetName(), target.GetType());
            message += string.Format("Distance: {0}m\n", UiUtils.RoundToNearestMetricFactor(Vector3d.Distance(activeVessel.transform.position, target.GetTransform().position)));
            message += string.Format("Relay's com power: {0}", UiUtils.RoundToNearestMetricFactor(targetAntennaComPower));

            return message;
        }

        private string customTargetComPowerEntered(string userInput)
        {
            if (!double.TryParse(userInput, out targetAntennaComPower))
                targetAntennaComPower = 0;

            return "";
        }

        private void DSNLevelSelected(bool toggleState, double DSNPower)
        {
            if (toggleState)
                targetAntennaComPower = DSNPower;
        }

        private void antennaSelected(bool toggleState, int indexAntenna)
        {
            if (toggleState)
            {
                vesselAntennaComPower += antennaModules[indexAntenna].CommPower;
                vesselAntennaDrainPower += antennaModules[indexAntenna].DataResourceCost;
            }
            else
            {
                vesselAntennaComPower -= antennaModules[indexAntenna].CommPower;
                vesselAntennaDrainPower -= antennaModules[indexAntenna].DataResourceCost;
            }
        }

        private string getCombinablePowerMessage()
        {
            return "Combinable com power: " + UiUtils.RoundToNearestMetricFactor(vesselAntennaComPower);
        }

        private string getWarningPowerMessage()
        {
            ElectricChargeReport chargeReport = ((PowerSection)sections[(short)SimulationType.POWER]).chargeReport;

            if (true)
                return string.Format("<color=red>Warning:</color> Estimated to run out of usable power in {0:0.0} seconds", (chargeReport.currentCapacity - chargeReport.lockedCapacity) / (chargeReport.flowRateWOAntenna + vesselAntennaDrainPower));
            else
                return "Sustainable";
        }

        private string getFullActionRange()
        {
            double maxRange = RemoteTechCommNetScenario.RangeModel.GetMaximumRange(vesselAntennaComPower, targetAntennaComPower);
            return string.Format("Maximum full probe control: {0}m", UiUtils.RoundToNearestMetricFactor(maxRange * (1.0 - partialControlRangeMultipler)));
        }

        private string getPartialActionRange()
        {
            return string.Format("Maximum partial probe control: {0}m", UiUtils.RoundToNearestMetricFactor(RemoteTechCommNetScenario.RangeModel.GetMaximumRange(vesselAntennaComPower, targetAntennaComPower)));
        }

        private void displayTargetNodeContent() { displayTargetNodeContent(currentRangeTargetNode); }
        private void displayTargetNodeContent(TargetNode whichNode)
        {
            currentRangeTargetNode = whichNode;

            SimulatorSection.deregisterLayoutComponents(targetNodePaneLayout);
            switch (whichNode)
            {
                case TargetNode.CUSTOM:
                    drawCustomTargetNode();
                    break;
                case TargetNode.TARGET:
                    drawUserTargetNode();
                    break;
                case TargetNode.DSN:
                    drawDSNTargetNode();
                    break;
            }
            SimulatorSection.registerLayoutComponents(targetNodePaneLayout);
        }
    }
}
