using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Diagnostics;
using Microsoft.Phone.Shell;


namespace AutotauschApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        TimeLineControl myTimeLineControl;
        bool orderAccepted = false;
        PivotItem firstObject;
        PivotItem lastObject;

        // Konstruktor
        public MainPage()
        {
            InitializeComponent();
            
            // Datenkontext des Listenfeldsteuerelements auf die Beispieldaten festlegen
            DataContext = App.ViewModel;
            app_overview.SelectionChanged += updateApplicationBar;
            myTimeLineControl = new TimeLineControl(app_overview, 1, archive, timetable, Pages);
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            ApplicationBar = new ApplicationBar();
            setUpApplicationBar();
        }

       


        // Auswahländerung für ListBox behandeln
        private void MainListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
          
        }

        // Daten für die ViewModel-Elemente laden
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        private void ZoomInTap(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            
                  NavigationService.Navigate(new Uri(string.Format("/Order.xaml"), UriKind.Relative));
            
        }

        private void ZoomIn(object sender, PinchGestureEventArgs e)
        {
            if (e.DistanceRatio > 1.5 && orderAccepted)
            {
                 NavigationService.Navigate(new Uri(string.Format("/Order.xaml"), UriKind.Relative));
            }
        }

        private void LoadOrder(object sender, PinchGestureEventArgs e)
        {
            
                NavigationService.Navigate(new Uri(string.Format("/Order.xaml"), UriKind.Relative));
            
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Cal_MonthChanged(object sender, WPControls.MonthChangedEventArgs e)
        {
            Debug.WriteLine("Cal_MonthChanged fired.  New year is " + e.Year.ToString() + " new month is " + e.Month.ToString());
        }

        private void Cal_MonthChanging(object sender, WPControls.MonthChangedEventArgs e)
        {
            Debug.WriteLine("Cal_MonthChanging fired.  New year is " + e.Year.ToString() + " new month is " + e.Month.ToString());
        }

        private void acceptOrder(object sender, EventArgs e) 
        {
            orderAccepted = true;
            ApplicationBar = emptyBar;
        
        }

        ApplicationBar emptyBar = new ApplicationBar();
        ApplicationBar acceptBar = new ApplicationBar();

        private void setUpApplicationBar()
        {

            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 1.0;
            ApplicationBar.IsVisible = true;
            ApplicationBar.IsMenuEnabled = true;

            ApplicationBarIconButton check = new ApplicationBarIconButton();
            check.IconUri = new Uri("/AppBarIcons/appbar.check.rest.png", UriKind.Relative);
            check.Text = "annehmen";
            check.Click += new EventHandler(acceptOrder);
            acceptBar.Buttons.Add(check);

            ApplicationBarIconButton abort = new ApplicationBarIconButton();
            abort.IconUri = new Uri("/AppBarIcons/appbar.close.rest.png", UriKind.Relative);
            abort.Text = "ablehnen";
            acceptBar.Buttons.Add(abort);

            ApplicationBar = acceptBar;
        }

        private void updateApplicationBar(object sender, SelectionChangedEventArgs e)
        {
            if (ApplicationBar!=null)
            if (app_overview.SelectedIndex != 1) {
                ApplicationBar = emptyBar;
            }
            else
                ApplicationBar = acceptBar;
        }



        //private void Cal_SelectionChanged(object sender, WPControls.SelectionChangedEventArgs e)
        //{
        //    Debug.WriteLine("Cal_SelectionChanged fired.  New date is " + e.SelectedDate.ToString());
        //}
        
    }
}