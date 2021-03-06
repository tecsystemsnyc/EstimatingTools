﻿using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TECUserControlLibrary.Models;
using TECUserControlLibrary.Utilities;

namespace TECUserControlLibrary.UserControls.PropertyControls
{
    /// <summary>
    /// Interaction logic for SubScopeTemplateUpdateControl.xaml
    /// </summary>
    public partial class SubScopeTemplateUpdateControl : UserControl
    {

        public TECSubScope Selected
        {
            get { return (TECSubScope)GetValue(SelectedProperty); }
            set { SetValue(SelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Selected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedProperty =
            DependencyProperty.Register("Selected", typeof(TECSubScope), typeof(SubScopeTemplateUpdateControl));
        
        public TECTemplates Templates
        {
            get { return (TECTemplates)GetValue(TemplatesProperty); }
            set { SetValue(TemplatesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Templates.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TemplatesProperty =
            DependencyProperty.Register("Templates", typeof(TECTemplates), typeof(SubScopeTemplateUpdateControl), new PropertyMetadata(null));
        
        public SubScopeTemplateUpdateControl()
        {
            InitializeComponent();
        }
    }

    public class ItemToTemplateUpdateItemConverter : BaseConverter, IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(value is TECSubScope subScope)
            {
                return new TemplateUpdatePropertiesItem(subScope);
            }
            else { return null; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

}
