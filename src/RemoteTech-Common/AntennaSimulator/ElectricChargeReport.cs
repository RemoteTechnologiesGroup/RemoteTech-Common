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

        public double maxCapacity = 0;
        public double currentCapacity = 0;
        public double lockedCapacity = 0;
        public double consumptionRate = 0.0;
        public double productionRate = 0.0;
        public double estimatedOverallRate = 0.0;

        private PartResourceDefinition ecResDef = PartResourceLibrary.Instance.resourceDefinitions[ECName];

        public ElectricChargeReport(List<Part> parts)
        {
            traverse(parts);
        }

        private void traverse(List<Part> parts)
        {
            for (int i = 0; i < parts.Count; i++)
            {
                Part thisPart = parts[i];

                // consumer except for antenna, whose state is determined by user
                List<IResourceConsumer> rcs;
                if ((rcs = thisPart.Modules.GetModules<IResourceConsumer>().FindAll(a => a.GetConsumedResources().Contains(this.ecResDef))) != null)
                {
                    for(int a=0; a< rcs.Count; a++) // TODO: finish this consumer part
                    {
                        //consumptionRate += rcs[a].GetConsumedResources()
                        //thisPart.resHan
                    }
                }

                // producer
                ModuleDeployableSolarPanel solarModule;
                if ((solarModule = thisPart.FindModuleImplementing<ModuleDeployableSolarPanel>()) != null) // solar panels
                {
                    productionRate += solarModule.flowRate;
                }
                if (thisPart.name != "launchClamp1" && thisPart.FindModuleImplementing<ModuleGenerator>() != null) // RTG
                {
                    ModuleGenerator genModule = thisPart.FindModuleImplementing<ModuleGenerator>();

                    if (genModule.generatorIsActive)
                    {
                        ModuleResource res = genModule.resHandler.outputResources.Find(x => x.name == ECName);
                        productionRate += res.rate;
                    }
                }
                if (thisPart.FindModuleImplementing<ModuleResourceConverter>() != null) // Fuel cells sucking from fuel tanks
                {
                    ModuleResourceConverter conModule = thisPart.FindModuleImplementing<ModuleResourceConverter>();

                    if (conModule.IsActivated)
                    {
                        ResourceRatio resRatio = conModule.outputList.Find(x => x.ResourceName == ECName);
                        productionRate += resRatio.Ratio;
                    }
                }
                if (thisPart.FindModuleImplementing<ModuleAlternator>() != null) // rocket engine running
                {
                    ModuleAlternator altModule = thisPart.FindModuleImplementing<ModuleAlternator>();
                    productionRate += altModule.outputRate;
                }

                // storage
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
}
