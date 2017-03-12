using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteTech.Common.AntennaSimulator
{
    public class PowerSection : SimulatorSection
    {
        private ElectricChargeReport chargeReport=null;
        public ElectricChargeReport PowerReport { get{ return this.chargeReport; } }
        private Texture2D graphImageTxt;

        public PowerSection(AntennaSimulator simulator) : base(SimulationType.POWER, simulator) { }

        public override void analyse(List<Part> parts)
        {
            this.chargeReport = new ElectricChargeReport();
            this.chargeReport.monitor(parts);
        }

        public override DialogGUIBase[] draw()
        {
            List<DialogGUIBase> components = new List<DialogGUIBase>();

            DialogGUILabel massiveMessageLabel = new DialogGUILabel(getPowerReportMessage, true, false);
            DialogGUILabel powerWarning = new DialogGUILabel(getWarningPowerMessage, true, false);

            DialogGUILabel graphMessageLabel = new DialogGUILabel("Battery status", true, false);
            graphImageTxt = new Texture2D(AntennaSimulator.dialogWidth - 50, 25, TextureFormat.ARGB32, false);
            for (int y = 0; y < graphImageTxt.height; y++)
            {
                for (int x = 0; x < graphImageTxt.width; x++)
                    graphImageTxt.SetPixel(x, y, Color.green);
            }
            graphImageTxt.Apply();
            DialogGUIImage graphImage = new DialogGUIImage(new Vector2(graphImageTxt.width, graphImageTxt.height), Vector2.zero, Color.white, graphImageTxt);

            components.Add(graphMessageLabel);
            components.Add(graphImage);
            components.Add(massiveMessageLabel);
            components.Add(powerWarning);

            return components.ToArray();
        }

        public override void destroy()
        {
            if (this.chargeReport != null)
            {
                this.chargeReport.terminate();
                this.chargeReport = null;
            }
        }

        private string getPowerReportMessage()
        {
            if (chargeReport == null)
                return "Probing the vessel parts...";

            string message = "";

            message += ElectricChargeReport.description + "\n\n";

            message += "<b>Storage of electric charge</b>\n";
            message += string.Format("Current usable storage: {0:0.00} / {1:0.00} charge\n", chargeReport.currentCapacity - chargeReport.lockedCapacity, chargeReport.maxCapacity);
            message += string.Format("Reserved storage: {0:0.00} charge\n", chargeReport.lockedCapacity);
            message += string.Format("Flow rate: {0:0.00} charge/s\n\n", chargeReport.vesselFlowRate);

            RangeSection ran = this.simulator.getSection(SimulationType.RANGE) as RangeSection;

            message += "<b>Antennas, producers and consumers</b>\n";
            message += string.Format("Approx production rate: {0:0.00} charge/s\n", chargeReport.productionRate);
            message += string.Format("Approx consumption rate: {0:0.00} charge/s\n", chargeReport.consumptionRateWOAntenna);
            message += string.Format("Power drain of standby antennas selected: {0:0.00} charge/s\n", ran.vesselAntennaDrainPower);
            message += string.Format("Approx flow rate: {0:0.00} charge/s", chargeReport.flowRateWOAntenna - ran.vesselAntennaDrainPower);

            return message;
        }

        private string getWarningPowerMessage()
        {
            if (chargeReport == null)
                return "Probing the vessel parts...";

            RangeSection ran = this.simulator.getSection(SimulationType.RANGE) as RangeSection;

            if (true) // TODO: finish this
                return string.Format("<color=red>Warning:</color> Estimated to run out of usable power in {0:0.0} seconds", (chargeReport.currentCapacity - chargeReport.lockedCapacity) / (chargeReport.flowRateWOAntenna + ran.vesselAntennaDrainPower));
            else
                return "Sustainable";
        }
    }
}
