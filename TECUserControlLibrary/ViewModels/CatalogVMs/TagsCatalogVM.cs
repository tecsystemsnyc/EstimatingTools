using EstimatingLibrary;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static TECUserControlLibrary.ViewModels.MaterialVM;

namespace TECUserControlLibrary.ViewModels.CatalogVMs
{
    public class TagsCatalogVM : CatalogVMBase
    {
        private string _newTagName = "";

        private TECTag _selectedTag;

        public string NewTagName
        {
            get { return _newTagName; }
            set
            {
                if (_newTagName != value)
                {
                    _newTagName = value;
                    RaisePropertyChanged("NewTagName");
                }
            }
        }

        public TECTag SelectedTag
        {
            get { return _selectedTag; }
            set
            {
                if (_selectedTag != value)
                {
                    _selectedTag = value;
                    RaisePropertyChanged("SelectedTag");
                    RaiseSelectedChanged(SelectedTag);
                }
            }
        }
        
        public ICommand AddTagCommand { get; }

        public TagsCatalogVM(TECTemplates templates, ReferenceDropper dropHandler) : base(templates, dropHandler)
        {
            this.AddTagCommand = new RelayCommand(addTagExecute, canAddTag);
        }


        private void addTagExecute()
        {
            TECTag newTag = new TECTag();
            newTag.Label = this.NewTagName;

            this.Templates.Catalogs.Add(newTag);

            this.NewTagName = "";
        }
        private bool canAddTag()
        {
            return (this.NewTagName != "");
        }
    }
}
