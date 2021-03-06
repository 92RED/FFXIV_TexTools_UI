﻿// Copyright © 2019 Rafael Gonzalez - All Rights Reserved
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using FFXIV_TexTools.Helpers;
using FFXIV_TexTools.Models;
using FFXIV_TexTools.Properties;
using FFXIV_TexTools.Resources;
using FFXIV_TexTools.ViewModels;
using FFXIV_TexTools.Views;
using MahApps.Metro;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using xivModdingFramework.General.Enums;
using xivModdingFramework.Items.Interfaces;
using xivModdingFramework.Mods;
using xivModdingFramework.Mods.FileTypes;
using xivModdingFramework.SqPack.FileTypes;
using Application = System.Windows.Application;

namespace FFXIV_TexTools
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            var mainViewModel = new MainViewModel(this);
            this.DataContext = mainViewModel;
        }

        /// <summary>
        /// Event handler for the language button clicked
        /// </summary>
        private void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            RightFlyout.Content = new LanguageOptionsView();
            RightFlyout.IsOpen = true;
        }

        /// <summary>
        /// Event handler for the light theme clicked
        /// </summary>
        private void MenuLightTheme_Click(object sender, RoutedEventArgs e)
        {
            var appStyle = ThemeManager.DetectAppStyle(Application.Current);

            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(appStyle.Item2.Name), ThemeManager.GetAppTheme("BaseLight"));

            Settings.Default.Application_Theme = "BaseLight";
            Settings.Default.Save();
        }

        /// <summary>
        /// Event handler for the dark theme clicked
        /// </summary>
        private void MenuDarkTheme_Click(object sender, RoutedEventArgs e)
        {
            var appStyle = ThemeManager.DetectAppStyle(Application.Current);

            ThemeManager.ChangeAppStyle(Application.Current, ThemeManager.GetAccent(appStyle.Item2.Name), ThemeManager.GetAppTheme("BaseDark"));

            Settings.Default.Application_Theme = "BaseDark";
            Settings.Default.Save();
        }

        /// <summary>
        /// Event handler for the treeview selected item changed
        /// </summary>
        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = e.NewValue as Category;

            if (selectedItem?.Item != null)
            {
                var textureView = TextureTabItem.Content as TextureView;
                var textureViewModel = textureView.DataContext as TextureViewModel;

                textureViewModel.UpdateTexture(selectedItem.Item);

                if (selectedItem.Item.Category.Equals(XivStrings.UI) ||
                    selectedItem.Item.ItemCategory.Equals(XivStrings.Face_Paint) ||
                    selectedItem.Item.ItemCategory.Equals(XivStrings.Equip_Decals))
                {
                    if (TabsControl.SelectedIndex == 1)
                    {
                        TabsControl.SelectedIndex = 0;
                    }

                    ModelTabItem.IsEnabled = false;
                }
                else
                {
                    ModelTabItem.IsEnabled = true;

                    var modelView = ModelTabItem.Content as ModelView;
                    var modelViewModel = modelView.DataContext as ModelViewModel;

                    modelViewModel.UpdateModel(selectedItem.Item as IItemModel);
                }
            }
        }

        /// <summary>
        /// Event handler for the model id search clicked
        /// </summary>
        private void Menu_ModelIDSearch_Click(object sender, RoutedEventArgs e)
        {
            var modelSearchView = new ModelSearchView(this) {Owner = this};
            modelSearchView.Show();
        }

        /// <summary>
        /// Event handler for the about menu item clicked
        /// </summary>
        private void Menu_About_Click(object sender, RoutedEventArgs e)
        {
            var about = new AboutView {Owner = this};
            about.Show();
        }

        /// <summary>
        /// Event handler for the PK Emporium site
        /// </summary>
        private void PKEmporium_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(WebUrl.PKEmporium_Website);
        }

        /// <summary>
        /// Event handler for the Xiv Mod Archive site
        /// </summary>
        private void XivModArchive_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(WebUrl.XivModArchive_Website);
        }

        /// <summary>
        /// Event handler for the Nexus Mods site
        /// </summary>
        private void NexusMods_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(WebUrl.NexusMods_Website);
        }

        /// <summary>
        /// Event handler for the Discord Invite
        /// </summary>
        private void DiscordButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(WebUrl.Discord_Invite);
        }

        /// <summary>
        /// Event handler for the BugReport site
        /// </summary>
        private void Menu_BugReport_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(WebUrl.BugReport_Website);
        }

        /// <summary>
        /// Event handler for the Tutorials site
        /// </summary>
        private void Menu_Tutorials_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(WebUrl.Tutorials_Website);
        }

        /// <summary>
        /// Event handler for the mod list menu item clicked
        /// </summary>
        private void Menu_ModList_Click(object sender, RoutedEventArgs e)
        {
            var modListView = new ModListView {Owner = this};
            modListView.Show();
        }

        /// <summary>
        /// Event handler for the problem check menu item clicked
        /// </summary>
        private void Menu_ProblemCheck_Click(object sender, RoutedEventArgs e)
        {
            var problemCheckView = new ProblemCheckView {Owner = this};
            problemCheckView.Show();
        }

        /// <summary>
        /// Event handler for the customize menu item clicked
        /// </summary>
        private void Customize_Click(object sender, RoutedEventArgs e)
        {
            var customize = new CustomizeSettingsView {Owner = this};
            customize.Show();
        }

        /// <summary>
        /// Event handler for the mod pack wizard menu item clicked
        /// </summary>
        private async void Menu_MakeModpackWizard_Click(object sender, RoutedEventArgs e)
        {
            var wizard = new ModPackWizard {Owner = this};
            var result = wizard.ShowDialog();

            if (result == true)
            {
                if (wizard.ModPackFileName.Equals("NoData"))
                {
                    await this.ShowMessageAsync("Error: Mod Pack Creation Failed", "No mods were detected in the options list.");
                }
                else
                {
                    await this.ShowMessageAsync("Mod Pack Creation Complete", $"The ModPack ({wizard.ModPackFileName}.ttmp2) has been successfully Created.");
                }
            }
        }

        /// <summary>
        /// Event handler for the import mod pack menu item clicked
        /// </summary>
        private async void Menu_ImportModpack_Click(object sender, RoutedEventArgs e)
        {
            var modPackDirectory = new DirectoryInfo(Settings.Default.ModPack_Directory);

            var openFileDialog = new OpenFileDialog {InitialDirectory = modPackDirectory.FullName, Filter = "TexToolsModPack TTMP (*.ttmp;*.ttmp2)|*.ttmp;*.ttmp2"};

            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    var ttmp = new TTMP(modPackDirectory, XivStrings.TexTools);
                    var ttmpData = ttmp.GetModPackJsonData(new DirectoryInfo(openFileDialog.FileName));

                    if (ttmpData.ModPackJson.TTMPVersion.Contains("w"))
                    {
                        var importWizard = new ImportModPackWizard(ttmpData.ModPackJson, ttmpData.ImageDictionary, new DirectoryInfo(openFileDialog.FileName)) { Owner = this };
                        var result = importWizard.ShowDialog();

                        if (result == true)
                        {
                            await this.ShowMessageAsync("Import Complete", $"{importWizard.TotalModsImported} mod(s) successfully imported.");
                        }
                    }
                    else
                    {
                        var simpleImport = new SimpleModPackImporter(new DirectoryInfo(openFileDialog.FileName), ttmpData.ModPackJson) { Owner = this };
                        var result = simpleImport.ShowDialog();

                        if (result == true)
                        {
                            await this.ShowMessageAsync("Import Complete", $"{simpleImport.TotalModsImported} mod(s) successfully imported.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    var simpleImport = new SimpleModPackImporter(new DirectoryInfo(openFileDialog.FileName), null) { Owner = this };
                    var result = simpleImport.ShowDialog();

                    if (result == true)
                    {
                        await this.ShowMessageAsync("Import Complete", $"{simpleImport.TotalModsImported} mod(s) successfully imported.");
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for the simple mod pack menu item clicked
        /// </summary>
        private async void Menu_MakeSimpleModpack_Click(object sender, RoutedEventArgs e)
        {
            var simpleCreator = new SimpleModPackCreator{Owner = this};
            var result = simpleCreator.ShowDialog();

            if (result == true)
            {
                await this.ShowMessageAsync("Mod Pack Creation Complete", $"The ModPack ({simpleCreator.ModPackFileName}.ttmp2) has been successfully Created.");
            }
        }

        /// <summary>
        /// Event handler for the start over menu item clicked
        /// </summary>
        private async void Menu_StartOver_Click(object sender, RoutedEventArgs e)
        {
            var gameDirectory = new DirectoryInfo(Settings.Default.FFXIV_Directory);

            var index = new Index(gameDirectory);

            if (index.IsIndexLocked(XivDataFile._0A_Exd))
            {
                FlexibleMessageBox.Show("Error Accessing Index File\n\n" +
                                        "Please exit the game before proceeding.\n" +
                                        "-----------------------------------------------------\n\n", 
                    "Index Access Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            var result = FlexibleMessageBox.Show("Starting over will:\n\n" +
                                                 "Restore index files to their original state.\n" +
                                                 "Delete all mod dat files from game folder.\n" +
                                                 "Delete all mod list file entries.\n\n" +
                                                 "Do you want to start over?", "Start Over", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == System.Windows.Forms.DialogResult.Yes)
            {
                var task = Task.Run((() =>
                {
                    var modding = new Modding(gameDirectory);
                    var dat = new Dat(gameDirectory);

                    var indexBackupsDirectory = new DirectoryInfo(Settings.Default.Backup_Directory);
                    var modListDirectory = new DirectoryInfo(Path.Combine(gameDirectory.Parent.Parent.FullName, XivStrings.ModlistFilePath));

                    var backupFiles = Directory.GetFiles(indexBackupsDirectory.FullName);

                    // Make sure backups exist
                    if (backupFiles.Length == 0)
                    {
                        FlexibleMessageBox.Show($"No backup files found in the following directory:\n{indexBackupsDirectory.FullName}\n\n" +
                                                $"Index entries will be put back to original offsets instead.\n" +
                                                "-----------------------------------------------------\n\n",
                            "Backup Files Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                        // Toggle off all mods
                        modding.ToggleAllMods(false);
                    }
                    else
                    {
                        // Copy backups to ffxiv folder
                        foreach (var backupFile in backupFiles)
                        {
                            if (backupFile.Contains(".win32.index"))
                            {
                                File.Copy(backupFile, $"{gameDirectory}/{Path.GetFileName(backupFile)}", true);
                            }
                        }

                    }

                    // Delete modded dat files
                    foreach (var xivDataFile in (XivDataFile[])Enum.GetValues(typeof(XivDataFile)))
                    {
                        var datFiles = dat.GetModdedDatList(xivDataFile);

                        foreach (var datFile in datFiles)
                        {
                            File.Delete(datFile);
                        }
                    }

                    // Delete mod list
                    File.Delete(modListDirectory.FullName);

                    modding.CreateModlist();

                }));

                task.Wait();

                await this.ShowMessageAsync("Start Over Complete", "The start over process has been completed.");
            }

        }

        private void Menu_Donate_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(WebUrl.FFXIV_Donate);
        }
    }
}
