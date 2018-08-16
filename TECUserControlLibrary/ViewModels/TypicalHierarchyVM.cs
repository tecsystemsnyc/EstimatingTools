using EstimatingLibrary;
using EstimatingLibrary.Utilities;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using TECUserControlLibrary.ViewModels.AddVMs;

namespace TECUserControlLibrary.ViewModels
{
    public class TypicalHierarchyVM : ViewModelBase
    {
        private TECTypical _selectedTypical;
        private TECBid bid;

        public SystemHierarchyVM SystemHierarchyVM { get; }
        public ObservableCollection<TECTypical> TypicalSystems { get; }
        public ObservableCollection<TECTypical> Singletons { get; }
        public TECTypical SelectedTypical
        {
            get { return _selectedTypical; }
            set
            {
                _selectedTypical = value;
                RaisePropertyChanged("SelectedTypical");
                Selected?.Invoke(value);
            }
        }

        public ICommand AddInstanceCommand { get; private set; }
        public RelayCommand<TECTypical> InstanceTypicalCommand { get; private set; }

        public TypicalHierarchyVM(TECBid bid, ChangeWatcher watcher)
        {
            this.bid = bid;
            SystemHierarchyVM = new SystemHierarchyVM(bid, false);
            SystemHierarchyVM.Selected += systemHierarchyVM_Selected;
            SystemHierarchyVM.SetDeleteCommand(
                sys =>
                {
                    SelectedTypical.Instances.Remove(sys);
                },
                sys =>
                {
                    return SelectedTypical != null;
                });
            TypicalSystems = new ObservableCollection<TECTypical>(bid.Systems.Where(x => x.Instances.Count != 1));
            Singletons = new ObservableCollection<TECTypical>(bid.Systems.Where(x => x.Instances.Count == 1));
            AddInstanceCommand = new RelayCommand(addInstanceExecute, canAddInstance);
            InstanceTypicalCommand = new RelayCommand<TECTypical>(instanceExecute, canInstanceExecute);
            watcher.Changed += handleChanged;
        }

        private void instanceExecute(TECTypical obj)
        {
            obj.Instances.ObservablyClear();
        }

        private bool canInstanceExecute(TECTypical arg)
        {
            return arg != null;
        }

        private void handleChanged(TECChangedEventArgs obj)
        {
            if(obj.Sender is TECBid && obj.Value is TECTypical typ)
            {
                if(obj.Change == Change.Add)
                {
                    if (typ.Instances.Count != 1)
                    {
                        TypicalSystems.Add(typ);
                    }
                    else
                    {
                        Singletons.Add(typ);
                    }
                }
                else if (obj.Change == Change.Remove)
                {
                    TypicalSystems.Remove(typ);
                    Singletons.Remove(typ);
                }
                
            }
            else if (obj.Sender is TECTypical typParent && obj.Value is TECSystem)
            {
                if(typParent.Instances.Count == 1)
                {
                    TypicalSystems.Remove(typParent);
                    Singletons.Add(typParent);
                }
                else
                {
                    Singletons.Remove(typParent);
                    if (!TypicalSystems.Contains(typParent))
                    {
                        TypicalSystems.Add(typParent);
                    }
                }
            }
        }

        private void systemHierarchyVM_Selected(TECObject obj)
        {
            Selected?.Invoke(obj);
        }

        public event Action<TECObject> Selected;

        private void addInstanceExecute()
        {
            SystemHierarchyVM.SelectedVM = new AddInstanceVM(SelectedTypical, bid);
        }

        private bool canAddInstance()
        {
            return SelectedTypical != null;
        }
    }
}
