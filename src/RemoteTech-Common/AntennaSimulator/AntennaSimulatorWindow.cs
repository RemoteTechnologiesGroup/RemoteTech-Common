using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteTech.Common.UI;
using UnityEngine;
using RemoteTech.Common.RemoteTechCommNet;
using UnityEngine.UI;
using RemoteTech.Common.Utils;

namespace RemoteTech.Common.AntennaSimulator
{
    //TODO: partition this messy class into portions
    public class AntennaSimulatorWindow : AbstractDialog
    {
        private enum InfoContent { RANGE, POWER, SCIENCE };
        private enum TargetNode { CUSTOM, TARGET, DSN };
        private TargetNode currentRangeTargetNode = TargetNode.CUSTOM;
        private InfoContent currentSectionContent;

        private DialogGUIVerticalLayout contentPaneLayout;
        private DialogGUIVerticalLayout targetNodePaneLayout;

        private ElectricChargeReport ecReport;
        private List<ModuleDataTransmitter> antennaModules = new List<ModuleDataTransmitter>();

        private static readonly int dialogWidth = 650;
        private static readonly int dialogHeight = 500;

        public AntennaSimulatorWindow() : base("RemoteTech Antenna Simulator",
                                                0.75f,
                                                0.5f,
                                                dialogWidth,
                                                dialogHeight,
                                                new DialogOptions[] { DialogOptions.HideDismissButton, DialogOptions.AllowBgInputs})
        {
        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> contentComponents = new List<DialogGUIBase>();

            string vesselName;
            if (HighLogic.LoadedSceneIsFlight)
                vesselName = FlightGlobals.ActiveVessel.vesselName;
            else
                vesselName = EditorLogic.fetch.ship.shipName;

            DialogGUILabel descrptionLabel = new DialogGUILabel(string.Format("Based on this vessel '{0}', a number of estimations are computed and displayed below.\n\n", vesselName), true, false);
            contentComponents.Add(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { descrptionLabel }));

            // BUTTON TABS
            DialogGUIButton rangeButton = new DialogGUIButton("Antenna range", delegate { displayContent(InfoContent.RANGE); }, false);
            DialogGUIButton scienceButton = new DialogGUIButton("Science data", delegate { displayContent(InfoContent.SCIENCE); }, false);
            DialogGUIButton powerButton = new DialogGUIButton("Power system", delegate { displayContent(InfoContent.POWER); }, false);
            DialogGUIButton refreshButton = new DialogGUIButton("Refresh", displayContent, false);

            DialogGUIHorizontalLayout tabbedButtonRow = new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { rangeButton, powerButton });
            if (ResearchAndDevelopment.Instance != null)
                tabbedButtonRow.AddChild(scienceButton);
            tabbedButtonRow.AddChild(new DialogGUIFlexibleSpace());
            tabbedButtonRow.AddChild(refreshButton);
            contentComponents.Add(tabbedButtonRow);

            DialogGUIBase[] rows = new DialogGUIBase[] { new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true) };
            contentPaneLayout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(10, 25, 10, 10), TextAnchor.UpperLeft, rows);
            contentComponents.Add(new DialogGUIScrollList(Vector2.one, false, true, contentPaneLayout));

            return contentComponents;
        }

        protected override void OnAwake(object[] args)
        {
            displayContent(InfoContent.RANGE); // the info panel a player sees for the first time
        }

        private void registerContentComponents(DialogGUIVerticalLayout layout)
        {
            if (layout.children.Count < 1)
                return;

            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(layout.uiItem.gameObject.transform);// problem for the target content
            for (int i = 0; i < layout.children.Count; i++)
            {
                if (!(layout.children[i] is DialogGUIContentSizer)) // avoid if DialogGUIContentSizer is detected
                    layout.children[i].Create(ref stack, HighLogic.UISkin);
            }
        }

        private void deleteContentComponents(DialogGUIVerticalLayout layout)
        {
            if (layout.children.Count < 1)
                return;

            int size = layout.children.Count;
            for (int i = size - 1; i >= 0; i--)
            {
                DialogGUIBase thisChild = layout.children[i];
                if (!(thisChild is DialogGUIContentSizer)) // avoid if DialogGUIContentSizer is detected
                {
                    layout.children.RemoveAt(i);
                    thisChild.uiItem.gameObject.DestroyGameObjectImmediate();
                }
            }
        }

        private void displayTargetNodeContent() { displayTargetNodeContent(currentRangeTargetNode); }
        private void displayTargetNodeContent(TargetNode whichNode)
        {
            currentRangeTargetNode = whichNode;

            deleteContentComponents(targetNodePaneLayout);
            switch(whichNode)
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
            registerContentComponents(targetNodePaneLayout);
        }

        private void displayContent() { displayContent(currentSectionContent); }
        private void displayContent(InfoContent whichContent)
        {
            currentSectionContent = whichContent;

            List<Part> parts;
            if (HighLogic.LoadedSceneIsFlight)
                parts = FlightGlobals.ActiveVessel.Parts;
            else
                parts = EditorLogic.fetch.ship.Parts;

            ecReport = new ElectricChargeReport(parts);

            deleteContentComponents(contentPaneLayout);
            switch (whichContent)
            {
                case InfoContent.RANGE:
                    drawRangeInfo(parts);
                    break;
                case InfoContent.POWER:
                    drawPowerInfo(parts);
                    break;
                case InfoContent.SCIENCE:
                    drawScienceInfo(parts);
                    break;
            }
            registerContentComponents(contentPaneLayout);

            return;

            /*
                int ecID = PartResourceLibrary.Instance.resourceDefinitions[ElectricChargeReport.ECName].id;
                FlightGlobals.ActiveVessel.GetConnectedResourceTotals(ecID, out currECAmt, out maxECAmt);

                if(currECAmt == maxECAmt)
                {
                    overallECRate = 0.0;
                }
                if ((prevCurrECAmt - currECAmt) != 0.00)
                {
                    overallECRate = (prevCurrECAmt - currECAmt) * -1.0 * ((1.0f/Time.deltaTime)>60? 60: (1.0f / Time.deltaTime));// Framerate is capped to 60 fps internally
                    prevCurrECAmt = currECAmt;
                }
            */
        }

        //------------------------------------
        // Range section
        //------------------------------------
        private double vesselAntennaComPower = 0.0;
        private double targetAntennaComPower = 0.0;
        private double vesselAntennaDrainPower = 0.0;
        private double currentConnectionStrength = 50.0;
        private double partialControlRangeMultipler = 0.2; //TODO: get from RT's CustomGameParams

        private void drawRangeInfo(List<Part> parts)
        {
            //reset information data
            antennaModules.Clear();
            vesselAntennaComPower = 0;
            vesselAntennaDrainPower = 0;
            targetAntennaComPower = 0;

            // CURRENT NODE PANE
            DialogGUIVerticalLayout currentNodeLayout = new DialogGUIVerticalLayout(true, false, 0, new RectOffset(5,5,5,5), TextAnchor.MiddleLeft, new DialogGUIBase[] { });
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

                    DialogGUIToggle toggleBtn = new DialogGUIToggle(inUseState, thisPart.partInfo.title, delegate(bool b) { antennaSelected(b, antennaIndex); }, 130, 24);
                    DialogGUILabel comPowerLabel = new DialogGUILabel("Power: "+ UiUtils.RoundToNearestMetricFactor(antennaModule.CommPower));
                    DialogGUILabel powerConsLabel = new DialogGUILabel(string.Format("Drain: {0:0.00} charge/s", antennaModule.DataResourceCost));
                    currentNodeLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { toggleBtn, comPowerLabel, powerConsLabel }));

                    antennaModules.Add(antennaModule);
                }
            }

            DialogGUILabel combinablePower = new DialogGUILabel(getCombinablePowerMessage, true, false);
            currentNodeLayout.AddChild(combinablePower);

            // TARGET NODE PANE
            DialogGUIVerticalLayout targetNodeLayout = new DialogGUIVerticalLayout(true, false, 0, new RectOffset(5,5,5,5), TextAnchor.MiddleLeft, new DialogGUIBase[] { });
            targetNodeLayout.AddChild(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            DialogGUILabel message2 = new DialogGUILabel("<b>Target</b>", true, false);
            targetNodeLayout.AddChild(message2);

            DialogGUIToggle commNodeToggleBtn = new DialogGUIToggle(true, "Custom node", delegate(bool b) { displayTargetNodeContent(TargetNode.CUSTOM); });
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
                DialogGUIToggle DSNLevel = new DialogGUIToggle(false, string.Format("Level {0} - Max DSN Power: {1}", lvl+1, UiUtils.RoundToNearestMetricFactor(dsnPower)), delegate (bool b) { DSNLevelSelected(b, dsnPower); });
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
            if(toggleState)
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
            return "Combinable com power: "+UiUtils.RoundToNearestMetricFactor(vesselAntennaComPower);
        }

        private string getWarningPowerMessage()
        {
            if (true)
                return string.Format("<color=red>Warning:</color> Estimated to run out of usable power in {0:0.0} seconds", (ecReport.currentCapacity-ecReport.lockedCapacity)/vesselAntennaDrainPower);
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

        //------------------------------------
        // Science section
        //------------------------------------
        private float totalScienceDataSize = 0;
        private float customScienceDataSize = 0;
        private float antennaBandwidthPerSec;
        private double antennaChargePerSec;

        private void drawScienceInfo(List<Part> parts)
        {
            if (ResearchAndDevelopment.Instance == null)
                return;

            totalScienceDataSize = 0;
            customScienceDataSize = 0;
            antennaBandwidthPerSec = 0;
            antennaChargePerSec = 0;

            // SCIENCE LIST
            contentPaneLayout.AddChild(new DialogGUILabel("<b>List of science experiments available:</b>", true, false));
            DialogGUIVerticalLayout scienceLayout = new DialogGUIVerticalLayout(true, false, 0, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] {});
            scienceLayout.AddChild(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            List<string> experimentIDs = ResearchAndDevelopment.GetExperimentIDs();
            for (int j = 0; j < experimentIDs.Count; j++)
            {
                ScienceExperiment thisExp = ResearchAndDevelopment.GetExperiment(experimentIDs[j]);

                DialogGUIToggle toggleBtn = new DialogGUIToggle(false, string.Format("{0} - {1} Mits", thisExp.experimentTitle, thisExp.dataScale * thisExp.baseValue) , delegate(bool b) { scienceSelected(b, thisExp.id); });
                scienceLayout.AddChild(toggleBtn);
            }

            DialogGUIToggle customToggleBtn = new DialogGUIToggle(false, "Custom data size (Mits)", customScienceSelected, 120, 24);
            DialogGUITextInput sizeInput = new DialogGUITextInput("", false, 5, customScienceInput);
            scienceLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { customToggleBtn, sizeInput, new DialogGUIFlexibleSpace() }));

            DialogGUIScrollList scienceScrollPane = new DialogGUIScrollList(new Vector2(dialogWidth-50, dialogHeight/3), false, true, scienceLayout);
            contentPaneLayout.AddChild(scienceScrollPane);

            // ANTENNA LIST
            contentPaneLayout.AddChild(new DialogGUILabel("<b>List of antennas detected:</b>", true, false));
            DialogGUIVerticalLayout antennaLayout = new DialogGUIVerticalLayout(true, false, 0, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { });
            antennaLayout.AddChild(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            DialogGUIToggleGroup antennaGroup = new DialogGUIToggleGroup();
            DialogGUIVerticalLayout bandwidthColumn = new DialogGUIVerticalLayout();
            DialogGUIVerticalLayout transmissionColumn = new DialogGUIVerticalLayout();
            for (int i = 0; i < parts.Count; i++)
            {
                Part thisPart = parts[i];
                if(thisPart.FindModuleImplementing<ModuleCommand>() != null)
                {
                    continue; // skip command part since it can't transmit data
                }

                ModuleDataTransmitter antennaModule;
                if ((antennaModule = thisPart.FindModuleImplementing<ModuleDataTransmitter>()) != null)
                {
                    DialogGUIToggle toggleBtn = new DialogGUIToggle(false, thisPart.partInfo.title, delegate (bool b) { scienceAntennaSelected(b, antennaModule.DataRate, antennaModule.DataResourceCost); });
                    DialogGUILabel bandwidth = new DialogGUILabel(string.Format("Bandwidth: {0:0.00} charge/s", antennaModule.DataRate));
                    DialogGUILabel rate = new DialogGUILabel(string.Format("Transmission: {0:0.00} charge/s", antennaModule.DataResourceCost));

                    antennaGroup.AddChild(toggleBtn);
                    bandwidthColumn.AddChild(bandwidth);
                    transmissionColumn.AddChild(rate);
                }
            }
            antennaLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { antennaGroup, bandwidthColumn, transmissionColumn }));

            DialogGUIScrollList antennaScrollPane = new DialogGUIScrollList(new Vector2(dialogWidth - 50, dialogHeight / 4), false, true, antennaLayout);
            contentPaneLayout.AddChild(antennaScrollPane);

            // RESULT
            DialogGUILabel scienceResults = new DialogGUILabel(getScienceResults, true, false);
            contentPaneLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { scienceResults }));
        }

        private void scienceSelected(bool toggleState, string scienceID)
        {
            ScienceExperiment thisExp = ResearchAndDevelopment.GetExperiment(scienceID);
            if (toggleState)
            {
                totalScienceDataSize += thisExp.baseValue * thisExp.dataScale;
            }
            else
            {
                totalScienceDataSize -= thisExp.baseValue * thisExp.dataScale;
            }
        }

        private void customScienceSelected(bool toggleState)
        {
            if (toggleState)
            {
                totalScienceDataSize += customScienceDataSize;
            }
            else
            {
                totalScienceDataSize -= customScienceDataSize;
            }
        }

        private string customScienceInput(string userInput)
        {
            if (!float.TryParse(userInput, out customScienceDataSize))
                customScienceDataSize = 0;

            return customScienceDataSize.ToString(); // DialogGUITextInput never uses the returned string.
        }

        private void scienceAntennaSelected(bool toggleState, float bandwidth, double chargeCost)
        {
            if(toggleState)
            {
                antennaBandwidthPerSec = bandwidth;
                antennaChargePerSec = chargeCost;
            }
        }

        private string getScienceResults()
        {
            string message = "<b>Science report:</b>\n";

            double duration = totalScienceDataSize / antennaBandwidthPerSec;
            double cost = duration * antennaChargePerSec;
            
            message += string.Format("Total science data: {0:0.0} Mits\n", totalScienceDataSize);
            message += string.Format("Total power required: {0:0.0} charges for {1:0.00} seconds\n", cost, duration);
            message += string.Format("Science bonus from the signal strength ({0:0.00}%): {1}%\n\n", currentConnectionStrength, GameVariables.Instance.GetDSNScienceCurve().Evaluate(currentConnectionStrength) * 100);

            if (ecReport.currentCapacity - cost < 0.0)
            {
                message += "Transmission: <color=red>Insufficient power</color> to transmit all of the selected experiments";
            }
            else
            {
                message += "Transmission: <color=green>Enough power</color> to transmit all of the selected experiments in one go";
            }

            return message;
        }

        //------------------------------------
        // Power section
        //------------------------------------

        private void drawPowerInfo(List<Part> parts)
        {
            DialogGUILabel massiveMessageLabel = new DialogGUILabel(getPowerReportMessage, true, false);
            DialogGUILabel powerWarning = new DialogGUILabel(getWarningPowerMessage, true, false);

            contentPaneLayout.AddChild(new DialogGUIVerticalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { massiveMessageLabel, new DialogGUISpace(12) ,powerWarning }));
        }

        private string getPowerReportMessage()
        {
            string message = "<b>Storage of electric charge</b>\n";
            message += string.Format("Current usable storage: {0:0.0} / {1:0.0} charge", ecReport.currentCapacity - ecReport.lockedCapacity, ecReport.maxCapacity) + "\n";
            message += string.Format("Reserved storage: {0:0.0} charge", ecReport.lockedCapacity) +"\n\n";

            message += "<b>Electric charge flow</b>\n";
            message += string.Format("Production rate: {0:0.0} charge/s", ecReport.productionRate) + "\n";
            message += string.Format("Consumption rate: {0:0.0} charge/s (antenna drain: {1:0.0})", ecReport.consumptionRate + vesselAntennaDrainPower, vesselAntennaDrainPower) + "\n";
            message += string.Format("Overall rate: {0:0.0} charge/s", ecReport.estimatedOverallRate - vesselAntennaDrainPower) + "\n";

            return message;
        }
    }
}
