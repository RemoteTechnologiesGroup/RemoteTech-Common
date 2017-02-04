using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RemoteTech.Common.AntennaSimulator
{
    // Note: Not intended to be simple and approx report not full-fledged funcitonality
    public class ElectricChargeReport
    {
        public static readonly string ECName = "ElectricCharge";
        private static PartResourceDefinition ecResDef = PartResourceLibrary.Instance.resourceDefinitions[ECName];

        public double maxCapacity = 0.0;
        public double currentCapacity = 0.0;
        public double lockedCapacity = 0.0;
        public double consumptionRateWOAntenna = 0.0;
        public double productionRate = 0.0;
        public double estimatedOverallRate = 0.0;

        public ElectricChargeReport(List<Part> parts)
        {
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
                //Not possible to get realtime consumption rate
                //ModuleReactionWheel rw = thisModule as ModuleReactionWheel;
                //consumptionRateWOAntenna += rw.resHandler.inputResources.Find(x => x.name == ECName).rate;
            }
        }

        private void readProducerData(PartModule thisModule, Part thisPart)
        {
            if(thisModule is ModuleDeployableSolarPanel)
            {
                ModuleDeployableSolarPanel solarModule = thisModule as ModuleDeployableSolarPanel;
                productionRate += solarModule.flowRate;
            }
            if (thisPart.name != "launchClamp1" && thisModule is ModuleGenerator) // RTG
            {
                ModuleGenerator genModule = thisModule as ModuleGenerator;
                if (genModule.generatorIsActive)
                {
                    ModuleResource res = genModule.resHandler.outputResources.Find(x => x.name == ECName);
                    productionRate += res.rate;
                }
            }
            if (thisModule is ModuleResourceConverter) // Fuel cell sucking from fuel tanks
            {
                ModuleResourceConverter conModule = thisModule as ModuleResourceConverter;
                if (conModule.IsActivated)
                {
                    ResourceRatio resRatio = conModule.outputList.Find(x => x.ResourceName == ECName);
                    productionRate += resRatio.Ratio;
                }
            }
            if (thisModule is ModuleAlternator) // rocket engine running
            {
                ModuleAlternator altModule = thisModule as ModuleAlternator;
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
