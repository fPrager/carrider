﻿#pragma checksum "D:\Studium\WS11\VizKP\AutotauschApp\Order.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "4086D45AF8ABA1D580A728313037C265"
//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.239
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace AutotauschApp {
    
    
    public partial class OrderPage : Microsoft.Phone.Controls.PhoneApplicationPage {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.Controls.Canvas Pages;
        
        internal Microsoft.Phone.Controls.Pivot order_overview;
        
        internal Microsoft.Phone.Controls.PivotItem notes;
        
        internal Microsoft.Phone.Controls.PivotItem docs;
        
        internal Microsoft.Phone.Controls.PivotItem overview;
        
        internal Microsoft.Phone.Controls.PivotItem journey;
        
        internal Microsoft.Phone.Controls.PivotItem acceptance;
        
        internal Microsoft.Phone.Controls.PivotItem route;
        
        internal Microsoft.Phone.Controls.PivotItem giving;
        
        internal System.Windows.Controls.StackPanel GivingForms;
        
        internal Microsoft.Phone.Controls.PivotItem retour;
        
        internal Microsoft.Phone.Controls.PivotItem costs;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/AutotauschApp;component/Order.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.Pages = ((System.Windows.Controls.Canvas)(this.FindName("Pages")));
            this.order_overview = ((Microsoft.Phone.Controls.Pivot)(this.FindName("order_overview")));
            this.notes = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("notes")));
            this.docs = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("docs")));
            this.overview = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("overview")));
            this.journey = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("journey")));
            this.acceptance = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("acceptance")));
            this.route = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("route")));
            this.giving = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("giving")));
            this.GivingForms = ((System.Windows.Controls.StackPanel)(this.FindName("GivingForms")));
            this.retour = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("retour")));
            this.costs = ((Microsoft.Phone.Controls.PivotItem)(this.FindName("costs")));
        }
    }
}

