using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TECUserControlLibrary.ViewModels;

namespace TECUserControlLibrary.Views
{
    /// <summary>
    /// Interaction logic for SplashView.xaml
    /// </summary>
    public partial class SplashView : UserControl
    {
        /// <summary>
        /// Gets or sets the ViewModel which is used
        /// </summary>
        public SplashVM ViewModel
        {
            get { return (SplashVM)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        /// <summary>
        /// Identified the ViewModel dependency property
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(SplashVM),
              typeof(SplashView));
        
        public Visibility BidVisibility
        {
            get { return (Visibility)GetValue(BidVisibilityProperty); }
            set { SetValue(BidVisibilityProperty, value); }
        }
        // Using a DependencyProperty as the backing store for BidVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BidVisibilityProperty =
            DependencyProperty.Register("BidVisibility", typeof(Visibility), typeof(SplashView));


        public string BidPath
        {
            get { return (string)GetValue(BidPathProperty); }
            set { SetValue(BidPathProperty, value); }
        }
        // Using a DependencyProperty as the backing store for BidPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BidPathProperty =
            DependencyProperty.Register("BidPath", typeof(string), typeof(SplashView), new PropertyMetadata(""));
        
        public ICommand GetBidPathCommand
        {
            get { return (ICommand)GetValue(GetBidPathCommandProperty); }
            set { SetValue(GetBidPathCommandProperty, value); }
        }
        // Using a DependencyProperty as the backing store for GetBidPathCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty GetBidPathCommandProperty =
            DependencyProperty.Register("GetBidPathCommand", typeof(ICommand), typeof(SplashView));
        
        public ICommand ClearBidPathCommand
        {
            get { return (ICommand)GetValue(ClearBidPathCommandProperty); }
            set { SetValue(ClearBidPathCommandProperty, value); }
        }
        // Using a DependencyProperty as the backing store for ClearBidPathCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClearBidPathCommandProperty =
            DependencyProperty.Register("ClearBidPathCommand", typeof(ICommand), typeof(SplashView));


        #region RecentBidProperties
        public string FirstRecentBid
        {
            get { return (string)GetValue(FirstRecentBidProperty); }
            set { SetValue(FirstRecentBidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FirstRecentBid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FirstRecentBidProperty =
            DependencyProperty.Register("FirstRecentBid", typeof(string), typeof(SplashView));
        
        public string SecondRecentBid
        {
            get { return (string)GetValue(SecondRecentBidProperty); }
            set { SetValue(SecondRecentBidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SecondRecentBid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SecondRecentBidProperty =
            DependencyProperty.Register("SecondRecentBid", typeof(string), typeof(SplashView));
        
        public string ThirdRecentBid
        {
            get { return (string)GetValue(ThirdRecentBidProperty); }
            set { SetValue(ThirdRecentBidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThirdRecentBid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThirdRecentBidProperty =
            DependencyProperty.Register("ThirdRecentBid", typeof(string), typeof(SplashView));
        
        public string FourthRecentBid
        {
            get { return (string)GetValue(FourthRecentBidProperty); }
            set { SetValue(FourthRecentBidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FourthRecentBid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FourthRecentBidProperty =
            DependencyProperty.Register("FourthRecentBid", typeof(string), typeof(SplashView));
        
        public string FifthRecentBid
        {
            get { return (string)GetValue(FifthRecentBidProperty); }
            set { SetValue(FifthRecentBidProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FifthRecentBid.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FifthRecentBidProperty =
            DependencyProperty.Register("FifthRecentBid", typeof(string), typeof(SplashView));
        
        public ICommand ChooseRecentBidCommand
        {
            get { return (ICommand)GetValue(ChooseRecentBidCommandProperty); }
            set { SetValue(ChooseRecentBidCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ChooseRecentBidCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ChooseRecentBidCommandProperty =
            DependencyProperty.Register("ChooseRecentBidCommand", typeof(ICommand), typeof(SplashView));
        #endregion

        public SplashView()
        {
            InitializeComponent();
        }
    }
}
