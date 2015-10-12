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
    class Main
    {
        #region " Fields "
        internal const string PluginName = "TTS SD Command";
        static string iniFilePath = null;
        static string branchRootPath = null;
        static frmSetRoot frmSetRoot = null;
        static int idMyDlg = -1;
        static Bitmap tbBmp = Properties.Resources.star;
        static Bitmap tbBmp_tbTab = Properties.Resources.star_bmp;
        static Icon tbIcon = null;
        #endregion

        #region " StartUp/CleanUp "
        internal static void CommandMenuInit()
        {
            StringBuilder sbIniFilePath = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sbIniFilePath);
            iniFilePath = sbIniFilePath.ToString();
            if (!Directory.Exists(iniFilePath)) Directory.CreateDirectory(iniFilePath);
            iniFilePath = Path.Combine(iniFilePath, PluginName + ".ini");

            StringBuilder sbRootPath = new StringBuilder(Win32.MAX_PATH);
            Win32.GetPrivateProfileString("BranchConfig", "RootPath", "", sbRootPath, Win32.MAX_PATH, iniFilePath);

            PluginBase.SetCommand(0, "sd edit", sdEdit, new ShortcutKey(false, false, false, Keys.None));
            PluginBase.SetCommand(1, "sd revert", sdRevert); idMyDlg = 1;
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
            Win32.WritePrivateProfileString("BranchConfig", "RootPath", branchRootPath, iniFilePath);
        }
        #endregion

        #region " Menu functions "

        private static void PromoptSelectBranchRoot(string iniPath)
        {
            if (String.IsNullOrEmpty(branchRootPath)
                   || !Directory.Exists(branchRootPath))
            {
                if (frmSetRoot == null)
                {
                    frmSetRoot = new frmSetRoot(iniPath);
                    if (frmSetRoot.ShowDialog() == DialogResult.OK)
                    {
                        Win32.WritePrivateProfileString("BranchConfig", "RootPath", frmSetRoot.SelectedPath, iniFilePath);
                    }
                }
            }
        }
        internal static void sdEdit()
        {
            MessageBox.Show(branchRootPath);
            MessageBox.Show("Hello N++!");
        }
        internal static void sdRevert()
        {
            PromoptSelectBranchRoot(branchRootPath);

            //if (frmMyDlg == null)
            //{
            //    frmMyDlg = new frmMyDlg();

            //    using (Bitmap newBmp = new Bitmap(16, 16))
            //    {
            //        Graphics g = Graphics.FromImage(newBmp);
            //        ColorMap[] colorMap = new ColorMap[1];
            //        colorMap[0] = new ColorMap();
            //        colorMap[0].OldColor = Color.Fuchsia;
            //        colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
            //        ImageAttributes attr = new ImageAttributes();
            //        attr.SetRemapTable(colorMap);
            //        g.DrawImage(tbBmp_tbTab, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
            //        tbIcon = Icon.FromHandle(newBmp.GetHicon());
            //    }

            //    NppTbData _nppTbData = new NppTbData();
            //    _nppTbData.hClient = frmMyDlg.Handle;
            //    _nppTbData.pszName = "My dockable dialog";
            //    _nppTbData.dlgID = idMyDlg;
            //    _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_RIGHT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
            //    _nppTbData.hIconTab = (uint)tbIcon.Handle;
            //    _nppTbData.pszModuleName = PluginName;
            //    IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
            //    Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

            //    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
            //}
            //else
            //{
            //    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMSHOW, 0, frmMyDlg.Handle);
            //}
        }
        #endregion
    }
}