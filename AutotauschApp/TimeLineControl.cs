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
using Microsoft.Phone.Controls;
using System.Diagnostics;
using System.Collections.Generic;

namespace AutotauschApp
{
    public class TimeLineControl
    {
        private Pivot myPivot { get; set; }
        private int startIndex { get; set; }
        private PivotItem lastItem;
        private PivotItem firstItem;
        private String originalFirstHeader;
        private String originalLastHeader;
        private Canvas pages;
        private double futurPagesOpacity = 0.4;
        private double pastPagesOpacity = 0.8;
        private double pageWidthFactor = 2;

        public void setStartIndex(int index)
        {
            startIndex = index;
            updateRectangles();
            myPivot.SelectedIndex = index;


        }

        private void updateRectangles()
        {
            int i = 0;
            foreach (FrameworkElement element in pages.Children) 
            {
                if (element.GetType() == typeof(Rectangle))
                {
                    Rectangle rec = (Rectangle)element;
                    if (i <= startIndex)
                        rec.Opacity = pastPagesOpacity;
                    else
                        rec.Opacity = futurPagesOpacity;
                    i++;
                }
            }
        }

        public TimeLineControl(Pivot myPivot, int startIndex, PivotItem firstItem, PivotItem lastItem, Canvas pages) {
            this.myPivot = myPivot;
            this.startIndex = startIndex;
            this.firstItem = firstItem;
            this.lastItem = lastItem;
            this.pages = pages;

            setUpPages();

            originalFirstHeader = firstItem.Header.ToString();
            originalLastHeader = lastItem.Header.ToString();

            myPivot.SelectionChanged += OnSelectionChanged;
            myPivot.SelectionChanged += changePageColor;
            myPivot.ManipulationDelta += OnManipulationDelta;
            myPivot.SelectedIndex = startIndex;
        }

        private void setUpPages() 
        {
            double width = pages.Width;
            int anz = myPivot.Items.Count;
            double recWidth = width / (anz + (1 / pageWidthFactor) * anz + (1 / pageWidthFactor));
            double gapWidth = recWidth / pageWidthFactor;
            foreach (PivotItem item in myPivot.Items) 
            {
                Rectangle page = new Rectangle();
                page.Width = recWidth;
                page.Height = pages.Height;

                int Index = myPivot.Items.IndexOf(item);
                double fromLeft = Index * (recWidth + gapWidth) + ((recWidth+gapWidth)/anz);

                page.SetValue(Canvas.LeftProperty, fromLeft);
                page.SetValue(Canvas.TopProperty, 0.0);

                page.Name = "Page" + Index;

                page.Fill = new SolidColorBrush(Colors.White);

                if (Index > startIndex)
                    page.Opacity = futurPagesOpacity;
                else
                    page.Opacity = pastPagesOpacity;
                pages.Children.Add(page);

                if (!(Index == myPivot.Items.Count - 1))
                {
                    Line line = new Line();
                    line.Y1 = pages.Height / 2;
                    line.Y2 = pages.Height / 2;
                    line.X1 = fromLeft + recWidth;
                    line.X2 = fromLeft + recWidth + gapWidth;
                    line.Stroke = new SolidColorBrush(Colors.Gray);
                    line.StrokeThickness = 5;
                    //pages.Children.Add(line);
                }
            }
        }

        private void changePageColor(object sender, EventArgs e) 
        {
            if (myPivot != null && pages != null) 
            {
                foreach (FrameworkElement page in pages.Children) 
                {
                    if(page.GetType() == typeof(Rectangle))
                    {
                        Rectangle rec = (Rectangle)page;
                        if (!(rec.Name == ("Page" + myPivot.SelectedIndex)))
                            rec.Fill = new SolidColorBrush(Colors.White);
                    else
                            rec.Fill = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
                    }
                }
            }
        
        }

        public void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e) 
        {
            if (e.DeltaManipulation.Translation.X > 0 && myPivot.SelectedIndex == 0)
            {
                Debug.WriteLine("left end - stop!");
                e.Complete();
            }
            if (e.DeltaManipulation.Translation.X < 0 && myPivot.SelectedIndex == myPivot.Items.Count-1)
            {
                Debug.WriteLine("right end - stop");
                e.Complete();
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (myPivot.SelectedItem == lastItem)
            {
                firstItem.Header = "               ";
            }
            else
                firstItem.Header = originalFirstHeader;

            if (myPivot.SelectedItem == firstItem)
            {
                lastItem.Header = "               ";
                
            }
            else
                lastItem.Header = originalLastHeader;
        }


    }
}
