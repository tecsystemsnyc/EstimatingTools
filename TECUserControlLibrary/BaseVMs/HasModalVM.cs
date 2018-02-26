using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.BaseVMs
{
    public abstract class HasModalVM : ViewModelBase
    {
        public event Action<ViewModelBase> ModalVMStarted;

        protected void StartModal(ViewModelBase vm)
        {
            ModalVMStarted?.Invoke(vm);
        }
    }
}
