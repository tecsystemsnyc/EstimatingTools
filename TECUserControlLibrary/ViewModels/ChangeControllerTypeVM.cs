using EstimatingLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TECUserControlLibrary.ViewModels
{
    public class ChangeControllerTypeVM : ViewModelBase
    {
        public List<TECControllerType> Types { get; }
        public TECProvidedController Controller { get; }

        public ICommand ChangeCommand { get; private set; }

        public ChangeControllerTypeVM(TECProvidedController controller, IEnumerable<TECControllerType> types)
        {
            this.Controller = controller;
            this.Types = getCompatibleTypes(types);

            ChangeCommand = new RelayCommand<TECControllerType>(changeTypeExecute, canChangeType);
        }

        private void changeTypeExecute(TECControllerType type)
        {
            Controller.ChangeType(type);
        }
        private bool canChangeType(TECControllerType type)
        {
            return Controller.CanChangeType(type) && type != Controller.Type;
        }

        private List<TECControllerType> getCompatibleTypes(IEnumerable<TECControllerType> types)
        {
            List<TECControllerType> compatible = new List<TECControllerType>();
            foreach(TECControllerType type in types.Where(x => this.Controller.CanChangeType(x)))
            {
                compatible.Add(type);
            }
            return compatible;
        }

    }
}
