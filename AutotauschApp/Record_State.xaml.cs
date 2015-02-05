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

namespace AutotauschApp
{
    public partial class state : PhoneApplicationPage
    {
        public state()
        {
            InitializeComponent();
            Debug.WriteLine("state go");
        }

        public void test(object sender, EventArgs e) {
            ListPicker picker = (ListPicker)sender;
            Debug.WriteLine(sender.ToString());
            Debug.WriteLine(picker.Background.ToString());
        }
    }
}