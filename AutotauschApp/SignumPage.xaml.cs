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
using System.Diagnostics;
using System.Windows.Ink;
using System.Windows.Media.Imaging;
using System.IO;

namespace AutotauschApp
{
    public partial class Signum : PhoneApplicationPage
    {
        String Person1 = "";
        String Person2 = "";
        String SignDiscription1 = "";
        String SignDiscription2 = "";
        String FormID = "";
        bool toSave = false;

        Dictionary<int, Stroke> activeStrokes = new Dictionary<int, Stroke>();


        public Signum()
        {
            InitializeComponent();
            Touch.FrameReported += OnTouchFrameReported;
            this.Unloaded += UnloadPage;
            this.Loaded += SetUpPage;
        }

        private void UnloadPage(object sender, EventArgs e) 
        {
            if (toSave)
                saveImage();
            Touch.FrameReported -= OnTouchFrameReported;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            try
            {
                Person1 = NavigationContext.QueryString["Person1"];
                SignDiscription1 = NavigationContext.QueryString["SignDiscription1"];

                if (NavigationContext.QueryString.ContainsKey("Person2"))
                {
                    Person2 = NavigationContext.QueryString["Person2"];
                    SignDiscription2 = NavigationContext.QueryString["SignDiscription2"];
                }
                if (NavigationContext.QueryString.ContainsKey("FormID"))
                {
                    FormID = NavigationContext.QueryString["FormID"];
                }
            }
            catch 
            {
                Debug.WriteLine("Fehler bei der Übergabe der Werte");
            }
            base.OnNavigatedTo(e);
        }

        private void SetUpPage(object sender, EventArgs e) 
        {
            SetUpApplicationBar();
            PersonName.Text = Person1;

            if (SignDiscription1 != "")
                Title.Text = SignDiscription1;
        }

        void OnTouchFrameReported(object sender, TouchFrameEventArgs args)
        {
            TouchPoint primaryTouchPoint = args.GetPrimaryTouchPoint(null);
            if (primaryTouchPoint != null && primaryTouchPoint.Action == TouchAction.Down)
            {
                args.SuspendMousePromotionUntilTouchUp();
            }

            TouchPointCollection touchPoints = args.GetTouchPoints(InkPresenter);
            foreach (TouchPoint touchPoint in touchPoints)
            {
                Point pt = touchPoint.Position;
                int id = touchPoint.TouchDevice.Id;
                switch (touchPoint.Action)
                {
                    case TouchAction.Down:
                        Stroke stroke = new Stroke();
                        stroke.DrawingAttributes.Color = Colors.White;
                        stroke.DrawingAttributes.Height = 3;
                        stroke.DrawingAttributes.Width = 3;
                        stroke.StylusPoints.Add(new StylusPoint(pt.X, pt.Y));
                        InkPresenter.Strokes.Add(stroke);
                        activeStrokes.Add(id, stroke);
                        break;
                    case TouchAction.Move:
                        activeStrokes[id].StylusPoints.Add(new StylusPoint(pt.X, pt.Y));
                        break;
                    case TouchAction.Up:
                        activeStrokes[id].StylusPoints.Add(new StylusPoint(pt.X, pt.Y));
                        activeStrokes.Remove(id);
                        break;
                }
            }
        }

        private void goBackToForm() 
        {
            NavigationService.GoBack();
        }

        private void saveImage() 
        {
            PersonName.FontSize = 50;
            WriteableBitmap wbBitmap = new WriteableBitmap(InkPresenter, new TranslateTransform());
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
            // Save it to disk
            Stream streamPNG = eiImage.GetStream();
            Debug.WriteLine("Speichere als " + Person1 + FormID + "Signum.png");
            if (App.formHandler.SaveSignum(streamPNG, (Person1 + FormID)))
            {
                Debug.WriteLine("Foto erfolgreich als " + Person1 + FormID + "Signum.png gespeichert");
                GlobalSettings.HasSignedList.Add(Person1 + FormID);
            }
            else
                Debug.WriteLine("Fehler beim Speichern.");
        }

        private void Check(object sender, EventArgs e)
        {
            toSave = true;
        goBackToForm();
        }

        private void Abort(object sender, EventArgs e)
        {
            InkPresenter.Strokes.Clear();
            goBackToForm();
        }

        private void Next(object sender, EventArgs e) 
        {
            NavigationService.Navigate(new Uri(string.Format("/SignumPage.xaml?Person1=" + Person2 + "&SignDiscription1=" + SignDiscription2 + "&Person2=" + Person1 + "&SignDiscription2=" + SignDiscription1), UriKind.Relative));
        }

        private void SetUpApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Default;
            ApplicationBar.Opacity = 1.0;
            ApplicationBar.IsVisible = true;
            ApplicationBar.IsMenuEnabled = false;

            ApplicationBarIconButton abort = new ApplicationBarIconButton();
            abort.IconUri = new Uri("/AppBarIcons/appbar.close.rest.png", UriKind.Relative);
            abort.Text = "abbrechen";
            abort.Click += Abort;
            ApplicationBar.Buttons.Add(abort);

            //if (Person2 != "")
            //{
            //    ApplicationBarIconButton next = new ApplicationBarIconButton();
            //    next.IconUri = new Uri("/AppBarIcons/appbar.next.rest.png", UriKind.Relative);
            //    next.Text = "wechsel";
            //    next.Click += Next;
            //    ApplicationBar.Buttons.Add(next);
            //}

            ApplicationBarIconButton check = new ApplicationBarIconButton();
            check.IconUri = new Uri("/AppBarIcons/appbar.save.rest.png", UriKind.Relative);
            check.Text = "fertig";
            check.Click += Check;
            ApplicationBar.Buttons.Add(check);
        }
    }
}