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
    public class Form
    {
        public String FormID;
        public List<FormPage> FormPageList;
        public String State = "Open";
        public String FormHeader;
        public bool FormOtherHaveToSign = false;
        public bool FormIHaveToSign = false;

        public Form() { 
        
        }

        public bool load(){
            return false;
        }

        public bool goInSignMode() {
            return false;
        }

        public bool upload() {
            return false;
        }

        public FormItem setStateOfFormItem(String id, FormItemState state)
        {
           FormItem ItemExist = null;
           foreach (FormPage page in FormPageList)
           {
               ItemExist = page.setStateOfFormItem(id, state);
               checkMyState();
               if (ItemExist!=null) break;
           }
            return ItemExist;
        }


        public FormItem getFormItem(String id)
        {
            FormItem ItemExist = null;
            foreach (FormPage page in FormPageList)
            {
                ItemExist = page.getFormItem(id);
                if (ItemExist != null) break;
            }
            return ItemExist;
        }

        public void checkMyState()
        {
            FormState myState = EnumerationMatcher.StringToFormState(this.State);
            if (myState == FormState.Signed)
            {
                State = FormState.Signed.ToString();
                return;
            }

            if (myState == FormState.Uploaded)
            {
                State = FormState.Uploaded.ToString();
                return;
            }

            bool signable = true;

            foreach (FormPage page in FormPageList)
            {
                page.checkMyState();
                FormPageState state = EnumerationMatcher.StringToFormPageState(page.State);
                if (state == FormPageState.Disabled || state == FormPageState.PartlyEdited) signable = false;  
            }

            if (signable)
            {
                State = FormState.Signable.ToString();
                return;
            }
            State = FormState.Open.ToString();
        }

    }
}
