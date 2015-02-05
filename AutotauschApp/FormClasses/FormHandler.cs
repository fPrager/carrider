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
                defaultFormItemType = FormItemType.Subheader.ToString();
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

        public bool SaveSignum(Stream stream, String PersonName) 
        {
            try
            {
                PersonName = PersonName.Replace(" ", "");
                StreamReader srPNG = new StreamReader(stream);
                byte[] baBinaryData = new Byte[stream.Length];
                long bytesRead = stream.Read(baBinaryData, 0, (int)stream.Length);
                IsolatedStorageFileStream isfStream = new IsolatedStorageFileStream(PersonName + "Signum.png", FileMode.Create, IsolatedStorageFile.GetUserStoreForApplication());
                isfStream.Write(baBinaryData, 0, baBinaryData.Length);
                isfStream.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public BitmapSource getSignumOf(String PersonName)
        {
            PersonName = PersonName.Replace(" ", "");
            IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            if (!myIsolatedStorage.FileExists(PersonName + "Signum.png")) return null;
                IsolatedStorageFileStream isfStream = new IsolatedStorageFileStream(PersonName + "Signum.png", FileMode.Open, IsolatedStorageFile.GetUserStoreForApplication());
                BitmapImage biImage = new BitmapImage();
                biImage.SetSource(isfStream);
                isfStream.Close();
                return (biImage);
        }

        public BitmapImage getPhotoFromIsolatedStorage(String id) {
          //  id = id.Replace(" ", "");
            BitmapImage bi = new BitmapImage();
            try
            {
                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (myIsolatedStorage.FileExists(id+".jpg"))
                        using (IsolatedStorageFileStream fileStream = myIsolatedStorage.OpenFile(id + ".jpg", FileMode.Open, FileAccess.Read))
                        {
                            bi.SetSource(fileStream);
                        }
                    else
                        return null;
                }
            }
            catch {
                Debug.WriteLine("Fehler beim Laden des Fotos " +id+"!");
            }
           return bi;
        }

        public bool savePhotoToIsolatedStorage(Stream imageStream, string fileName)
        {
           // fileName = fileName.Replace(" ", "");
            try
            {
                using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (myIsolatedStorage.FileExists(fileName))
                    {
                        myIsolatedStorage.DeleteFile(fileName);
                    }

                    IsolatedStorageFileStream fileStream = myIsolatedStorage.CreateFile(fileName);
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.SetSource(imageStream);

                    WriteableBitmap wb = new WriteableBitmap(bitmap);
                    wb.SaveJpeg(fileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);
                    fileStream.Close();
                }
                return true;
            }
            catch {
                Debug.WriteLine("Fehler beim speichern des Fotos.");
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

        public bool loadTemplateImagesToISolatedStorage(List<String> files) {
            
            bool savingCheck = true;
            foreach (String file in files) {
                String tempJPEG = file;
                try
                {
                    // Create virtual store and file stream. Check for duplicate tempJPEG files.
                    using (IsolatedStorageFile myIsolatedStorage = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        if (myIsolatedStorage.FileExists(tempJPEG))
                        {
                            myIsolatedStorage.DeleteFile(tempJPEG);
                        }

                        IsolatedStorageFileStream fileStream = myIsolatedStorage.CreateFile(tempJPEG);

                        StreamResourceInfo sri = null;
                        Uri uri = new Uri(tempJPEG, UriKind.Relative);
                        sri = Application.GetResourceStream(uri);

                        BitmapImage bitmap = new BitmapImage();
                        bitmap.SetSource(sri.Stream);
                        WriteableBitmap wb = new WriteableBitmap(bitmap);

                        // Encode WriteableBitmap object to a JPEG stream.
                        Extensions.SaveJpeg(wb, fileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);

                        //wb.SaveJpeg(fileStream, wb.PixelWidth, wb.PixelHeight, 0, 85);
                        fileStream.Close();
                    }
                }
                catch { 
                    Debug.WriteLine("Fehler beim Speichern von "+file+"!");
                    savingCheck = false;
                }
              }
            return savingCheck;
        
        }
    }
}
