using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TECUserControlLibrary.Utilities;
using TECUserControlLibrary.ViewModels;

namespace TECUserControlLibrary.Views
{
    /// <summary>
    /// Description for LaborView.
    /// </summary>
    public partial class LaborView : UserControl
    {

        /// <summary>
        /// Gets or sets the ViewModel which is used
        /// </summary>
        public LaborVM ViewModel
        {
            get { return (LaborVM)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        /// <summary>
        /// Identified the ViewModel dependency property
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(LaborVM),
              typeof(LaborView));

        /// <summary>
        /// Initializes a new instance of the LaborView class.
        /// </summary>
        public LaborView()
        {
            InitializeComponent();
        }
    }

    public class LaborTimeConverter : BaseConverter, IMultiValueConverter
    {
        #region IValueConverter Members

        public object Convert(object[] values, Type targetType,
           object parameter, System.Globalization.CultureInfo culture)
        {
            double hours = (double)values[0];
            double time = 0.0;
            LaborTime laborTime = (LaborTime)values[1];
            switch (laborTime)
            {
                case LaborTime.Hours:
                    time = hours;
                    return String.Format("{0:n2} Hours", time);
                case LaborTime.Days:
                    time = hours / 8.0;
                    return String.Format("{0:n2} Days", time);
                case LaborTime.Weeks:
                    time = hours / 40;
                    return String.Format("{0:n2} Weeks", time);
                default:
                    time = hours;
                    return String.Format("{0:n2} Hours", time);
            }

        }
        public object[] ConvertBack(object value, Type[] targetTypes,
               object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public enum LaborTime { Hours, Days, Weeks }
}