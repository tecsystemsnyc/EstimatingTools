using GalaSoft.MvvmLight;
using EstimatingLibrary;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using System.Collections.ObjectModel;
using System;
using Microsoft.Win32;
using EstimatingUtilitiesLibrary;
using System.Windows;
using System.IO;
using GongSolutions.Wpf.DragDrop;
using GalaSoft.MvvmLight.Messaging;
using TECUserControlLibrary;
using System.Collections;
using System.Drawing.Imaging;
using System.Deployment.Application;
using System.ComponentModel;
using System.Windows.Controls;
using System.Reflection;
using TECUserControlLibrary.ViewModelExtensions;
using TECUserControlLibrary.ViewModels;

namespace Scope_Builder.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : BidEditorBase, IDropTarget
    {
        #region Properties

        private GridIndex _dGTabIndex;
        public GridIndex DGTabIndex
        {
            get { return _dGTabIndex; }
            set
            {
                _dGTabIndex = value;
                RaisePropertyChanged("DGTabIndex");
                updateVisibility();
            }
        }

        protected override string ScopeDirectoryPath
        {
            get
            {
                return Properties.Settings.Default.ScopeDirectoryPath;
            }

            set
            {
                Properties.Settings.Default.ScopeDirectoryPath = value;
                Properties.Settings.Default.Save();
            }
        }

        #region VMExtensions
        public ScopeDataGridExtension ScopeDataGrid { get; set; }
        public LocationDataGridExtension LocationDataGrid { get; set; }
        public ScopeCollectionExtension ScopeCollection { get; set; }

        public DocumentBuilderViewModel DocumentBuilderVM { get; set; }
        public BudgetViewModel BudgetVM { get; set; }
        #endregion

        #region Commands Properties
        public ICommand ToggleTemplatesVisibilityCommand { get; private set; }
        #endregion //Commands Properties

        #region Visibility Properties
        private Visibility _templatesVisibility;
        public Visibility TemplatesVisibility
        {
            get
            { return _templatesVisibility; }
            set
            {
                _templatesVisibility = value;
                RaisePropertyChanged("TemplatesVisibility");
            }
        }
        #endregion Visibility Properties
        #endregion
        
        #region Intitializer
        public MainViewModel()
        {
            isEstimate = false;
            programName = "Scope Builder";

            setupAll();
            DGTabIndex = GridIndex.Scope;

            ToggleTemplatesVisibilityCommand = new RelayCommand(ToggleTemplatesVisibilityExecute);
            TemplatesVisibility = Visibility.Visible;

            startupFile = Properties.Settings.Default.StartupFile;
            checkForOpenWith(Properties.Settings.Default.StartupFile);
            Properties.Settings.Default.StartupFile = "";
            Properties.Settings.Default.Save();
            MenuVM.ToggleTemplatesCommand = ToggleTemplatesVisibilityCommand;

            BidSet += () =>
            { refreshAll(); };
            TemplatesLoadedSet += () =>
            { refreshTemplates(); };

            LocationDataGrid.PropertyChanged += LocationDataGrid_PropertyChanged;
        }
        
        #endregion

        #region Commands Methods
        private void ToggleTemplatesVisibilityExecute()
        {
            if (TemplatesVisibility == Visibility.Visible)
            {
                TemplatesVisibility = Visibility.Hidden;
                MenuVM.TemplatesHidden = true;
            }
            else if (TemplatesVisibility == Visibility.Hidden)
            {
                TemplatesVisibility = Visibility.Visible;
                MenuVM.TemplatesHidden = false;
            }
        }

        #endregion //Commands Methods

        #region Helper Functions
        private void updateVisibility()
        {
            if (DGTabIndex == GridIndex.Scope)
            {
                ScopeCollection.SystemsVisibility = Visibility.Visible;
                ScopeCollection.EquipmentVisibility = Visibility.Visible;
                ScopeCollection.SubScopeVisibility = Visibility.Visible;
                ScopeCollection.DevicesVisibility = Visibility.Visible;
                ScopeCollection.DevicesEditVisibility = Visibility.Collapsed;
                ScopeCollection.ManufacturerVisibility = Visibility.Collapsed;
                ScopeCollection.TagsVisibility = Visibility.Collapsed;
                ScopeCollection.ControllerEditVisibility = Visibility.Collapsed;
                ScopeCollection.ControllerVisibility = Visibility.Collapsed;
                ScopeCollection.AssociatedCostsVisibility = Visibility.Collapsed;
                ScopeCollection.ControlledScopeVisibility = Visibility.Visible;
                ScopeCollection.PanelsVisibility = Visibility.Collapsed;
                ScopeCollection.AddPanelVisibility = Visibility.Collapsed;

                ScopeCollection.TabIndex = ScopeCollectionIndex.System;
            }
            else if (DGTabIndex == GridIndex.Location)
            {
                if (LocationDataGrid.SelectedScopeType == LocationScopeType.System)
                {
                    ScopeCollection.EquipmentVisibility = Visibility.Visible;
                    ScopeCollection.SubScopeVisibility = Visibility.Visible;
                    ScopeCollection.DevicesVisibility = Visibility.Visible;

                    ScopeCollection.TabIndex = ScopeCollectionIndex.Equipment;
                }
                else if (LocationDataGrid.SelectedScopeType == LocationScopeType.Equipment)
                {
                    ScopeCollection.EquipmentVisibility = Visibility.Collapsed;
                    ScopeCollection.SubScopeVisibility = Visibility.Visible;
                    ScopeCollection.DevicesVisibility = Visibility.Visible;

                    ScopeCollection.TabIndex = ScopeCollectionIndex.SubScope;
                }
                else if (LocationDataGrid.SelectedScopeType == LocationScopeType.SubScope)
                {
                    ScopeCollection.EquipmentVisibility = Visibility.Collapsed;
                    ScopeCollection.SubScopeVisibility = Visibility.Collapsed;
                    ScopeCollection.DevicesVisibility = Visibility.Visible;

                    ScopeCollection.TabIndex = ScopeCollectionIndex.Devices;
                }
                else
                {
                    throw new NotImplementedException();
                }
                ScopeCollection.SystemsVisibility = Visibility.Collapsed;
                ScopeCollection.DevicesEditVisibility = Visibility.Collapsed;
                ScopeCollection.ManufacturerVisibility = Visibility.Collapsed;
                ScopeCollection.TagsVisibility = Visibility.Collapsed;
                ScopeCollection.ControllerEditVisibility = Visibility.Collapsed;
                ScopeCollection.ControllerVisibility = Visibility.Collapsed;
                ScopeCollection.AssociatedCostsVisibility = Visibility.Collapsed;
                ScopeCollection.ControlledScopeVisibility = Visibility.Collapsed;
                ScopeCollection.PanelsVisibility = Visibility.Collapsed;
                ScopeCollection.AddPanelVisibility = Visibility.Collapsed;
            }
            else if (DGTabIndex == GridIndex.Proposal)
            {
                ScopeCollection.SystemsVisibility = Visibility.Collapsed;
                ScopeCollection.EquipmentVisibility = Visibility.Collapsed;
                ScopeCollection.SubScopeVisibility = Visibility.Collapsed;
                ScopeCollection.DevicesVisibility = Visibility.Collapsed;
                ScopeCollection.DevicesEditVisibility = Visibility.Collapsed;
                ScopeCollection.ManufacturerVisibility = Visibility.Collapsed;
                ScopeCollection.TagsVisibility = Visibility.Collapsed;
                ScopeCollection.ControllerEditVisibility = Visibility.Collapsed;
                ScopeCollection.ControllerVisibility = Visibility.Collapsed;
                ScopeCollection.AssociatedCostsVisibility = Visibility.Collapsed;
                ScopeCollection.ControlledScopeVisibility = Visibility.Collapsed;
                ScopeCollection.PanelsVisibility = Visibility.Collapsed;
                ScopeCollection.AddPanelVisibility = Visibility.Collapsed;

                ScopeCollection.TabIndex = ScopeCollectionIndex.None;
            }
            else if (DGTabIndex == GridIndex.Budget)
            {
                ScopeCollection.SystemsVisibility = Visibility.Collapsed;
                ScopeCollection.EquipmentVisibility = Visibility.Collapsed;
                ScopeCollection.SubScopeVisibility = Visibility.Collapsed;
                ScopeCollection.DevicesVisibility = Visibility.Collapsed;
                ScopeCollection.DevicesEditVisibility = Visibility.Collapsed;
                ScopeCollection.ManufacturerVisibility = Visibility.Collapsed;
                ScopeCollection.TagsVisibility = Visibility.Collapsed;
                ScopeCollection.ControllerEditVisibility = Visibility.Collapsed;
                ScopeCollection.ControllerVisibility = Visibility.Collapsed;
                ScopeCollection.AssociatedCostsVisibility = Visibility.Collapsed;
                ScopeCollection.ControlledScopeVisibility = Visibility.Collapsed;
                ScopeCollection.PanelsVisibility = Visibility.Collapsed;
                ScopeCollection.AddPanelVisibility = Visibility.Collapsed;

                ScopeCollection.TabIndex = ScopeCollectionIndex.None;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        private void setContextText(object selected)
        {
            if(selected is TECScope && selected != null)
            {
                StatusBarVM.ContextText = makeContextString(selected as TECScope);
            }
        }
        private string makeContextString(TECScope scope)
        {
            var outString = "";

            outString += scope.Name + ": ";
            if(scope.Location != null)
            {
                outString += scope.Location;
                outString += " is in bid: ";
                outString += ScopeDataGrid.LocationSelections.Contains(scope.Location);
            }
            else
            {
                outString += "No location";
            }
            
            return outString;
        }
        #endregion //Helper Functions

        #region Setup Extensions

        private void setupAll()
        {
            setupScopeDataGrid();
            setupLocationDataGrid();
            setupScopeCollection();
            setupBudget();
        }
        private void refreshAll()
        {
            ScopeDataGrid.Refresh(Bid);
            LocationDataGrid.Refresh(Bid);
            BudgetVM.Refresh(Bid);
        }
        private void refreshTemplates()
        {
            ScopeCollection.Refresh(Templates);
        }
        private void setupScopeDataGrid()
        {
            ScopeDataGrid = new ScopeDataGridExtension(Bid);
            ScopeDataGrid.DragHandler += DragOver;
            ScopeDataGrid.DropHandler += Drop;
            ScopeDataGrid.SelectionChanged += setContextText;
            ScopeDataGrid.DataGridVisibilty.SubScopeLength = Visibility.Collapsed;

        }
        private void setupLocationDataGrid()
        {
            LocationDataGrid = new LocationDataGridExtension(Bid);
            LocationDataGrid.DragHandler += DragOver;
            LocationDataGrid.DropHandler += Drop;
        }
        private void setupScopeCollection()
        {
            ScopeCollection = new ScopeCollectionExtension(Templates);
            ScopeCollection.DragHandler += DragOver;
            ScopeCollection.DropHandler += Drop;
        }
        private void setupBudget()
        {
            BudgetVM = new BudgetViewModel(Bid);
        }
        #endregion
        
        #region Drag Drop
        public void DragOver(IDropInfo dropInfo)
        {
            var sourceItem = dropInfo.Data;
            var targetCollection = dropInfo.TargetCollection;
            Type sourceType = sourceItem.GetType();
            Type targetType = targetCollection.GetType().GetTypeInfo().GenericTypeArguments[0];

            if (sourceItem != null && sourceType == targetType)
            {
                dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            Object sourceItem;
            if (dropInfo.VisualTarget != dropInfo.DragInfo.VisualSource)
            {
                sourceItem = ((TECScope)dropInfo.Data).DragDropCopy();
                if (dropInfo.InsertIndex > ((IList)dropInfo.TargetCollection).Count)
                { ((IList)dropInfo.TargetCollection).Add(sourceItem); }
                else
                { ((IList)dropInfo.TargetCollection).Insert(dropInfo.InsertIndex, sourceItem); }
            }
            else
            {
                sourceItem = dropInfo.Data;
                int currentIndex = ((IList)dropInfo.TargetCollection).IndexOf(sourceItem);
                int removeIndex = currentIndex;
                if (dropInfo.InsertIndex < currentIndex)
                { removeIndex += 1; }
                if (dropInfo.InsertIndex > ((IList)dropInfo.TargetCollection).Count)
                { ((IList)dropInfo.TargetCollection).Add(sourceItem); }
                else
                { ((IList)dropInfo.TargetCollection).Insert(dropInfo.InsertIndex, sourceItem); }
                ((IList)dropInfo.TargetCollection).RemoveAt(removeIndex);
            }
        }
        #endregion

        #region Event Handlers

        private void LocationDataGrid_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedScopeType")
            {
                updateVisibility();
            }
        }

        #endregion
    }
}