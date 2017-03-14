using ModuleWheels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RemoteTech.Common.AntennaSimulator
{
    // Note: Intended to be simple and approx report, not full-fledged functionality
    // Credit: Ratzap's Fusebox for references
    public class ElectricChargeReport
    {
        public static readonly string ECName = "ElectricCharge";
        private static PartResourceDefinition ecResDef = PartResourceLibrary.Instance.resourceDefinitions[ECName];
        public static readonly string liability = "Friendly reminder - This power measurement is deliberately simple and approximate.";

        public List<Part> hostVesselparts;
        private ElectricChargeReportMonitor cycleMonitor = null;

        public double maxCapacity = 0.0;
        public double currentCapacity = 0.0;
        public double lockedCapacity = 0.0;
        public double consumptionRateWOAntenna = 0.0;
        public double productionRate = 0.0;
        public double vesselFlowRate = 0.0;
        public double flowRateWOAntenna
        {
            get { return productionRate - consumptionRateWOAntenna; }
        }

        public void clearData()
        {
            this.maxCapacity = 0.0;
            this.currentCapacity = 0.0;
            this.lockedCapacity = 0.0;
            this.consumptionRateWOAntenna = 0.0;
            this.productionRate = 0.0;
            this.vesselFlowRate = 0.0;
        }

        public void monitor(List<Part> parts)
        {
            this.hostVesselparts = parts;

            cycleMonitor = UnityEngine.Object.FindObjectOfType<ElectricChargeReportMonitor>();

            if (cycleMonitor != null)
            {
                cycleMonitor.linkToReport(this);
                cycleMonitor.StartMonitor();
            }
            else
            {
                Logging.Error("Monitor of the electric charge report is not found!");
            }
        }

        public void terminate()
        {
            if (cycleMonitor != null)
            {
                cycleMonitor.StopMonitor();
            }
        }
    }

    [KSPAddon(KSPAddon.Startup.FlightAndEditor, false)]
    public class ElectricChargeReportMonitor : CommonCore
    {
        public enum ContextMode { EDITOR, FLIGHT }
        private ContextMode mode;
        private bool activeMonitorFlag = false;
        private ElectricChargeReport referenceReport =  null;
        private double previousCurrentCapacity = 0.0;

        public new void Start()
        {
            if (HighLogic.LoadedSceneIsFlight)
                mode = ContextMode.FLIGHT;
            else
                mode = ContextMode.EDITOR;
        }

        public void linkToReport(ElectricChargeReport report)
        {
            this.referenceReport = report;
        }

        public void FixedUpdate()
        {
            if (referenceReport == null || !activeMonitorFlag)
                return;

            this.referenceReport.clearData();

            for (int i = 0; i < referenceReport.hostVesselparts.Count; i++)
            {
                Part thisPart = referenceReport.hostVesselparts[i];

                for (int x = 0; x < thisPart.Modules.Count; x++)
                {
                    readConsumerData(thisPart.Modules[x], thisPart);
                    readProducerData(thisPart.Modules[x], thisPart);
                }

                readStorageData(thisPart);
            }

            referenceReport.vesselFlowRate = (referenceReport.currentCapacity - previousCurrentCapacity) / Time.fixedDeltaTime;
            previousCurrentCapacity = referenceReport.currentCapacity;
        }

        public void StartMonitor()
        {
            this.activeMonitorFlag = true;
        }

        public void StopMonitor()
        {
            this.activeMonitorFlag = false;
        }

        private void readConsumerData(PartModule thisModule, Part thisPart)
        {
            if (thisModule is ModuleReactionWheel)
            {
                //cannot get even approximate rate
            }
            else if (thisModule is ModuleCommand) // unmanned probes consuming charge
            {
                ModuleCommand mc = thisModule as ModuleCommand;
                ModuleResource mr = mc.resHandler.inputResources.Find(x => x.name == ElectricChargeReport.ECName);
                if (mr != null)
                    this.referenceReport.consumptionRateWOAntenna += mr.rate;
            }
            else if (thisModule is ModuleEnginesFX) // ion engine
            {
                ModuleEnginesFX efx = thisModule as ModuleEnginesFX;
                Propellant ecProp = efx.propellants.Find(x => x.name == ElectricChargeReport.ECName);
                if (ecProp != null && this.mode == ContextMode.FLIGHT && efx.isOperational)
                {
                    float massFlowRate = (efx.currentThrottle * efx.maxThrust) / (efx.atmosphereCurve.Evaluate(0) * 9.81f); // TODO: wrong calculate
                    this.referenceReport.consumptionRateWOAntenna += (ecProp.ratio * massFlowRate);
                }
            }
            else if (thisModule is ModuleLight)
            {
                ModuleLight ml = thisModule as ModuleLight;
                ModuleResource mr = ml.resHandler.inputResources.Find(x => x.name == ElectricChargeReport.ECName);
                if (mr != null)
                {
                    if ((ml.isOn && this.mode == ContextMode.FLIGHT) || this.mode == ContextMode.EDITOR)
                        this.referenceReport.consumptionRateWOAntenna += mr.rate;
                }
            }
            else if (thisPart.name != "launchClamp1" && thisModule is ModuleGenerator) // maybe third-party parts?
            {
                ModuleGenerator genModule = thisModule as ModuleGenerator;
                ModuleResource res = genModule.resHandler.inputResources.Find(x => x.name == ElectricChargeReport.ECName);
                if (res != null)
                {
                    if (this.mode == ContextMode.FLIGHT)
                    {
                        if (genModule.generatorIsActive || genModule.isAlwaysActive)
                            this.referenceReport.consumptionRateWOAntenna += res.rate;
                    }
                    else
                    {
                        this.referenceReport.consumptionRateWOAntenna += res.rate;
                    }
                }
            }
            else if (thisModule is ModuleWheelMotor)
            {
                //cannot get even approximate rate
            }
            else if (thisModule is ModuleActiveRadiator)
            {
                ModuleActiveRadiator ar = thisModule as ModuleActiveRadiator;
                ModuleResource res = ar.resHandler.inputResources.Find(x => x.name == ElectricChargeReport.ECName);
                if (res != null)
                {
                    if (this.mode == ContextMode.FLIGHT && ar.IsCooling)
                    {
                        this.referenceReport.consumptionRateWOAntenna += res.rate;
                    }
                }
            }
            else if (thisModule is ModuleEnviroSensor)
            {
                ModuleEnviroSensor es = thisModule as ModuleEnviroSensor;
                ModuleResource res = es.resHandler.inputResources.Find(x => x.name == ElectricChargeReport.ECName);
                if (res != null)
                {
                    this.referenceReport.consumptionRateWOAntenna += res.rate;
                }
            }
            else if (thisModule is ModuleResourceConverter)
            {
                ModuleResourceConverter rc = thisModule as ModuleResourceConverter;
                ResourceRatio ratio = rc.inputList.Find(x => x.ResourceName == ElectricChargeReport.ECName);
                if (this.mode == ContextMode.FLIGHT)
                {
                    if (rc.IsActivated || rc.AlwaysActive)
                        this.referenceReport.consumptionRateWOAntenna += ratio.Ratio;
                }
            }
            else if (thisModule is ModuleResourceHarvester)
            {
                ModuleResourceHarvester rh = thisModule as ModuleResourceHarvester;
                ResourceRatio ratio = rh.inputList.Find(x => x.ResourceName == ElectricChargeReport.ECName);
                if (this.mode == ContextMode.FLIGHT)
                {
                    if (rh.IsActivated || rh.AlwaysActive)
                        this.referenceReport.consumptionRateWOAntenna += ratio.Ratio;
                }
            }
            else if (thisModule is ModuleAsteroidDrill)
            {
                ModuleAsteroidDrill ad = thisModule as ModuleAsteroidDrill;
                ResourceRatio ratio = ad.inputList.Find(x => x.ResourceName == ElectricChargeReport.ECName);
                if (this.mode == ContextMode.FLIGHT)
                {
                    if (ad.IsActivated || ad.AlwaysActive)
                        this.referenceReport.consumptionRateWOAntenna += ratio.Ratio;
                }
            }
        }

        private void readProducerData(PartModule thisModule, Part thisPart)
        {
            if (thisModule is ModuleDeployableSolarPanel)
            {
                ModuleDeployableSolarPanel solarModule = thisModule as ModuleDeployableSolarPanel;
                if (this.mode == ContextMode.FLIGHT)
                    this.referenceReport.productionRate += solarModule.flowRate;
                // can't use chargeRate attribute because it could give false security in editor (all panels output the full rate)
            }
            else if (thisPart.name != "launchClamp1" && thisModule is ModuleGenerator) // RTG
            {
                ModuleGenerator genModule = thisModule as ModuleGenerator;
                ModuleResource res = genModule.resHandler.outputResources.Find(x => x.name == ElectricChargeReport.ECName);
                if (res != null)
                {
                    if (this.mode == ContextMode.FLIGHT)
                    {
                        if (genModule.generatorIsActive || genModule.isAlwaysActive)
                            this.referenceReport.productionRate += res.rate;
                    }
                    else
                    {
                        this.referenceReport.productionRate += res.rate;
                    }
                }
            }
            else if (thisModule is ModuleResourceConverter) // Fuel cell sucking from fuel tanks
            {
                ModuleResourceConverter conModule = thisModule as ModuleResourceConverter;
                ResourceRatio resRatio = conModule.outputList.Find(x => x.ResourceName == ElectricChargeReport.ECName); // struct type so can't be null
                if (this.mode == ContextMode.FLIGHT)
                {
                    if (conModule.IsActivated || conModule.AlwaysActive)
                        this.referenceReport.productionRate += resRatio.Ratio;
                }
                else
                {
                    this.referenceReport.productionRate += resRatio.Ratio;
                }
            }
            else if (thisModule is ModuleAlternator) // rocket engine running
            {
                ModuleAlternator altModule = thisModule as ModuleAlternator;
                if (this.mode == ContextMode.FLIGHT)
                    this.referenceReport.productionRate += altModule.outputRate;
            }
        }

        private void readStorageData(Part thisPart)
        {
            if (thisPart.Resources.Count >= 1)
            {
                if (thisPart.Resources.Contains(ElectricChargeReport.ECName))
                {
                    PartResource ec = thisPart.Resources.Get(ElectricChargeReport.ECName);

                    this.referenceReport.maxCapacity += ec.maxAmount;
                    this.referenceReport.currentCapacity += ec.amount;

                    if (!ec.flowState)
                        this.referenceReport.lockedCapacity += ec.amount;
                }
            }
        }
    }
}
