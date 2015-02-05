
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Diagnostics;

namespace AutotauschApp
{
    public enum OwnListPickerState {
        IsOpen,
        IsClosed
    }

    public enum OwnListPickerSelectionMode { 
        Multiple,
        Single
    }

    public enum OwnListPickerHeaderMode
    {
        InList,
        OutsideRight
    }

    public enum OwnListPickerFillOutState 
    { 
        Blank,
        Edited,
        Disabled
    }

    public class OwnListPicker : StackPanel
    {
        public List<FrameworkElement> SelectedChildren = new List<FrameworkElement>();
        public List<FrameworkElement> AllChildren = new List<FrameworkElement>();
        public OwnListPickerState State = OwnListPickerState.IsClosed;
        public OwnListPickerSelectionMode SelectionMode = OwnListPickerSelectionMode.Single;
        public FrameworkElement Header = new TextBlock();
        public FormItemState FillOutState = FormItemState.Blank;

        public void Add(FrameworkElement element) 
        {
            AllChildren.Add(element);
            if (this.Children.Count == 0)
                this.Children.Add(element);
        }

        public String GetSelectionAsString() 
        { 
            String text = "";
            foreach (FrameworkElement element in SelectedChildren)
                if (element.GetType() == typeof(TextBlock)) text += ((TextBlock)element).Text;
            return text;
        }

        public void SetFillOutState(FormItemState state) 
        {
            FillOutState = state;
            if((Style)App.resources["ListPicker" + state.ToString()] != null)
                this.Style = (Style)App.resources["ListPicker" + state.ToString()];
            foreach (TextBlock tb in AllChildren) 
            {
                if ((Style)App.resources["List" + state.ToString()] != null)
                {
                    Style style = (Style)App.resources["List" + state.ToString()];
                    tb.Style = style;
                    foreach(Setter setter in style.Setters){
                        if(setter.Property == FrameworkElement.HeightProperty)
                            tb.Height = (double)setter.Value;
                    }
                }
            }
        }

        private double calculateSizeOpen() 
        {
            double height = 0;

            foreach (FrameworkElement element in AllChildren)
                height += element.Height;
            return height;
        }

        private void playOpening() 
        {
            this.Children.Clear();
            foreach (FrameworkElement element in SelectedChildren)
                this.Children.Add(element);

            Storyboard sb1 = new Storyboard();
            DoubleAnimation ani1 = new DoubleAnimation();

            Storyboard.SetTarget(ani1, this);
            Storyboard.SetTargetProperty(ani1, new PropertyPath(FrameworkElement.HeightProperty));
            ani1.From = calculateSizeClose();
            ani1.To = calculateSizeOpen();
            int time1 = 100;
            ani1.Duration = new Duration(new TimeSpan(0, 0, 0, 0, time1));
            sb1.Children.Add(ani1);
            sb1.Begin();
            sb1.Completed += setElementHeight;

            foreach (FrameworkElement element in AllChildren) 
            {
                if (!SelectedChildren.Contains(element))
                {
                    ElementHeight = element.Height;
                    double height = element.Height;
                    element.Height = 0;
                    this.Children.Insert(AllChildren.IndexOf(element), element);

                    Storyboard sb = new Storyboard();
                    DoubleAnimation ani = new DoubleAnimation();

                    Storyboard.SetTarget(ani, element);
                    Storyboard.SetTargetProperty(ani, new PropertyPath(FrameworkElement.HeightProperty));
                    ani.From = 0;
                    ani.To = height;
                    int time = 100;
                    ani.Duration = new Duration(new TimeSpan(0, 0, 0, 0, time));
                    sb.Children.Add(ani);
                    sb.Begin();
                }
            }
        }

        private double calculateSizeClose() 
        {
            double height = 0;
            if (SelectedChildren.Count > 0)
                foreach (FrameworkElement element in SelectedChildren)
                    height += element.Height;
            else
                height = Header.Height;

            return height;
        }

        double ElementHeight = 0;
        private void playClosing()
        {
            Storyboard sb = new Storyboard();
            DoubleAnimation ani = new DoubleAnimation();

            Storyboard.SetTarget(ani, this);
            Storyboard.SetTargetProperty(ani, new PropertyPath(StackPanel.HeightProperty));
            ani.From = (int)calculateSizeOpen();
            ani.To = (int)calculateSizeClose();
            int time = 50;
            time *= AllChildren.Count;
            ani.Duration = new Duration(new TimeSpan(0, 0, 0, 0, time));
            sb.Children.Add(ani);
            sb.Begin();
        }

        private void setElementHeight(object sender, EventArgs e) 
        {
            if (ElementHeight != 0)
            {
                foreach (FrameworkElement element in AllChildren)
                {
                    element.Height = ElementHeight;
                }
                foreach (FrameworkElement element in SelectedChildren)
                {
                    element.Height = ElementHeight;
                }
            }
        }

        public void SetHeader(FrameworkElement Header) 
        {
            this.Header = Header;
            if (this.Children.Count == 0)
            {
                this.Children.Add(Header);
                this.Height = Header.Height;
            }
        }

        public void AddAsSelected(FrameworkElement element) 
        {
            AllChildren.Add(element);
            if(!SelectedChildren.Contains(element))
                SelectedChildren.Add(element);
            this.Children.Clear();
            foreach (FrameworkElement e in SelectedChildren)
                this.Children.Add(e);
            this.SetFillOutState(FormItemState.Edited);
            this.Height = calculateSizeClose();
        }

        public void Open() 
        {
            if (State == OwnListPickerState.IsClosed) 
            {
                this.Children.Clear();
                foreach (FrameworkElement element in AllChildren)
                {
                    this.Children.Add(element);
                }
                playOpening();
                State = OwnListPickerState.IsOpen;
            }
        }

        public void Close() 
        {
            if (State == OwnListPickerState.IsOpen) 
            {
                playClosing();
                this.Children.Clear();
                if (SelectedChildren.Count == 0) 
                {
                        this.Children.Add(Header);
                }
                else
                    foreach (FrameworkElement element in SelectedChildren) 
                    {
                        this.Children.Add(element);
                    }
                State = OwnListPickerState.IsClosed;
            }
        }

        public void HandleTap(System.Windows.Input.GestureEventArgs e)
        {
            if (State == OwnListPickerState.IsClosed)
            {
                SetFillOutState(FormItemState.Selected);
                Open();
            }
            else
                if (State == OwnListPickerState.IsOpen)
                {
                     if(e.OriginalSource.GetType() != typeof(OwnListPicker))
                     {
                    if (SelectionMode == OwnListPickerSelectionMode.Single)
                    {
                        try
                        {
                            SelectedChildren.Clear();                          
                                SelectedChildren.Add((FrameworkElement)e.OriginalSource);
                            SetFillOutState(FormItemState.Edited);
                        }
                        catch
                        {
                            Debug.WriteLine("Fehler beim Handeln des TapEventsauf OwnListpicker.");
                        }
                        Close();
                    }
                    else
                    {

                    }
                     }
                     else
                     {
                         Debug.WriteLine("Tap auf ListPicker nicht auf Listenelement");
                     }
                }
        }

        public void Select(FrameworkElement element) 
        {
            if (State == OwnListPickerState.IsOpen) 
            {
                if (SelectedChildren.Contains(element)) 
                    SelectedChildren.Remove(element);
                else
                    SelectedChildren.Add(element);

                if (SelectionMode == OwnListPickerSelectionMode.Single)
                    Close();
            }

        }


    }
}
