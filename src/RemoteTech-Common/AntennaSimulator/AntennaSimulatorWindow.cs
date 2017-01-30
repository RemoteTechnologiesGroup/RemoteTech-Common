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

        private DialogGUIVerticalLayout contentPaneLayout;
        private DialogGUIVerticalLayout targetNodePaneLayout;

        private ElectricChargeReport ecReport;
        private List<ModuleDataTransmitter> antennaModules = new List<ModuleDataTransmitter>();

        private static readonly int dialogWidth = 650;
        private static readonly int dialogHeight = 400;

        public AntennaSimulatorWindow() : base("RemoteTech Antenna Simulator",
                                                0.8f,
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

            DialogGUIHorizontalLayout tabbedButtonRow = new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { rangeButton, powerButton });
            if (ResearchAndDevelopment.Instance != null)
                tabbedButtonRow.AddChild(scienceButton);
            tabbedButtonRow.AddChild(new DialogGUIFlexibleSpace());
            tabbedButtonRow.AddChild(refreshButton);
            contentComponents.Add(tabbedButtonRow);

            DialogGUIBase[] rows = new DialogGUIBase[] { new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true) };
            contentPaneLayout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, rows);
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

        private TargetNode currentTargetNode = TargetNode.CUSTOM;
        private void displayTargetNodeContent() { displayTargetNodeContent(currentTargetNode); }
        private void displayTargetNodeContent(TargetNode whichNode)
        {
            currentTargetNode = whichNode;

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

        private InfoContent currentContent;
        private void displayContent() { displayContent(currentContent); }
        private void displayContent(InfoContent whichContent)
        {
            currentContent = whichContent;

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

            

            //leftVesselLabel.SetOptionText(activeVesselText);

            string targetVesselText = "<b>Target node</b>\n";
            targetVesselText += "Type: Com node or Target or DSN\n";
            targetVesselText += "Target: VESSEL12 (Distance: 1234 m) or Not applicable\n";
            targetVesselText += "Custom com power: 1234 or blank\n";

            //more options like presets of next DSN level?

            //rightVesselLabel.SetOptionText(targetVesselText);

            string resultText = "\n<b>Range simulation</b>\n";
            double maxPartialControlRange = RemoteTechCommNetScenario.RangeModel.GetMaximumRange(0, 0);
            double maxFullControlRange = maxPartialControlRange * (1.0 - 0.2);

            resultText += string.Format("Full probe control range: {0} m\nPartial probe control range: {1} m\n", maxFullControlRange, maxPartialControlRange);
            resultText += "~ Graph ~\n";
            resultText += "--------------------------------------\n";
            resultText += "| YOU------------FC-------PC      TAR |\n";
            resultText += "--------------------------------------\n";
            resultText += "Color legend     GREEN--------------RED\n";
            resultText += "<color=red>Warning: Out of range</color>\n";

            //resultLabel.SetOptionText(resultText);            
        }

        //------------------------------------
        // Range section
        //------------------------------------
        private double totalAntennaComPower = 0.0;
        private double totalAntennaDrainPower = 0.0;
        private double antennaSignalStrength = 50.0;

        private void drawRangeInfo(List<Part> parts)
        {
            double partialControlRangeMultipler = 0.2; //TODO: get from RT's CustomGameParams

            //reset information data
            antennaModules.Clear();
            totalAntennaComPower = 0;
            totalAntennaDrainPower = 0;

            // CURRENT NODE PANE
            DialogGUIVerticalLayout currentNodeLayout = new DialogGUIVerticalLayout(true, false, 4, new RectOffset(5,5,5,5), TextAnchor.MiddleLeft, new DialogGUIBase[] { });
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
                        totalAntennaComPower += antennaModule.CommPower;
                        totalAntennaDrainPower += antennaModule.DataResourceCost;
                    }

                    int antennaIndex = antennaModules.Count; // antennaModules.Count doesn't work due to the compiler optimization

                    DialogGUIToggle toggleBtn = new DialogGUIToggle(inUseState, thisPart.partInfo.title, delegate(bool b) { antennaSelected(b, antennaIndex); }, 130, 24);
                    DialogGUILabel comPowerLabel = new DialogGUILabel("Power: "+ UiUtils.RoundToNearestMetricFactor(antennaModule.CommPower));
                    DialogGUILabel powerConsLabel = new DialogGUILabel(string.Format("Drain: {0:0.00} charge/s", antennaModule.DataResourceCost));
                    currentNodeLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { toggleBtn, comPowerLabel, powerConsLabel }));

                    antennaModules.Add(antennaModule);
                }
            }

            DialogGUILabel combinablePower = new DialogGUILabel(getCombinablePowerMessage, true, false);
            DialogGUILabel powerWarning = new DialogGUILabel(getWarningPowerMessage, true, false);
            currentNodeLayout.AddChild(combinablePower);
            currentNodeLayout.AddChild(powerWarning);

            // TARGET NODE PANE
            DialogGUIVerticalLayout targetNodeLayout = new DialogGUIVerticalLayout(true, false, 4, new RectOffset(5,5,5,5), TextAnchor.MiddleLeft, new DialogGUIBase[] { });
            DialogGUILabel message2 = new DialogGUILabel("<b>Target</b>", true, false);
            targetNodeLayout.AddChild(message2);

            DialogGUIToggle commNodeToggleBtn = new DialogGUIToggle(true, "Custom node", delegate(bool b) { displayTargetNodeContent(TargetNode.CUSTOM); });
            DialogGUIToggle targetToggleBtn = new DialogGUIToggle(false, "Designated target", delegate (bool b) { displayTargetNodeContent(TargetNode.TARGET); });
            DialogGUIToggle DSNToggleBtn = new DialogGUIToggle(false, "Deep Space Network", delegate (bool b) { displayTargetNodeContent(TargetNode.DSN); });
            DialogGUIToggleGroup targetToggleGroup = new DialogGUIToggleGroup(new DialogGUIToggle[] { commNodeToggleBtn, targetToggleBtn, DSNToggleBtn });
            targetNodeLayout.AddChild(targetToggleGroup);

            DialogGUILabel message3 = new DialogGUILabel("Configuration below:", true, false);
            targetNodeLayout.AddChild(message3);

            targetNodePaneLayout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(10, 25, 10, 10), TextAnchor.UpperLeft, new DialogGUIBase[] { });// empty initially
            drawCustomTargetNode();
            targetNodeLayout.AddChild(targetNodePaneLayout);

            //Top row
            DialogGUILabel nodeMessage = new DialogGUILabel("<b>Connection between your vessel and a target node</b>", true, false);
            contentPaneLayout.AddChild(nodeMessage);
            DialogGUIScrollList targetNodeScrollPane = new DialogGUIScrollList(new Vector2((dialogWidth - 50) / 2, dialogHeight / 2), false, true, targetNodeLayout);
            DialogGUIScrollList currentNodeScrollPane = new DialogGUIScrollList(new Vector2((dialogWidth - 50) / 2, dialogHeight / 2), false, true, currentNodeLayout);
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
            
        }

        private void drawCustomTargetNode()
        {
            DialogGUILabel commPowerLabel = new DialogGUILabel("Com power", 120, 24);
            DialogGUITextInput powerInput = new DialogGUITextInput("", false, 12, null, 24);

            targetNodePaneLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { commPowerLabel, powerInput }));
        }

        private void drawUserTargetNode()
        {
            DialogGUILabel massTargetLabel = new DialogGUILabel(getTargetInfo, 120, 24);

            targetNodePaneLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { massTargetLabel }));
        }

        private string getTargetInfo()
        {
            ITargetable target;
            Vessel activeVessel;

            if (!HighLogic.LoadedSceneIsFlight)
                return "Only in flight";
            else if ((target = FlightGlobals.fetch.VesselTarget) == null)
                return "Please designate your target";
            else if (!(target is CelestialBody) && !(target is Vessel))
                return "This target is neither vessel nor celestial body";

            activeVessel = FlightGlobals.fetch.activeVessel;

            string message = string.Format("Target: {0} ({1})\n", target.GetName(), target.GetType());
            message += string.Format("Distance: {0}m\n", UiUtils.RoundToNearestMetricFactor(Vector3d.Distance(activeVessel.transform.position, target.GetTransform().position)));
            message += string.Format("Com power: {0}", (target is Vessel)? UiUtils.RoundToNearestMetricFactor(123456) : "Nil"); // replace it when target vessel has RT interface

            return message;
        }

        private void drawDSNTargetNode()
        {
            //TrackingStationBuilding.
            List<DialogGUIToggle> DSNLevels = new List<DialogGUIToggle>();

            int numLevels = ScenarioUpgradeableFacilities.GetFacilityLevelCount(SpaceCenterFacility.TrackingStation) + 1; // why is GetFacilityLevelCount zero-index??
            for (int lvl = 0; lvl < numLevels; lvl++)
            {
                float normalizedLevel = (1f / numLevels) * lvl;
                DialogGUIToggle DSNLevel = new DialogGUIToggle(false, string.Format("Level {0} - Max DSN Power: {1}", lvl+1, UiUtils.RoundToNearestMetricFactor(GameVariables.Instance.GetDSNRange(normalizedLevel))), delegate (bool b) { });
                DSNLevels.Add(DSNLevel);
            }

            DialogGUIToggleGroup DSNLevelGroup = new DialogGUIToggleGroup(DSNLevels.ToArray());

            targetNodePaneLayout.AddChild(new DialogGUIVerticalLayout(true, false, 4, new RectOffset(), TextAnchor.UpperLeft, new DialogGUIBase[] { DSNLevelGroup }));
        }

        private void antennaSelected(bool toggleState, int indexAntenna)
        {
            if (toggleState)
            {
                totalAntennaComPower += antennaModules[indexAntenna].CommPower;
                totalAntennaDrainPower += antennaModules[indexAntenna].DataResourceCost;
            }
            else
            {
                totalAntennaComPower -= antennaModules[indexAntenna].CommPower;
                totalAntennaDrainPower -= antennaModules[indexAntenna].DataResourceCost;
            }
        }

        private string getCombinablePowerMessage()
        {
            return "Combinable com power: "+UiUtils.RoundToNearestMetricFactor(totalAntennaComPower);
        }

        private string getWarningPowerMessage()
        {
            if (true)
                return string.Format("<color=red>Warning:</color> Estimated to run out of usable power in {0:0.0} seconds", (ecReport.currentCapacity-ecReport.lockedCapacity)/totalAntennaDrainPower);
            else
                return "Sustainable";
        }

        

        //------------------------------------
        // Science section
        //------------------------------------
        private float totalScienceDataSize = 0;
        private float customScienceDataSize = 0;

        private void drawScienceInfo(List<Part> parts)
        {
            if (ResearchAndDevelopment.Instance == null)
                return;

            DialogGUILabel message = new DialogGUILabel("<b>List of science experiments available:</b>", true, false);
            contentPaneLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { message }));

            List<string> experimentIDs = ResearchAndDevelopment.GetExperimentIDs();
            for (int j = 0; j < experimentIDs.Count; j++)
            {
                ScienceExperiment thisExp = ResearchAndDevelopment.GetExperiment(experimentIDs[j]);

                DialogGUIToggle toggleBtn = new DialogGUIToggle(false, string.Format("{0} - {1} Mits", thisExp.experimentTitle, thisExp.dataScale * thisExp.baseValue) , delegate(bool b) { scienceSelected(b, thisExp.id); });
                contentPaneLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { toggleBtn }));
            }

            DialogGUIToggle customToggleBtn = new DialogGUIToggle(false, "Custom data size (Mits)", customScienceSelected, 120, 24);
            DialogGUITextInput sizeInput = new DialogGUITextInput("", false, 5, customScienceInput);
            contentPaneLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { customToggleBtn, sizeInput, new DialogGUIFlexibleSpace() }));

            //scienceText += string.Format("Science bonus from the signal strength: {0:0.0}%\n", 12.2);
            //scienceText += string.Format("Total power required: {0:0.00} charges for {1:0.00} seconds\n", 1234, 1234);
            //scienceText += "<color=red>Warning: Insufficient power to transmit the largest science data (123 Mits)</color>\n";
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
            if(toggleState)
            {
                totalScienceDataSize += customScienceDataSize;
            }
            else
                totalScienceDataSize -= customScienceDataSize;
        }

        private string customScienceInput(string userInput)
        {
            bool resultParsing = float.TryParse(userInput, out customScienceDataSize);

            return ""; // DialogGUITextInput never uses the returned string.
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
            message += string.Format("Consumption rate: {0:0.0} charge/s (antenna drain: {1:0.0})", ecReport.consumptionRate + totalAntennaDrainPower, totalAntennaDrainPower) + "\n";
            message += string.Format("Overall rate: {0:0.0} charge/s", ecReport.estimatedOverallRate - totalAntennaDrainPower) + "\n";

            return message;
        }
    }
}
