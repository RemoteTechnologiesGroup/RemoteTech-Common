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
    public class ElectricChargeReportFixedUpdate : MonoBehaviour
    {
        public void FixedUpdate()
        {
            //TODO: spawn momobehaviour and measure the flow in fixedupdate
        }
    }

    public class ElectricChargeReport
    {
        public enum ContextMode { EDITOR, FLIGHT }

        public static readonly string ECName = "ElectricCharge";
        private static PartResourceDefinition ecResDef = PartResourceLibrary.Instance.resourceDefinitions[ECName];
        public static readonly string description = "<color=red>Warning:</color> This power-measurement functionality is simple and approximate. For example, solar panels in the editor are ignored because there is no sun there.";

        public ContextMode mode;
        public double maxCapacity = 0.0;
        public double currentCapacity = 0.0;
        public double lockedCapacity = 0.0;
        public double consumptionRateWOAntenna = 0.0;
        public double productionRate = 0.0;
        public double overallRateWOAntenna
        {
            get { return productionRate - consumptionRateWOAntenna; }
        }

        public ElectricChargeReport(List<Part> parts)
        {
            if (HighLogic.LoadedSceneIsFlight)
                mode = ContextMode.FLIGHT;
            else
                mode = ContextMode.EDITOR;

            traverse(parts);
        }

        private void traverse(List<Part> parts)
        {
            for (int i = 0; i < parts.Count; i++)
            {
                Part thisPart = parts[i];

                for(int x=0; x< thisPart.Modules.Count; x++)
                {
                    readConsumerData(thisPart.Modules[x], thisPart);
                    readProducerData(thisPart.Modules[x], thisPart);
                }

                readStorageData(thisPart);
            }
        }

        private void readConsumerData(PartModule thisModule, Part thisPart)
        {
            if(thisModule is ModuleReactionWheel)
            {
                //cannot get even approximate rate
            }
            else if (thisModule is ModuleCommand) // unmanned probes consuming charge
            {
                ModuleCommand mc = thisModule as ModuleCommand;
                ModuleResource mr = mc.resHandler.inputResources.Find(x => x.name == ECName);
                if (mr != null)
                    consumptionRateWOAntenna += mr.rate;
            }
            else if (thisModule is ModuleEnginesFX) // ion engine
            {
                ModuleEnginesFX efx = thisModule as ModuleEnginesFX;
                Propellant ecProp = efx.propellants.Find(x => x.name == ECName);
                if (ecProp != null && this.mode == ContextMode.FLIGHT && efx.isOperational)
                {
                    float massFlowRate = (efx.currentThrottle * efx.maxThrust) / (efx.atmosphereCurve.Evaluate(0) * 9.81f); // TODO: wrong calculate
                    consumptionRateWOAntenna += (ecProp.ratio * massFlowRate);
                }
            }
            else if (thisModule is ModuleLight)
            {
                ModuleLight ml = thisModule as ModuleLight;
                ModuleResource mr = ml.resHandler.inputResources.Find(x => x.name == ECName);
                if (mr != null)
                {
                    if((ml.isOn && this.mode == ContextMode.FLIGHT) || this.mode == ContextMode.EDITOR)
                        consumptionRateWOAntenna += mr.rate;
                }
            }
            else if (thisPart.name != "launchClamp1" && thisModule is ModuleGenerator) // maybe third-party parts?
            {
                ModuleGenerator genModule = thisModule as ModuleGenerator;
                ModuleResource res = genModule.resHandler.inputResources.Find(x => x.name == ECName);
                if (res != null)
                {
                    if (this.mode == ContextMode.FLIGHT)
                    {
                        if (genModule.generatorIsActive || genModule.isAlwaysActive)
                            consumptionRateWOAntenna += res.rate;
                    }
                    else
                    {
                        consumptionRateWOAntenna += res.rate;
                    }
                }
            }
            else if(thisModule is ModuleWheelMotor)
            {
                //cannot get even approximate rate
            }
            else if(thisModule is ModuleActiveRadiator)
            {
                ModuleActiveRadiator ar = thisModule as ModuleActiveRadiator;
                ModuleResource res = ar.resHandler.inputResources.Find(x => x.name == ECName);
                if(res != null)
                {
                    if(this.mode == ContextMode.FLIGHT && ar.IsCooling)
                    {
                        consumptionRateWOAntenna += res.rate;
                    }
                }
            }
            else if(thisModule is ModuleEnviroSensor)
            {
                ModuleEnviroSensor es = thisModule as ModuleEnviroSensor;
                ModuleResource res = es.resHandler.inputResources.Find(x => x.name == ECName);
                if (res != null)
                {
                    consumptionRateWOAntenna += res.rate;
                }
            }
            else if(thisModule is ModuleResourceConverter)
            {
                ModuleResourceConverter rc = thisModule as ModuleResourceConverter;
                ResourceRatio ratio = rc.inputList.Find(x => x.ResourceName == ECName);
                if (this.mode == ContextMode.FLIGHT)
                {
                    if (rc.IsActivated || rc.AlwaysActive)
                        consumptionRateWOAntenna += ratio.Ratio;
                }
            }
            else if (thisModule is ModuleResourceHarvester)
            {
                ModuleResourceHarvester rh = thisModule as ModuleResourceHarvester;
                ResourceRatio ratio = rh.inputList.Find(x => x.ResourceName == ECName);
                if (this.mode == ContextMode.FLIGHT)
                {
                    if (rh.IsActivated || rh.AlwaysActive)
                        consumptionRateWOAntenna += ratio.Ratio;
                }
            }
            else if(thisModule is ModuleAsteroidDrill)
            {
                ModuleAsteroidDrill ad = thisModule as ModuleAsteroidDrill;
                ResourceRatio ratio = ad.inputList.Find(x => x.ResourceName == ECName);
                if (this.mode == ContextMode.FLIGHT)
                {
                    if (ad.IsActivated || ad.AlwaysActive)
                        consumptionRateWOAntenna += ratio.Ratio;
                }
            }
        }

        private void readProducerData(PartModule thisModule, Part thisPart)
        {
            if(thisModule is ModuleDeployableSolarPanel)
            {
                ModuleDeployableSolarPanel solarModule = thisModule as ModuleDeployableSolarPanel;
                if (this.mode == ContextMode.FLIGHT)
                    productionRate += solarModule.flowRate;
                // can't use chargeRate attribute because it could give false security in editor (all panels output the full rate)
            }
            else if (thisPart.name != "launchClamp1" && thisModule is ModuleGenerator) // RTG
            {
                ModuleGenerator genModule = thisModule as ModuleGenerator;
                ModuleResource res = genModule.resHandler.outputResources.Find(x => x.name == ECName);
                if (res != null)
                {
                    if (this.mode == ContextMode.FLIGHT)
                    {
                        if (genModule.generatorIsActive || genModule.isAlwaysActive)
                            productionRate += res.rate;
                    }
                    else
                    {
                        productionRate += res.rate;
                    }
                }
            }
            else if (thisModule is ModuleResourceConverter) // Fuel cell sucking from fuel tanks
            {
                ModuleResourceConverter conModule = thisModule as ModuleResourceConverter;
                ResourceRatio resRatio = conModule.outputList.Find(x => x.ResourceName == ECName); // struct type so can't be null
                if (this.mode == ContextMode.FLIGHT)
                {
                    if (conModule.IsActivated || conModule.AlwaysActive)
                        productionRate += resRatio.Ratio;
                }
                else
                {
                    productionRate += resRatio.Ratio;
                }
            }
            else if (thisModule is ModuleAlternator) // rocket engine running
            {
                ModuleAlternator altModule = thisModule as ModuleAlternator;
                if (this.mode == ContextMode.FLIGHT)
                    productionRate += altModule.outputRate;
            }
        }

        private void readStorageData(Part thisPart)
        {
            if (thisPart.Resources.Count >= 1)
            {
                if (thisPart.Resources.Contains(ECName))
                {
                    PartResource ec = thisPart.Resources.Get(ECName);

                    maxCapacity += ec.maxAmount;
                    currentCapacity += ec.amount;

                    if (!ec.flowState)
                        lockedCapacity += ec.amount;
                }
            }
        }
    }
}
