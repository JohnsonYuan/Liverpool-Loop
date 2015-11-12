using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NppSDPlugin
{
    public static class Util
    {
        private const string SDPath = @"tools\coretools\sd.exe";
        
        /// <summary>
        /// sd edit file
        /// </summary>
        /// <param name="filePath">file path</param>
        public static void SdCheckoutFile(string branchRootPath, string filePath)
        {
            string message;
            if (filePath.IndexOf(branchRootPath) != 0)
            {
                MessageBox.Show("Cannot sd edit file not in branch!");
                return;
            }

            string sdToolPath = Path.Combine(branchRootPath, SDPath);

            try
            {
                Int32 sdExitCode = CommandLine.RunCommand(sdToolPath, string.Format("edit {0}", filePath), true);

                if (sdExitCode == 0)
                {
                    message = string.Format("Checked out file: {0}", filePath);
                }
                else
                {
                    message = string.Format("Failed to check out file: {0}", filePath);
                }
            }
            catch (Exception e)
            {
                message = string.Format("{0} Failed to check out file: {1}", e.Message, filePath);
                MessageBox.Show(message);
            }
        }

        /// <summary>
        /// sd revert file
        /// </summary>
        /// <param name="branchRootPath"></param>
        /// <param name="filePath"></param>
        public static void SdRevertFile(string branchRootPath, string filePath)
        {
            string message;
            if (filePath.IndexOf(branchRootPath) != 0)
            {
                MessageBox.Show("Cannot sd revert file not in branch!");
                return;
            }

            string sdToolPath = Path.Combine(branchRootPath, SDPath);

            try
            {
                Int32 sdExitCode = CommandLine.RunCommand(sdToolPath, string.Format("revert {0}", filePath), true);

                if (sdExitCode == 0 )
                {
                    message = string.Format("Reverted unchanged file: {0}", filePath);
                }
                else
                {
                    message = string.Empty;
                }
            }
            catch (Exception e)
            {
                message = string.Format("{0} Failed to revert unchanged file: {1}.", e.Message, filePath);
                MessageBox.Show(message);
            }
        }
    }

    public static class CommandLine
    {        
        /// <summary>
        /// Lock object to protect Process.Start.
        /// </summary>
        private static object _processStartLock = new object();

        private static Encoding defaultOutputEncoding = Encoding.UTF8;
        private static Encoding defaultErrorEncoding = Encoding.UTF8;

        public static int RunCommand(string command, string arguments, bool waitDone)
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new ArgumentNullException("command");
            }

            if (string.IsNullOrEmpty(arguments))
            {
                arguments = string.Empty;
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = command;
            psi.Arguments = arguments;
            psi.WorkingDirectory = Path.GetDirectoryName(command);
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = false;
            psi.RedirectStandardError = false;
            psi.WindowStyle = ProcessWindowStyle.Hidden;

            Process process = new Process();
            process.StartInfo = psi;
            process.Start();

            if (waitDone)
            {
                process.WaitForExit();
            }
            return process.ExitCode;
        }

 
    }
}
