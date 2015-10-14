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
        public static string sdToolPath;
        private const string SDPath = @"tools\coretools\sd.exe";
        private static readonly string amd64OfflineFolder = @"target\distrib\debug\amd64\dev\TTS\Server\bin\Offline";
        public static string BranchRootPath { set; get; }
        public static string SdToolPath
        {
            get
            {
                string sdToolPath = Path.Combine(BranchRootPath, SDPath);

                if (!File.Exists(sdToolPath))
                {
                    MessageBox.Show(string.Format("SD command file not found: {0}", sdToolPath));
                }
            
                return sdToolPath;
            }
        }
        /// <summary>
        /// cmd.exe /c set inetroot=D:\Enlistments\IPESpeechCore_Dev&set corextbranch=IPESpeechCore_Dev&D:\Enlistments\IPESpeechCore_Dev\tools\path1st\myenv.cmd&D:\Enlistments\IPESpeechCore_Dev\private\dev\speech\tts\shenzhou\build\tools\nightlybuild.cmd
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="message"></param>
        public static void SdCheckoutFile(string filePath, ref string message)
        {
            if(filePath.IndexOf(BranchRootPath) != 0)
            {
                MessageBox.Show("Cannot sd edit file not in branch!");
                return;
            }

            string sdMsg = string.Empty;
            try
            {
                Int32 sdExitCode = CommandLine.RunCommandWithOutputAndError("cmd.exe", string.Format(@"/c set inetroot={0}&set corextbranch={0}&{0}\tools\path1st\myenv.cmd&{1} edit {2}", BranchRootPath, SdToolPath, filePath), null, ref sdMsg);

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
                message = string.Format("{0}. Failed to check out file: {1}", e.Message, filePath);
            }
        }

        public static void SdRevertFile(string filePath, bool forceRevert, ref string message)
        {
            if (filePath.IndexOf(BranchRootPath) != 0)
            {
                MessageBox.Show("Cannot sd revert file not in branch!");
                return;
            }

            string sdMsg = string.Empty;

            try
            {
                string forceSd = forceRevert ? "" : "-a";
                Int32 sdExitCode = CommandLine.RunCommandWithOutputAndError("cmd.exe", string.Format(@"/c set inetroot={0}&set corextbranch={0}&{0}\tools\path1st\myenv.cmd&{1} revert {2}", BranchRootPath, SdToolPath, filePath), null, ref sdMsg);

                if (sdExitCode == 0 && !string.IsNullOrEmpty(sdMsg))
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
                message = string.Format("--{0}. Failed to revert unchanged file: {1}", e.Message, filePath);
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


        public static int RunCommand(string command, string arguments,
            bool useShellExecute, bool waitDone, string workingDirectory)
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new ArgumentNullException("command");
            }

            if (string.IsNullOrEmpty(arguments))
            {
                arguments = string.Empty;
            }

            if (!String.IsNullOrEmpty(workingDirectory) 
                && !Directory.Exists(workingDirectory))
            {
                throw new ArgumentException(workingDirectory,
                    new DirectoryNotFoundException(workingDirectory));
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = command;
            psi.Arguments = arguments;
            psi.WorkingDirectory = workingDirectory;
            psi.UseShellExecute = useShellExecute;
            psi.CreateNoWindow = true;
            psi.RedirectStandardOutput = false;
            psi.RedirectStandardError = false;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            Process process = Process.Start(psi);

            if (waitDone)
            {
                process.WaitForExit();
            }
            return process.ExitCode;
        }

        public static int RunCommandWithOutputAndError(string command, string arguments, string workingDirectory, ref string log)
        {
            int exitCode = 0;
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture))
            {
                exitCode = RunCommand(command, arguments, true, true, workingDirectory);
            }

            log = sb.ToString();

            return exitCode;
        }
    }
}
