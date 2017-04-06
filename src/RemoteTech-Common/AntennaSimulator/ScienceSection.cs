using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteTech.Common.AntennaSimulator
{
    public class ScienceSection : SimulatorSection
    {
        public float antennaBandwidthPerSec;
        public double antennaChargePerSec;
        public float totalScienceDataSize = 0;
        private float customScienceDataSize = 0;
        private bool customDataInputSelected = false;

        private List<ModuleDataTransmitter> antennas = new List<ModuleDataTransmitter>();

        public ScienceSection(AntennaSimulator simulator) : base(SimulationType.SCIENCE, simulator) { }

        public override void analyse(List<Part> parts)
        {
            antennas.Clear();

            for (int i = 0; i < parts.Count; i++)
            {
                Part thisPart = parts[i];
                if (thisPart.FindModuleImplementing<ModuleCommand>() != null)
                {
                    continue; // skip command part since it can't transmit data
                }

                ModuleDataTransmitter antennaModule;
                if ((antennaModule = thisPart.FindModuleImplementing<ModuleDataTransmitter>()) != null)
                {
                    antennas.Add(antennaModule);
                }
            }
        }

        public override DialogGUIBase[] draw()
        {
            List<DialogGUIBase> components = new List<DialogGUIBase>();

            if (ResearchAndDevelopment.Instance == null)
                return components.ToArray();

            totalScienceDataSize = 0;
            customScienceDataSize = 0;
            antennaBandwidthPerSec = 0;
            antennaChargePerSec = 0;

            // SCIENCE LIST
            components.Add(new DialogGUILabel("<b>List of science experiments available:</b>", true, false));
            DialogGUIVerticalLayout scienceLayout = new DialogGUIVerticalLayout(true, false, 0, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { });
            scienceLayout.AddChild(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            List<string> experimentIDs = ResearchAndDevelopment.GetExperimentIDs();
            for (int j = 0; j < experimentIDs.Count; j++)
            {
                ScienceExperiment thisExp = ResearchAndDevelopment.GetExperiment(experimentIDs[j]);

                DialogGUIToggle toggleBtn = new DialogGUIToggle(false, string.Format("{0} - {1} Mits", thisExp.experimentTitle, thisExp.dataScale * thisExp.baseValue), delegate (bool b) { scienceSelected(b, thisExp.id); });
                scienceLayout.AddChild(toggleBtn);
            }

            DialogGUIToggle customToggleBtn = new DialogGUIToggle(false, "Custom data size (Mits)", customScienceSelected, 120, 24);
            DialogGUITextInput sizeInput = new DialogGUITextInput("", false, 5, customScienceInput);
            scienceLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { customToggleBtn, sizeInput, new DialogGUIFlexibleSpace() }));

            DialogGUIScrollList scienceScrollPane = new DialogGUIScrollList(new Vector2(AntennaSimulator.dialogWidth - 50, AntennaSimulator.dialogHeight / 3), false, true, scienceLayout);
            components.Add(scienceScrollPane);

            // ANTENNA LIST
            components.Add(new DialogGUILabel("<b>List of antennas detected:</b>", true, false));
            DialogGUIVerticalLayout antennaLayout = new DialogGUIVerticalLayout(true, false, 0, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { });
            antennaLayout.AddChild(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            DialogGUIToggleGroup antennaGroup = new DialogGUIToggleGroup();
            DialogGUIVerticalLayout antennaColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout bandwidthColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout transmissionColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            UIStyle style = new UIStyle();
            style.alignment = TextAnchor.MiddleLeft;
            style.fontStyle = FontStyle.Normal;
            style.normal = new UIStyleState();
            style.normal.textColor = Color.white;

            for(int i=0; i< antennas.Count; i++)
            {
                ModuleDataTransmitter antenna = antennas[i];

                if(i==0)
                    scienceAntennaSelected(true, antenna.DataRate, antenna.DataResourceCost);

                DialogGUIToggle toggleBtn = new DialogGUIToggle(i==0?true:false, antenna.part.partInfo.title, delegate (bool b) { scienceAntennaSelected(b, antenna.DataRate, antenna.DataResourceCost); }, 150, 32);
                DialogGUILabel bandwidth = new DialogGUILabel(string.Format("Bandwidth: {0:0.00} charge/s", antenna.DataRate), style); bandwidth.size = new Vector2(150, 32);
                DialogGUILabel rate = new DialogGUILabel(string.Format("Transmission: {0:0.00} charge/s", antenna.DataResourceCost), style); rate.size = new Vector2(150, 32);

                antennaGroup.AddChild(toggleBtn);
                bandwidthColumn.AddChild(bandwidth);
                transmissionColumn.AddChild(rate);
            }

            antennaColumn.AddChild(antennaGroup);
            antennaLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { antennaColumn, bandwidthColumn, transmissionColumn }));

            DialogGUIScrollList antennaScrollPane = new DialogGUIScrollList(new Vector2(AntennaSimulator.dialogWidth - 50, AntennaSimulator.dialogHeight / 4), false, true, antennaLayout);
            components.Add(antennaScrollPane);

            // RESULT
            DialogGUILabel scienceResults = new DialogGUILabel(getScienceResults, true, false);
            components.Add(new DialogGUIHorizontalLayout(true, false, 4, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { scienceResults }));

            return components.ToArray();
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
            customDataInputSelected = toggleState;
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
            float newInputData = 0f;
            bool success = float.TryParse(userInput, out newInputData);
            if (success && customDataInputSelected)
            {
                totalScienceDataSize -= customScienceDataSize;
                totalScienceDataSize += newInputData;
                customScienceDataSize = newInputData;
            }

            return customScienceDataSize.ToString(); // DialogGUITextInput never uses the returned string.
        }

        private void scienceAntennaSelected(bool toggleState, float bandwidth, double chargeCost)
        {
            if (toggleState)
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

            PowerSection pow = this.simulator.getSection(SimulationType.POWER) as PowerSection;
            RangeSection ran = this.simulator.getSection(SimulationType.RANGE) as RangeSection;

            message += string.Format("Total science data: {0:0.0} Mits\n", totalScienceDataSize);
            message += string.Format("Total power required: {0:0.0} charges for {1:0.00} seconds ({2:0.0} charges available)\n", cost, duration, pow.PowerReport.currentCapacity - pow.PowerReport.lockedCapacity);
            message += string.Format("Science bonus from the signal strength ({0:0.00}%): {1}%\n\n", ran.currentConnectionStrength, GameVariables.Instance.GetDSNScienceCurve().Evaluate(ran.currentConnectionStrength) * 100);

            if (pow.PowerReport.currentCapacity - pow.PowerReport.lockedCapacity - cost < 0.0)
            {
                message += "Transmission: <color=red>Insufficient power</color> to transmit all of the selected experiments";
            }
            else
            {
                message += "Transmission: <color=green>Enough power</color> to transmit all of the selected experiments in one go";
            }

            return message;
        }
    }
}
