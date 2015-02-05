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
    public class Order
    {
        public String OrderID;
        public List<Form> FormList;
        public String State = "Unsigned";
        public List<OrderInformation> Information;
        public List<DamageEntry> DamageList;

        public FormItem setStateOfFormItem(String id, FormItemState state) {
            FormItem ItemExist = null;
           foreach (Form form in FormList) { 
                    ItemExist = form.setStateOfFormItem(id, state);
                    if (ItemExist!=null) break;
           }

           return ItemExist;  
        }

        public FormItem getFormItem(String id)
        {
            FormItem ItemExist = null;
            foreach (Form form in FormList)
            {
                ItemExist = form.getFormItem(id);
                if (ItemExist != null) break;
            }
            return ItemExist;
        }
        
    }
}
