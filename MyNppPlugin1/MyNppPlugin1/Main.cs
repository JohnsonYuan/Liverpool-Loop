using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using NppPluginNET;

namespace NppSDPlugin
{
    struct ShortCutConfig
    {
        public string ConfigKeyName;
        public Keys ShortKey;
        public bool IsCtrl;
        public bool IsAlt;
        public bool IsShift;
    }
    class Main
    {
        #region " Fields "
        internal const string PluginName = "TTS_SD_Command";
        internal const string PluginRootPathKey = "BranchRoot";
        static string iniFilePath = null;
        static string BanchRootPath = null;
        static frmSetRoot frmSetRoot = null;
        static int idMyDlg = -1;
        static Bitmap tbBmp = Properties.Resources.star;
        static Bitmap tbBmp_tbTab = Properties.Resources.star_bmp;
        //static Icon tbIcon = null;

        static ShortCutConfig sdEditConfig = new ShortCutConfig { ConfigKeyName = "SD edit Shortcut", ShortKey = Keys.S, IsCtrl = true, IsAlt = true };
        static ShortCutConfig sdRevertConfig = new ShortCutConfig { ConfigKeyName = "SD revert Shortcut", ShortKey = Keys.None };
        #endregion

        #region " StartUp/CleanUp "
        internal static void CommandMenuInit()
        {
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            iniFilePath = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFilePath)) Directory.CreateDirectory(iniFilePath);
            iniFilePath = Path.Combine(iniFilePath, PluginName + ".ini");

            // get branch root path
            StringBuilder sbTemp = new StringBuilder(Win32.MAX_PATH);
            Win32.GetPrivateProfileString(PluginName, PluginRootPathKey, "", sbTemp, Win32.MAX_PATH, iniFilePath);
            BanchRootPath = sbTemp.ToString();
            SetShortCut(ref sdEditConfig);
            SetShortCut(ref sdRevertConfig);

            PluginBase.SetCommand(0, "sd edit", SDEdit, new ShortcutKey(sdEditConfig.IsCtrl, sdEditConfig.IsAlt, sdEditConfig.IsShift, sdEditConfig.ShortKey));
            PluginBase.SetCommand(1, "sd revert", SDRevert, new ShortcutKey(sdRevertConfig.IsCtrl, sdRevertConfig.IsAlt, sdRevertConfig.IsShift, sdRevertConfig.ShortKey));
            PluginBase.SetCommand(3, "set root path", SetBranchRoot);
            idMyDlg = 1;
        }
        internal static void SetToolBarIcon()
        {
            toolbarIcons tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = tbBmp.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[idMyDlg]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }
        internal static void PluginCleanUp()
        {
            Win32.WritePrivateProfileString(PluginName, PluginRootPathKey, BanchRootPath, iniFilePath);
        }
        #endregion

        #region " Menu functions "

        /// <summary>
        /// Set shortcut config
        /// </summary>
        /// <param name="config">config object</param>
        private static void SetShortCut(ref ShortCutConfig config)
        {
            StringBuilder sbTemp = new StringBuilder(10);

            try
            {
                Win32.GetPrivateProfileString(config.ConfigKeyName, "ShortKey", "", sbTemp, Win32.MAX_PATH, iniFilePath);
                if (sbTemp.Length > 0)
                {
                    config.ShortKey = (Keys)Enum.Parse(typeof(Keys), sbTemp.ToString());
                }
                else
                {
                    if (config.ShortKey != Keys.None)
                    {
                        Win32.WritePrivateProfileString(config.ConfigKeyName, "ShortKey", config.ShortKey.ToString(), iniFilePath);
                    }
                }

                Win32.GetPrivateProfileString(config.ConfigKeyName, "IsCtrl", "", sbTemp, Win32.MAX_PATH, iniFilePath);
                if (sbTemp.Length > 0)
                {
                    config.IsCtrl = (int)Enum.Parse(typeof(int), sbTemp.ToString()) != 0;
                }
                else
                {
                    Win32.WritePrivateProfileString(config.ConfigKeyName, "IsCtrl", config.IsCtrl ? "" : "0", iniFilePath);
                }

                Win32.GetPrivateProfileString(config.ConfigKeyName, "IsAlt", "", sbTemp, Win32.MAX_PATH, iniFilePath);
                if (sbTemp.Length > 0)
                {
                    config.IsAlt = (int)Enum.Parse(typeof(int), sbTemp.ToString()) != 0;
                }
                else
                {
                    Win32.WritePrivateProfileString(config.ConfigKeyName, "IsAlt", config.IsAlt ? "1" : "0", iniFilePath);
                }

                Win32.GetPrivateProfileString(config.ConfigKeyName, "IsShift", "", sbTemp, Win32.MAX_PATH, iniFilePath);
                if (sbTemp.Length > 0)
                {
                    config.IsShift = (int)Enum.Parse(typeof(int), sbTemp.ToString()) != 0;
                }
                else
                {
                    Win32.WritePrivateProfileString(config.ConfigKeyName, "IsShift", config.IsShift ? "1" : "0", iniFilePath);
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Promopt user set the branch root path
        /// </summary>
        /// <param name="initPath">init branch root path</param>
        /// <param name="forceSet">force to reset path</param>
        /// <returns>success or not</returns>
        private static bool PromoptSelectBranchRoot(string initPath, bool forceSet = false)
        {
            if (forceSet || (String.IsNullOrEmpty(BanchRootPath)
                   || !Directory.Exists(BanchRootPath)))
            {
                if (frmSetRoot == null)
                {
                    frmSetRoot = new frmSetRoot(initPath);
                }

                if (frmSetRoot.ShowDialog() == DialogResult.OK)
                {
                    Win32.WritePrivateProfileString(PluginName, PluginRootPathKey, frmSetRoot.SelectedPath, iniFilePath);
                    BanchRootPath = frmSetRoot.SelectedPath;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// sd edit file
        /// </summary>
        private static void SDEdit()
        {
            if (PromoptSelectBranchRoot(BanchRootPath))
            {
                Util.SdCheckoutFile(BanchRootPath, GetCurrentFilePath());
            }
        }

        /// <summary>
        /// sd revert file
        /// </summary>
        private static void SDRevert()
        {
            if (PromoptSelectBranchRoot(BanchRootPath))
            {
                Util.SdRevertFile(BanchRootPath, GetCurrentFilePath());
            }
        }

        /// <summary>
        /// Set branch root path
        /// </summary>
        private static void SetBranchRoot()
        {
            PromoptSelectBranchRoot(BanchRootPath, true);
        }

        /// <summary>
        /// Get current opened file path
        /// </summary>
        /// <returns></returns>
        private static string GetCurrentFilePath()
        {
            // get current file path
            StringBuilder sbFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, 0, sbFilePath);

            return sbFilePath.ToString();
        }

        #endregion
    }
}