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
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;

namespace AutotauschApp
{
    public class OverviewElementFactory : ElementFactory
    {


        private SignPages SignPage;

        #region For SignPages

        public override FrameworkElement getFrameworkElementFromItem(FormItem item, PhoneApplicationPage sender)
       {
           SignPage = (SignPages)sender;
           try
           {
               FormItemType type = EnumerationMatcher.StringToFormItemType(item.ControlType);

               switch (type)
               {
                   case FormItemType.Header:
                       return giveMeAHeader(item);

                   case FormItemType.Subheader:
                       return giveMeASubheader(item);
                       
                   case FormItemType.TextBox:
                       return giveMeATextBoxAsTextBlock(item);
                       
                   case FormItemType.ListPicker:
                       return giveMeATextBoxAsTextBlock(item);
                      
                   case FormItemType.Photo:
                       return giveMeAPhoto(item);
                      
                   case FormItemType.CheckBox:
                       return giveMeACheckedCheckBox(item);

                   case FormItemType.Listing:
                       return giveMeASignableList(item);
               }
               return null;
           }
           catch
           {
               return null;
           }
       }

        public StackPanel giveMeASignableList(FormItem item) 
       {
           StackPanel stackPanel = new StackPanel();
           if (item.ID == "DamageList") 
           {
               List<DamageEntry> list = new List<DamageEntry>();
               list.AddRange(SignPage.order.DamageList);
               
               int pointSize = 10;

               if (list.Count == 0) {
                   FormItem subheader = new FormItem();
                   subheader.Header = "keine Schäden";
                   subheader.ID = "NoDamageListSubheader";
                   subheader.ControlType = "Subheader";
                   stackPanel.Children.Add(giveMeASubheader(subheader));
               }
               else
               while (list.Count>0) 
               {
                       FormItem subheader = new FormItem();
                       String location = list[0].Location;
                       subheader.Header = location;
                       subheader.ID = location + "DamageListSubheader";
                       subheader.ControlType = "Subheader";
                       //stackPanel.Children.Add(UIElementControler.giveMeATextBlock(subheader));
                       List<DamageEntry> entryToThisLocation = new List<DamageEntry>();
                       Canvas canvas = new Canvas();
                       ImageBrush brush = new ImageBrush();
                       brush.ImageSource = new BitmapImage(new Uri("/CarScheme/" + location + ".png", UriKind.Relative));
                       brush.Stretch = Stretch.Uniform;
                       canvas.Background = brush;
                       canvas.Style = (Style)resources["OverviewDamageCanvas"];
                       Border border = new Border();
                       border.Style = (Style)resources["OverviewDamageBorder"];
                       int j = 0;
                       while(j<list.Count)
                       {
                           if(list[j].Location == location)
                           {
                               Ellipse ellipse = new Ellipse();
                               ellipse.Style = (Style)resources["OverviewDamagePoint"];
                               ellipse.SetValue(Canvas.LeftProperty, (((double)list[j].RelativeLocationX * canvas.Width) - pointSize * 0.5));
                               ellipse.SetValue(Canvas.TopProperty, (((double)list[j].RelativeLocationY * canvas.Height) - pointSize * 0.5));
                               canvas.Children.Add(ellipse);
                               list.RemoveAt(j);
                           }
                           else
                           j++;
                       }
                       border.Child = canvas;
                       //Button button = new Button();
                       //button.BorderThickness = new Thickness(0);
                       //button.Content = border;
                       //button.Tag = location;
                       //button.Click += SignPages.navigateToDamageSite;
                       border.Tag = location;
                       stackPanel.Children.Add(border);
               }
               
           }
           SignPage.order.setStateOfFormItem(item.ID, FormItemState.Disabled);
           stackPanel.Name = item.ID;
           return stackPanel;
       }

       public TextBlock giveMeACheckedCheckBox(FormItem item) {
           if (resources == null) setResourceDictionary();
           TextBlock textBlock = new TextBlock();
           textBlock.Text = item.Input;
           Style style = (Style)resources["OverviewEdit"];
           if (style == null)
               Debug.WriteLine("Fehler: Der Style \"OverviewEdit\" fehlt!");
           else
               textBlock.Style = style;
           textBlock.Name = item.ID;
           return textBlock;
       }

       public Button giveMeAPhoto(FormItem item) {
           Button button = new Button();
           Image image = new Image();
           Style style = (Style)resources["OverviewPhoto"];
           if (style == null)
               Debug.WriteLine("Fehler: Der Style \"OverviewPhoto\" fehlt!");
           else
               button.Style = style;
           image.Source = App.formHandler.getPhotoFromIsolatedStorage(SignPage.order.OrderID + SignPage.FormPage.FormPageID + item.ID);
           image.Name = item.ID;
           button.Content = image;
           //setDefaultValues(button, item);
           return button;
       }

       public TextBlock giveMeAHeader(FormItem item)
       {
           if (resources == null) setResourceDictionary();
           TextBlock textBlock = new TextBlock();
           textBlock.Text = item.Header;
           Style style = (Style)resources["Subheader"];
           if (style == null)
               Debug.WriteLine("Fehler: Der Style \"Subheader\" fehlt!");
           else
               textBlock.Style = style;
           textBlock.Name = item.ID;
           return textBlock;
       }

       public TextBlock giveMeASubheader(FormItem item)
       {
           if (resources == null) setResourceDictionary();
           TextBlock textBlock = new TextBlock();
           textBlock.Text = item.Header;
           Style style = (Style)resources["Fact"];
           if (style == null)
               Debug.WriteLine("Fehler: Der Style \"Fact\" fehlt!");
           else
               textBlock.Style = style;
           textBlock.Name = item.ID;
           return textBlock;
       }

       public FrameworkElement giveMeATextBoxAsTextBlock(FormItem item)
       {
           StackPanel panel = new StackPanel();
           panel.Orientation = Orientation.Horizontal;

           if (resources == null) setResourceDictionary();
               if (item.Input == "(" + item.Header + ")")
               {
                   TextBlock textBlock = new TextBlock();
                   textBlock.Text = "XXX";
                   Style style = (Style)resources["OverviewEdit"];
                   if (style == null)
                       Debug.WriteLine("Fehler: Der Style \"OverviewEdit\" fehlt!");
                   else
                       textBlock.Style = style;
                   textBlock.Name = item.ID;
                   panel.Children.Add(textBlock);

                   TextBlock headerBlock = new TextBlock();
                   headerBlock.Text = "(" + item.Header + ")";
                   if (style == null)
                       Debug.WriteLine("Fehler: Der Style \"OverviewEdit\" fehlt!");
                   else
                       headerBlock.Style = style;
                   headerBlock.Name = item.ID;
                   headerBlock.Foreground = (SolidColorBrush)resources["ColorShortHeaderEdited"];
                   panel.Children.Add(headerBlock);
                   return panel;
               }
               else
                    {
                       TextBlock textBlock = new TextBlock();
                       textBlock.Text = item.Input;
                       Style style = (Style)resources["OverviewEdit"];
                       if (style == null)
                           Debug.WriteLine("Fehler: Der Style \"OverviewEdit\" fehlt!");
                       else
                           textBlock.Style = style;
                       textBlock.Name = item.ID;
                       panel.Children.Add(textBlock);

                       if (item.ShortHeader != "")
                       {
                           TextBlock headerBlock = new TextBlock();
                           if(item.IsEditedStateHeader)
                                    headerBlock.Text = "(" + item.ShortHeader + ")";
                           else
                               headerBlock.Text = item.ShortHeader;
                           if (style == null)
                               Debug.WriteLine("Fehler: Der Style \"OverviewEdit\" fehlt!");
                           else
                               headerBlock.Style = style;
                           headerBlock.Name = item.ID;
                           if (item.IsEditedStateHeader)
                                headerBlock.Foreground = (SolidColorBrush)resources["ColorShortHeaderEdited"];
                           panel.Children.Add(headerBlock);
                       }
                       return panel;
                   }
       }

       public override void editedTextBox(TextBox textBox, bool isANote)
       { }

       public override void selectedTextBlock(TextBlock textBlock, bool isANote)
       {
       }
       #endregion;
    }
}
