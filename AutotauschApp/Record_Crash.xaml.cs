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

namespace AutotauschApp
{
    public partial class record_crash : PhoneApplicationPage
    {

        
        public record_crash()
        {
            InitializeComponent();
        }
        private void openDamage(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(string.Format("/Damage.xaml"), UriKind.Relative));
        }
    }
}

