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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Diagnostics;
using Microsoft.Phone.Tasks;
using Microsoft.Phone;
using Microsoft.Phone.Shell;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace AutotauschApp
{
    public partial class EditPage : PhoneApplicationPage
    {

        String EditPageType = "";
        String orderID = "";
        public String formID = "";
        Panorama pagePanorama;
        List<FormItem> formItems;
        public Order order;
        public FormPage page;
        private DispatcherTimer timer;
        private ElementFactory elementFactory;

        public EditPage()
        {
            InitializeComponent();
            ApplicationBar = new ApplicationBar();
            elementFactory = new EditableElementFactory();
            formItems = new  List<FormItem>();
            this.Loaded += loadStuff;
            SetUpApplicationBar();
            DrawPlane.Width = Application.Current.RootVisual.RenderSize.Width;
            DrawPlane.Height = Application.Current.RootVisual.RenderSize.Height;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0,0,5);
            timer.Tick += new EventHandler(updateOpenPoint);
        }

        private void loadStuff(object sender, EventArgs e) {
            pagePanorama = FormPanorama;
            loadPage();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            EditPageType = NavigationContext.QueryString["EditPage"];
            orderID = NavigationContext.QueryString["OrderID"];
            base.OnNavigatedTo(e);
        }

        private void loadPage() {
            order = App.formHandler.loadOrderFromIsolatedStorage(orderID);
            if (order.State != OrderState.Giving.ToString()) order.State = OrderState.Giving.ToString();
            foreach (Form form in order.FormList)
                    foreach (FormPage fp in form.FormPageList)
                        if (fp.FormPageID == EditPageType)
                        {
                            formID = form.FormID;
                            page = fp;
                        }
            if (page != null)
                setUpPage();
            else
                Debug.WriteLine("Fehler beim Erstellen der Seite");
            App.formHandler.saveData(order);
        }

        public void loadAllFormPagesInOrder(Order o)
        {
            order = o;
            foreach (Form form in order.FormList)
            {
                formID = form.FormID;
                foreach (FormPage fp in form.FormPageList)
                {
                    EditPageType = fp.FormPageID;
                    page = fp;
                    setUpPage();
                    App.formHandler.saveData(order);
                }
            }
        }

        private void setUpPage() {
            
            int numFormParts = 0;
            pagePanorama.Title = page.FormPageHeader;
            List<PanoramaItem> itemList = new List<PanoramaItem>();
            PanoramaItem panoramaItem = new PanoramaItem();
            ScrollViewer scrollViewer = new ScrollViewer();
            StackPanel stackPanel = new StackPanel();
            scrollViewer.Content = stackPanel;
            ((PanoramaItem)FormPanorama.Items[numFormParts]).Content = scrollViewer;
            bool InOneLine = false;
            StackPanel OneLine = new StackPanel();
            foreach (FormItem item in page.FormItemList)
            {
                if (item.ControlType == FormItemType.Header.ToString())
                {
                    item.State = FormItemState.Disabled.ToString();
                    //panoramaItem.Header = item.Header;
                    //numFormParts++;
                    if (numFormParts > 0)
                    {
                        //itemList.Add(panoramaItem);
                        panoramaItem = new PanoramaItem();
                        stackPanel = new StackPanel();
                        scrollViewer = new ScrollViewer();
                        scrollViewer.Content = stackPanel;
                        ((PanoramaItem)FormPanorama.Items[numFormParts]).Content = scrollViewer;
                        ((PanoramaItem)FormPanorama.Items[numFormParts]).Header = item.Header;
                        ((PanoramaItem)FormPanorama.Items[numFormParts]).Visibility = Visibility.Visible;
                    }
                    else
                        ((PanoramaItem)FormPanorama.Items[numFormParts]).Header = item.Header;
                    numFormParts++;
                }
                else
                {
                    FrameworkElement element = elementFactory.getFrameworkElementFromItem(item, this);

                    if (element != null)
                    {

                        
                        if ((!item.IsEditedStateHeader || item.Input != "") && item.ShortHeader != "")
                        {
                            StackPanel ShortHeaderElement = new StackPanel();
                            ShortHeaderElement.Orientation = System.Windows.Controls.Orientation.Horizontal;
                            TextBlock ShortHeader = new TextBlock();
                            Style tbStyle = (Style)App.resources["ShortHeader" + item.ShortHeaderSide];
                            if (tbStyle != null)
                            {
                                ShortHeader.Style = tbStyle;
                            }
                            else
                                Debug.WriteLine("Der Style für ShortHeaders ist nicht \"ShortHeader" + item.ShortHeaderSide + "\"");
                            ShortHeader.Text = item.ShortHeader;
                            ShortHeader.Name = element.Name + "ShortHeader";
                            if (item.IsEditedStateHeader) ShortHeader.Foreground = (SolidColorBrush)App.resources["ColorShortHeaderEdited"];
                            ShortHeaderElement.Children.Add(element);
                            if (item.ShortHeaderSide == FormItemShortHeaderSide.Left.ToString())
                            {
                                ShortHeaderElement.Children.Insert(0, ShortHeader);
                            }
                            if (item.ShortHeaderSide == FormItemShortHeaderSide.Right.ToString())
                            {
                                ShortHeaderElement.Children.Add(ShortHeader);
                            }
                            if (item.Input != "" && element.GetType() == typeof(TextBox)) elementFactory.editedTextBox((TextBox)element, false);
                            element = ShortHeaderElement;
                        }

                            if (item.InLineWithNext)
                            {
                                if (InOneLine)
                                {
                                    if (OneLine != null)
                                        element.Margin = new Thickness(70, element.Margin.Top, element.Margin.Right, element.Margin.Bottom);
                                        OneLine.Children.Add(element);
                                }
                                else {
                                    InOneLine = true;
                                    OneLine = new StackPanel();
                                    OneLine.Orientation = System.Windows.Controls.Orientation.Horizontal;
                                    OneLine.Name = element.Name + "Panel";
                                    OneLine.Children.Add(element);
                                }
                            }
                            else
                            {
                                if (InOneLine)
                                {
                                    element.Margin = new Thickness(70, element.Margin.Top, element.Margin.Right, element.Margin.Bottom);
                                    OneLine.Children.Add(element);
                                    stackPanel.Children.Add(OneLine);
                                    InOneLine = false;
                                }
                                else
                                {
                                    stackPanel.Children.Add(element);
                                }
                            }
                            if (item.Input != "" && element.GetType() == typeof(TextBox)) elementFactory.editedTextBox((TextBox)element, false);
                    }
                    else
                        Debug.WriteLine("Fehler beim generieren eines UIElements");
                }
            }
            //add Notes
            foreach (FormItem item in page.FormItemList)
                    if (item.Note != "") makeANote(item.ID, item.Note);
            //itemList.Add(panoramaItem);
            //int i = 0;
            //while (i < itemList.Count) {
            //    ((PanoramaItem)FormPanorama.Items[i]).Content = itemList.ElementAt(i).Content;
            //    i++;
            //}
        }

        public void textChangedEvent(object sender, KeyEventArgs e){
            //Hier könnte die Validierung der Eingabe rein  
            if (e.Key == Key.Enter)
                changeStateToEdited(sender, e);
        }

        public void handleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            FormItem item = order.getFormItem(element.Name);
            if (item == null)
            {
                Debug.WriteLine("UIElement ist keiner Instanz von FormItem zuweisbar!");
                return;
            }

            if (item.ID == itemID) {
                itemID = "";
                hideOpenPoint.Begin();
                ((FrameworkElement)sender).LayoutUpdated -= updateOpenPoint;
            }

            if (sender.GetType() == typeof(TextBlock) && item.ControlType == FormItemType.TextBox.ToString())
                elementFactory.selectedTextBlock((TextBlock)sender,false);

            if (item.State != FormItemState.Selected.ToString())
            {
                if (sender.GetType() == typeof(TextBox))
                {
                    ((TextBox)sender).SelectAll();
                    ((TextBox)sender).Hold -= makeANote;
                }
                changeStateToSelected(sender, e);
            }

            if (sender.GetType() == typeof(OwnListPicker))
            {
                ((OwnListPicker)sender).HandleTap(e);
                if(((OwnListPicker)sender).State == OwnListPickerState.IsOpen)
                    changeStateToSelected(sender, e);
                else
                if (((OwnListPicker)sender).State == OwnListPickerState.IsClosed)
                    changeStateToEdited(sender, e);
            }
            if (item.State == FormItemState.Selected.ToString() && sender.GetType() == typeof(ListPicker))
                changeStateToEdited(sender, e);
        }

        public void changeStateToSelected(object sender, EventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            FormItem item = order.setStateOfFormItem(element.Name, FormItemState.Selected);
            if (item == null)
            {
                Debug.WriteLine("UIElement ist keiner Instanz von FormItem zuweisbar!");
                return;
            }
            //String styleKey = item.ControlType + "Selected";

            //if (sender.GetType() == typeof(TextBox)) ((TextBox)sender).SelectAll();
            //if (sender.GetType() == typeof(ListPicker)) ((ListPicker)sender).Open();

            //Style style = (Style)App.resources[styleKey];
            //if (style == null)
            //    Debug.WriteLine("Der Style mit dem Key \"" + styleKey + "\" fehlt!");
            //else
            //    element.Style = style;
        }

        public void hideApplicationBar(object sender, EventArgs e) 
        {
            ApplicationBar.IsVisible = false;
        }

        public void showApplicationBar(object sender, EventArgs e) 
        {
            ApplicationBar.IsVisible = true;
        }


        public void changeStateToEdited(object sender, EventArgs e) {
            Debug.WriteLine("Lost Focus");
            if (sender.GetType() == typeof(TextBox))
            {
                TextBox textBox = (TextBox)sender;
                FormItem item = order.getFormItem(textBox.Name);
                if (item == null)
                {
                    Debug.WriteLine("UIElement ist keiner Instanz von FormItem zuweisbar!");
                    return;
                }
                if (textBox.Text != item.Header)
                {
                    order.setStateOfFormItem(textBox.Name, FormItemState.Edited);
                    if (textBox.Text == "")
                        item.Input = "(" + item.Header + ")";
                    else
                        item.Input = textBox.Text;
                    textBox.LostFocus -= changeStateToEdited;
                    if (item.IsEditedStateHeader && item.ShortHeader != "" && textBox.Parent.GetType() == typeof(StackPanel) && this.FindName(item.ID+"ShortHeader")==null)
                    {
                        if (textBox.Text == "")
                            item.Input = "XXX";
                        StackPanel ShortHeaderElement = new StackPanel();
                        ShortHeaderElement.Orientation = System.Windows.Controls.Orientation.Horizontal;
                        TextBlock ShortHeader = new TextBlock();
                        Style tbStyle = (Style)App.resources["ShortHeader" + item.ShortHeaderSide];
                        if (tbStyle != null)
                        {
                            ShortHeader.Style = tbStyle;
                        }
                        else
                            Debug.WriteLine("Der Style für ShortHeaders ist nicht \"ShortHeader" + item.ShortHeaderSide + "\"");
                        ShortHeader.Text = item.ShortHeader;
                        ShortHeader.Name = item.ID + "ShortHeader";
                        ShortHeader.Foreground = (SolidColorBrush)App.resources["ColorShortHeaderEdited"];
                        StackPanel Parent = (StackPanel)textBox.Parent;
                        Parent.Children.Insert(Parent.Children.IndexOf(textBox), ShortHeaderElement);
                        Parent.Children.Remove(textBox);
                        ShortHeaderElement.Children.Add(textBox);
                        if (item.ShortHeaderSide == FormItemShortHeaderSide.Left.ToString())
                        {
                            ShortHeaderElement.Children.Insert(0, ShortHeader);
                        }
                        if (item.ShortHeaderSide == FormItemShortHeaderSide.Right.ToString())
                        {
                            ShortHeaderElement.Children.Add(ShortHeader);
                        }
                    }
                    App.formHandler.saveData(order);
                    elementFactory.editedTextBox(textBox, false);
                }
                else
                {
                    order.setStateOfFormItem(textBox.Name, FormItemState.Blank);
                    textBox.Hold += makeANote;
                }
            }
            else
            {
                FrameworkElement element = (FrameworkElement)sender;
                FormItem item = order.setStateOfFormItem(element.Name, FormItemState.Edited);
                if (item == null)
                {
                    Debug.WriteLine("UIElement ist keiner Instanz von FormItem zuweisbar!");
                    return;
                }
                try
                {
                    Style style = (Style)App.resources[item.ControlType + FormItemState.Edited.ToString()];
                    if (style != null)
                        element.Style = style;
                }
                catch
                {
                    Debug.WriteLine("fehler beim zuweisen von Edited-Style zu " + item.ControlType);
                }

                if (sender.GetType() == typeof(OwnListPicker))
                {
                    item.Input = ((OwnListPicker)sender).GetSelectionAsString();
                    ((OwnListPicker)sender).Close();
                }
                if(sender.GetType() == typeof(CheckBox))
                {
                    if ((bool)((CheckBox)sender).IsChecked)
                        item.Input = item.Header;
                    else
                        item.Input = "kein(e) " + item.Header;
                }

                App.formHandler.saveData(order);
                updateUIElement(item);
            }
            lookUpState();
            this.Focus();
        }

        void updateUIElement(FormItem item){
            //String styleKey = item.ControlType + "Edited";
            //Style style = (Style)App.resources[styleKey];
            //if (style == null)
            //    Debug.WriteLine("Der Style mit dem Key \"" + styleKey + "\" fehlt!");
            //else
            //    ((FrameworkElement)this.FindName(item.ID)).Style = style;
        }

        public void noteFinished(object sender, EventArgs e) 
        {
            //this.Focus();
            TextBox textBox = (TextBox)sender;
            FormItem item = order.getFormItem((String)textBox.Tag);

            if (item == null)
            {
                Debug.WriteLine("Konnte kein Item zu Notiz mit dem Tag " + (String)textBox.Tag + " finden.");
                return;
            }
            item.Note = textBox.Text;
            if (textBox.Text == "")
            {
                ((StackPanel)((FrameworkElement)this.FindName(item.ID)).Parent).Children.Remove(textBox);
            }
            else
            {
                elementFactory.editedTextBox(textBox, true);
            }
        }

        public void lookIfNoteFinished(object sender, KeyEventArgs e) 
        {
            if (e.Key == Key.Enter)
                noteFinished(sender, e);
        }

        public void selectedNote(object sender, EventArgs e) 
        {
            Debug.WriteLine("Focus bekommen");
            TextBox textBox = (TextBox)sender;
            textBox.Style = (Style)App.resources["NoteSelected"];
        }

        public void makeANote(object sender, EventArgs e) {
            Debug.WriteLine("Notiz bearbeiten");
            FrameworkElement item = (FrameworkElement)sender;
            //Storyboard animation = new Storyboard();
            if (this.FindName(item.Name + "Note") != null)
            {
                if (this.FindName(item.Name + "Note").GetType() == typeof(TextBlock))
                {
                    elementFactory.selectedTextBlock((TextBlock)this.FindName(item.Name + "Note"), true);
                    //if (this.FindName(item.Name + "Note").GetType() == typeof(TextBox))
                    //{
                    //    ((TextBox)this.FindName(item.Name + "Note")).Focus();
                    //}
                }
                else
                    Debug.WriteLine("Unter " + item.Name + "Note wurde etwas unerwartetes gefunden.");
                return;
            }
            TextBox textBox = new TextBox();
            textBox.Style = (Style)App.resources["NoteSelected"];
            textBox.Tag = item.Name;
            textBox.GotFocus += selectedNote;
            textBox.LostFocus += noteFinished;
            textBox.KeyDown += lookIfNoteFinished;
            textBox.Name = item.Name + "Note";
            if (item.Parent.GetType() == typeof(StackPanel))
            {
                StackPanel parent = (StackPanel)item.Parent;
                parent.Children.Insert(parent.Children.IndexOf(item) + 1, textBox);
                textBox.Focus();
            }
            else
                Debug.WriteLine("Notiz muss anders behandelt werden");
        }

        private void makeANote(String itemName, String note) 
        {
            FrameworkElement item = (FrameworkElement)this.FindName(itemName);
            if (item != null)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Style = (Style)App.resources["NoteEdited"];
                textBlock.Tag = item.Name;
                textBlock.Name = item.Name + "Note";
                textBlock.Text = note;
                if (item.Parent.GetType() == typeof(StackPanel))
                {
                    StackPanel parent = (StackPanel)item.Parent;
                    parent.Children.Insert(parent.Children.IndexOf(item) + 1, textBlock);
                }
                else
                    Debug.WriteLine("Notiz muss anders behandelt werden");
            }
            else {
                Debug.WriteLine("Zu "+itemName+" kann keine Notiz gesetzt werden, da es nicht existiert");
            }
        }

        String elementName = "";
        public void makeAPhoto(object sender, EventArgs e) {
            FrameworkElement element = (FrameworkElement)sender;
            if (element.Name == itemID)
            {
                itemID = "";
                hideOpenPoint.Begin();
                ((FrameworkElement)sender).LayoutUpdated -= updateOpenPoint;
            }
            elementName = element.Name;
            ShowCameraCaptureTask();
        }

        private void ShowCameraCaptureTask() {
            CameraCaptureTask photoCameraCapture = new CameraCaptureTask();
            photoCameraCapture.Completed += photoCompleted;
            try
            {
                photoCameraCapture.Show();
            }
            catch { };
        }

        private void photoCompleted(object sender, PhotoResult e) {
            if (e.TaskResult == TaskResult.OK) 
            {
                FormItem item = order.setStateOfFormItem(elementName, FormItemState.Edited);
                if (item == null)
                {
                    Debug.WriteLine("UIElement ist keiner Instanz von FormItem zuweisbar!");
                    return;
                }
                if (App.formHandler.savePhotoToIsolatedStorage(e.ChosenPhoto, (order.OrderID + page.FormPageID + elementName) + ".jpg"))
                    Debug.WriteLine("Foto erfolgreich als " + (order.OrderID + page.FormPageID + elementName) + ".jpg gespeichert");
                else
                    order.setStateOfFormItem(elementName, FormItemState.Blank);
                App.formHandler.saveData(order);
            }
            setUpPage();
            lookUpState();
        }

        public void openPage(object sender, EventArgs e)
        {
            FrameworkElement element = (FrameworkElement)sender;
            if (element.Name == itemID)
            {
                itemID = "";
                hideOpenPoint.Begin();
            }
            FormItem item = order.setStateOfFormItem(element.Name, FormItemState.Edited);
            if (item == null)
            {
                Debug.WriteLine("UIElement ist keiner Instanz von FormItem zuweisbar!");
                return;
            }
            NavigationService.Navigate(new Uri(string.Format("/"+item.Link+"?OrderID="+order.OrderID+"&PageID="+page.FormPageID), UriKind.Relative));
        }

        public void navigateToDamageSite(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(string.Format("/Damage.xaml?OrderID=" + order.OrderID + "&PageID=" + page.FormPageID + "&Location=" + ((FrameworkElement)sender).Tag), UriKind.Relative));
        }

        void lookUpState()
        {
            bool finished = true;
            foreach (FormItem item in page.FormItemList)
                if (item.State == FormItemState.Blank.ToString() && item.Important) finished = false;
            if (finished)
            {
                ApplicationBar.IsMenuEnabled = false;
                ApplicationBar.Mode = ApplicationBarMode.Default;

                ApplicationBarIconButton check = new ApplicationBarIconButton();
                check.IconUri = new Uri("/AppBarIcons/appbar.check.rest.png", UriKind.Relative);
                check.Text = "abschließen";
                check.Click += NavigateBack;
                ApplicationBar.Buttons.Clear();
                ApplicationBar.Buttons.Add(check);
            }
        }

        private void NavigateBack(object sender, EventArgs e) 
        {
            NavigationService.Navigate(new Uri(string.Format("/Order.xaml"), UriKind.Relative));
        }

        private void validateInputs(object sender, EventArgs e) 
        {
            this.Focus();
            foreach (FormItem item in page.FormItemList) {
                if (item.State == FormItemState.Blank.ToString() && item.Important)
                {
                    Debug.WriteLine("Das FormItem " + item.ID + " ist noch nicht editiert wurden");
                    ScrollViewer scrollViewer = new ScrollViewer();
                    PanoramaItem panoramaItem = new PanoramaItem();
                    bool found = false;
                    var parent = ((FrameworkElement)this.FindName(item.ID)).Parent;
                    while (parent.GetType() != typeof(PanoramaItem)) {
                        if (parent.GetType() == typeof(ScrollViewer))
                        {
                            scrollViewer = (ScrollViewer)parent;
                            found = true;
                        }
                        parent = ((FrameworkElement)parent).Parent;
                    }
                    panoramaItem = (PanoramaItem)parent;
                    if (found)
                    {
                        itemID = item.ID;
                        var transform = ((FrameworkElement)(this.FindName(item.ID))).TransformToVisual(Application.Current.RootVisual);
                        Point offset = transform.Transform(new Point(0, 0));
                        Debug.WriteLine("PositionY=" + offset.Y + " mit Offset=" + scrollViewer.VerticalOffset);
                        double height = Application.Current.RootVisual.RenderSize.Height;
                        if (offset.Y > height)
                        {
                            double center = height / 2;
                            double diff = offset.Y - center;
                            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + diff);
                        }
                        showQuestionMark(offset);
                        //((FrameworkElement)(this.FindName(item.ID))).LayoutUpdated += updateOpenPoint;
                        //pagePanorama.ManipulationDelta += updateOpenPoint;
                        //pagePanorama.ManipulationStarted += updateOpenPoint;
                        //pagePanorama.ManipulationCompleted += updateOpenPoint;
                       
                        //pagePanorama.LayoutUpdated += updateOpenPoint;
                        timer.Start();
                    }
                        return;
                }
            }
        }

        int zahl = 0;
        private void updateOpenPoint(object sender, EventArgs e) 
        {
            try
            {
                if (itemID != "")
                {
                    var transform = ((FrameworkElement)(this.FindName(itemID))).TransformToVisual(Application.Current.RootVisual);
                    Point point = transform.Transform(new Point(0, 0));
                    leftSide.Opacity = 0;
                    rightSide.Opacity = 0;
                    upSide.Opacity = 0;
                    downSide.Opacity = 0;
                    double height = Application.Current.RootVisual.RenderSize.Height;
                    double width = Application.Current.RootVisual.RenderSize.Width;
                    bool found = false;
                    PanoramaItem panoramaItem;
                    FrameworkElement item = (FrameworkElement)this.FindName(itemID);
                    var parent = item.Parent;

                    while (parent.GetType() != typeof(PanoramaItem) && parent != null)
                    {
                        parent = ((FrameworkElement)parent).Parent;
                    }

                    if (parent.GetType() == typeof(PanoramaItem))
                    {
                        panoramaItem = (PanoramaItem)parent;
                        zahl++;
                        if (pagePanorama.Items.IndexOf(panoramaItem) != pagePanorama.SelectedIndex)
                        {

                            int count = 0;
                            foreach (PanoramaItem pi in pagePanorama.Items)
                                if (pi.Visibility == Visibility.Visible) count++;
                            int leftSteps = 0;
                            int rightSteps = 0;
                            int index = pagePanorama.SelectedIndex;

                            while (index != pagePanorama.Items.IndexOf(panoramaItem))
                            {
                                index++;
                                rightSteps++;
                                if (index == count) index = 0;
                            }
                            index = pagePanorama.SelectedIndex;
                            while (index != pagePanorama.Items.IndexOf(panoramaItem))
                            {
                                index--;
                                leftSteps++;
                                if (index == -1) index = count - 1;
                            }
                            //Debug.WriteLine("da is er: " + pagePanorama.SelectedIndex + " da will ich hin:" + pagePanorama.Items.IndexOf(panoramaItem) + "(left:" + leftSteps + ", right:" + rightSteps); 

                            if (leftSteps < rightSteps)
                            {
                                point.X = 10;
                                leftSide.Opacity = 1;
                            }
                            else
                            {
                                point.X = width - 50;
                                rightSide.Opacity = 1;
                            }
                        }
                        else
                        {
                            if (point.Y < 280)
                            {
                                point.Y = 280;
                                upSide.Opacity = 1;
                            }
                            if (point.Y > height - 110)
                            {
                                point.Y = height - 110;
                                downSide.Opacity = 1;
                            }
                        }
                        OpenPoint.SetValue(Canvas.LeftProperty, point.X);
                        OpenPoint.SetValue(Canvas.TopProperty, point.Y);
                        OpenPointQuestionMark.SetValue(Canvas.LeftProperty, point.X);
                        OpenPointQuestionMark.SetValue(Canvas.TopProperty, point.Y);

                        upSide.SetValue(Canvas.LeftProperty, point.X);
                        upSide.SetValue(Canvas.TopProperty, point.Y - 12);

                        downSide.SetValue(Canvas.LeftProperty, point.X);
                        downSide.SetValue(Canvas.TopProperty, point.Y + 12);

                        leftSide.SetValue(Canvas.LeftProperty, point.X - 12);
                        leftSide.SetValue(Canvas.TopProperty, point.Y);

                        rightSide.SetValue(Canvas.LeftProperty, point.X + 12);
                        rightSide.SetValue(Canvas.TopProperty, point.Y);
                    }
                }
                else
                {
                    timer.Stop();
                }
            }
            catch 
            {
                timer.Stop();
            }
            
        }

        private void unloadStuff(object sender, EventArgs e) 
        {

        }

        private String itemID = "";
        private void showQuestionMark(Point point) 
        {
            leftSide.Opacity = 0;
            rightSide.Opacity = 0;
            upSide.Opacity = 0;
            downSide.Opacity = 0;
                double height = Application.Current.RootVisual.RenderSize.Height;
                double width = Application.Current.RootVisual.RenderSize.Width;
                  bool found = false;
                PanoramaItem panoramaItem;
                FrameworkElement item = (FrameworkElement)this.FindName(itemID);
                var parent = item.Parent;

                while (parent.GetType() != typeof(PanoramaItem) && parent!=null)
                {
                    parent = ((FrameworkElement)parent).Parent;
                }

                if (parent.GetType() == typeof(PanoramaItem))
                {
                    panoramaItem = (PanoramaItem)parent;
                    Debug.WriteLine("look up");
                    if (pagePanorama.Items.IndexOf(panoramaItem) != pagePanorama.SelectedIndex)
                    {

                        int count = 0;
                        foreach (PanoramaItem pi in pagePanorama.Items)
                            if (pi.Visibility == Visibility.Visible) count++;
                        int leftSteps = 0;
                        int rightSteps = 0;
                        int index = pagePanorama.SelectedIndex;

                        while (index != pagePanorama.Items.IndexOf(panoramaItem))
                        {
                            index++;
                            rightSteps++;
                            if (index == count) index = 0;
                        }
                        index = pagePanorama.SelectedIndex;
                        while (index != pagePanorama.Items.IndexOf(panoramaItem))
                        {
                            index--;
                            leftSteps++;
                            if (index == -1) index = count - 1;
                        }
                        //Debug.WriteLine("da is er: " + pagePanorama.SelectedIndex + " da will ich hin:" + pagePanorama.Items.IndexOf(panoramaItem) + "(left:" + leftSteps + ", right:" + rightSteps); 

                        if (leftSteps < rightSteps)
                        {
                            point.X = 10;
                            leftSide.Opacity = 1;
                            Debug.WriteLine("left out");
                        }
                        else
                        {
                            point.X = width - 50;
                            rightSide.Opacity = 1;
                            Debug.WriteLine("right out");
                        }
                    }
                    else
                    {
                        if (point.Y < 280)
                        {
                            point.Y = 280;
                            upSide.Opacity = 1;
                        }
                        if (point.Y > height - 110)
                        {
                            point.Y = height - 110;
                            downSide.Opacity = 1;
                        }
                    }
                }
                OpenPoint.SetValue(Canvas.LeftProperty, point.X);
                OpenPoint.SetValue(Canvas.TopProperty, point.Y);
                OpenPointQuestionMark.SetValue(Canvas.LeftProperty, point.X);
                OpenPointQuestionMark.SetValue(Canvas.TopProperty, point.Y);

                upSide.SetValue(Canvas.LeftProperty, point.X);
                upSide.SetValue(Canvas.TopProperty, point.Y-12);

                downSide.SetValue(Canvas.LeftProperty, point.X);
                downSide.SetValue(Canvas.TopProperty, point.Y + 12);

                leftSide.SetValue(Canvas.LeftProperty, point.X-12);
                leftSide.SetValue(Canvas.TopProperty, point.Y);

                rightSide.SetValue(Canvas.LeftProperty, point.X + 12);
                rightSide.SetValue(Canvas.TopProperty, point.Y);
                showOpenPoint.Begin();
        }

        private void SetUpApplicationBar() 
        {
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.IsMenuEnabled = false;
            ApplicationBar.Mode = ApplicationBarMode.Minimized;

            ApplicationBarIconButton check = new ApplicationBarIconButton();
            check.IconUri = new Uri("/AppBarIcons/appbar.questionmark.rest.png", UriKind.Relative);
            check.Text = "zeigen";
            check.Click += validateInputs;

            ApplicationBar.Buttons.Add(check);
        }
    }
}