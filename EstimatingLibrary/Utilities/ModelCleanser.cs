using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EstimatingLibrary.Utilities
{
    public static class ModelCleanser
    {
        internal static bool addRequiredIOModules(TECProvidedController controller)
        {
            //The IO needed by the points connected to the controller
            IOCollection necessaryIO = new IOCollection();
            bool needsSave = false;

            foreach (TECHardwiredConnection ssConnect in
                controller.ChildrenConnections.Where(con => con is TECHardwiredConnection))
            {
                foreach (TECIO io in ssConnect.Child.HardwiredIO.ToList())
                {
                    for (int i = 0; i < io.Quantity; i++)
                    {
                        //The point IO that exists on our controller at the moment.
                        IOCollection totalPointIO = getPointIO(controller);
                        necessaryIO.Add(io.Type);
                        //Check if our io that exists satisfies the IO that we need.
                        if (!totalPointIO.Contains(necessaryIO))
                        {
                            needsSave = true;
                            bool moduleFound = false;
                            //If it doesn't, we need to add an IO module that will satisfy it.
                            foreach (TECIOModule module in controller.Type.IOModules)
                            {
                                //We only need to check for the type of the last IO that we added.
                                if (module.IOCollection.Contains(io.Type) && controller.CanAddModule(module))
                                {
                                    controller.AddModule(module);
                                    moduleFound = true;
                                    break;
                                }
                            }
                            if (!moduleFound)
                            {
                                controller.DisconnectAll();
                                MessageBox.Show(string.Format("The controller type of the controller '{0}' is incompatible with the connected points. Please review the controller's connections.",
                                                                    controller.Name));

                                return true;
                            }
                        }
                    }
                }
            }
            return needsSave;

            IOCollection getPointIO(TECController con)
            {
                IOCollection pointIOCollection = new IOCollection();
                foreach (TECIO pointIO in controller.IO.ToList().Where(io => (TECIO.PointIO.Contains(io.Type) || TECIO.UniversalIO.Contains(io.Type))))
                {
                    pointIOCollection.Add(pointIO);
                }
                return pointIOCollection;
            }
        }
    }
}
