﻿using ILRepacking;
using Microsoft.Win32;
using Mono.Cecil;
using Mono.Cecil.Cil;
using SFMF;
using SFMFLauncher.Injection;
using SFMFLauncher.Util;
using Supremes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SFMFLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string SettingsURL = "https://gist.github.com/Phlarfl/e504a0ac94fd004ec02ebaaccd3aa335";
        private const string SteamRegistry = @"HKEY_CURRENT_USER\Software\Valve\Steam";
        private const string SteamConfig = "config/config.vdf";
        private const string InstallDirectory = "steamapps/common/SuperFlight";
        private const string ManagedDirectory = "superflight_Data/Managed";
        private const string AssemblyLib = "Assembly-CSharp.dll";
        private const string PluginDirectory = "plugins";
        private const string PluginExt = "dll";
        private bool Reinstalling = false;

        private string AbsoluteInstallDirectory;

        private Dictionary<Control, bool> States = new Dictionary<Control, bool>();

        public MainWindow()
        {
            InitializeComponent();

            AbsoluteInstallDirectory = GetAbsoluteInstallDirectory();

            RefreshButtonStates();
            RefreshMods();
            RefreshInstalled();
        }

        private void RefreshButtonStates()
        {
            bool Installed = IsFrameworkInstalled();
            BtnInstallFramework.IsEnabled = !Installed;
            BtnUninstallFramework.IsEnabled = BtnRefreshMods.IsEnabled = BtnRefreshInstalled.IsEnabled = BtnInstallMod.IsEnabled = BtnUninstallMod.IsEnabled = LbxMods.IsEnabled = LbxInstalled.IsEnabled = Installed;
            if (LbxInstalled.Items.Count == 0)
                BtnUninstallMod.IsEnabled = false;
            if (LbxMods.Items.Count == 0)
                BtnInstallMod.IsEnabled = false;
        }

        private void BtnInstallFramework_Click(object sender, RoutedEventArgs e)
        {
            ChangeState(false);
            Inject();
            ReturnState();
            RefreshButtonStates();
        }

        private void BtnUninstallFramework_Click(object sender, RoutedEventArgs e)
        {
            ChangeState(false);
            try
            {
                File.Copy(GetAssemblyBackupLocation(), GetAssemblyLocation(), true);
                File.Delete(GetAssemblyBackupLocation());
                Directory.Delete(GetPluginDirectory(), true);
            }
            catch { }
            ReturnState();
            RefreshButtonStates();
            LbxInstalled.Items.Clear();
            Reinstalling = true;
        }

        private void BtnInstallMod_Click(object sender, RoutedEventArgs e)
        {
            if (LbxMods.SelectedItem != null && LbxMods.SelectedItem is Plugin)
            {
                if (!Directory.Exists(GetPluginDirectory()))
                    Directory.CreateDirectory(GetPluginDirectory());
                Plugin plugin = LbxMods.SelectedItem as Plugin;
                using (var client = new WebClient())
                    try
                    {
                        client.DownloadFile(new Uri(plugin.Download), $"{GetPluginDirectory()}/{plugin.Name}.dll");
                        RefreshInstalled();
                    } catch
                    {
                        MessageBox.Show("Failed to download mod, please try again later");
                    }
            }
        }

        private void BtnUninstallMod_Click(object sender, RoutedEventArgs e)
        {
            if (LbxInstalled.SelectedItem != null)
            {
                File.Delete($"{GetPluginDirectory()}/{LbxInstalled.SelectedItem.ToString()}.dll");
                LbxInstalled.Items.Remove(LbxInstalled.SelectedItem);
                if (LbxInstalled.Items.Count > 0)
                    LbxInstalled.SelectedIndex = Math.Min(Math.Max(0, LbxInstalled.SelectedIndex), LbxInstalled.Items.Count - 1);
                if (LbxInstalled.SelectedIndex == -1)
                    BtnUninstallMod.IsEnabled = false;
            }
        }

        private void BtnRefreshMods_Click(object sender, RoutedEventArgs e)
        {
            RefreshMods();
        }

        private void BtnRefreshInstalled_Click(object sender, RoutedEventArgs e)
        {
            RefreshInstalled();
        }

        private void LbxMods_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnInstallMod.IsEnabled = LbxMods.SelectedItem != null;
        }

        private void LbxInstalled_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnUninstallMod.IsEnabled = LbxInstalled.SelectedItem != null;
        }

        private void ChangeState(bool state)
        {
            List<Control> controls = new List<Control>();

            controls.Add(BtnInstallFramework);
            controls.Add(BtnUninstallFramework);
            controls.Add(BtnRefreshMods);
            controls.Add(BtnRefreshInstalled);
            controls.Add(BtnInstallMod);
            controls.Add(BtnUninstallMod);
            controls.Add(LbxMods);
            controls.Add(LbxInstalled);

            States.Clear();
            foreach (Control control in controls)
            {
                States.Add(control, control.IsEnabled);
                control.IsEnabled = state;
            }
        }

        private void ReturnState()
        {
            foreach (Control control in States.Keys)
                control.IsEnabled = States[control];
        }

        private string GetAbsoluteInstallDirectory()
        {
            string steam = Registry.GetValue($"{SteamRegistry}", "SteamPath", null)?.ToString();
            if (steam != null)
            {
                List<string> paths = new List<string>();
                paths.Add(steam);
                string[] lines = File.ReadAllLines($"{steam}/{SteamConfig}");
                VDFPopulate(paths, lines, 1);
                foreach (var path in paths)
                    if (Directory.Exists($"{path}/{InstallDirectory}/{ManagedDirectory}") && File.Exists($"{path}/{InstallDirectory}/{ManagedDirectory}/Assembly-CSharp.dll"))
                        return $"{path}/{InstallDirectory}";
            }
            return null;
        }

        private void VDFPopulate(List<string> paths, string[] lines, int index)
        {
            foreach (string line in lines)
                if (line.Trim().StartsWith($"\"BaseInstallFolder_{index}\""))
                {
                    string path = line.Trim().Split('\t')[2];
                    paths.Add(path.Substring(1, path.Length - 2).Replace("\\\\", "/"));
                    VDFPopulate(paths, lines, index + 1);
                }
        }

        private void RefreshMods()
        {
            LbxMods.Items.Clear();

            string html;
            using (var client = new WebClient())
            {
                html = client.DownloadString(SettingsURL);
                Supremes.Nodes.Document document = Dcsoup.Parse(html);
                string json = document.GetElementsByClass("blob-wrapper").Text;
                Settings settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(json);
                foreach (Plugin plugin in settings.modlist)
                {
                    LbxMods.Items.Add(plugin);
                }
            }
        }

        private void RefreshInstalled()
        {
            LbxInstalled.Items.Clear();

            if (Directory.Exists(GetPluginDirectory()))
            {
                string[] files = Directory.GetFiles(GetPluginDirectory(), $"*.{PluginExt}", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                    LbxInstalled.Items.Add(file.Substring(file.LastIndexOf('\\') + 1).Replace(".dll", ""));
            }
        }

        private bool IsFrameworkInstalled()
        {
            if (!File.Exists(GetAssemblyLocation()))
                return false;

            AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(GetAssemblyLocation());
            ModuleDefinition modDef = asmDef.MainModule;

            return Util.Util.GetDefinition(modDef.Types, "SFMF.Core", true) != null;
        }
        
        private void Inject()
        {
            if (!Reinstalling)
            {
                List<string> managed = Directory.GetFiles(GetManagedLocation()).Where(x => (x.Contains("UnityEngine") || x.Contains("AmplifyMotion") || x.Contains("Assembly-CSharp.") || x.Contains("Mono.Security")) && x.EndsWith(".dll")).ToList();
                foreach (string file in managed)
                    File.Copy(file, file.Substring(file.LastIndexOf("\\") + 1), true);
            }

            Merge(GetAssemblyLocation(), GetAssemblyBackupLocation(), "./SFMF.dll");
            Inject(GetAssemblyLocation());
        }

        private void Merge(string asmLoc, string asmBacLoc, string sfmfLoc)
        {
            AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(asmLoc);
            ModuleDefinition modDef = asmDef.MainModule;

            if (Util.Util.GetDefinition(modDef.Types, "SFMF.Core", true) == null)
                File.Copy(asmLoc, asmBacLoc, true);

            var options = new RepackOptions
            {
                OutputFile = asmLoc,
                InputAssemblies = new[]
                {
                    asmBacLoc, sfmfLoc
                },
                SearchDirectories = new List<string>().AsEnumerable(),
                TargetKind = ILRepack.Kind.Dll
            };

            var repack = new ILRepack(options);
            repack.Repack();
        }

        private void Inject(string asmLoc)
        {
            Assembly asm = Assembly.Load(File.ReadAllBytes(asmLoc));

            AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(asmLoc);
            ModuleDefinition modDef = asmDef.MainModule;

            InjectionMethod imStart = new InjectionMethod(modDef, "LocalGameManager", "Start");
            MethodReference mrInit = modDef.Import(asm.GetType("SFMF.Core").GetMethod("Init", new Type[] { }));
            imStart.MethodDef.Body.Instructions.Insert(7, imStart.MethodDef.Body.GetILProcessor().Create(OpCodes.Call, mrInit));

            InjectionMethod imSubmitHighscore = new InjectionMethod(modDef, "SteamIntegration", "SubmitHighscore");
            imSubmitHighscore.MethodDef.Body.Instructions.Insert(0, imSubmitHighscore.MethodDef.Body.GetILProcessor().Create(OpCodes.Ret));

            InjectionMethod imSubmitCombo = new InjectionMethod(modDef, "SteamIntegration", "SubmitCombo");
            imSubmitCombo.MethodDef.Body.Instructions.Insert(0, imSubmitCombo.MethodDef.Body.GetILProcessor().Create(OpCodes.Ret));

            asmDef.Write(asmLoc);
        }


        private string GetManagedLocation()
        {
            return $"{AbsoluteInstallDirectory}/{ManagedDirectory}";
        }

        private string GetAssemblyLocation()
        {
            return $"{GetManagedLocation()}/{AssemblyLib}";
        }

        private string GetAssemblyBackupLocation()
        {
            return $"{GetAssemblyLocation()}.backup";
        }

        private string GetPluginDirectory()
        {
            return $"{AbsoluteInstallDirectory}/{PluginDirectory}";
        }
    }
}
