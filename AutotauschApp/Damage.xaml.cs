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
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using Microsoft.Phone.Tasks;
using System.Windows.Ink;

namespace AutotauschApp
{
    public partial class Damage : PhoneApplicationPage
    {
        String orderID = "";
        String pageID = "";
        Order order;
        bool entryIsOpen;
        bool selectionIsOpen;
        DamageEntry currentEntry;

        bool isScratch;
        bool isDent;
        bool isBroken;
        bool isCrack;
        String other;
        float relativeXCoord = 0;
        float relativeYCoord = 0;

        Image TempBackRight = new Image();
        Image TempFrontLeft = new Image();
        double PointSize = 70;

        float canvasRatio = 0;
        

        public Damage()
        {
            InitializeComponent();
            this.Loaded += loadData;
            this.Unloaded += handleUnload;
            this.OrientationChanged += handleOrientation;
        }

        private void handleUnload(object sender, EventArgs e) 
        {
            saveOrDeleteInput();
        }

        private void handleOrientation(object sender, OrientationChangedEventArgs e) 
        {
            

            if (this.Orientation == PageOrientation.Landscape || this.Orientation == PageOrientation.LandscapeLeft || this.Orientation == PageOrientation.LandscapeRight)
            {
                DamageEntryBar.Margin = new Thickness(0, 0, 0, 0);
                DamageEntryBar.HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
                DamageSelection.Margin = new Thickness(250, 90, 0, 0);
                DamageSelection.Width = 280;
                FrontLeft.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                BackRight.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                FrontLeft.Margin = new Thickness(0, 0, 0, -50);
                BackRight.Margin = new Thickness(0, 0, 0, -50);
                TemplateBackRight.Margin = new Thickness(10, 100, 0, 0);
                TemplateFrontLeft.Margin = new Thickness(10, 100, 0, 0);
            }
            else
            {
                DamageEntryBar.Margin = new Thickness(0, 90, 0, 0);
                DamageEntryBar.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                DamageSelection.Margin = new Thickness(0, 180, 0, 0);
                DamageSelection.Width = 280;
                FrontLeft.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                BackRight.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                FrontLeft.Margin = new Thickness(0, 0, 0, 50);
                BackRight.Margin = new Thickness(0, 0, 0, 50);
                TemplateBackRight.Margin = new Thickness(10, 250, 0, 0);
                TemplateFrontLeft.Margin = new Thickness(10, 250, 0, 0);
            }
        }

        private void OnGotFocus(object sender, EventArgs e) 
        { 
            if(sender.GetType() == typeof(TextBox))
            {
                    ((TextBox)sender).SelectAll();
            }
        }

        private void copyCanvasToTemplate(Canvas canvas) 
        {
            Debug.WriteLine("Kopieren der Canvas: "+ canvas.Name);
            
            Canvas tempCanvas = new Canvas();
            if(this.FindName("TemplateCanvas"+canvas.Name).GetType() == typeof(Canvas))
            {
                tempCanvas = (Canvas)this.FindName("TemplateCanvas"+canvas.Name);
            }
            else
            {
                Debug.WriteLine("TemplateCanvas"+canvas.Name +" ist keine Canvas!");
                return;
            }

            if(canvasRatio!=0)
            {
            tempCanvas.Children.Clear();
            foreach (Ellipse ellipse in canvas.Children) 
            {
                if (ellipse.Width != 0)
                {
                    Ellipse tempEllipse = new Ellipse();
                    tempEllipse.Opacity = 0.5;
                    tempEllipse.Fill = ellipse.Fill;
                    double X = ((double)ellipse.GetValue(Canvas.LeftProperty) / canvasRatio);
                    double Y = ((double)ellipse.GetValue(Canvas.TopProperty) / canvasRatio);
                    Debug.WriteLine("ratio: "+canvasRatio+" X: "+X+ " Y: "+Y);
                    tempEllipse.Width = ellipse.Width / canvasRatio;
                    tempEllipse.Height = ellipse.Height / canvasRatio;
                    tempEllipse.SetValue(Canvas.LeftProperty, X);
                    tempEllipse.SetValue(Canvas.TopProperty, Y);
                    tempCanvas.Children.Add(tempEllipse);
                }
            }
            }
        }

        private void renderToFrontLeft(Canvas canvas) 
        {
            WriteableBitmap wb = new WriteableBitmap(canvas, new TranslateTransform());
            wb.Invalidate();
            TempFrontLeft.Source = wb;
        }

        private void renderToBackRight(Canvas canvas)
        {
            WriteableBitmap wb = new WriteableBitmap(canvas, new TranslateTransform());
            wb.Invalidate();
            TempBackRight.Source = wb;
        }

        private void loadData(object sender, EventArgs e)
        {
            entryIsOpen = false;
            selectionIsOpen = false;
            DamageEntryBar.Height = 0;
            DamageSelection.Height = 0;
            DamageOther.Tag = DamageOther.Text;
            //Template.Content = TempBackRight;

            order = App.formHandler.loadOrderFromIsolatedStorage(orderID);
            float imageRation = 400f/300f;
            FrontLeft.Width = FrontLeft.Height * imageRation;
            BackRight.Width = BackRight.Height * imageRation;
            TemplateFrontLeft.Width = TemplateFrontLeft.Height * imageRation;
            TemplateCanvasFrontLeft.Width = TemplateCanvasFrontLeft.Height * imageRation;

            canvasRatio = (float)(FrontLeft.Width / TemplateCanvasFrontLeft.Width);

            //renderToFrontLeft(FrontLeft);
            //renderToBackRight(BackRight);

            foreach (DamageEntry entry in order.DamageList)
            {
                Canvas canvas = new Canvas();
                if (entry.Location == "FrontLeft")
                {
                    canvas = FrontLeft;
                }
                else
                if (entry.Location == "BackRight")
                    canvas = BackRight;
                else
                {
                    Debug.WriteLine("Es ist keine Canvas auf " + entry.Location + " zuweisbar.");
                }
                    Point point = new Point();
                    point.X = (float)entry.RelativeLocationX * canvas.Width;
                    point.Y = (float)entry.RelativeLocationY * canvas.Height;

                    Ellipse ellipse = new Ellipse();
                    ellipse.Fill = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
                    ellipse.Height = PointSize;
                    ellipse.Width = PointSize;
                    ellipse.Name = entry.ID;
                    ellipse.SetValue(Canvas.LeftProperty, point.X - (PointSize*0.5));
                    ellipse.SetValue(Canvas.TopProperty, point.Y - (PointSize*0.5));
                    ellipse.Opacity = 0.5;
                    canvas.Children.Add(ellipse);
                    copyCanvasToTemplate(canvas);
            }

            addEventHandlers();
        }

        private void addEventHandlers() {
            pivot.SelectionChanged += updateImage;
            pivot.Hold += handleHold;
            pivot.Tap += handleTap;
            pivot.ManipulationStarted += blockScrolling;

            TemplateBackRight.Click += selectionChange;
            TemplateFrontLeft.Click += selectionChange;
            SelectionButton.Click += changeDamageSelection;
            CameraButton.Click += ShowCameraCaptureTask;
            DeleteButton.Click += DeleteCurrentEntry;
        }

        private void blockScrolling(object sender, ManipulationStartedEventArgs e) {
            if (entryIsOpen) e.Complete();
            else saveOrDeleteInput();
        }

        private void DeleteCurrentEntry(object sender, EventArgs e) 
        { 
            removeDamageEntry(currentEntry.ID);
            hideDamageEntryBar();
        }

        private void saveInputTemporary() {
            isBroken = DamageBroken.IsChecked.Value;
            isDent = DamageDent.IsChecked.Value;
            isCrack = DamageCrack.IsChecked.Value;
            isScratch = DamageScratch.IsChecked.Value;
            other = DamageOther.Text;
        }

        private void changeDamageSelection(object sender, EventArgs e) {
            if (selectionIsOpen)
            {
                saveInputTemporary();
                closeDamageSelection.Begin();
                selectionIsOpen = false;
                String text = "";
                if (isCrack) text += " R :";
                if (isDent) text += " D :";
                if (isScratch) text += " K :";
                if (isBroken) text += " G :";
                if (other != ((String)DamageOther.Tag)) text += " " + other + " :";
                if(text.Length>0){
                    text = text.Substring(0, text.Length - 1);
                ((Button)sender).Content = text;
            }
                currentEntry.Short = text;
                currentEntry.IsCrack = isCrack;
                currentEntry.IsDent = isDent;
                currentEntry.IsScratch = isScratch;
                currentEntry.IsBroken = isBroken;
                if (other != ((String)DamageOther.Tag)) currentEntry.Other = other;
            }
            else
            {
                ((Button)sender).Content = "fertig";
                openDamageSelection.Begin();
                selectionIsOpen = true;
            }
        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.Focus();
            }
        }

        private void removeDamageEntry(String ID) 
        {
            Debug.WriteLine("Entferne die Stelle wieder");
            ((Ellipse)this.FindName(currentEntry.ID)).Width = 0;
            ((Ellipse)this.FindName(currentEntry.ID)).Height = 0;
            if (((Ellipse)this.FindName(currentEntry.ID)).Parent.GetType() == typeof(Canvas))
                copyCanvasToTemplate((Canvas)((Ellipse)this.FindName(currentEntry.ID)).Parent);
            int i = 0;
            while (i < order.DamageList.Count) 
            {
                if (order.DamageList.ElementAt(i).ID == ID)
                {
                    order.DamageList.RemoveAt(i);
                    App.formHandler.saveData(order);
                }
                i++;
            }
        }

        private void saveOrDeleteInput() {
            if (entryIsOpen && !selectionIsOpen)
            {
                if (currentEntry != null)
                {
                    if (currentEntry.IsBroken || currentEntry.IsCrack || currentEntry.IsDent || currentEntry.IsScratch || currentEntry.PhotoPath != "")
                    {
                        int i = 0;
                        while (i < order.DamageList.Count)
                        {
                            if (order.DamageList.ElementAt(i).ID == currentEntry.ID)
                            {
                                order.DamageList.RemoveAt(i);
                            }
                            i++;
                        }
                        order.DamageList.Add(currentEntry);
                        App.formHandler.saveData(order);
                        ((Ellipse)this.FindName(currentEntry.ID)).Opacity = 0.5;
                    }
                    else
                    {
                        if (this.FindName(currentEntry.ID).GetType() == typeof(Ellipse))
                        {
                            removeDamageEntry(currentEntry.ID);
                        }
                    }
                }
                hideDamageEntryBar();
            }
        }

        private void handleTap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            saveOrDeleteInput();

            if (e.OriginalSource.GetType() == typeof(Ellipse)) {
                Ellipse ellipse = (Ellipse)e.OriginalSource;
                ellipse.Opacity = 1;
                bool found = false;
                foreach (DamageEntry entry in order.DamageList) 
                {
                    if (entry.ID == ellipse.Name)
                    {
                        currentEntry = entry;
                        found = true;
                    }
                }
                if (found) {
                    Debug.WriteLine("DamageEntry gefunden");
                    loadDamageEntry();
                    showDamageEntryBar(sender, e);
                }

            }
        }

        private void ShowCameraCaptureTask(object sender, EventArgs e)
        {
            CameraCaptureTask photoCameraCapture = new CameraCaptureTask();
            photoCameraCapture.Completed += photoCompleted;
            photoCameraCapture.Show();
        }

        private void photoCompleted(object sender, PhotoResult e)
        {
            Debug.WriteLine("now");
            if (e.TaskResult == TaskResult.OK)
            {

                if (App.formHandler.savePhotoToIsolatedStorage(e.ChosenPhoto, currentEntry.ID + ".jpg"))
                {
                    Debug.WriteLine("Foto erfolgreich als " + currentEntry.ID + ".jpg gespeichert");
                    currentEntry.PhotoPath = currentEntry.ID;
                }
                CameraPicture.Source = App.formHandler.getPhotoFromIsolatedStorage(currentEntry.ID);
            }
        }

        private void loadDamageEntry() {
            DamageBroken.IsChecked = currentEntry.IsBroken;
            DamageCrack.IsChecked = currentEntry.IsCrack;
            DamageDent.IsChecked = currentEntry.IsDent;
            DamageScratch.IsChecked = currentEntry.IsScratch;
            if (currentEntry.Other != "")
                DamageOther.Text = currentEntry.Other;
            else
                DamageOther.Text = "Sonstiges";
            if (currentEntry.Short != "")
                SelectionButton.Content = currentEntry.Short;
            else
                SelectionButton.Content = "Art";
            if (currentEntry.PhotoPath != "") CameraPicture.Source = App.formHandler.getPhotoFromIsolatedStorage(currentEntry.ID);
        }

        private void handleHold(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!entryIsOpen)
            {
                if (e.OriginalSource.GetType() == typeof(Canvas))
                {
                    Canvas canvas = (Canvas)e.OriginalSource;
                    Debug.WriteLine("Hold auf " + canvas.Name);
                    savePosition(e.GetPosition(canvas).X / canvas.Width, e.GetPosition(canvas).Y / canvas.Height, canvas);
                }
                else
                    Debug.WriteLine("Hold nicht auf Image");
                showDamageEntryBar(sender, e);
            }
            else
            {
                handleTap(sender, e);
                handleHold(sender, e);
            }
        }

        private void savePosition(double X, double Y, Canvas canvas)
        {
            Debug.WriteLine("Positionen " + X +" und "+Y);
            relativeXCoord = (float)X;
            relativeYCoord = (float)Y;
            Point point = new Point();
            point.X = X*canvas.Width;
            point.Y= Y*canvas.Height;

            currentEntry = new DamageEntry();
            currentEntry.ID = canvas.Name + X + Y;
            currentEntry.RelativeLocationX = (decimal)X;
            currentEntry.RelativeLocationY = (decimal)Y;
            currentEntry.Location = canvas.Name;

            Ellipse ellipse = new Ellipse();
            ellipse.Fill = Application.Current.Resources["PhoneAccentBrush"] as SolidColorBrush;
            ellipse.Height = PointSize;
            ellipse.Width = PointSize;
            ellipse.Name = currentEntry.ID;
            ellipse.SetValue(Canvas.LeftProperty, point.X-((PointSize)*0.5));
            ellipse.SetValue(Canvas.TopProperty, point.Y -((PointSize) * 0.5));

            canvas.Children.Add(ellipse);
            copyCanvasToTemplate(canvas);
        }

        private void showDamageEntryBar(object sender, EventArgs e) {
            openDamageEntryBar.Begin();
            entryIsOpen = true;
        }

        private void hideDamageEntryBar()
        {
            closeDamageEntryBar.Begin();
            entryIsOpen = false;
            DamageOther.Text = (String)DamageOther.Tag;
            DamageScratch.IsChecked = false;
            DamageBroken.IsChecked = false;
            DamageCrack.IsChecked = false;
            DamageDent.IsChecked = false;
            SelectionButton.Content = "Art";
            CameraPicture.Source = new BitmapImage(new Uri("AppBarIcons/appbar.feature.camera.rest.png", UriKind.Relative)); 
        }

        private void selectionChange(object sender, EventArgs e) {
            if (!entryIsOpen)
            {
                int index = pivot.SelectedIndex;
                index++;

                if (pivot.Items.Count == index)
                    index = 0;

                int next = index + 1;

                if (pivot.Items.Count == next)
                    next = 0;

                if ((String)((PivotItem)pivot.Items[next]).Tag == "FrontLeft")
                {
                    showTemplateFrontLeft.Begin();
                    hideTemplateBackRight.Begin();
                }
                else
                    if ((String)((PivotItem)pivot.Items[next]).Tag == "BackRight")
                    {
                        hideTemplateFrontLeft.Begin();
                        showTemplateBackRight.Begin();
                    }

                pivot.SelectedIndex = index;
            }
            else
                saveOrDeleteInput();
        }

        private void updateImage(object sender, SelectionChangedEventArgs e) {
            int next = pivot.SelectedIndex + 1;
            Canvas canvas = new Canvas();
            
            if (pivot.Items.Count == next)
                next = 0;

            if ((String)((PivotItem)pivot.Items[next]).Tag == "FrontLeft")
            {
                showTemplateFrontLeft.Begin();
                hideTemplateBackRight.Begin();
            }
            else
                if ((String)((PivotItem)pivot.Items[next]).Tag == "BackRight")
                {
                    hideTemplateFrontLeft.Begin();
                    showTemplateBackRight.Begin();
                }
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            orderID = NavigationContext.QueryString["OrderID"];
            pageID = NavigationContext.QueryString["PageID"];
            if (NavigationContext.QueryString.ContainsKey("Location"))
            {
                if (NavigationContext.QueryString["Location"] == "BackRight")
                    pivot.SelectedIndex = 1;
                else
                    pivot.SelectedIndex = 0;
            }
            base.OnNavigatedTo(e);
        }


    }
}