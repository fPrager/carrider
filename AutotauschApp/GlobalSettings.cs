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

namespace AutotauschApp
{
    public static class GlobalSettings
    {
        public static bool fromSignumPage = false;
        public static FrameworkElement focusElement;
        public static bool fromSignPages = false;
        public static String myName = "Klaus Mobil";
        public static List<string> HasSignedList = new List<string>();
    }
}
