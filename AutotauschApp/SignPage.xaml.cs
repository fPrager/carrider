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
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Phone.Shell;

namespace AutotauschApp
{
    public partial class SignPages : PhoneApplicationPage
    {
        public Order order;
        public FormPage FormPage;
        public String orderID;
        private ElementFactory elementFactory;

        public SignPages()
        {
            InitializeComponent();
            elementFactory = new OverviewElementFactory();
            this.Loaded += setUpPages;
            //this.Loaded += SetUpApplicationBar;
            Pivot.SelectionChanged += showOrHideApplicationBar;
        }



        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            orderID = NavigationContext.QueryString["OrderID"];
            order = App.formHandler.loadOrderFromIsolatedStorage(orderID);
            base.OnNavigatedTo(e);
        }

        private void navigateBack(object sender, EventArgs e)
        {
            bool finished = true;
            if (order != null) 
            {
                if (Pivot.SelectedIndex < order.FormList.Count)
                    order.FormList.ElementAt(Pivot.SelectedIndex).State = FormState.Uploaded.ToString();
                App.formHandler.saveData(order);
                foreach (Form form in order.FormList)
                    if (form.State != FormState.Uploaded.ToString()) finished = false;
            }
            if(finished)
                NavigationService.Navigate(new Uri(string.Format("/Order.xaml"), UriKind.Relative));
            ApplicationBar.IsVisible = false;
        }

        private void showOrHideApplicationBar(object sender, EventArgs e)
        {
            if (ApplicationBar == null) SetUpApplicationBar(sender, e);
            if (order != null) 
            {
                int index = Pivot.SelectedIndex;
                if (order.FormList.Count > index) 
                {
                    if (order.FormList.ElementAt(index).State == FormState.Signed.ToString())
                    {
                        ApplicationBar.IsVisible = true;
                        return;
                    }
                    else
                        ApplicationBar.IsVisible = false;
                }
            }
            ApplicationBar.IsVisible = false;
        }

        private void SetUpApplicationBar(object sender, EventArgs e) 
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsMenuEnabled = false;
            ApplicationBar.IsVisible = false;

            ApplicationBarIconButton check = new ApplicationBarIconButton();
            check.IconUri = new Uri("/AppBarIcons/appbar.check.rest.png", UriKind.Relative);
            check.Text = "fertig";
            check.Click += navigateBack;
            ApplicationBar.Buttons.Add(check);
        }

        private void setUpPages(object sender, EventArgs e) 
        {
            Pivot.Items.Clear();
            foreach (Form form in order.FormList) 
            {
                //if (Pivot.Items.Count > order.FormList.IndexOf(form))
                //{
                    PivotItem pivotItem = new PivotItem();
                    Pivot.Items.Add(pivotItem);
                    pivotItem.Header = form.FormHeader;
                    StackPanel panel = new StackPanel();
                    ListBox viewer = new ListBox();
                    panel.Tag = form.FormID;
                    viewer.Items.Add(panel);
                    pivotItem.Content = viewer;
                    bool signable = true;
                    foreach (FormPage page in form.FormPageList) 
                    {
                        StackPanel pagePanel = new StackPanel();
                        FormPage = page;
                        foreach (FormItem item in page.FormItemList) 
                        {
                            if (EnumerationMatcher.StringToFormItemState(item.State) != FormItemState.Edited)
                            {
                                bool found = false;
                                foreach (OrderInformation info in order.Information)
                                {

                                    if (info.ID == item.ID || info.ID == (form.FormID + item.ID))
                                    {
                                        Debug.WriteLine("Die Information zu " + item.ID + " ist bereits vorhanden: " + info.Information);
                                        item.Input = info.Information;
                                        found = true;
                                    }
                                }
                                if (found) order.setStateOfFormItem(item.ID, FormItemState.Edited);
                            }

                            if (item.State == FormItemState.Edited.ToString() || item.Input != "" || item.ControlType == FormItemType.Header.ToString() || item.ControlType == FormItemType.Subheader.ToString() || item.ControlType == FormItemType.Listing.ToString() || item.ControlType == FormItemType.PageLink.ToString())
                            {
                                FrameworkElement element = elementFactory.getFrameworkElementFromItem(item, this);
                                if(element!=null) pagePanel.Children.Add(element);
                            }
                            else
                                if (item.Important)
                                {
                                    signable = false;
                                    if ((item.Input == "" || item.State == FormItemState.Blank.ToString()))
                                    {
                                        TextBlock textBlock = new TextBlock();
                                        textBlock.Text = item.Header + " (?)";
                                        Style style = (Style)App.resources["OverviewBlank"];
                                        if (style == null)
                                            Debug.WriteLine("Fehler: Der Style \"OverviewBlank\" fehlt!");
                                        else
                                            textBlock.Style = style;
                                        textBlock.Name = item.ID;
                                        pagePanel.Children.Add(textBlock);
                                    }
                                }
                        }
                        pagePanel.Tag = page.FormPageID;
                        if(form.State == FormState.Open.ToString() || form.State == FormState.Signable.ToString())
                                pagePanel.DoubleTap += JumpToEditFormPage;
                        panel.Children.Add(pagePanel);
                    }

                        TextBlock SignumHeader = new TextBlock();
                        SignumHeader.Text = "Unterschriften";
                        SignumHeader.Style = (Style)App.resources["Subheader"];
                        panel.Children.Add(SignumHeader);

                        bool OtherSigned = false;
                        bool ISigned = false;
                    string Person1 = "";
                    if (form.FormOtherHaveToSign)
                    {
                        foreach (OrderInformation info in order.Information)
                            if (info.ID == "GivingPersonName") Person1 = info.Information;
                        if (Person1 != "")
                        {
                            if (!GlobalSettings.HasSignedList.Contains(Person1 + form.FormID) && ((form.State == FormState.Open.ToString() || form.State == FormState.Signable.ToString())))
                            {
                                Image image = new Image();
                                MakeABackground(Person1, (form.State == FormState.Signable.ToString() && signable));
                                BitmapSource source = App.formHandler.getSignumOf(Person1 + "Button");
                                if (source != null)
                                {
                                    image.Source = source;
                                }
                                else
                                    Debug.WriteLine("Buttonbild sollte da sein, wurde als Bild aber nicht gefunden.");
                                Button button = new Button();
                                button.Width = 655 / 2;
                                button.Height = 480 / 2;
                                button.BorderThickness = new Thickness(0);
                                button.Tag = form.FormID;
                                button.Name = "SignOther" + form.FormID;
                                button.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                                if (!(form.State == FormState.Signable.ToString() && signable))
                                {
                                    button.IsEnabled = false;
                                }
                                button.Click += JumpToSignumPage;
                                button.Content = image;
                                panel.Children.Add(button);
                            }
                            else
                            {
                                OtherSigned = true;
                                BitmapSource source = App.formHandler.getSignumOf(Person1 + form.FormID);
                                if (source != null)
                                {
                                    Image image = new Image();
                                    image.Source = source;
                                    image.Width = 655 / 2;
                                    image.Height = 480 / 2;
                                    panel.Children.Add(image);
                                }
                                else
                                    Debug.WriteLine("Signum sollte da sein, wurde als Bild aber nicht gefunden.");
                            }
                        }
                    }
                    else
                        OtherSigned = true;

                    if (form.FormIHaveToSign)
                    {
                        Person1 = GlobalSettings.myName;
                        if (Person1 != "")
                        {
                            if (!GlobalSettings.HasSignedList.Contains(Person1 + form.FormID) && ((form.State == FormState.Open.ToString() || form.State == FormState.Signable.ToString())))
                            {
                                Image image = new Image();
                                MakeABackground(Person1, (form.State == FormState.Signable.ToString() && signable));
                                BitmapSource source = App.formHandler.getSignumOf(Person1 + "Button");
                                if (source != null)
                                {
                                    image.Source = source;
                                }
                                else
                                    Debug.WriteLine("Buttonbild sollte da sein, wurde als Bild aber nicht gefunden.");
                                Button button = new Button();
                                button.Width = 655 / 2;
                                button.Height = 480 / 2;
                                button.BorderThickness = new Thickness(0);
                                button.Tag = form.FormID;
                                button.Name = "SignMe" + form.FormID;
                                button.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                                if (!(form.State == FormState.Signable.ToString() && signable))
                                {
                                    button.IsEnabled = false;
                                }
                                button.Click += JumpToSignumPage;
                                button.Content = image;
                                panel.Children.Add(button);
                            }
                            else
                            {
                                ISigned = true;
                                BitmapSource source = App.formHandler.getSignumOf(Person1 + form.FormID);
                                if (source != null)
                                {
                                    Image image = new Image();
                                    image.Source = source;
                                    image.Width = 655 / 2;
                                    image.Height = 480 / 2;
                                    panel.Children.Add(image);
                                }
                                else
                                    Debug.WriteLine("Signum sollte da sein, wurde als Bild aber nicht gefunden.");
                            }
                        }
                    }
                    else
                        ISigned = true;
                    if(OtherSigned && ISigned)
                    {
                        form.State = FormState.Signed.ToString();
                        if (ApplicationBar != null) ApplicationBar.IsVisible = true;
                        App.formHandler.saveData(order);
                    }
                   pivotItem.Visibility = System.Windows.Visibility.Visible;
                
            }
  //          if (GlobalSettings.focusElement != null)
    //            focusOnElement(GlobalSettings.focusElement);
        }

        private void JumpToSignumPage(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            String Person1 = "";
            String SignDiscription1 = "";
            String Person2 = "";
            String SignDiscription2 = "";

            if (button.Name == "SignOther" + button.Tag)
            {
                foreach (OrderInformation info in order.Information)
                    if (info.ID == "GivingPersonName")
                        Person1 = info.Information;
                if (button.Tag.ToString() == "Giving")
                {
                    SignDiscription1 = "Unterschrift des Abnehmers";
                    SignDiscription2 = "Unterschrift des Abgebers";
                }
                if (this.FindName("SignMe" + button.Tag) != null) 
                {
                    Person2 = GlobalSettings.myName;
                }
            }
            else
                if (button.Name == "SignMe" + button.Tag)
                {
                    Person1 = GlobalSettings.myName;
                    if (button.Tag.ToString() == "Giving")
                    {
                        SignDiscription2 = "Unterschrift des Abnehmers";
                        SignDiscription1 = "Unterschrift des Abgebers";
                    }
                    if (this.FindName("SignMe" + button.Tag) != null)
                    {
                        foreach (OrderInformation info in order.Information)
                            if (info.ID == button.Tag + "PersonName")
                                Person2 = info.Information;
                    }
                }
            GlobalSettings.focusElement = (FrameworkElement)sender;
            NavigationService.Navigate(new Uri(string.Format("/SignumPage.xaml?Person1=" + Person1 + "&SignDiscription1=" + SignDiscription1 + "&Person2=" + Person2 + "&SignDiscription2=" + SignDiscription2+"&FormID="+button.Tag), UriKind.Relative));

        }

        private void JumpToEditFormPage(object sender, EventArgs e) 
        {
            if (sender.GetType() == typeof(StackPanel)) 
            {
                StackPanel panel = (StackPanel)sender;
                FrameworkElement parent = (FrameworkElement)panel.Parent;
                String FormID = "";
                while(parent!=null && FormID=="")
                {
                    FormID = parent.Tag.ToString();
                    parent = (FrameworkElement)parent.Parent;
                }
                if(parent!=null)
                    NavigationService.Navigate(new Uri(string.Format("/FormPage.xaml?FormPage="+panel.Tag.ToString()+"&OrderID="+order.OrderID+"&FormID="+FormID), UriKind.Relative));
           
            }
        }

        private void MakeABackground(String Name, bool signable) 
        { 
            InkPresenter presenter = new InkPresenter();
            presenter.Width=655;
            presenter.Height = 480;
            
            presenter.Background = new SolidColorBrush(Colors.Black);
           
            Line line = new Line();
            line.X1= 20;
            line.X2 = 635;
            line.Y1= 380;
            line.Y2 = 380;
            if(signable)
                line.Stroke = new SolidColorBrush(Colors.White);
            else
                line.Stroke = new SolidColorBrush(Colors.DarkGray);
            line.StrokeThickness = 2;

            presenter.Children.Add(line);

            TextBlock text = new TextBlock();
            text.Text = Name;
            if(signable)
                text.Foreground = new SolidColorBrush(Colors.White);
            else
                text.Foreground = new SolidColorBrush(Colors.DarkGray);
            text.FontSize = 50;
            text.Margin = new Thickness(20,400,0,0);
            presenter.Children.Add(text);

            WriteableBitmap wbBitmap = new WriteableBitmap(presenter, new TranslateTransform());
            EditableImage eiImage = new EditableImage(wbBitmap.PixelWidth, wbBitmap.PixelHeight);
            try
            {
                for (int y = 0; y < wbBitmap.PixelHeight; ++y)
                {
                    for (int x = 0; x < wbBitmap.PixelWidth; ++x)
                    {
                        int pixel = wbBitmap.Pixels[wbBitmap.PixelWidth * y + x];
                        eiImage.SetPixel(x, y,
                        (byte)((pixel >> 16) & 0xFF),
                        (byte)((pixel >> 8) & 0xFF),
                        (byte)(pixel & 0xFF), (byte)((pixel >> 24) & 0xFF)
                        );
                    }
                }
            }
            catch (System.Security.SecurityException)
            {
                throw new Exception("Cannot print images from other domains");
            }
            Stream streamPNG = eiImage.GetStream();
            App.formHandler.SaveSignum(streamPNG, Name+"Button");
        }

        private void focusOnElement(FrameworkElement element) 
        {
                if (this.FindName(element.Name) != null)
                {
                    object parent = element;
                    ScrollViewer viewer = null;
                    Pivot pivot = Pivot;
                    PivotItem pivotItem = null;
                    if(Pivot.FindName(element.Name)!=null)
                        foreach(object item in Pivot.Items)
                            if(item.GetType() == typeof(PivotItem))
                                if(((PivotItem)item).FindName(element.Name)!=null) pivotItem = (PivotItem)item;
                    if(pivotItem!=null)
                    {
                        Pivot.SelectedItem = pivotItem;
                    }

                    object Parent = element.Parent;
                    while (Parent != null)
                    {
                        if (Parent.GetType() == typeof(ScrollViewer))
                            viewer = (ScrollViewer)Parent;
                        Parent = ((FrameworkElement)Parent).Parent;
                    }
                    if (viewer != null)
                        {
                            //var transform = element.TransformToVisual(Application.Current.RootVisual);
                            //Point offset = transform.Transform(new Point(0, 0));
                            //double height = Application.Current.RootVisual.RenderSize.Height;
                            //if (offset.Y > height)
                            //{
                            //    double center = height / 2;
                            //    double diff = offset.Y - center;
                                viewer.ScrollToVerticalOffset(viewer.VerticalOffset + 100);
                            //}
                        }
                    }
                }
        }
    }
