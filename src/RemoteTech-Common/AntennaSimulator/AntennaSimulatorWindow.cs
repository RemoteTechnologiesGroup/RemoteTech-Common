using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RemoteTech.Common.UI;
using UnityEngine;
using RemoteTech.Common.RemoteTechCommNet;

namespace RemoteTech.Common.AntennaSimulator
{
    public class AntennaSimulatorWindow : AbstractDialog
    {
        private DialogGUILabel rightVesselLabel;
        private DialogGUILabel leftVesselLabel;
        private DialogGUILabel resultLabel;
        private DialogGUILabel scienceLabel;

        private bool refresh = true;
        DialogGUIButton refreshButton = null;

        private double prevCurrECAmt = 0.0, overallECRate = 0.0;  

        public AntennaSimulatorWindow() : base("",
                                        0.8f,
                                        0.5f,
                                        550,
                                        450,
                                        new DialogOptions[] { DialogOptions.HideDismissButton, DialogOptions.AllowBgInputs})
        {

        }

        protected override List<DialogGUIBase> drawContentComponents()
        {
            List<DialogGUIBase> contentComponents = new List<DialogGUIBase>();

            leftVesselLabel = new DialogGUILabel("", true, false);
            rightVesselLabel = new DialogGUILabel("", true, false);
            DialogGUIButton swapVesselButton = new DialogGUIButton("<=>", null, 32, 32, false);
            contentComponents.Add(new DialogGUIHorizontalLayout(true, true, 4, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { leftVesselLabel, swapVesselButton, rightVesselLabel }));

            resultLabel = new DialogGUILabel("", true, false);
            contentComponents.Add(new DialogGUIHorizontalLayout(true, true, 4, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { resultLabel }));

            if (ResearchAndDevelopment.Instance != null)
            {
                scienceLabel = new DialogGUILabel("", true, false);
                contentComponents.Add(new DialogGUIHorizontalLayout(true, true, 4, new RectOffset(), TextAnchor.UpperCenter, new DialogGUIBase[] { scienceLabel }));
            }

            refreshButton = new DialogGUIButton("Pause plot", refreshClick, 80, 24, false);
            contentComponents.Add(new DialogGUIHorizontalLayout(false, false, 4, new RectOffset(), TextAnchor.MiddleCenter, new DialogGUIBase[] { new DialogGUIFlexibleSpace(), refreshButton, new DialogGUIFlexibleSpace() }));

            return contentComponents;
        }

        private void refreshClick()
        {
            if (refresh)
                refreshButton.SetOptionText("Resume plot");
            else
                refreshButton.SetOptionText("Pause plot");

            refresh = !refresh;
        }

        //Issue: memory leak of repeatedly creating report
        protected override void OnUpdate()
        {
            if (!refresh)
                return;

            if (EditorLogic.fetch == null && FlightGlobals.fetch == null)
                return;

            string vesselName;
            List<Part> parts;

            if (HighLogic.LoadedSceneIsFlight)
            {
                parts = FlightGlobals.ActiveVessel.Parts;
                vesselName = FlightGlobals.ActiveVessel.vesselName;
            }
            else
            {
                parts = EditorLogic.fetch.ship.Parts;
                vesselName = EditorLogic.fetch.ship.shipName;
            }

            ElectricChargeReport ecReport = new ElectricChargeReport(parts);
            string activeVesselText = string.Format("<b>This vessel ({0})</b>\n", vesselName);
            activeVesselText += string.Format("Power storage: {0:0.0}/{1:0.0}\n", ecReport.currentCapacity, ecReport.maxCapacity - ecReport.lockedCapacity);
            activeVesselText += "List of antennas detected:\n";

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

            double sourceComPower = 0, targetComPower = 0;
            double partialControlRangeMultipler = 0.2; //TODO: get from RT's CustomGameParams

            for (int i = 0; i < parts.Count; i++)
            {
                Part thisPart = parts[i];
                ModuleDataTransmitter antennaModule;
                if ((antennaModule = thisPart.FindModuleImplementing<ModuleDataTransmitter>()) != null)
                {
                    activeVesselText += string.Format("{0} {1} - Com power: {2}, Power consumption: {3:0.00} charge/s\n", (i%2==0)?"✔":"✖", thisPart.partInfo.title, antennaModule.CommPower, 0);
                    sourceComPower = antennaModule.CommPower;
                }
            }

            targetComPower = sourceComPower; // temp
            activeVesselText += string.Format("Combinable com power: {0}\n", sourceComPower); // I dunno
            activeVesselText += string.Format("<color=red>Warning: {0}</color>\n", "Not sustainable, estimated to run out of power in 123 seconds"); 
            leftVesselLabel.SetOptionText(activeVesselText);

            string targetVesselText = "<b>Target node</b>\n";
            targetVesselText += "Type: Com node or Target or DSN\n";
            targetVesselText += "Target: VESSEL12 (Distance: 1234 m) or Not applicable\n";
            targetVesselText += "Custom com power: 1234 or blank\n";

            rightVesselLabel.SetOptionText(targetVesselText);

            string resultText = "\n<b>Range simulation</b>\n";
            double maxPartialControlRange = RemoteTechCommNetScenario.RangeModel.GetMaximumRange(sourceComPower, targetComPower);
            double maxFullControlRange = maxPartialControlRange*(1.0-partialControlRangeMultipler);

            resultText += string.Format("Full probe control range: {0} m\nPartial probe control range: {1} m\n", maxFullControlRange, maxPartialControlRange);
            resultText += "~ Graph ~\n";
            resultText += "--------------------------------------\n";
            resultText += "| YOU------------FC-------PC      TAR |\n";
            resultText += "--------------------------------------\n";
            resultText += "Color legend     GREEN--------------RED\n";
            resultText += "<color=red>Warning: Out of range</color>\n";

            resultLabel.SetOptionText(resultText);            

            if (ResearchAndDevelopment.Instance != null)
            {
                string scienceText = "\n<b>Science data transmission</b>\n";
                scienceText += "List of science experiments:\n";

                List<string> expIDs = ResearchAndDevelopment.GetExperimentIDs();
                for(int j=0; j<3; j++)
                {
                    ScienceExperiment thisExp = ResearchAndDevelopment.GetExperiment(expIDs[j]);
                    scienceText += string.Format("{0} - {1} of {2} Mits\n", "✔", thisExp.experimentTitle, thisExp.dataScale*thisExp.baseValue);
                }
                scienceText += string.Format("{0} - {1} of {2} Mits\n", "✖", "Custom data", 0);
                scienceText += string.Format("Science bonus from the signal strength: {0:0.0}%\n", 12.2);
                scienceText += string.Format("Total power required: {0:0.00} charges for {1:0.00} seconds\n", 1234, 1234);
                scienceText += "<color=red>Warning: Insufficient power to transmit the largest science data (123 Mits)</color>\n";

                scienceLabel.SetOptionText(scienceText);
            }
        }
    }
}
