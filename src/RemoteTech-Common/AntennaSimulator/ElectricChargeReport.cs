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
        public double antennaRate = 0.0;
        public double estimatedOverallRate = 0.0;

        private PartResourceDefinition ecResDef;

        public ElectricChargeReport(List<Part> parts)
        {
            traverse(parts);
            this.ecResDef = PartResourceLibrary.Instance.resourceDefinitions[ECName];
        }

        private void traverse(List<Part> parts)
        {
            for (int i = 0; i < parts.Count; i++)
            {
                Part thisPart = parts[i];
                
                // consumer
                if (thisPart.FindModuleImplementing<ModuleDataTransmitter>() != null)
                {
                    ModuleDeployableAntenna delayModule;
                    if ((delayModule = thisPart.FindModuleImplementing<ModuleDeployableAntenna>()) != null)
                    {
                        if(delayModule.deployState == ModuleDeployablePart.DeployState.EXTENDED)
                        {

                        }
                    }

                    if (RUIutils.Any<IResourceConsumer>(thisPart.Modules.GetModules<IResourceConsumer>(), (IResourceConsumer a) => a.GetConsumedResources().Contains(this.ecResDef)))
                    {

                    }
                }
                List<IResourceConsumer> rcs;
                if ((rcs = thisPart.Modules.GetModules<IResourceConsumer>().FindAll(a => a.GetConsumedResources().Contains(this.ecResDef))) != null)
                {
                    for(int a=0; a< rcs.Count; a++)
                    {
                        //consumptionRate += rcs[a].GetConsumedResources()
                        //thisPart.resHan
                    }
                }

                // producer
                ModuleDeployableSolarPanel solarModule;
                if ((solarModule = thisPart.FindModuleImplementing<ModuleDeployableSolarPanel>()) != null)
                {
                    productionRate += solarModule.flowRate;
                }
                if (thisPart.name != "launchClamp1" && thisPart.FindModuleImplementing<ModuleGenerator>() != null)
                {
                    ModuleGenerator genModule = thisPart.FindModuleImplementing<ModuleGenerator>();

                    if (genModule.generatorIsActive)
                    {
                        ModuleResource res = genModule.resHandler.outputResources.Find(x => x.name == ECName);
                        productionRate += res.rate;
                    }
                }
                if (thisPart.FindModuleImplementing<ModuleResourceConverter>() != null)
                {
                    ModuleResourceConverter conModule = thisPart.FindModuleImplementing<ModuleResourceConverter>();

                    if (conModule.IsActivated)
                    {
                        ResourceRatio resRatio = conModule.outputList.Find(x => x.ResourceName == ECName);
                        productionRate += resRatio.Ratio;
                    }
                }
                if (thisPart.FindModuleImplementing<ModuleAlternator>() != null)
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
