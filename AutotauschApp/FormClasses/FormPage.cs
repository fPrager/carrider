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
    public class FormPage
    {
        public String FormPageID;
        public String FormPageHeader;
        public List<FormItem> FormItemList;
        public String State = "Disabled";

        public FormItem setStateOfFormItem(String id, FormItemState state)
        {
            FormItem ItemExist = null;
            foreach (FormItem item in FormItemList)
                {
                    if (item.ID == id)
                    {
                        ItemExist = item;
                        item.State = state.ToString();
                        checkMyState();
                        break;
                    }
                }
            return ItemExist;
        }

        public FormItem getFormItem(String id)
        {
            FormItem ItemExist = null;
            foreach (FormItem item in FormItemList)
            {
                if (item.ID == id)
                {
                    ItemExist = item;
                    break;
                }
            }
            return ItemExist;
        }

        public void checkMyState() {

            bool fullyEdited = true;
            bool partlyEdited = false;

            foreach (FormItem item in FormItemList){
                FormItemState state = EnumerationMatcher.StringToFormItemState(item.State);
                if (state == FormItemState.Edited) partlyEdited = true;
                if (state == FormItemState.Blank && item.Important) fullyEdited = false;
            }

            if (fullyEdited) State = FormPageState.FullyEdited.ToString();
            else
                if (partlyEdited) State = FormPageState.PartlyEdited.ToString();
                else
                    State = FormPageState.Disabled.ToString();
        }
    }
}
