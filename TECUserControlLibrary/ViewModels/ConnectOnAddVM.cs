using EstimatingLibrary;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace TECUserControlLibrary.ViewModels
{
    public class ConnectOnAddVM : ViewModelBase
    {

        private List<TECSubScope> toConnect;
        private TECSystem parent;

        public List<TECController> ParentControllers { get; private set; }
        
        public ConnectOnAddVM(IEnumerable<TECSubScope> toConnect, TECSystem parent)
        {
            this.toConnect = new List<TECSubScope>(toConnect);
            this.parent = parent;
            ParentControllers = getCompatibleControllers(parent);
        }

        private List<TECController> getCompatibleControllers(TECSystem parent)
        {
            List<TECController> result = new List<TECController>();
            foreach(TECController controller in parent.Controllers)
            {
                if (controller.CanConnectSubScope(toConnect))
                {
                    result.Add(controller);
                }
            }
            return result;
        }

        public void Update(IEnumerable<TECSubScope> toConnect)
        {
            this.toConnect = new List<TECSubScope>(toConnect);
            ParentControllers = getCompatibleControllers(parent);
            RaisePropertyChanged("ParentControllers");
        }
    }
}
