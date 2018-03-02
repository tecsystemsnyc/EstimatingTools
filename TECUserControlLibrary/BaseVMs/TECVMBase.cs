using EstimatingLibrary;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.BaseVMs
{
    public abstract class TECVMBase : ViewModelBase
    {
        public event Action<ViewModelBase> ModalVMStarted;
        public event Action<TECObject> ObjectSelected;

        protected void StartModal(ViewModelBase vm)
        {
            ModalVMStarted?.Invoke(vm);
        }
        protected void RaiseSelectedChanged(TECObject obj)
        {
            ObjectSelected?.Invoke(obj);
        }
    }
}
