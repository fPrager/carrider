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
using System.Diagnostics;

namespace AutotauschApp
{
    public enum FormItemShortHeaderSide 
    { 
    Left,
        Right
    }

    public enum FormItemState { 
        Disabled,
        Edited,
        Selected,
        Blank
    }

    public enum FormItemType { 
        DatePicker,
        ListPicker,
        TimePicker,
        CheckBox,
        Header,
        Subheader,
        TextBox,
        Photo,
        DamageEntry,
        PageLink,
        Listing
    }

    public enum FormType {
        GivingForm
    }

    public enum FormState { 
        Open,
        Signable,
        Signed,
        Uploaded
    }

    public enum OrderState
    {
        Overview,
        Journey,
        Acceptance,
        Route,
        Giving,
        Retour,
        Costs

    }

    public enum FormPageState { 
        Disabled,
        PartlyEdited,
        FullyEdited
    }

    public enum FormPageType { 
        None,
        Data,
        State,
        Person,
        Crash
    }

    public static class EnumerationMatcher
    {
        public static FormPageType StringToFormPageType(String s)
        {
            try
            {
                FormPageType type = (FormPageType)Enum.Parse(typeof(FormPageType), s, true);
                return type;
            }
            catch
            {
                Debug.WriteLine("Fehler beim Parsen von String zu FormPageType");
                return FormPageType.None;
            }
        }

        public static FormItemShortHeaderSide StringToFormItemShortHeaderSide(String s)
        {
            try
            {
                FormItemShortHeaderSide side = (FormItemShortHeaderSide)Enum.Parse(typeof(FormItemShortHeaderSide), s, true);
                return side;
            }
            catch
            {
                Debug.WriteLine("Fehler beim Parsen von String zu FormItemShortHeaderSide");
                return FormItemShortHeaderSide.Left;
            }
        }

        public static FormPageState StringToFormPageState(String s)
        {
            try {
                FormPageState state = (FormPageState)Enum.Parse(typeof(FormPageState), s, true);
                return state;
            }
            catch
            {
                Debug.WriteLine("Fehler beim Parsen von String zu FormPageState");
                return FormPageState.Disabled;
            }
        }

        public static FormItemState StringToFormItemState(String s)
        {
            try
            {
                FormItemState state = (FormItemState)Enum.Parse(typeof(FormItemState), s, true);
                return state;
            }
            catch
            {
                Debug.WriteLine("Fehler beim Parsen von String zu FormItemState");
                return FormItemState.Disabled;
            }
        }

        public static OrderState StringToOrderState(String s)
        {
            try
            {
                OrderState state = (OrderState)Enum.Parse(typeof(OrderState), s, true);
                return state;
            }
            catch
            {
                Debug.WriteLine("Fehler beim Parsen von String zu FormItemState");
                return OrderState.Overview;
            }
        }

        public static FormState StringToFormState(String s)
        {
            try
            {
                FormState state = (FormState)Enum.Parse(typeof(FormState), s, true);
                return state;
            }
            catch
            {
                Debug.WriteLine("Fehler beim Parsen von String zu FormState");
                return FormState.Open;
            }
        }

        public static FormType StringToFormType(String s)
        {
            try
            {
                FormType type = (FormType)Enum.Parse(typeof(FormType), s, true);
                return type;
            }
            catch
            {
                Debug.WriteLine("Fehler beim Parsen von String zu FormType");
                return FormType.GivingForm;
            }
        }

        public static FormItemType StringToFormItemType(String s)
        {
            try
            {
                FormItemType type = (FormItemType)Enum.Parse(typeof(FormItemType), s, true);
                return type;
            }
            catch
            {
                Debug.WriteLine("Fehler beim Parsen von String zu FormItemType: "+s);
                return FormItemType.Subheader;
            }
        }
    }
}
