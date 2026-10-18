using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using System.Security.Principal;
using System.Windows.Forms;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Drawing;
using Microsoft.VisualBasic;

namespace Shortcut_Virus_Removal_Tool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex > -1)
            {
                //DriveInfo selectedDrive = comboBox1.SelectedItem as DriveInfo;
                //string drive = selectedDrive.Name;
                Clean();
            }
            else
            {
                MessageBox.Show(Lang.Get("MsgDrive"), Lang.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Clean()
        {
            try
            {
                string logFile = Path.GetTempPath() + "\\Clean.log";
                string newLine = Environment.NewLine;
                var selectedDriveLetter = comboBox1.SelectedItem as DriveInfo;
                if (File.Exists(logFile))
                {
                    File.Delete(logFile);
                }
                File.AppendAllText(logFile, Lang.Get("LogTitle") + newLine);
                File.AppendAllText(logFile, DateTime.Now.ToString() + newLine);
                File.AppendAllText(logFile, Lang.Get("StartClean1") + selectedDriveLetter + Lang.Get("StartClean2") + newLine);
                DriveInfo selectedDrive = comboBox1.SelectedItem as DriveInfo;
                string drive = selectedDrive.Name;
                if (selectedDrive.DriveFormat == "NTFS")
                {
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = "C:\\Windows\\system32\\takeown.exe",
                        Arguments = "/F \"" + drive + "System Volume Information\"",
                        CreateNoWindow = true,
                        Verb = "runas"
                    };
                    try
                    {
                        using (var process = Process.Start(startInfo))
                        {
                            if (process != null)
                            {
                                process.WaitForExit();
                            }
                        }
                        File.AppendAllText(logFile, Lang.Get("TakeownSuccess") + selectedDriveLetter + "System Volume Information" + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("TakeownError") + ex.Message + newLine);
                    }
                    startInfo.FileName = "C:\\Windows\\system32\\icacls.exe";
                    startInfo.Arguments = "\"" + drive + "System Volume Information\"" + " /grant Everyone:(F)";
                    try
                    {
                        using (var process = Process.Start(startInfo))
                        {
                            if (process != null)
                            {
                                process.WaitForExit();
                            }
                        }
                        File.AppendAllText(logFile, Lang.Get("icaclSuccess") + selectedDriveLetter + "System Volume Information" + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("icaclError") + ex.Message + newLine);
                    }
                }
                foreach (Process process in Process.GetProcessesByName("wscript"))
                {
                    try
                    {
                        process.Kill();
                        File.AppendAllText(logFile, Lang.Get("WscriptStopped") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("WscriptError") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (Process process in Process.GetProcessesByName("cscript"))
                {
                    try
                    {
                        process.Kill();
                        File.AppendAllText(logFile, Lang.Get("CscriptStopped") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("CscriptError") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (Process process in Process.GetProcessesByName("cmd"))
                {
                    try
                    {
                        process.Kill();
                        File.AppendAllText(logFile, Lang.Get("CmdStopped") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("CmdError") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (Process process in Process.GetProcessesByName("conhost"))
                {
                    try
                    {
                        process.Kill();
                        File.AppendAllText(logFile, Lang.Get("ConhostStopped") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("ConhostError") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (Process process in Process.GetProcessesByName("powershell"))
                {
                    try
                    {
                        process.Kill();
                        File.AppendAllText(logFile, Lang.Get("PowershellStopped") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("PowershellError") + ex.Message + newLine);
                        continue;
                    }
                }
                string emptyCharacterFolderName = drive + " ";
                if (Directory.Exists(emptyCharacterFolderName))
                {
                    try
                    {
                        Directory.Move(emptyCharacterFolderName, drive + "RECOVERED");
                        File.AppendAllText(logFile, Lang.Get("RecoverDirectory") + selectedDriveLetter + "RECOVERED" + newLine);
                    }
                    catch
                    {
                        //MessageBox.Show("Unable to rename directory with empty character. Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                foreach (string file in Directory.GetFiles(drive, "*.*", SearchOption.AllDirectories))
                {
                    try
                    {
                        FileAttributes attributes = File.GetAttributes(file);
                        attributes &= ~FileAttributes.Hidden;
                        attributes &= ~FileAttributes.System;
                        attributes &= ~FileAttributes.ReadOnly;
                        File.SetAttributes(file, attributes);
                        File.AppendAllText(logFile, Lang.Get("RecoverHiddenFile") + file + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("RecoverHiddenFileError") + file + Lang.Get("Error2") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (string folder in Directory.GetDirectories(drive, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        FileAttributes attributes = File.GetAttributes(folder);
                        attributes &= ~FileAttributes.Hidden;
                        attributes &= ~FileAttributes.System;
                        attributes &= ~FileAttributes.ReadOnly;
                        File.SetAttributes(folder, attributes);
                        File.AppendAllText(logFile, Lang.Get("RecoverHiddenDirectory") + folder + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("RecoverHiddenDirectoryError") + folder + Lang.Get("Error2") + ex.Message + newLine);
                        continue;
                    }
                }
                if (File.Exists(drive + "autorun.inf") == true)
                {
                    string openValue = null; ;
                    foreach (var line in File.ReadAllLines(drive + "autorun.inf"))
                    {
                        if (line.Trim().ToLower().StartsWith("open="))
                        {
                            openValue = line.Substring(5).Trim();
                            break;
                        }
                        if (line.Trim().ToLower().StartsWith("shellexecute="))
                        {
                            openValue = line.Substring(13).Trim();
                            break;
                        }
                        if (line.Trim().ToLower().StartsWith("run="))
                        {
                            openValue = line.Substring(4).Trim();
                            break;
                        }
                    }
                    if (openValue != null)
                    {
                        string autorunProgram = Path.Combine(drive, openValue);
                        if (File.Exists(autorunProgram))
                        {
                            try
                            {
                                File.Delete(autorunProgram);
                                File.AppendAllText(logFile, Lang.Get("DeleteFile") + autorunProgram + Lang.Get("Success") + newLine);
                            }
                            catch (Exception ex)
                            {
                                File.AppendAllText(logFile, Lang.Get("FileDeleteError") + autorunProgram + Lang.Get("Error2") + ex.Message + newLine);
                            }
                        }
                    }
                }
                if (File.Exists(drive + "autorun.inf") == true)
                {
                    try
                    {
                        File.Delete(drive + "autorun.inf");
                        File.AppendAllText(logFile, Lang.Get("DeleteFile") + drive + "autorun.inf" + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("FileDeleteError") + drive + "autorun.inf" + Lang.Get("Error2") + ex.Message + newLine);
                    }
                }
                foreach (string file in Directory.GetFiles(drive, "*.lnk", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.Delete(file);
                        File.AppendAllText(logFile, Lang.Get("DeleteFile") + file + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("FileDeleteError") + file + Lang.Get("Error2") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (string vbs in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "*.vbs", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        File.Delete(vbs);
                        File.AppendAllText(logFile, Lang.Get("DeleteFile") + vbs + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("FileDeleteError") + vbs + Lang.Get("Error2") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (string bat in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "*.bat", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        File.Delete(bat);
                        File.AppendAllText(logFile, Lang.Get("DeleteFile") + bat + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("FileDeleteError") + bat + Lang.Get("Error2") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (string exe in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "*.exe", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        File.Delete(exe);
                        File.AppendAllText(logFile, Lang.Get("DeleteFile") + exe + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("FileDeleteError") + exe + Lang.Get("Error2") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (string vbs in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "*.vbs", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        File.Delete(vbs);
                        File.AppendAllText(logFile, Lang.Get("DeleteFile") + vbs + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("FileDeleteError") + vbs + Lang.Get("Error2") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (string bat in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "*.bat", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        File.Delete(bat);
                        File.AppendAllText(logFile, Lang.Get("DeleteFile") + bat + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("FileDeleteError") + bat + Lang.Get("Error2") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (string exe in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "*.exe", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        File.Delete(exe);
                        File.AppendAllText(logFile, Lang.Get("DeleteFile") + exe + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("FileDeleteError") + exe + Lang.Get("Error2") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (string vbs in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "*.vbs", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        File.Delete(vbs);
                        File.AppendAllText(logFile, Lang.Get("DeleteFile") + vbs + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("FileDeleteError") + vbs + Lang.Get("Error2") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (string bat in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "*.bat", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        File.Delete(bat);
                        File.AppendAllText(logFile, Lang.Get("DeleteFile") + bat + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("FileDeleteError") + bat + Lang.Get("Error2") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (string exe in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "*.exe", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        File.Delete(exe);
                        File.AppendAllText(logFile, Lang.Get("DeleteFile") + exe + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("FileDeleteError") + exe + Lang.Get("Error2") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (string vbs in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "*.vbs", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        File.Delete(vbs);
                        File.AppendAllText(logFile, Lang.Get("DeleteFile") + vbs + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("FileDeleteError") + vbs + Lang.Get("Error2") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (string bat in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "*.bat", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        File.Delete(bat);
                        File.AppendAllText(logFile, Lang.Get("DeleteFile") + bat + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("FileDeleteError") + bat + Lang.Get("Error2") + ex.Message + newLine);
                        continue;
                    }
                }
                foreach (string exe in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "*.exe", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        File.Delete(exe);
                        File.AppendAllText(logFile, Lang.Get("DeleteFile") + exe + Lang.Get("Success") + newLine);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("FileDeleteError") + exe + Lang.Get("Error2") + ex.Message + newLine);
                        continue;
                    }
                }
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\RUN", true))
                    {
                        if (key != null)
                        {
                            foreach (string valueName in key.GetValueNames())
                            {
                                object value = key.GetValue(valueName);
                                if (value is string valueData)
                                {
                                    if (valueData.IndexOf("wscript", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.IndexOf("cscript", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.IndexOf("vbs", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.IndexOf("ps1", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.Contains("wscript") || valueData.Contains("cscript") || valueData.Contains("vbs") || valueData.Contains("ps1") || valueData.IndexOf("wscript.exe", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.IndexOf("cscript.exe", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        key.DeleteValue(valueName, false);
                                        string keyString = "HKCU\\SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\RUN\\";
                                        File.AppendAllText(logFile, Lang.Get("SuspiciousKeyDeleted") + keyString + valueName + newLine);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(logFile, Lang.Get("SuspiciousKeyDeleteError") + ex.Message + newLine);
                }
                try
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\RUNONCE", true))
                    {
                        if (key != null)
                        {
                            foreach (string valueName in key.GetValueNames())
                            {
                                object value = key.GetValue(valueName);
                                if (value is string valueData)
                                {
                                    if (valueData.IndexOf("wscript", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.IndexOf("cscript", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.IndexOf("vbs", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.IndexOf("ps1", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        key.DeleteValue(valueName, false);
                                        string keyString = "HKCU\\SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\RUNONCE\\";
                                        File.AppendAllText(logFile, Lang.Get("SuspiciousKeyDeleted") + keyString + valueName + newLine);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(logFile, Lang.Get("SuspiciousKeyDeleteError") + ex.Message + newLine);
                }
                if (IsRunningAsAdministrator())
                {
                    foreach (string file in Directory.GetFiles("C:\\Windows\\system32\\Tasks", "*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            string content = File.ReadAllText(file);
                            if (content.IndexOf("wscript", StringComparison.OrdinalIgnoreCase) >= 0 || content.IndexOf("cscript", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                File.Delete(file);
                                DeleteTask(file);
                                File.AppendAllText(logFile, Lang.Get("SuspiciousTaskDeleted") + file + newLine);
                            }
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logFile, Lang.Get("TaskError") + ex.Message + newLine);
                            continue;
                        }
                    }
                    try
                    {
                        RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\POLICIES\\SYSTEM", true);
                        if (key != null)
                        {
                            if (key.GetValue("DisableTaskMgr") != null)
                            {
                                key.DeleteValue("DisableTaskMgr");
                                File.AppendAllText(logFile, Lang.Get("FixTaskMgr") + newLine);
                            }
                            key.Close();
                        }
                        RegistryKey key2 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\POLICIES\\MICROSOFT\\WINDOWS\\SYSTEM", true);
                        if (key2 != null)
                        {
                            if (key2.GetValue("DisableCMD") != null)
                            {
                                key2.DeleteValue("DisableCMD");
                                File.AppendAllText(logFile, Lang.Get("FixCMD") + newLine);
                            }
                            key2.Close();
                        }
                        RegistryKey key3 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\POLICIES\\SYSTEM", true);
                        if (key3 != null)
                        {
                            if (key3.GetValue("DisableRegistryTools") != null)
                            {
                                key3.DeleteValue("DisableRegistryTools");
                                File.AppendAllText(logFile, Lang.Get("FixRegedit") + newLine);
                            }
                            key3.Close();
                        }
                        RegistryKey key4 = Registry.LocalMachine.OpenSubKey("SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\POLICIES\\EXPLORER", true);
                        if (key4 != null)
                        {
                            if (key4.GetValue("NoFolderOptions") != null)
                            {
                                key4.DeleteValue("NoFolderOptions");
                                File.AppendAllText(logFile, Lang.Get("FixFolder") + newLine);
                            }
                            key4.Close();
                        }
                        RegistryKey key5 = Registry.CurrentUser.OpenSubKey("SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\EXPLORER\\ADVANCED", true);
                        if (key5 != null)
                        {
                            if (key5.GetValue("HideFileExt") != null)
                            {
                                key5.SetValue("HideFileExt", 0);
                                File.AppendAllText(logFile, Lang.Get("ExtensionFix") + newLine);
                            }
                            key5.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("RegError") + ex.Message + newLine);
                    }
                    try
                    {
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\RUN", true))
                        {
                            if (key != null)
                            {
                                foreach (string valueName in key.GetValueNames())
                                {
                                    object value = key.GetValue(valueName);
                                    if (value is string valueData)
                                    {
                                        if (valueData.IndexOf("wscript", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.IndexOf("cscript", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.IndexOf("vbs", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.IndexOf("ps1", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.Contains("wscript") || valueData.Contains("cscript") || valueData.Contains("vbs") || valueData.Contains("ps1") || valueData.IndexOf("wscript.exe", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.IndexOf("cscript.exe", StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            key.DeleteValue(valueName, false);
                                            string keyString = "HKLM\\SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\RUN\\";
                                            File.AppendAllText(logFile, Lang.Get("SuspiciousKeyDeleted") + keyString + valueName + newLine);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("SuspiciousKeyDeleteError") + ex.Message + newLine);
                    }
                    try
                    {
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\RUNONCE", true))
                        {
                            if (key != null)
                            {
                                foreach (string valueName in key.GetValueNames())
                                {
                                    object value = key.GetValue(valueName);
                                    if (value is string valueData)
                                    {
                                        if (valueData.IndexOf("wscript", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.IndexOf("cscript", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.IndexOf("vbs", StringComparison.OrdinalIgnoreCase) >= 0 || valueData.IndexOf("ps1", StringComparison.OrdinalIgnoreCase) >= 0)
                                        {
                                            key.DeleteValue(valueName, false);
                                            string keyString = "HKLM\\SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\RUNONCE\\";
                                            File.AppendAllText(logFile, Lang.Get("SuspiciousKeyDeleted") + keyString + valueName + newLine);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logFile, Lang.Get("SuspiciousKeyDeleteError") + ex.Message + newLine);
                    }
                }
                try
                {
                    RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\POLICIES\\SYSTEM", true);
                    if (key != null)
                    {
                        if (key.GetValue("DisableTaskMgr") != null)
                        {
                            key.DeleteValue("DisableTaskMgr");
                            File.AppendAllText(logFile, Lang.Get("FixTaskMgr") + newLine);
                        }
                        key.Close();
                    }
                    RegistryKey key2 = Registry.CurrentUser.OpenSubKey("SOFTWARE\\POLICIES\\MICROSOFT\\WINDOWS\\SYSTEM", true);
                    if (key2 != null)
                    {
                        if (key2.GetValue("DisableCMD") != null)
                        {
                            key2.DeleteValue("DisableCMD");
                            File.AppendAllText(logFile, Lang.Get("FixCMD") + newLine);
                        }
                        key2.Close();
                    }
                    RegistryKey key3 = Registry.CurrentUser.OpenSubKey("SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\POLICIES\\SYSTEM", true);
                    if (key3 != null)
                    {
                        if (key3.GetValue("DisableRegistryTools") != null)
                        {
                            key3.DeleteValue("DisableRegistryTools");
                            File.AppendAllText(logFile, Lang.Get("FixRegedit") + newLine);
                        }
                        key3.Close();
                    }
                    RegistryKey key4 = Registry.CurrentUser.OpenSubKey("SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\POLICIES\\EXPLORER", true);
                    if (key4 != null)
                    {
                        if (key4.GetValue("NoFolderOptions") != null)
                        {
                            key4.DeleteValue("NoFolderOptions");
                            File.AppendAllText(logFile, Lang.Get("FixFolder") + newLine);
                        }
                        key4.Close();
                    }
                    RegistryKey key5 = Registry.CurrentUser.OpenSubKey("SOFTWARE\\MICROSOFT\\WINDOWS\\CURRENTVERSION\\EXPLORER\\ADVANCED", true);
                    if (key5 != null)
                    {
                        if (key5.GetValue("HideFileExt") != null)
                        {
                            key5.SetValue("HideFileExt", 0);
                            File.AppendAllText(logFile, Lang.Get("ExtensionFix") + newLine);
                        }
                        key5.Close();
                    }
                }
                catch (Exception ex)
                {
                    File.AppendAllText(logFile, Lang.Get("RegErrorAdmin") + ex.Message + newLine);
                }
                MessageBox.Show(Lang.Get("MsgVirusRemoved"), Lang.Get("Clean"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                string msg = Lang.Get("MsgScriptDelete");
                string title = Lang.Get("DeleteScripts");
                if (MessageBox.Show(msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    foreach (string file in Directory.GetFiles(drive, "*.vbs", SearchOption.AllDirectories))
                    {
                        try
                        {
                            File.Delete(file);
                            File.AppendAllText(logFile, Lang.Get("DeleteFile") + file + Lang.Get("Success") + newLine);
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logFile, Lang.Get("FileDeleteError") + file + Lang.Get("Error2") + ex.Message + newLine);
                            continue;
                        }
                    }
                    foreach (string file in Directory.GetFiles(drive, "*.vbe", SearchOption.AllDirectories))
                    {
                        try
                        {
                            File.Delete(file);
                            File.AppendAllText(logFile, Lang.Get("DeleteFile") + file + Lang.Get("Success") + newLine);
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logFile, Lang.Get("FileDeleteError") + file + Lang.Get("Error2") + ex.Message + newLine);
                            continue;
                        }
                    }
                    foreach (string file in Directory.GetFiles(drive, "*.scr", SearchOption.AllDirectories))
                    {
                        try
                        {
                            File.Delete(file);
                            File.AppendAllText(logFile, Lang.Get("DeleteFile") + file + Lang.Get("Success") + newLine);
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logFile, Lang.Get("FileDeleteError") + file + Lang.Get("Error2") + ex.Message + newLine);
                            continue;
                        }
                    }
                    foreach (string file in Directory.GetFiles(drive, "*.com", SearchOption.AllDirectories))
                    {
                        try
                        {
                            File.Delete(file);
                            File.AppendAllText(logFile, Lang.Get("DeleteFile") + file + Lang.Get("Success") + newLine);
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logFile, Lang.Get("FileDeleteError") + file + Lang.Get("Error2") + ex.Message + newLine);
                            continue;
                        }
                    }
                    foreach (string file in Directory.GetFiles(drive, "*.pif", SearchOption.AllDirectories))
                    {
                        try
                        {
                            File.Delete(file);
                            File.AppendAllText(logFile, Lang.Get("DeleteFile") + file + Lang.Get("Success") + newLine);
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logFile, Lang.Get("FileDeleteError") + file + Lang.Get("Error2") + ex.Message + newLine);
                            continue;
                        }
                    }
                    foreach (string file in Directory.GetFiles(drive, "*.ps1", SearchOption.AllDirectories))
                    {
                        try
                        {
                            File.Delete(file);
                            File.AppendAllText(logFile, Lang.Get("DeleteFile") + file + Lang.Get("Success") + newLine);
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logFile, Lang.Get("FileDeleteError") + file + Lang.Get("Error2") + ex.Message + newLine);
                            continue;
                        }
                    }
                    foreach (string file in Directory.GetFiles(drive, "*.bat", SearchOption.AllDirectories))
                    {
                        try
                        {
                            File.Delete(file);
                            File.AppendAllText(logFile, Lang.Get("DeleteFile") + file + Lang.Get("Success") + newLine);
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logFile, Lang.Get("FileDeleteError") + file + Lang.Get("Error2") + ex.Message + newLine);
                            continue;
                        }
                    }
                    foreach (string file in Directory.GetFiles(drive, "*.cmd", SearchOption.AllDirectories))
                    {
                        try
                        {
                            File.Delete(file);
                            File.AppendAllText(logFile, Lang.Get("DeleteFile") + file + Lang.Get("Success") + newLine);
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logFile, Lang.Get("FileDeleteError") + file + Lang.Get("Error2") + ex.Message + newLine);
                            continue;
                        }
                    }
                    foreach (string file in Directory.GetFiles(drive, "*.exe", SearchOption.AllDirectories))
                    {
                        try
                        {
                            File.Delete(file);
                            File.AppendAllText(logFile, Lang.Get("DeleteFile") + file + Lang.Get("Success") + newLine);
                        }
                        catch (Exception ex)
                        {
                            File.AppendAllText(logFile, Lang.Get("FileDeleteError") + file + Lang.Get("Error2") + ex.Message + newLine);
                            continue;
                        }
                    }
                    MessageBox.Show(Lang.Get("MsgDeletedScripts"), Lang.Get("Clean"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                File.AppendAllText(logFile, Lang.Get("OperationFinished") + DateTime.Now.ToString());
                Process.Start("notepad", logFile);
            }
            catch (Exception ex)
            {
                string logFile = Path.GetTempPath() + "\\Clean.log";
                MessageBox.Show(Lang.Get("MsgError") + ex.Message, Lang.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                File.AppendAllText(logFile, Lang.Get("MsgError") + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var tile = sender as Control;
            contextMenuStrip1.Show(tile, new Point(0, tile.Height));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var selectedDrive = comboBox1.SelectedItem as DriveInfo;
            comboBox1.Items.Clear();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name != "C:\\" && drive.DriveType != DriveType.CDRom)
                {
                    comboBox1.Items.Add(drive);
                }
            }

            bool restored = false;
            if (selectedDrive != null)
            {
                foreach (DriveInfo drive in comboBox1.Items)
                {
                    if (drive.Name == selectedDrive.Name)
                    {
                        comboBox1.SelectedItem = drive;
                        restored = true;
                        break;
                    }
                }
            }

            if (!restored && comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (Lang.culture == "en")
            {
                button1.Text = Lang.Get("CleanButton");
                button2.Text = Lang.Get("OptionsButton");
                button3.Text = Lang.Get("RefreshButton");
                label1.Text = Lang.Get("DriveToClean");
                vaccinationToolStripMenuItem.Text = Lang.Get("VaccinationButton");
                vaccinateComputerToolStripMenuItem.Text = Lang.Get("ComputerVaccinatedButton");
                vaccinateUSBToolStripMenuItem.Text = Lang.Get("VaccinateUSBButton");
                removeVaccinationUSBToolStripMenuItem.Text = Lang.Get("RemoveUSBVaccinationButton");
                translateToolStripMenuItem.Text = Lang.Get("TranslateButton");
                visitWebsiteToolStripMenuItem.Text = Lang.Get("VisitWebsiteButton");
                contactToolStripMenuItem.Text = Lang.Get("ContactButton");
                getHelpFileToolStripMenuItem.Text = Lang.Get("GetHelpFileButton");
                aboutToolStripMenuItem.Text = Lang.Get("AboutButton");
                comboBox2.SelectedIndex = 0;
            }
            else if (Lang.culture == "de")
            {
                button1.Text = Lang.Get("CleanButton");
                button2.Text = Lang.Get("OptionsButton");
                button3.Text = Lang.Get("RefreshButton");
                label1.Text = Lang.Get("DriveToClean");
                button3.Size = new System.Drawing.Size(83, 23);
                vaccinationToolStripMenuItem.Text = Lang.Get("VaccinationButton");
                vaccinateComputerToolStripMenuItem.Text = Lang.Get("ComputerVaccinatedButton");
                vaccinateUSBToolStripMenuItem.Text = Lang.Get("VaccinateUSBButton");
                removeVaccinationUSBToolStripMenuItem.Text = Lang.Get("RemoveUSBVaccinationButton");
                translateToolStripMenuItem.Text = Lang.Get("TranslateButton");
                visitWebsiteToolStripMenuItem.Text = Lang.Get("VisitWebsiteButton");
                contactToolStripMenuItem.Text = Lang.Get("ContactButton");
                getHelpFileToolStripMenuItem.Text = Lang.Get("GetHelpFileButton");
                aboutToolStripMenuItem.Text = Lang.Get("AboutButton");
                comboBox2.SelectedIndex = 1;
            }
            else if (Lang.culture == "tr")
            {
                button1.Text = Lang.Get("CleanButton");
                button2.Text = Lang.Get("OptionsButton");
                button3.Text = Lang.Get("RefreshButton");
                label1.Text = Lang.Get("DriveToClean");
                this.Text = "Kısayol Virüsü Temizleme Aracı";
                vaccinationToolStripMenuItem.Text = Lang.Get("VaccinationButton");
                vaccinateComputerToolStripMenuItem.Text = Lang.Get("ComputerVaccinatedButton");
                vaccinateUSBToolStripMenuItem.Text = Lang.Get("VaccinateUSBButton");
                removeVaccinationUSBToolStripMenuItem.Text = Lang.Get("RemoveUSBVaccinationButton");
                translateToolStripMenuItem.Text = Lang.Get("TranslateButton");
                visitWebsiteToolStripMenuItem.Text = Lang.Get("VisitWebsiteButton");
                contactToolStripMenuItem.Text = Lang.Get("ContactButton");
                getHelpFileToolStripMenuItem.Text = Lang.Get("GetHelpFileButton");
                aboutToolStripMenuItem.Text = Lang.Get("AboutButton");
                comboBox2.SelectedIndex = 2;
            }
            else if (Lang.culture == "ar")
            {
                this.RightToLeft = RightToLeft.Yes;
                this.RightToLeftLayout = true;
                button1.Text = Lang.Get("CleanButton");
                button2.Text = Lang.Get("OptionsButton");
                button3.Text = Lang.Get("RefreshButton");
                label1.Text = Lang.Get("DriveToClean");
                vaccinationToolStripMenuItem.Text = Lang.Get("VaccinationButton");
                vaccinateComputerToolStripMenuItem.Text = Lang.Get("ComputerVaccinatedButton");
                vaccinateUSBToolStripMenuItem.Text = Lang.Get("VaccinateUSBButton");
                removeVaccinationUSBToolStripMenuItem.Text = Lang.Get("RemoveUSBVaccinationButton");
                translateToolStripMenuItem.Text = Lang.Get("TranslateButton");
                visitWebsiteToolStripMenuItem.Text = Lang.Get("VisitWebsiteButton");
                contactToolStripMenuItem.Text = Lang.Get("ContactButton");
                getHelpFileToolStripMenuItem.Text = Lang.Get("GetHelpFileButton");
                aboutToolStripMenuItem.Text = Lang.Get("AboutButton");
                comboBox2.SelectedIndex = 3;
            }
            else if (Lang.culture == "id")
            {
                button1.Text = Lang.Get("CleanButton");
                button2.Text = Lang.Get("OptionsButton");
                button3.Text = Lang.Get("RefreshButton");
                label1.Text = Lang.Get("DriveToClean");
                comboBox2.SelectedIndex = 4;
            }
            else
            {
                button1.Text = Lang.Get("CleanButton");
                button2.Text = Lang.Get("OptionsButton");
                button3.Text = Lang.Get("RefreshButton");
                label1.Text = Lang.Get("DriveToClean");
                vaccinationToolStripMenuItem.Text = Lang.Get("VaccinationButton");
                vaccinateComputerToolStripMenuItem.Text = Lang.Get("ComputerVaccinatedButton");
                vaccinateUSBToolStripMenuItem.Text = Lang.Get("VaccinateUSBButton");
                removeVaccinationUSBToolStripMenuItem.Text = Lang.Get("RemoveUSBVaccinationButton");
                translateToolStripMenuItem.Text = Lang.Get("TranslateButton");
                visitWebsiteToolStripMenuItem.Text = Lang.Get("VisitWebsiteButton");
                contactToolStripMenuItem.Text = Lang.Get("ContactButton");
                getHelpFileToolStripMenuItem.Text = Lang.Get("GetHelpFileButton");
                aboutToolStripMenuItem.Text = Lang.Get("AboutButton");
                comboBox2.SelectedIndex = 0;
            }
            if (IsRunningAsAdministrator() == false)
            {
                string msg = Lang.Get("MsgAdmin");
                string title = Lang.Get("Warning");
                if (MessageBox.Show(msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    try
                    {
                        ProcessStartInfo info = new ProcessStartInfo();
                        info.FileName = Application.ExecutablePath;
                        info.Verb = "runas";
                        Process.Start(info);
                        Application.Exit();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Lang.Get("AdminError") + ex.Message, Lang.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                vaccinateComputerToolStripMenuItem.Enabled = false;
            }
            else
            {
                try
                {
                    using (RegistryKey regKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\MICROSOFT\\WINDOWS SCRIPT HOST\\SETTINGS"))
                    {
                        if (regKey.GetValue("ENABLED") == null)
                        {
                            vaccinateComputerToolStripMenuItem.Checked = false;
                        }
                        else if (regKey.GetValue("ENABLED").ToString() == "0")
                        {
                            vaccinateComputerToolStripMenuItem.Checked = true;
                        }
                        else
                        {
                            vaccinateComputerToolStripMenuItem.Checked = false;
                        }
                        regKey.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Lang.Get("GetComputerVaccinationStatusError") + ex.Message, Lang.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady == true && drive.Name != "C:\\" && drive.DriveType != DriveType.CDRom)
                {
                    comboBox1.Items.Add(drive);
                }
            }
            /*foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Removable && drive.IsReady)
                {
                    comboBox1.Items.Add(drive);
                }
            }*/
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
            timer1.Start();
            string curDir = Application.StartupPath;
            string langDir = Path.Combine(curDir, "SVRTlang.json");

            if (File.Exists(langDir))
            {
                string langCode = Interaction.InputBox("Enter custom language code:", "Custom Language");
                Lang.culture = langCode;
                Lang.Initialize();
                Size refresh = new Size(62, 23);
                ApplyLanguageToControls(refresh);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var selectedDrive = comboBox1.SelectedItem as DriveInfo;
            comboBox1.Items.Clear();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name != "C:\\" && drive.DriveType != DriveType.CDRom)
                {
                    comboBox1.Items.Add(drive);
                }
            }

            bool restored = false;
            if (selectedDrive != null)
            {
                foreach (DriveInfo drive in comboBox1.Items)
                {
                    if (drive.Name == selectedDrive.Name)
                    {
                        comboBox1.SelectedItem = drive;
                        restored = true;
                        break;
                    }
                }
            }

            if (!restored && comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }
        private bool IsRunningAsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        private void DeleteTask(string taskName)
        {
            string newLine = Environment.NewLine;
            string logFile = Path.GetTempPath() + "\\Clean.log";
            string name = Path.GetFileNameWithoutExtension(taskName);
            ProcessStartInfo info = new ProcessStartInfo();
            info.Arguments = $"/delete /tn \"{name}\" /f";
            info.FileName = "schtasks.exe";
            info.CreateNoWindow = true;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.Verb = "runas";
            try
            {
                Process.Start(info);
            }
            catch (Exception ex)
            {
                File.AppendAllText(logFile, Lang.Get("TaskError") + ex.Message + newLine);
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string newCulture;
            bool newRTL;
            Size refreshButtonSize;

            switch (comboBox2.SelectedIndex)
            {
                case 0: newCulture = "en"; newRTL = false; refreshButtonSize = new Size(62, 23); break;
                case 1: newCulture = "de"; newRTL = false; refreshButtonSize = new Size(83, 23); break;
                case 2: newCulture = "tr"; newRTL = false; refreshButtonSize = new Size(62, 23); break;
                case 3: newCulture = "ar"; newRTL = true; refreshButtonSize = new Size(62, 23); break;
                case 4: newCulture = "id"; newRTL = false; refreshButtonSize = new Size(62, 23); break;
                default: return;
            }

            Lang.culture = newCulture;
            bool rtlIsChanging = (this.RightToLeftLayout != newRTL);

            if (rtlIsChanging)
            {
                BeginInvoke(new MethodInvoker(() =>
                {
                    this.RightToLeft = newRTL ? RightToLeft.Yes : RightToLeft.No;
                    this.RightToLeftLayout = newRTL;
                    ApplyLanguageToControls(refreshButtonSize);
                }));
                return;
            }
            ApplyLanguageToControls(refreshButtonSize);
        }

        private void ApplyLanguageToControls(Size refreshButtonSize)
        {
            button1.Text = Lang.Get("CleanButton");
            button2.Text = Lang.Get("OptionsButton");
            button3.Text = Lang.Get("RefreshButton");
            label1.Text = Lang.Get("DriveToClean");
            this.Text = Lang.Get("SVRT");
            vaccinationToolStripMenuItem.Text = Lang.Get("VaccinationButton");
            vaccinateComputerToolStripMenuItem.Text = Lang.Get("ComputerVaccinatedButton");
            vaccinateUSBToolStripMenuItem.Text = Lang.Get("VaccinateUSBButton");
            removeVaccinationUSBToolStripMenuItem.Text = Lang.Get("RemoveUSBVaccinationButton");
            translateToolStripMenuItem.Text = Lang.Get("TranslateButton");
            visitWebsiteToolStripMenuItem.Text = Lang.Get("VisitWebsiteButton");
            contactToolStripMenuItem.Text = Lang.Get("ContactButton");
            getHelpFileToolStripMenuItem.Text = Lang.Get("GetHelpFileButton");
            aboutToolStripMenuItem.Text = Lang.Get("AboutButton");
            button3.Size = refreshButtonSize;
        }

        private void vaccinateUSBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex > -1)
            {
                DriveInfo selectedDrive = comboBox1.SelectedItem as DriveInfo;
                if (selectedDrive.DriveFormat == "NTFS")
                {
                    try
                    {
                        DirectoryInfo info = new DirectoryInfo(selectedDrive.Name);
                        DirectorySecurity security = info.GetAccessControl();
                        FileSystemAccessRule vaccination = new FileSystemAccessRule("Everyone", FileSystemRights.ExecuteFile, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Deny);
                        security.AddAccessRule(vaccination);
                        info.SetAccessControl(security);
                        MessageBox.Show(Lang.Get("DriveVaccinatedMessage"), Lang.Get("Vaccinate"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Lang.Get("DriveVaccinationError") + ex.Message, Lang.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show(Lang.Get("NTFSOnly"), Lang.Get("Vaccinate"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show(Lang.Get("MsgDrive"), Lang.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void removeVaccinationUSBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex > -1)
            {
                DriveInfo selectedDrive = comboBox1.SelectedItem as DriveInfo;
                if (selectedDrive.DriveFormat == "NTFS")
                {
                    try
                    {
                        DirectoryInfo info = new DirectoryInfo(selectedDrive.Name);
                        DirectorySecurity security = info.GetAccessControl();
                        FileSystemAccessRule vaccination = new FileSystemAccessRule("Everyone", FileSystemRights.ExecuteFile, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Deny);
                        security.RemoveAccessRule(vaccination);
                        info.SetAccessControl(security);
                        MessageBox.Show(Lang.Get("DriveVaccinationRemoved"), Lang.Get("Vaccinate"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Lang.Get("DriveVaccinationRemoveError") + ex.Message, Lang.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show(Lang.Get("NTFSOnly"), Lang.Get("Vaccinate"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show(Lang.Get("MsgDrive"), Lang.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void vaccinateComputerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (vaccinateComputerToolStripMenuItem.Checked == true)
            {
                try
                {
                    using (RegistryKey regKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\MICROSOFT\\WINDOWS SCRIPT HOST\\SETTINGS"))
                    {
                        regKey.SetValue("ENABLED", 0, RegistryValueKind.DWord);
                        regKey.Close();
                    }
                    using (RegistryKey regKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\MICROSOFT\\WINDOWS NT\\CURRENTVERSION\\IMAGE FILE EXECUTION OPTIONS\\wscript.exe"))
                    {
                        regKey.SetValue("Debugger", "ntsd -c -qd");
                        regKey.Close();
                    }
                    using (RegistryKey regKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\MICROSOFT\\WINDOWS NT\\CURRENTVERSION\\IMAGE FILE EXECUTION OPTIONS\\cscript.exe"))
                    {
                        regKey.SetValue("Debugger", "ntsd -c -qd");
                        regKey.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                try
                {
                    using (RegistryKey regKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\MICROSOFT\\WINDOWS SCRIPT HOST\\SETTINGS"))
                    {
                        regKey.SetValue("ENABLED", 1, RegistryValueKind.DWord);
                        regKey.Close();
                    }
                    using (RegistryKey regKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\MICROSOFT\\WINDOWS NT\\CURRENTVERSION\\IMAGE FILE EXECUTION OPTIONS\\wscript.exe"))
                    {
                        regKey.DeleteValue("Debugger", false);
                        regKey.Close();
                    }
                    using (RegistryKey regKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\MICROSOFT\\WINDOWS NT\\CURRENTVERSION\\IMAGE FILE EXECUTION OPTIONS\\cscript.exe"))
                    {
                        regKey.DeleteValue("Debugger", false);
                        regKey.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Lang.Get("MsgAbout"), Lang.Get("About"));
        }

        private void translateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://deuteriumsoftware.com/translate");
        }

        private void visitWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://deuteriumsoftware.com");
        }

        private void getHelpFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://sourceforge.net/projects/shortcut-virus-removal-tool/files/v2.4.0/HOW%20TO%20USE.txt/download");
        }

        private void contactToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://deuteriumsoftware.com/contact");
        }
    }
    public static class Lang
    {
        public static string culture { get; set; } = "en";
        private static Dictionary<string, object> _translations;
        public static void Initialize()
        {
            string curDir = Application.StartupPath;
            string langDir = Path.Combine(curDir, "SVRTlang.json");

            if (File.Exists(langDir))
            {
                using (Stream stream = File.OpenRead(langDir))
                {
                    if (stream == null) return;
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string jsonText = reader.ReadToEnd();
                        var serializer = new JavaScriptSerializer();
                        _translations = serializer.Deserialize<Dictionary<string, object>>(jsonText);
                    }
                }
            }
            else
            {
                var assembly = Assembly.GetExecutingAssembly();
                string resourceName = "Shortcut_Virus_Removal_Tool.translations.json";
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null) return;
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string jsonText = reader.ReadToEnd();
                        var serializer = new JavaScriptSerializer();
                        _translations = serializer.Deserialize<Dictionary<string, object>>(jsonText);
                    }
                }
            }
        }
        public static string Get(string key)
        {
            try
            {
                if (_translations != null && _translations.ContainsKey(culture))
                {
                    var cultureDict = _translations[culture] as Dictionary<string, object>;
                    if (cultureDict != null && cultureDict.ContainsKey(key))
                    {
                        return cultureDict[key].ToString();
                    }
                }
            }
            catch
            {

            }
            try
            {
                if (_translations != null && _translations.ContainsKey("en"))
                {
                    var englishDict = _translations["en"] as Dictionary<string, object>;
                    if (englishDict != null && englishDict.ContainsKey(key))
                    {
                        return englishDict[key].ToString();
                    }
                }
            }
            catch { }
            return "[" + key + "]";
        }
    }
}