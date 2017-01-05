using FolderSyncronizer.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FolderSyncronizer.Business
{
    public static class SettingsIO
    {

        /// <summary>
        /// Persists the Application Post Analysis Settings
        /// </summary>
        /// <param name="settings">Settings to Persist</param>
        /// <param name="filePath"></param>
        public static void PersistSettings(Settings settings, string filePath = null)
        {
            filePath = GetAppropriateFilePath(filePath);
            //Write settings to Applicatin Data Folder
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                XmlSerializer serializer = new XmlSerializer(settings.GetType());
                serializer.Serialize(writer, settings);
                writer.Flush();
                writer.Close();
            }
        }

        public static Settings LoadSettings(string filePath = null)
        {
            Settings settings = null;
            filePath = GetAppropriateFilePath(filePath);
            if (File.Exists(filePath))
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    settings = serializer.Deserialize(sr) as Settings;
                }
            }
            else
                settings = new Settings(); //Initialize Default

            return settings;
        }

        #region Helpers

        private static string GetAppropriateFilePath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                filePath = AppSettingsFilePath;

            return filePath;
        }


        private static string AppSettingsFilePath
        {
            get
            {
                string path = Path.Combine(AppFolder, "FolderSynchronizerAppData");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                path = Path.Combine(path, "Settings.xml");
                return path;
            }
        }

        private static string AppFolder
        {
            get
            {
                string path = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    path = Directory.GetParent(path).ToString();
                }

                return path;
            }
        }

        #endregion
    }
}
