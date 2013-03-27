using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Windows;
using System.Xml.Serialization;
using HAOest.Utils.Environment;

namespace HAOest.ListManager
{
    [Serializable]
    public class ProjectVersion
    {
        private static Uri latestVersionXmlUrl = new Uri("http://listmanager.haoest.com/LatestVersion.xml");

        public int BuildNumber
        {
            get;
            set;
        }

        public string AssemblyVersion
        {
            get;
            set;
        }

        public List<string> WhatsNew
        {
            get;
            set;
        }

        public FilesPackage NewFilesPackage
        {
            get;
            set;
        }

        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="xmlFilePath"></param>
        /// <returns></returns>
        public static ProjectVersion Deserialize(string xmlFilePath)
        {
            try
            {
                FileStream fileStream = new FileStream(xmlFilePath, FileMode.Open);
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ProjectVersion));
                ProjectVersion newVersion = xmlSerializer.Deserialize(fileStream) as ProjectVersion;
                fileStream.Close();
                return newVersion;
            }
            catch (Exception ex)
            {
                throw new Exception("HAOest.ListManager.ProjectVersion.Deserialize: 反序列化失败！", ex);
            }

        }

        public static ProjectVersion GetLatestVersion()
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(latestVersionXmlUrl, "LatestVersion.xml");
                return ProjectVersion.Deserialize("LatestVersion.xml");
            }
            catch
            {
                throw;
            }
        }

        public static ProjectVersion GetLocalVersion()
        {
            try
            {
                ProjectVersion localVersion = new ProjectVersion();
                localVersion.BuildNumber = Info.GetBuildNumber(Info.GetDirectory() + "\\ListManager.exe");
                localVersion.AssemblyVersion = Info.GetAssemblyVersion(Info.GetDirectory() + "\\ListManager.exe");
                return localVersion;
            }
            catch
            {
                throw;
            }
        }
    }

    public struct FilesPackage
    {
        public string FileName { get; set; }
        public string Size { get; set; }
        public string FileUrl { get; set; }
    }
}
