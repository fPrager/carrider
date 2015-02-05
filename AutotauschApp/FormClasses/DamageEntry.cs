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

namespace AutotauschApp
{
    public class DamageEntry
    {
       public String ID;
       public String PhotoPath = "";
       public String Location = "";
       public decimal RelativeLocationX = 0;
       public decimal RelativeLocationY = 0;
       public bool IsCrack = false;
       public bool IsDent = false;
       public bool IsBroken = false;
       public bool IsScratch = false;
       public String Other = "";
       public String Short = "";
       public bool Signed = false;
    }
}
