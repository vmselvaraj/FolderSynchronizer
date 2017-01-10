using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderSyncronizer.Business
{
    public class RoboCopy
    {
        public static void Copy(string strSourceFilePath, string DestinationFilePath, string FileName)
        {
            try
            {
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo();
                p.StartInfo.FileName = "Robocopy.exe";
                p.StartInfo.Arguments = string.Format("\"{0}\"  \"{1}\"  \"{2}\" ", strSourceFilePath, DestinationFilePath, FileName);
                p.StartInfo.UseShellExecute = false; // set this to false so that we can redirect the output
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                string result = p.StandardOutput.ReadToEnd();
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                // Setting the Error Description in CommonFunctions Class.
                throw ex;
            }
        }
    }
}
