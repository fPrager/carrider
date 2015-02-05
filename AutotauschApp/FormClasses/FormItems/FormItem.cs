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
    public class FormItem
    {

        public String Header = "";
        public String Note = "";
        public String ControlType = "";

        public List<String> ControlList = new List<String>();
        public String Input = "";
        public String ID;
        public String State = "Blank";
        public bool Important = true;

        public String PhotoPath = "";

        public String Link = "";

        public String ShortHeader = "";
        public String ShortHeaderSide = "Left";
        public bool IsEditedStateHeader = true;

        public bool InLineWithNext = false;
        
        public FormItem() {
        }

        public void setNote(String note){
            this.Note = note;
    }

    }
}
