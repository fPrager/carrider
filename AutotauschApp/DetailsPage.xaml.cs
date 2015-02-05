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
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace AutotauschApp
{
    public partial class DetailsPage : PhoneApplicationPage
    {
        // Konstruktor
        public DetailsPage()
        {
            InitializeComponent();
        }

        // Legt beim Navigieren auf der Seite den Datenkontext auf das in der Liste ausgewählte Element fest
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string selectedIndex = "";
            if (NavigationContext.QueryString.TryGetValue("selectedItem", out selectedIndex))
            {
                int index = int.Parse(selectedIndex);
                DataContext = App.ViewModel.Items[index];
            }
        }
    }
}