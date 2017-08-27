using System;
using System.Collections.Generic;
using UnityEngine;

namespace RemoteTech.Common.AntennaSimulator
{
    public class PowerSection : SimulatorSection
    {
        private ElectricChargeReport chargeReport=null;
        public ElectricChargeReport PowerReport { get{ return this.chargeReport; } }
        private Texture2D batteryTexture;

        public PowerSection(AntennaSimulator simulator) : base(SimulationType.POWER, simulator) { }

        public override void analyse(List<Part> parts)
        {
            this.chargeReport = new ElectricChargeReport();
            this.chargeReport.monitor(parts);
        }

        public override DialogGUIBase[] draw()
        {
            List<DialogGUIBase> components = new List<DialogGUIBase>();

            DialogGUILabel graphMessageLabel = new DialogGUILabel(ElectricChargeReport.liability + "\n\n<b>Battery status:</b>\n", true, false);
            DialogGUILabel massiveMessageLabel = new DialogGUILabel(getPowerReportMessage, true, false);
            DialogGUILabel powerWarning = new DialogGUILabel(getWarningPowerMessage, true, false);
                        
            batteryTexture = new Texture2D(AntennaSimulator.dialogWidth - 50, 25, TextureFormat.ARGB32, false);
            renderBatteryTexture(batteryTexture);
            DialogGUIImage batteryImage = new DialogGUIImage(new Vector2(batteryTexture.width, batteryTexture.height), Vector2.zero, Color.white, batteryTexture);

            UIStyle style = new UIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            style.fontStyle = FontStyle.Normal;
            style.fontSize = 18;
            style.normal = new UIStyleState();
            style.normal.textColor = Color.white;
            //style.stretchWidth = true;
            DialogGUILabel batteryLabel = new DialogGUILabel(batteryString, style, true, false);

            components.Add(graphMessageLabel);
            components.Add(batteryImage);
            components.Add(new DialogGUIHorizontalLayout(10, 10, 0, new RectOffset(0, 0, -25, 0), TextAnchor.UpperCenter, new DialogGUIBase[] { batteryLabel }));
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

            message += "<b>Storage of electric charge:</b>\n";
            message += string.Format("Current usable storage: {0:0.00} / {1:0.00} charge\n", chargeReport.currentCapacity - chargeReport.lockedCapacity, chargeReport.maxCapacity);
            message += string.Format("Reserved storage: {0:0.00} charge\n", chargeReport.lockedCapacity);
            message += string.Format("Actual flow rate: {0:0.00} charge/s\n\n", chargeReport.vesselFlowRate);

            renderBatteryTexture(batteryTexture);

            RangeSection ran = this.simulator.getSection(SimulationType.RANGE) as RangeSection;

            message += "<b>Antennas, producers and consumers:</b>\n";
            message += string.Format("Approx production rate: {0:0.00} charge/s\n", chargeReport.productionRate);
            message += string.Format("Approx consumption rate: {0:0.00} charge/s\n", chargeReport.consumptionRateWOAntenna);
            message += string.Format("Power drain of antennas selected: {0:0.00} charge/s\n", ran.vesselAntennaDrainPower);
            message += string.Format("Expected flow rate: {0:0.00} charge/s", chargeReport.flowRateWOAntenna - ran.vesselAntennaDrainPower);

            return message;
        }

        private string getWarningPowerMessage()
        {
            if (chargeReport == null)
                return "Probing the vessel parts...";

            string message = "\n<b>Comment:</b>\n";
            double percent = ((chargeReport.currentCapacity - chargeReport.lockedCapacity) / (chargeReport.maxCapacity - chargeReport.lockedCapacity)) * 100.0;
            RangeSection ran = this.simulator.getSection(SimulationType.RANGE) as RangeSection;

            if (chargeReport.vesselFlowRate < 0.0)
                message += string.Format("<color=red>Warning:</color> Running out of usable power in {0:0.0} seconds", (chargeReport.currentCapacity - chargeReport.lockedCapacity) / (chargeReport.flowRateWOAntenna + ran.vesselAntennaDrainPower));
            else if (percent <= 30.0)
                message += "<color=orange>Warning:</color> Low battery juice!";
            else if (percent >= 80.0 && chargeReport.vesselFlowRate >= 0.0)
                message += "<color=green>Lot of juice</color> for your unplanned disassembly fun!";
            else
                message += "Plenty battery juice";

            return message;
        }

        private void renderBatteryTexture(Texture2D batteryTexture)
        {
            Color bgColor = Color.grey;
            Color lockedColor = Color.yellow;
            Color freeColor = new Color(0.22f, 0.71f, 0.29f, 1.0f); //light green;

            int lockedWidth = (int)((chargeReport.lockedCapacity / chargeReport.maxCapacity) * batteryTexture.width);
            int freeWidth = (int)((chargeReport.currentCapacity / chargeReport.maxCapacity) * batteryTexture.width);

            for (int x = 0; x < batteryTexture.width; x++)
            {
                for (int y = 0; y < batteryTexture.height; y++)
                {
                    if(x <= lockedWidth && lockedWidth >= 1)
                        batteryTexture.SetPixel(x, y, lockedColor);
                    else if (x <= freeWidth)
                        batteryTexture.SetPixel(x, y, freeColor);
                    else
                        batteryTexture.SetPixel(x, y, bgColor);
                }
            }
            batteryTexture.Apply();
        }

        private string batteryString()
        {
            double percent = ((chargeReport.currentCapacity - chargeReport.lockedCapacity) / (chargeReport.maxCapacity - chargeReport.lockedCapacity))* 100.0;
            double remainingMins = ((chargeReport.currentCapacity - chargeReport.lockedCapacity) / Math.Abs(chargeReport.vesselFlowRate))/60.0;
            string remainingTime = "";

            if (chargeReport.vesselFlowRate < 0.0) // draining
                remainingTime = string.Format("{0:0.0} mins left", remainingMins);
            else if (percent >= 100.0)
                remainingTime = "full"; 
            else
                remainingTime = "charging";


            return string.Format("{0:0}% ({1})", percent, remainingTime);
        }
    }
}
