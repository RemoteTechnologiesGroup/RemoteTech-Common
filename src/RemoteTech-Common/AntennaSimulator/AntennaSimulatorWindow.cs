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
    public class AntennaSimulatorWindow : AbstractDialog
    {
        private enum SimContentType { RANGE, POWER, SCIENCE };
        private SimContentType whichContentOnDisplay;
        private DialogGUIVerticalLayout contentRows;

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
            whichContentOnDisplay = SimContentType.RANGE; // the section a player see for the first time
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

            DialogGUIButton rangeButton = new DialogGUIButton("Antenna range", delegate { whichContentOnDisplay = SimContentType.RANGE; refreshContent(); }, false);
            DialogGUIButton scienceButton = new DialogGUIButton("Science data", delegate { whichContentOnDisplay = SimContentType.SCIENCE; refreshContent(); }, false);
            DialogGUIButton powerButton = new DialogGUIButton("Power system", delegate { whichContentOnDisplay = SimContentType.POWER; refreshContent(); }, false);
            DialogGUIButton refreshButton = new DialogGUIButton("Refresh", refreshContent, false);
            contentComponents.Add(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { rangeButton, scienceButton, powerButton, new DialogGUIFlexibleSpace(), refreshButton }));

            DialogGUIBase[] rows = new DialogGUIBase[] { new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true) };
            contentRows = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, rows);
            contentComponents.Add(new DialogGUIScrollList(Vector2.one, false, true, contentRows));

            return contentComponents;
        }

        protected override void OnAwake(object[] args)
        {
            refreshContent();
        }

        private void registerContentComponents()
        {
            if (contentRows.children.Count <= 1)
                return;

            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(contentRows.uiItem.gameObject.transform);
            for (int i = 1; i < contentRows.children.Count; i++)
            {
                contentRows.children[i].Create(ref stack, HighLogic.UISkin);
            }
        }

        private void deleteContentComponents()
        {
            if (contentRows.children.Count <= 1)
                return;

            int size = contentRows.children.Count;
            for (int i = size - 1; i >= 1; i--)
            {
                DialogGUIBase thisChild = contentRows.children[i];
                if (!(thisChild is DialogGUIContentSizer)) // avoid if DialogGUIContentSizer is detected
                {
                    contentRows.children.RemoveAt(i);
                    thisChild.uiItem.gameObject.DestroyGameObjectImmediate();
                }
            }
        }

        private void refreshContent()
        {
            List<Part> parts;
            if (HighLogic.LoadedSceneIsFlight)
                parts = FlightGlobals.ActiveVessel.Parts;
            else
                parts = EditorLogic.fetch.ship.Parts;

            ecReport = new ElectricChargeReport(parts);

            deleteContentComponents();
            switch (whichContentOnDisplay)
            {
                case SimContentType.RANGE:
                    displayRangeInfo(parts);
                    break;
                case SimContentType.POWER:
                    displayPowerInfo(parts);
                    break;
                case SimContentType.SCIENCE:
                    displayScienceInfo(parts);
                    break;
            }
            registerContentComponents();

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

        private void displayRangeInfo(List<Part> parts)
        {
            double partialControlRangeMultipler = 0.2; //TODO: get from RT's CustomGameParams

            //reset information data
            antennaModules.Clear();
            totalAntennaComPower = 0;
            totalAntennaDrainPower = 0;

            DialogGUIBox currentNodeBox = new DialogGUIBox("", (dialogWidth - 50)/2, dialogHeight / 2, null, new DialogGUIBox[] { });
            DialogGUIBox targetNodeBox = new DialogGUIBox("", (dialogWidth - 50) / 2, dialogHeight / 2, null, new DialogGUIBox[] { });

            // CURRENT NODE LAYOUT
            DialogGUIVerticalLayout currentNodeLayout = new DialogGUIVerticalLayout(true, false, 4, new RectOffset(5,5,5,5), TextAnchor.MiddleLeft, new DialogGUIBase[] { });
            DialogGUILabel message = new DialogGUILabel("<b>List of antennas detected:</b>", true, false);
            currentNodeLayout.AddChild(message);

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
                    DialogGUILabel comPowerLabel = new DialogGUILabel("Com power: "+ UiUtils.RoundToNearestMetricFactor(antennaModule.CommPower));
                    DialogGUILabel powerConsLabel = new DialogGUILabel(string.Format("Drain: {0:0.00} charge/s", antennaModule.DataResourceCost));
                    currentNodeLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { toggleBtn, comPowerLabel, powerConsLabel }));

                    antennaModules.Add(antennaModule);
                }
            }

            DialogGUILabel combinablePower = new DialogGUILabel(getCombinablePowerMessage, true, false);
            DialogGUILabel powerWarning = new DialogGUILabel(getWarningPowerMessage, true, false);
            currentNodeLayout.AddChild(combinablePower);
            currentNodeLayout.AddChild(powerWarning);
            currentNodeBox.AddChild(currentNodeLayout);

            // TARGET NODE LAYOUT
            DialogGUIVerticalLayout targetNodeLayout = new DialogGUIVerticalLayout(true, false, 4, new RectOffset(5,5,5,5), TextAnchor.MiddleLeft, new DialogGUIBase[] { });
            targetNodeLayout.AddChild(message);

            DialogGUILabel nodeMessage = new DialogGUILabel("<b>Communication nodes</b>", true, false);
            contentRows.AddChild(nodeMessage);
            contentRows.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { currentNodeBox, new DialogGUISpace(5), targetNodeBox }));

            DialogGUILabel resultMessage = new DialogGUILabel("<b>Estimated ranges and other predictions</b>", true, false);
            contentRows.AddChild(resultMessage);
            Texture2D graphImageTxt = new Texture2D(400, 100, TextureFormat.ARGB32, false);
            for (int y = 0; y < graphImageTxt.height; y++)
            {
                for (int x = 0; x < graphImageTxt.width; x++)
                    graphImageTxt.SetPixel(x, y, Color.grey);
            }
            graphImageTxt.Apply();
            DialogGUIImage graphImage = new DialogGUIImage(new Vector2(graphImageTxt.width, graphImageTxt.height), Vector2.zero, Color.white, graphImageTxt);
            contentRows.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), graphImage, new DialogGUIFlexibleSpace() }));
            
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

        private void displayScienceInfo(List<Part> parts)
        {
            if (ResearchAndDevelopment.Instance == null)
            {
                DialogGUILabel message = new DialogGUILabel("The research and development facility is closed.", true, false);
                contentRows.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { message }));
            }
            else
            {
                DialogGUILabel message = new DialogGUILabel("<b>List of science experiments available:</b>", true, false);
                contentRows.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { message }));

                List<string> experimentIDs = ResearchAndDevelopment.GetExperimentIDs();
                for (int j = 0; j < experimentIDs.Count; j++)
                {
                    ScienceExperiment thisExp = ResearchAndDevelopment.GetExperiment(experimentIDs[j]);

                    DialogGUIToggle toggleBtn = new DialogGUIToggle(false, string.Format("{0} - {1} Mits", thisExp.experimentTitle, thisExp.dataScale * thisExp.baseValue) , delegate(bool b) { scienceSelected(b, thisExp.id); });
                    contentRows.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { toggleBtn }));
                }

                DialogGUIToggle customToggleBtn = new DialogGUIToggle(false, "Custom data size (Mits)", customScienceSelected, 120, 24);
                DialogGUITextInput sizeInput = new DialogGUITextInput("", false, 5, customScienceInput);
                contentRows.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { customToggleBtn, sizeInput, new DialogGUIFlexibleSpace() }));

                //scienceText += string.Format("Science bonus from the signal strength: {0:0.0}%\n", 12.2);
                //scienceText += string.Format("Total power required: {0:0.00} charges for {1:0.00} seconds\n", 1234, 1234);
                //scienceText += "<color=red>Warning: Insufficient power to transmit the largest science data (123 Mits)</color>\n";
            }
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

        private void displayPowerInfo(List<Part> parts)
        {
            DialogGUILabel massiveMessageLabel = new DialogGUILabel(getPowerReportMessage, true, false);
            DialogGUILabel powerWarning = new DialogGUILabel(getWarningPowerMessage, true, false);

            contentRows.AddChild(new DialogGUIVerticalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { massiveMessageLabel, new DialogGUISpace(12) ,powerWarning }));
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
