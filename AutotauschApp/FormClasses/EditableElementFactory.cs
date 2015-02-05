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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace AutotauschApp
{
    public class EditableElementFactory : ElementFactory
    {
      
       private  EditPage FormPage;
      
       #region For FormPage
       public override FrameworkElement getFrameworkElementFromItem(FormItem item, PhoneApplicationPage sender)
       {
           FormPage = (EditPage)sender;
           try
           {
               FormItemType type = EnumerationMatcher.StringToFormItemType(item.ControlType);

               switch (type)
               {
                   case FormItemType.Subheader:
                       return giveMeATextBlock(item);
                       
                   case FormItemType.TextBox:
                       return giveMeATextBoxOrTextBlock(item);
                       
                   case FormItemType.ListPicker:
                       return giveMeAListPicker(item);
                      
                   case FormItemType.Photo:
                       return giveMeAPhotoButton(item);
                      
                   case FormItemType.CheckBox:
                       return giveMeACheckBox(item);
                    
                   case FormItemType.PageLink:
                       return giveMeAPageLink(item);
                   case FormItemType.Listing:
                       return giveMeAList(item);
               }
               return null;
           }
           catch
           {
               return null;
           }
       }

       public StackPanel giveMeAList(FormItem item) 
       {
           StackPanel stackPanel = new StackPanel();
           if (item.ID == "DamageList") 
           {
               List<DamageEntry> list = new List<DamageEntry>();
               list.AddRange(FormPage.order.DamageList);
               int canvasWidth = 333;
               int canvasHeight = 250;
               
               int pointSize = 10;

               if (list.Count == 0) {
                   FormItem subheader = new FormItem();
                   subheader.Header = "keine Schäden";
                   subheader.ID = "NoDamageListSubheader";
                   subheader.ControlType = "Subheader";
                   stackPanel.Children.Add(giveMeATextBlock(subheader));
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
                       canvas.Style = (Style)resources["DamageCanvas"];
                       Border border = new Border();
                       border.Style = (Style)resources["DamageBorder"];
                       int j = 0;
                       while(j<list.Count)
                       {
                           if(list[j].Location == location)
                           {
                               Ellipse ellipse = new Ellipse();
                               ellipse.Style = (Style)resources["DamagePoint"];
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
                       //button.Click += FormPage.navigateToDamageSite;
                       border.Tag = location;
                       border.Tap += FormPage.navigateToDamageSite;
                       stackPanel.Children.Add(border);
               }
               
           }
           FormPage.order.setStateOfFormItem(item.ID, FormItemState.Disabled);
           stackPanel.Name = item.ID;
           return stackPanel;
       }

       public CheckBox giveMeACheckBox(FormItem item) {
           CheckBox checkBox = new CheckBox();
           setDefaultValues(checkBox,item);
           checkBox.Content = item.Header;
           checkBox.Unchecked += FormPage.changeStateToEdited;
           checkBox.Checked += FormPage.changeStateToEdited;
           return checkBox;
       }

       public Button giveMeAPhotoButton(FormItem item) {
           Button button = new Button();
           Image image = new Image();
           if (item.State == FormItemState.Blank.ToString())
               image.Source = App.formHandler.getPhotoFromIsolatedStorage(item.ID + "Template");
           else
           {
               image.Source = App.formHandler.getPhotoFromIsolatedStorage(FormPage.order.OrderID + FormPage.page.FormPageID + item.ID);
               FormPage.order.setStateOfFormItem(item.ID, FormItemState.Edited);
           }
           image.Name = item.ID;
           button.Content = image;
           button.Click += FormPage.makeAPhoto;
           setDefaultValues(button, item);
           return button;
       }

       public TextBlock giveMeATextBlock(FormItem item)
       {
           if (resources == null) setResourceDictionary();
           TextBlock textBlock = new TextBlock();
           textBlock.Text = item.Header;
           Style style = (Style)resources[item.ControlType];
           if (style == null)
               Debug.WriteLine("Fehler: Der Style für " + item.ControlType + " fehlt!");
           else
               textBlock.Style = style;

           FormPage.order.setStateOfFormItem(item.ID, FormItemState.Disabled);
           textBlock.Name = item.ID;
           return textBlock;
       }

       public Button giveMeAPageLink(FormItem item)
       {
           if (resources == null) setResourceDictionary();
           Button link = new Button();
           link.Content = item.Header;
           Style style = (Style)resources[item.ControlType];
           if (style == null)
               Debug.WriteLine("Fehler: Der Style für " + item.ControlType + " fehlt!");
           else
               link.Style = style;

           link.Click += FormPage.openPage;
           link.Name = item.ID;
           item.State = FormItemState.Disabled.ToString();
           return link;
       }



       public override void editedTextBox(TextBox textBox, bool isANote) 
       {
           Debug.WriteLine("zu nem TextBlock");
           TextBlock textBlock = new TextBlock();

           if(isANote)
               textBlock.Style = (Style)resources["NoteEdited"];
           else
               textBlock.Style = (Style)resources["OverviewEdit"];

           textBlock.Text = textBox.Text;
           String name = textBox.Name;
           if (isANote)
               textBlock.Tag = textBox.Tag;
           textBlock.Name = textBox.Name+"Temp";
           FormItem item = FormPage.order.setStateOfFormItem(textBox.Name, FormItemState.Edited);
           if (item == null && !isANote)
           {
               Debug.WriteLine("Der TextBox " + textBox.Name + " kann kein FormItem zugeordnet werden!");
               return;
           }
           else
           {
                if(!isANote){
                               if (textBlock.Text == "") textBlock.Text = "("+item.Header + ")";
                }
           }
           if(textBox.Parent!=null)
               if (textBox.Parent.GetType() == typeof(StackPanel))
               {
                   StackPanel parent = (StackPanel)textBox.Parent;
                   int index = parent.Children.IndexOf(textBox);
                   parent.Children.Remove(textBox);
                   textBox.Name = "";
                   parent.Children.Insert(index, textBlock);
                   textBlock.Name = name;
                   if(!isANote)
                   {
                   textBlock.Tap += FormPage.handleTap;
                   textBlock.Hold += FormPage.makeANote;
                   }
               }
               else
               {
                   Debug.WriteLine("Die Umwandlung der TextBox in ein TextBlock ist nicht möglich, da ihr Parent kein StackPanel ist.");
               }
           
       }

       public override void selectedTextBlock(TextBlock textBlock, bool isANote)
       {
           Debug.WriteLine("zu ner TextBox");
           TextBox textBox = new TextBox();
           if(isANote)
               textBox.Style = (Style)resources["NoteSelected"];
           else
           textBox.Style = (Style)resources["TextBoxSelected"];
           if (isANote)
               textBox.Tag = textBlock.Tag;
           textBox.Text = textBlock.Text;
           textBox.Name = textBlock.Name;
           if (textBlock.Parent.GetType() == typeof(StackPanel))
           {
               StackPanel parent = (StackPanel)textBlock.Parent;
               int index = parent.Children.IndexOf(textBlock);
               parent.Children.Remove(textBlock);
               parent.Children.Insert(index, textBox);
               if (isANote)
               {
                   textBox.GotFocus += FormPage.selectedNote;
                   textBox.LostFocus += FormPage.noteFinished;
                   textBox.KeyDown += FormPage.lookIfNoteFinished;
               }
               else
               {
                   textBox.Tap += FormPage.handleTap;
                   textBox.LostFocus += FormPage.changeStateToEdited;
                   textBox.LostFocus += FormPage.showApplicationBar;
                   textBox.GotFocus += FormPage.hideApplicationBar;
                   textBox.KeyDown += FormPage.textChangedEvent;
               }
               textBox.SelectAll();
               textBox.Focus();
           }
           else
           {
               Debug.WriteLine("Die Umwandlung des TextBlocks in eine TextBlox ist nicht möglich, da ihr Parent kein StackPanel ist.");
           }
       }

       public FrameworkElement giveMeATextBoxOrTextBlock(FormItem item)
       {
           if (resources == null) setResourceDictionary();
           TextBox textBox = new TextBox();
           setDefaultValues(textBox, item);
           textBox.Text = item.Input;
           if (item.Input == "")
               textBox.Text = item.Header;
           textBox.KeyDown += FormPage.textChangedEvent;
           
           return textBox;
       }

       public OwnListPicker giveMeAListPicker(FormItem item)
       {
           if (resources == null) setResourceDictionary();

           OwnListPicker listPicker = new OwnListPicker();
           setDefaultValues(listPicker, item);

           Style tbStyle = (Style)resources["ListBlank"];
           
               List<TextBlock> tbList = new List<TextBlock>();
               foreach (string s in item.ControlList)
               {
                   TextBlock tb = new TextBlock();
                   tb.Text = s;
                   if(tbStyle!=null)
                   tb.Style = tbStyle;

                   if (item.Input != "" && item.Input == s)
                   {
                       listPicker.AddAsSelected(tb);
                   }
                   else
                       listPicker.Add(tb);
               }

          if (tbStyle == null)
               {
                Debug.WriteLine("Fehler: Entweder gibt es keinen Style mehr für TextBlocks in ListPicker, oder er heißt nicht mehr \"List\"!");
           }
          TextBlock header = new TextBlock();
          header.Style = tbStyle;
          header.Text = item.Header;
          listPicker.SetHeader(header);
           return listPicker;
       }

        private void setDefaultValues(FrameworkElement element, FormItem item)
        {
            //guck nach, ob es die Informationen zu diesem Formularelement shcon gibt
            //Debug.WriteLine("Der Zustand des Items  " + item.ID + " ist " + item.State);
            if (EnumerationMatcher.StringToFormItemState(item.State) != FormItemState.Edited)
            {
                bool found = false;
                foreach (OrderInformation info in FormPage.order.Information)
                {

                    if (info.ID == item.ID || info.ID == (FormPage.formID + item.ID))
                    {
                        Debug.WriteLine("Die Information zu " + item.ID + " ist bereits vorhanden: " + info.Information);
                        item.Input = info.Information;
                        found = true;
                    }
                }
                if (found) FormPage.order.setStateOfFormItem(item.ID, FormItemState.Edited);
            }

            if ((Style)resources[item.ControlType + item.State] == null)
                Debug.WriteLine("Style für " + item.ControlType + " im Zustand "+item.State+" fehlt!");
            else
                element.Style = (Style)resources[item.ControlType + item.State];

            if (FormPage != null)
            {
                //element.GotFocus += FormPage.changeStateToSelected;
                element.LostFocus += FormPage.changeStateToEdited;
                if (element.GetType() == typeof(TextBox)) 
                {
                    element.LostFocus += FormPage.showApplicationBar;
                    element.GotFocus += FormPage.hideApplicationBar;
                }
                element.Tap += FormPage.handleTap;
                element.Hold += FormPage.makeANote;
            }
            else
                Debug.WriteLine("Page fehlt für EventHandler");

            
            element.Name = item.ID;
        }
#endregion

    }
}
