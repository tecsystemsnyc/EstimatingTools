using EstimatingLibrary;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.Models
{
    public class TemplateUpdatePropertiesItem : ViewModelBase
    {
        private TECSubScope subScope;

        public RelayCommand<TECTemplates> UpdateReferencesCommand { get; private set; }

        public TemplateUpdatePropertiesItem(TECSubScope subScope)
        {
            this.subScope = subScope;
            UpdateReferencesCommand = new RelayCommand<TECTemplates>(executeUpdate, canExecuteUpdate);
        }

        private void executeUpdate(TECTemplates templates)
        {
            templates.UpdateSubScopeReferenceProperties(subScope);
        }

        private bool canExecuteUpdate(TECTemplates templates)
        {
            return templates != null && templates.IsTemplateObject(subScope);
        }
    }
}
