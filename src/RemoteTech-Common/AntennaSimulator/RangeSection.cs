using RemoteTech.Common.RemoteTechCommNet;
using RemoteTech.Common.UI;
using RemoteTech.Common.Utils;
using Smooth.Algebraics;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteTech.Common.AntennaSimulator
{
    public class RangeSection : SimulatorSection
    {
        private enum TargetType { CUSTOMISED, TARGET, DSN };

        private List<Tuple<ModuleDataTransmitter, bool>> vesselAntennas = new List<Tuple<ModuleDataTransmitter, bool>>();
        private List<ProtoPartModuleSnapshot> targetAntennas = new List<ProtoPartModuleSnapshot>();
        private DialogGUIVerticalLayout targetPanelLayout;
        private DialogGUIVerticalLayout targetAntennaLayout;
        private ITargetable savedTarget;

        private static readonly Texture2D satTexture = UiUtils.LoadTexture("commSat");
        private Texture2D rangeAreaTxt;
        private Texture2D graphAreaTxt;

        public double vesselAntennaComPower = 0.0;
        public double targetAntennaComPower = 0.0;
        public double targetDistance = 0.0;
        public double vesselAntennaDrainPower = 0.0;
        public double currentConnectionStrength = 50.0;
        public double connectionMaxRange = 0.0;
        private double partialControlRangeMultipler = 0.2; //TODO: get from RT's CustomGameParams

        private UIStyle style;

        public RangeSection(AntennaSimulator simulator) : base(SimulationType.RANGE, simulator)
        {
            style = new UIStyle();
            style.alignment = TextAnchor.MiddleLeft;
            style.fontStyle = FontStyle.Normal;
            style.normal = new UIStyleState();
            style.normal.textColor = Color.white;
        }

        public override void analyse(List<Part> parts)
        {
            //reset information data
            vesselAntennaComPower = 0;
            vesselAntennaDrainPower = 0;
            targetAntennaComPower = 0;
            targetDistance = 0;
            vesselAntennas.Clear();

            //list the antennas detected on the active vessel
            for (int i = 0; i < parts.Count; i++)
            {
                bool inUseState = false;
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

                    vesselAntennas.Add(new Tuple<ModuleDataTransmitter, bool>(antennaModule, inUseState));
                }
            }
        }

        public override DialogGUIBase[] draw()
        {
            List<DialogGUIBase> components = new List<DialogGUIBase>();

            //YOUR VESSEL PANEL
            components.Add(new DialogGUILabel("<b>Your vessel with antennas detected:</b>", true, false));
            components.Add(new DialogGUILabel(getVesselAttributeMessage, true, false));

            DialogGUIVerticalLayout vesselLayout = new DialogGUIVerticalLayout(true, false, 0, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { });
            vesselLayout.AddChild(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));

            DialogGUIVerticalLayout antennaColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout comPowerColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
            DialogGUIVerticalLayout drainPowerColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);

            for (int i = 0; i < vesselAntennas.Count; i++)
            {
                Tuple<ModuleDataTransmitter, bool> thisAntenna = vesselAntennas[i];
                ModuleDataTransmitter antennaModule = thisAntenna.Item1;
                int antennaIndex = i; // antennaModules.Count doesn't work due to the compiler optimization

                DialogGUIToggle toggleBtn = new DialogGUIToggle(thisAntenna.Item2, antennaModule.part.partInfo.title, delegate (bool b) { vesselAntennaSelected(b, antennaIndex); }, 150, 32);
                DialogGUILabel comPowerLabel = new DialogGUILabel(string.Format("Com power: {0:0}", UiUtils.RoundToNearestMetricFactor(antennaModule.CommPower, 2)), style); comPowerLabel.size = new Vector2(120, 32);
                DialogGUILabel powerDrainLabel = new DialogGUILabel(string.Format("Drain: {0:0.00} charge/s", antennaModule.DataResourceCost), style); powerDrainLabel.size = new Vector2(120, 32);

                antennaColumn.AddChild(toggleBtn);
                comPowerColumn.AddChild(comPowerLabel);
                drainPowerColumn.AddChild(powerDrainLabel);
            }

            vesselLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { antennaColumn, comPowerColumn, drainPowerColumn }));
            DialogGUIScrollList antennaScrollPane = new DialogGUIScrollList(new Vector2(AntennaSimulator.dialogWidth - 50, AntennaSimulator.dialogHeight / 3), false, true, vesselLayout);
            components.Add(antennaScrollPane);

            // VESSEL'S TARGET PANEL
            components.Add(new DialogGUILabel("\n<b>Target of your vessel:</b>", true, false));
            components.Add(new DialogGUILabel(getTargetAttributeMessage, true, false));

            DialogGUIToggle commNodeToggleBtn = new DialogGUIToggle(true, "Custom Target", delegate (bool b) { displayTargetContent(b, TargetType.CUSTOMISED); });
            DialogGUIToggle targetToggleBtn = new DialogGUIToggle(false, "Designated Target", delegate (bool b) { displayTargetContent(b, TargetType.TARGET); });
            DialogGUIToggle DSNToggleBtn = new DialogGUIToggle(false, "Deep Space Network", delegate (bool b) { displayTargetContent(b, TargetType.DSN); });
            components.Add(new DialogGUIHorizontalLayout(10, 10, 0, new RectOffset(5, 5, 0, 0), TextAnchor.UpperLeft, new DialogGUIBase[] { new DialogGUIToggleGroup(new DialogGUIToggle[] { commNodeToggleBtn, targetToggleBtn, DSNToggleBtn }) }));

            DialogGUIVerticalLayout targetLayout = new DialogGUIVerticalLayout(true, false, 0, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true) });
            targetPanelLayout = new DialogGUIVerticalLayout(10, 10, 0, new RectOffset(5, 25, 5, 5), TextAnchor.UpperLeft, new DialogGUIBase[] { });// empty initially
            drawCustomTarget(targetPanelLayout);
            targetLayout.AddChild(targetPanelLayout);

            DialogGUIScrollList targetScrollPane = new DialogGUIScrollList(new Vector2(AntennaSimulator.dialogWidth - 50, AntennaSimulator.dialogHeight / 4), false, true, targetLayout);
            components.Add(targetScrollPane);

            //RESULTS
            components.Add(new DialogGUILabel("\n<b>Estimated ranges and other predictions:</b>", true, false));

            rangeAreaTxt = new Texture2D(AntennaSimulator.dialogWidth - 50, satTexture.height + 40, TextureFormat.ARGB32, false);
            renderRangeTexture(rangeAreaTxt);
            components.Add(new DialogGUIImage(new Vector2(rangeAreaTxt.width, rangeAreaTxt.height), Vector2.zero, Color.white, rangeAreaTxt));
            components.Add(new DialogGUILabel(getFullActionRange, true, false));
            components.Add(new DialogGUILabel(getPartialActionRange, true, false));
            components.Add(new DialogGUILabel(getConnectionStatus, true, false));
            components.Add(new DialogGUILabel("\n", true, false));

            graphAreaTxt = new Texture2D(AntennaSimulator.dialogWidth - 50, 300, TextureFormat.ARGB32, false);
            renderGraphTexture(graphAreaTxt);
            components.Add(new DialogGUIImage(new Vector2(graphAreaTxt.width, graphAreaTxt.height), Vector2.zero, Color.white, graphAreaTxt));
            components.Add(new DialogGUILabel(getGraphStatus, true, false));
            components.Add(new DialogGUILabel("\n", true, false));

            components.Add(new DialogGUILabel(getWarningPowerMessage, true, false));

            return components.ToArray();
        }

        public override void destroy()
        {
            UnityEngine.GameObject.DestroyImmediate(rangeAreaTxt, true);
            UnityEngine.GameObject.DestroyImmediate(graphAreaTxt, true);
        }

        private void drawCustomTarget(DialogGUIVerticalLayout paneLayout)
        {
            DialogGUILabel commPowerLabel = new DialogGUILabel("Communication power", style, true, false);
            DialogGUITextInput powerInput = new DialogGUITextInput("0", false, 12, customTargetComPowerEntered, 110, 32);
            paneLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { commPowerLabel, powerInput }));

            DialogGUILabel distanceLabel = new DialogGUILabel("Distance from your vessel", style, true, false);
            DialogGUITextInput distanceInput = new DialogGUITextInput("0", false, 12, customTargetDistanceEntered, 110, 32);
            paneLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { distanceLabel, distanceInput }));
        }

        private void drawUserTarget(DialogGUIVerticalLayout paneLayout)
        {
            paneLayout.AddChild(new DialogGUILabel(getTargetInfo, true, false));
            drawTargetAntennas(ref targetAntennaLayout);
            paneLayout.AddChild(targetAntennaLayout);
        }

        private void drawDSNTarget(DialogGUIVerticalLayout paneLayout)
        {
            //TrackingStationBuilding
            List<DialogGUIToggle> DSNLevels = new List<DialogGUIToggle>();

            int numLevels = ScenarioUpgradeableFacilities.GetFacilityLevelCount(SpaceCenterFacility.TrackingStation) + 1; // why is GetFacilityLevelCount zero-index??
            for (int lvl = 0; lvl < numLevels; lvl++)
            {
                float normalizedLevel = (1f / numLevels) * lvl;
                double dsnPower = GameVariables.Instance.GetDSNRange(normalizedLevel);
                DialogGUIToggle DSNLevel = new DialogGUIToggle(false, string.Format("Level {0} - Max Power: {1}", lvl + 1, UiUtils.RoundToNearestMetricFactor(dsnPower, 1)), delegate (bool b) { DSNLevelSelected(b, dsnPower); });
                DSNLevels.Add(DSNLevel);
            }

            DialogGUIToggleGroup DSNLevelGroup = new DialogGUIToggleGroup(DSNLevels.ToArray());

            paneLayout.AddChild(new DialogGUILabel("Tracking Station Levels:", true, false));
            paneLayout.AddChild(new DialogGUIVerticalLayout(true, false, 0, new RectOffset(), TextAnchor.UpperLeft, new DialogGUIBase[] { DSNLevelGroup }));
        }

        private string getTargetInfo()
        {
            ITargetable potentialTarget = FlightGlobals.fetch.VesselTarget;
            Vessel activeVessel = FlightGlobals.fetch.activeVessel;

            //Condition checks
            try
            {
                targetChangedEvent();

                if (!HighLogic.LoadedSceneIsFlight || activeVessel == null)
                    throw new Exception("Available in the flight scene only.");
                else if (potentialTarget == null)
                    throw new Exception("Please designate your target.");
                else if (!(potentialTarget is CelestialBody) && !(potentialTarget is Vessel))
                    throw new Exception("This target is neither vessel nor celestial body.");
            }
            catch (Exception e)
            {
                clearTargetData();
                return e.Message;
            }

            targetDistance = Vector3d.Distance(activeVessel.transform.position, potentialTarget.GetTransform().position);
            return string.Format("Target: {0} ({1})", potentialTarget.GetName(), potentialTarget.GetType());
        }

        private void targetChangedEvent()
        {
            ITargetable potentialTarget = FlightGlobals.fetch.VesselTarget;
            Vessel activeVessel = FlightGlobals.fetch.activeVessel;

            if (potentialTarget == null)
            {
                AbstractDialog.deregisterLayoutComponents(targetAntennaLayout);
                savedTarget = null;
            }
            else if (savedTarget != null)
            {
                if (!savedTarget.GetTransform().position.Equals(potentialTarget.GetTransform().position)) // detect target change
                {
                    AbstractDialog.deregisterLayoutComponents(targetAntennaLayout);
                    drawTargetAntennas(ref targetAntennaLayout);
                    AbstractDialog.registerLayoutComponents(targetAntennaLayout);
                    savedTarget = potentialTarget;
                }
            }
            else if (savedTarget == null)
            {
                AbstractDialog.deregisterLayoutComponents(targetAntennaLayout);
                drawTargetAntennas(ref targetAntennaLayout);
                AbstractDialog.registerLayoutComponents(targetAntennaLayout);
                savedTarget = potentialTarget;
            }
        }

        private void drawTargetAntennas(ref DialogGUIVerticalLayout antennaLayout)
        {
            if (antennaLayout == null) // for the first time
                antennaLayout = new DialogGUIVerticalLayout(10, 10, 0, new RectOffset(), TextAnchor.UpperLeft, new DialogGUIBase[] { });

            ITargetable potentialTarget = FlightGlobals.fetch.VesselTarget;
            Vessel activeVessel = FlightGlobals.fetch.activeVessel;

            clearTargetData();

            if (potentialTarget == null)
            {
                return;
            }
            else if (potentialTarget is CelestialBody)
            {
                DialogGUILabel commPowerLabel = new DialogGUILabel("Communication power", style, true, false);
                DialogGUITextInput powerInput = new DialogGUITextInput("0", false, 12, customTargetComPowerEntered, 110, 32);
                antennaLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { commPowerLabel, powerInput }));
            }
            else if (potentialTarget is Vessel)
            {
                Vessel thisTarget = potentialTarget as Vessel;

                DialogGUIVerticalLayout antennaColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);
                DialogGUIVerticalLayout comPowerColumn = new DialogGUIVerticalLayout(false, false, 0, new RectOffset(), TextAnchor.MiddleLeft);

                List<ProtoPartSnapshot> parts = thisTarget.protoVessel.protoPartSnapshots;
                for (int i = 0; i < parts.Count; i++)
                {
                    ProtoPartModuleSnapshot thisAntennaModule = parts[i].FindModule("ModuleDataTransmitter");
                    if (thisAntennaModule == null)
                        continue;

                    //TODO: switch to RT's antenna system to get antenna data. 
                    double antennaComPower = 0;
                    antennaComPower = thisAntennaModule.moduleValues.TryGetValue("antennaPower", ref antennaComPower) ? antennaComPower : 1337; //Lacking antenna data in packed snapshot

                    targetAntennas.Add(thisAntennaModule);

                    int antennaIndex = targetAntennas.Count - 1; // targetAntennas.Count doesn't work due to the compiler optimization
                    DialogGUIToggle toggleBtn = new DialogGUIToggle(false, parts[i].partInfo.title, delegate (bool b) { targetAntennaSelected(b, antennaIndex); }, 170, 32);
                    DialogGUILabel comPowerLabel = new DialogGUILabel(string.Format("Com power: {0:0}", UiUtils.RoundToNearestMetricFactor(antennaComPower, 2)), style); comPowerLabel.size = new Vector2(150, 32);

                    antennaColumn.AddChild(toggleBtn);
                    comPowerColumn.AddChild(comPowerLabel);
                }

                antennaLayout.AddChild(new DialogGUILabel("\nAntennas detected:", true, false));
                antennaLayout.AddChild(new DialogGUIHorizontalLayout(true, false, 0, new RectOffset(), TextAnchor.MiddleLeft, new DialogGUIBase[] { antennaColumn, comPowerColumn }));
            }
        }

        private string customTargetComPowerEntered(string userInput)
        {
            if (!double.TryParse(userInput, out targetAntennaComPower))
                targetAntennaComPower = 0;

            return userInput;
        }

        private string customTargetDistanceEntered(string userInput)
        {
            if (!double.TryParse(userInput, out targetDistance))
                targetDistance = 0;

            return userInput;
        }

        private void DSNLevelSelected(bool toggleState, double DSNPower)
        {
            if (toggleState)
                targetAntennaComPower = DSNPower;
        }

        private void vesselAntennaSelected(bool toggleState, int indexAntenna)
        {
            if (toggleState)
            {
                vesselAntennaComPower += vesselAntennas[indexAntenna].Item1.CommPower;
                vesselAntennaDrainPower += vesselAntennas[indexAntenna].Item1.DataResourceCost;
            }
            else
            {
                vesselAntennaComPower -= vesselAntennas[indexAntenna].Item1.CommPower;
                vesselAntennaDrainPower -= vesselAntennas[indexAntenna].Item1.DataResourceCost;
            }
        }

        private void targetAntennaSelected(bool toggleState, int indexAntenna)
        {
            double antennaComPower = 0;
            if (toggleState)
            {
                targetAntennaComPower += targetAntennas[indexAntenna].moduleValues.TryGetValue("antennaPower", ref antennaComPower) ? antennaComPower : 1337;
            }
            else
            {
                targetAntennaComPower -= targetAntennas[indexAntenna].moduleValues.TryGetValue("antennaPower", ref antennaComPower) ? antennaComPower : 1337;
            }
        }

        private string getVesselAttributeMessage()
        {
            return string.Format("Total Com Power: {0}", UiUtils.RoundToNearestMetricFactor(vesselAntennaComPower, 1));
        }

        private string getTargetAttributeMessage()
        {
            return string.Format("Total Com Power: {0}\nDistance from your vessel: {1}m", UiUtils.RoundToNearestMetricFactor(targetAntennaComPower, 1), UiUtils.RoundToNearestMetricFactor(targetDistance, 2));
        }

        private string getWarningPowerMessage()
        {
            ElectricChargeReport chargeReport = (simulator.getSection(SimulationType.POWER) as PowerSection).PowerReport;

            if (chargeReport.flowRateWOAntenna + vesselAntennaDrainPower < 0)
                return string.Format("<color=red>Warning:</color> Estimated to run out of usable power in {0:0.0} seconds", (chargeReport.currentCapacity - chargeReport.lockedCapacity) / (chargeReport.flowRateWOAntenna + vesselAntennaDrainPower));
            else
                return "Sustainable power for working connection";
        }

        private string getConnectionStatus()
        {
            string premessage = "Connectivity: ";
            renderRangeTexture(rangeAreaTxt);

            connectionMaxRange = RemoteTechCommNetScenario.RangeModel.GetMaximumRange(vesselAntennaComPower, targetAntennaComPower);

            if(connectionMaxRange <= 0)
            {
                return premessage + "<color=red>Zero comm power to transmit</color>";
            }
            else if (targetDistance > 0)
            {
                if (targetDistance > connectionMaxRange) // out of range
                    return premessage + "<color=red>Out of range!</color>";
                else
                    return premessage + "<color=green>Can connect</color>";
            }
            else // target distance is zero
            {
                return string.Format("{0} Max range is {1}m", premessage, UiUtils.RoundToNearestMetricFactor(connectionMaxRange, 2));
            }
            
        }

        private string getGraphStatus()
        {
            return "Unfinished Graph - pending on RT Antenna System";
        }

        private string getFullActionRange()
        {
            return string.Format("Maximum full probe control: {0}m", UiUtils.RoundToNearestMetricFactor(connectionMaxRange * (1.0 - partialControlRangeMultipler), 2));
        }

        private string getPartialActionRange()
        {
            return string.Format("Maximum partial probe control: {0}m", UiUtils.RoundToNearestMetricFactor(RemoteTechCommNetScenario.RangeModel.GetMaximumRange(vesselAntennaComPower, targetAntennaComPower), 2));
        }

        private void displayTargetContent(bool toggleState, TargetType node)
        {
            if (!toggleState)
                return;

            clearTargetData();
            AbstractDialog.deregisterLayoutComponents(targetPanelLayout);
            switch (node)
            {
                case TargetType.CUSTOMISED:
                    drawCustomTarget(targetPanelLayout);
                    break;
                case TargetType.TARGET:
                    drawUserTarget(targetPanelLayout);
                    break;
                case TargetType.DSN:
                    drawDSNTarget(targetPanelLayout);
                    break;
            }
            AbstractDialog.registerLayoutComponents(targetPanelLayout);
        }

        private void clearTargetData()
        {
            targetAntennaComPower = 0;
            targetDistance = 0;
            targetAntennas.Clear();
            savedTarget = null;
        }

        private void renderRangeTexture(Texture2D rangeTexture)
        {
            //center points
            int vesselX = 20 + satTexture.width / 2;
            int vesselY = rangeTexture.height / 2;
            int targetX = rangeAreaTxt.width - 20 - satTexture.width / 2;
            int targetY = vesselY;

            //background
            for (int y = 0; y < rangeAreaTxt.height; y++)
            {
                for (int x = 0; x < rangeAreaTxt.width; x++)
                    rangeAreaTxt.SetPixel(x, y, Color.gray);
            }

            //draw connection line
            int connectionLeftX = vesselX + satTexture.width / 2;
            int connectionRightX = targetX - satTexture.width / 2; // can reach target
            int connectionY = rangeAreaTxt.height / 2;

            if (targetDistance > 0 && connectionMaxRange < targetDistance && connectionMaxRange > 0) //out of range
            {
                connectionRightX = connectionLeftX + (int)((connectionRightX- connectionLeftX) * (connectionMaxRange / targetDistance));
            }
            else if(connectionMaxRange <= 0) // absolutely no connection
            {
                connectionRightX = connectionLeftX;
            }

            for (int x = connectionLeftX; x <= connectionRightX; x++)
            {
                rangeAreaTxt.SetPixel(x, connectionY - 1, Color.green);
                rangeAreaTxt.SetPixel(x, connectionY, Color.green);
                rangeAreaTxt.SetPixel(x, connectionY + 1, Color.green);
            }

            //draw two sats at left and right side
            for (int x = 0; x < satTexture.width; x++)
            {
                for (int y = 0; y < satTexture.height; y++)
                {
                    if (satTexture.GetPixel(x, y).a > 0f)
                    {
                        rangeAreaTxt.SetPixel(vesselX - (satTexture.width / 2) + x, vesselY - (satTexture.height / 2) + y, satTexture.GetPixel(x, y));
                        rangeAreaTxt.SetPixel(targetX - (satTexture.width / 2) + x, targetY - (satTexture.height / 2) + y, satTexture.GetPixel(x, y));
                    }
                }
            }

            rangeAreaTxt.Apply();
        }

        private void renderGraphTexture(Texture2D graphTexture)
        {
            //background
            for (int y = 0; y < graphTexture.height; y++)
            {
                for (int x = 0; x < graphTexture.width; x++)
                    graphTexture.SetPixel(x, y, Color.gray);
            }

            graphAreaTxt.Apply();
        }
    }
}
