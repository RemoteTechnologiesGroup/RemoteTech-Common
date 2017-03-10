using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteTech.Common.AntennaSimulator
{
    public class PowerSection : SimulatorWindowSection
    {
        public ElectricChargeReport chargeReport=null;
        private AntennaSimulatorWindow primary;

        public override DialogGUIBase[] create(List<Part> parts, AntennaSimulatorWindow primary)
        {
            this.primary = primary;
            this.chargeReport = new ElectricChargeReport();
            this.chargeReport.monitor(parts);

            List<DialogGUIBase> components = new List<DialogGUIBase>();

            DialogGUILabel massiveMessageLabel = new DialogGUILabel(getPowerReportMessage, true, false);
            DialogGUILabel powerWarning = new DialogGUILabel(getWarningPowerMessage, true, false);

            components.Add(massiveMessageLabel);
            components.Add(powerWarning);

            return components.ToArray();
        }

        public override void destroy()
        {
            if (chargeReport != null)
                chargeReport.terminate();
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

            message += "<b>Antennas, producers and consumers</b>\n";
            message += string.Format("Approx production rate: {0:0.00} charge/s\n", chargeReport.productionRate);
            message += string.Format("Approx consumption rate: {0:0.00} charge/s\n", chargeReport.consumptionRateWOAntenna);
            message += string.Format("Power drain of standby antennas selected: {0:0.00} charge/s\n", this.primary.vesselAntennaDrainPower);
            message += string.Format("Approx flow rate: {0:0.00} charge/s", chargeReport.flowRateWOAntenna - this.primary.vesselAntennaDrainPower);

            return message;
        }

        private string getWarningPowerMessage()
        {
            if (chargeReport == null)
                return "Probing the vessel parts...";

            if (true) // LOOK!
                return string.Format("<color=red>Warning:</color> Estimated to run out of usable power in {0:0.0} seconds", (chargeReport.currentCapacity - chargeReport.lockedCapacity) / (chargeReport.flowRateWOAntenna + this.primary.vesselAntennaDrainPower));
            else
                return "Sustainable";
        }
    }
}
