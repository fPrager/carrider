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
using System.Windows.Navigation;
using System.Diagnostics;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;

namespace AutotauschApp
{
    public partial class OrderPage : PhoneApplicationPage
    {
        TimeLineControl myTimeLineControl;
        String orderID = "01234";
        Order currentOrder;
 
        public OrderPage()
        {
            InitializeComponent();
            ApplicationBar = new ApplicationBar();
            currentOrder = App.formHandler.loadOrderFromIsolatedStorage(orderID);
            myTimeLineControl = new TimeLineControl(order_overview, 0, notes, costs, Pages);
     
            this.Loaded += updateApplicationBar;
            this.Loaded += loadControl;
            order_overview.SelectionChanged += updateApplicationBar;
            setUpApplicationBar();
            this.Loaded+=loadForms;
        }

        private void loadControl(object sender, EventArgs e) 
        {
            int startIndex = 0;
            currentOrder = App.formHandler.loadOrderFromIsolatedStorage(orderID);
            OrderState state = EnumerationMatcher.StringToOrderState(currentOrder.State);

            switch (state)
            {
                case OrderState.Overview:
                    startIndex = 2;
                    break;
                case OrderState.Journey:
                    startIndex = 3;
                    break;
                case OrderState.Acceptance:
                    startIndex = 4;
                    break;
                case OrderState.Route:
                    startIndex = 5;
                    break;
                case OrderState.Giving:
                    startIndex = 6;
                    break;
                case OrderState.Retour:
                    startIndex = 7;
                    break;
            }
            myTimeLineControl.setStartIndex(startIndex);
        }

        private void ZoomOut(object sender, PinchGestureEventArgs e)
        {
            if (e.DistanceRatio < 0.5)
            {
                if(NavigationService.CanGoBack)
                NavigationService.GoBack();
                
            }
        }

        private void openRecordData(object sender, EventArgs e){
            NavigationService.Navigate(new Uri(string.Format("/Record_Data.xaml"), UriKind.Relative));
        }

        private void openState(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(string.Format("/Record_State.xaml"), UriKind.Relative));
        }

        private void openPerson(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(string.Format("/Record_Person.xaml"), UriKind.Relative));
        }
        private void openCrash(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(string.Format("/Record_Crash.xaml"), UriKind.Relative));
        }

        private void openDynamic(object sender, EventArgs e) {
            if (sender.GetType() == typeof(Button)) 
            {
                String tag = ((Button)sender).Tag.ToString();

                if (EnumerationMatcher.StringToFormPageType(tag) == FormPageType.None)
                    Debug.WriteLine("Die Tag-Eigenschaft des Buttons \"" + tag + "\" enthält keinen gültigen FormPageType!");
                else
                    NavigationService.Navigate(new Uri(string.Format("/EditPage.xaml?EditPage=" + tag + "&OrderID=" + orderID + "&FormID=Giving"), UriKind.Relative));
            }
        
        }

        private void navigateToSignPages(object sender, EventArgs e) 
        {
            NavigationService.Navigate(new Uri(string.Format("/SignPage.xaml?OrderID=" + orderID), UriKind.Relative));
            
        }

        #region forms loading

        private void loadForms(object sender, EventArgs e) {
            GivingForms.Children.Clear();
            bool nextStage = true;
            foreach (Form form in currentOrder.FormList)
            {
                if (form.State != FormState.Uploaded.ToString()) nextStage = false;
                form.checkMyState();
                if (currentOrder.FormList.IndexOf(form) == 1)
                {
                    TextBlock subheader = new TextBlock();
                    subheader.Text = "weitere Protokolle";
                    subheader.Style = (Style)App.resources["Subheader"];
                    GivingForms.Children.Add(subheader);
                }

                foreach (FormPage page in form.FormPageList)
                {
                    Button pageButton = new Button();
                    pageButton.Style = (Style)App.resources["PageButton"+page.State];
                    StackPanel panel = new StackPanel();
                    TextBlock title = new TextBlock();
                    title.Text = page.FormPageHeader;
                    panel.Children.Add(title);

                    Image image = new Image();
                    form.checkMyState();
                    if (form.State == FormState.Uploaded.ToString())
                        image.Source = new BitmapImage(new Uri("/AppBarIcons/appbar.check.rest.png", UriKind.Relative));
                    else
                    if(form.State == FormState.Signable.ToString())
                        image.Source = new BitmapImage(new Uri("/AppBarIcons/appbar.view.rest.png", UriKind.Relative));
                    else
                        image.Source = new BitmapImage(new Uri("/AppBarIcons/appbar.questionmark.rest.png", UriKind.Relative));

                    image.Width = 50;
                    image.Height = 50;
                    panel.Children.Add(image);

                    pageButton.Content = panel;
                    pageButton.Tag = page.FormPageID;

                    if (!(form.State == FormState.Uploaded.ToString()))
                            pageButton.Click += openDynamic;
                    else
                        pageButton.Click += navigateToSignPages;
                    GivingForms.Children.Add(pageButton);
                }
            }
            if (nextStage)
            {
                currentOrder.State = OrderState.Retour.ToString();
                App.formHandler.saveData(currentOrder);
            }
            loadControl(sender, e);
        }
#endregion

        #region application bar stuff

        ApplicationBar notesAppBar = new ApplicationBar();
        ApplicationBar overviewAppBar = new ApplicationBar();
        ApplicationBar journeyAppBar = new ApplicationBar();
        ApplicationBar acceptanceAppBar = new ApplicationBar();
        ApplicationBar routeAppBar = new ApplicationBar();
        ApplicationBar givingAppBar = new ApplicationBar();
        ApplicationBar retourAppBar = new ApplicationBar();
        ApplicationBar costsAppBar = new ApplicationBar();

        bool appBarIsVisible = true;

        private void setUpApplicationBar()
        {
            
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 1.0;
            ApplicationBar.IsVisible = appBarIsVisible;
            ApplicationBar.IsMenuEnabled = true;

            
            ApplicationBarIconButton edit = new ApplicationBarIconButton();
            edit.IconUri = new Uri("/AppBarIcons/appbar.edit.rest.png", UriKind.Relative);
            edit.Text = "editieren";
            
            ApplicationBarIconButton refresh = new ApplicationBarIconButton();
            refresh.IconUri = new Uri("/AppBarIcons/appbar.refresh.rest.png", UriKind.Relative);
            refresh.Text = "aktualisieren";

            ApplicationBarIconButton up = new ApplicationBarIconButton();
            up.IconUri = new Uri("/AppBarIcons/appbar.up.rest.png", UriKind.Relative);
            up.Text = "früher";

            ApplicationBarIconButton down = new ApplicationBarIconButton();
            down.IconUri = new Uri("/AppBarIcons/appbar.down.rest.png", UriKind.Relative);
            down.Text = "später";

            ApplicationBarIconButton view = new ApplicationBarIconButton();
            view.IconUri = new Uri("/AppBarIcons/appbar.view.rest.png", UriKind.Relative);
            view.Text = "überblick";
            view.Click += navigateToSignPages;

            notesAppBar.Buttons.Add(edit);

            journeyAppBar.Buttons.Add(refresh);
            journeyAppBar.Buttons.Add(up);
            journeyAppBar.Buttons.Add(down);
            journeyAppBar.Buttons.Add(edit);

            acceptanceAppBar.Buttons.Add(view);
            acceptanceAppBar.Buttons.Add(edit);

            routeAppBar.Buttons.Add(edit);

            givingAppBar = acceptanceAppBar;

            retourAppBar.Buttons.Add(edit);

            costsAppBar.Buttons.Add(edit);
            
        }

        private void updateApplicationBar(object sender, EventArgs e) {
            if (order_overview.SelectedItem == notes)
            {
                 ApplicationBar = notesAppBar;
            }
            if (order_overview.SelectedItem == overview)
                ApplicationBar = overviewAppBar;
            if (order_overview.SelectedItem == journey)
            {
                  ApplicationBar = journeyAppBar;
            }
            if (order_overview.SelectedItem == acceptance)
                ApplicationBar = acceptanceAppBar;
            if (order_overview.SelectedItem == route)
                ApplicationBar = routeAppBar;
            if (order_overview.SelectedItem == giving)
                ApplicationBar = givingAppBar;
            if (order_overview.SelectedItem == retour)
                ApplicationBar = retourAppBar;
            if (order_overview.SelectedItem == costs)
                ApplicationBar = costsAppBar;
        
        }


        #endregion

    }
}