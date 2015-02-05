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
using System.Windows.Media.Imaging;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.Windows.Resources;
using System.Collections.Generic;

namespace AutotauschApp
{
    public class FormHandler
    {
        String defaultFormItemType;
            public FormHandler(){
                defaultFormItemType = FormItemType.FormPartItem.ToString();
            }


        public Order loadOrderFromIsolatedStorage(String OrderID){
            try
            {
                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = myIsolatedStorage.OpenFile("Order"+OrderID+".xml", FileMode.Open))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(Order));
                        Order data = (Order)serializer.Deserialize(stream);
                        Debug.WriteLine("Order geladen geladen");
                        return data;
                    }
                }
            }
            catch
            {
                //add some code here
                Debug.WriteLine("Fehler beim laden der XML aus dem IsolatedStorage");
                return null;
            }
        }

        public bool loadOrderFromUri(Uri uri) {
            Order data;
            XmlSerializer serializer;

            try
            {
                StreamResourceInfo sri = App.GetResourceStream(uri);
                serializer = new XmlSerializer(typeof(Order));
                data = (Order)serializer.Deserialize(sri.Stream);
                Debug.WriteLine(uri + " geladen");
                validateData(data);
            }
            catch
            {
                //add some code here
                Debug.WriteLine("Fehler beim laden der XML");
                return false;
            }

            try
            {
               
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = myIsolatedStorage.OpenFile("Order" + data.OrderID + ".xml", FileMode.Create))
                    {
                        using (XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                        {
                            serializer.Serialize(xmlWriter, data);
                            Debug.WriteLine(uri + " im IsolatedStorage gespeichert.");
                        }
                    }
                }
                return true;
            }
            catch
            {
                //add some code here
                Debug.WriteLine("Fehler beim speichern der XML im Storage");
                return false;
            }
        }

        public bool saveData(Order data) {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Order));;
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream stream = myIsolatedStorage.OpenFile("Order" + data.OrderID + ".xml", FileMode.Create))
                    {
                        using (XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                        {
                            serializer.Serialize(xmlWriter, data);
                            Debug.WriteLine("Order" + data.OrderID + ".xml im IsolatedStorage gespeichert.");
                        }
                    }
                }
                return true;
            }
            catch
            {
                //add some code here
                Debug.WriteLine("Fehler beim speichern der XML im Storage");
                return false;
            }
        
        }


        private void validateData(Order data) { 
           
            if (data.OrderID=="") {
                Debug.WriteLine("Warnung---> ID des Auftrags fehlt!");
                data.OrderID="000000";
            }
            else  
            if(data.FormList.Count>0)
                     Debug.WriteLine("In dem Auftrag ("+data.OrderID+") ist ...");          
            int i = 0;
            while (i < data.FormList.Count) {
                Form form = data.FormList[i];
                if (form.FormID == "")
                {
                    Debug.WriteLine("Warnung---> ID für das " + (i + 1) + ". Formular fehlt!");
                    data.FormList[i].FormID = "Form" + i;
                }
                else
                    Debug.WriteLine("... das Formular " + form.FormID + " enthalten");

                int j = 0;

                while (j < form.FormPageList.Count) {
                    FormPage page = form.FormPageList[j];
                    if (page.FormPageID == "")
                    {
                        Debug.WriteLine("Warnung---> ID für die " + (j + 1) + ". Formularseite fehlt!");
                        data.FormList[i].FormPageList[j].FormPageID = "FormPage" + i + "_" + j;
                    }
                    else
                        Debug.WriteLine("      mit der Formularseite: " + page.FormPageID);

                    int k = 0;

                    while (k < page.FormItemList.Count) {
                        FormItem item = page.FormItemList[k];
                        if (item.ID == "") {
                            Debug.WriteLine("Warnung---> ID für das " + (k + 1) + ". Formularelement fehlt!");
                            data.FormList[i].FormPageList[j].FormItemList[k].ID = "FormItem" + i + "_" + j + "_" + k;
                        }
                        else
                            Debug.WriteLine("                    Formularelement: " + item.ID);
                        
                        if (item.ControlType == "")
                        {
                            Debug.WriteLine("Warnung---> Typ für das " + (k + 1) + ". Formularelement fehlt!");
                            data.FormList[i].FormPageList[j].FormItemList[k].ControlType = defaultFormItemType;
                        }
                        k++;
                    }
                    j++;
                }
                i++;
            }
        
        }

    }
}
