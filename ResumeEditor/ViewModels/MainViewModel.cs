using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using ResumeEditor.Models;
using ResumeEditor.ResumeData;

namespace ResumeEditor.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private Dictionary<string, string> _labelModes = new Dictionary<string, string> { { "letter", "By Drive Letter" }, { "label", "By Drive Label" }, { "tracker", "By Tracker" } };
        private IEnumerable<ResumeItem> _displayItems;
        private Labels _displayLabels;
        private string _labelMode;
        private string _resumeDataFilePath;
        private string _titleFileter;
        private string _sizeFilter;
        private string _trackerFilter;

        private IList<ResumeItem> _resumeItems;
        private Resume _resume;
        private List<UTorrent> _uTorrents;
        private string _selectedResume;

        private RelayCommand _loadResumeDataCommand;
        private RelayCommand _createLabelsCommand;
        private RelayCommand _saveCommand;
        private RelayCommand _resetCommand;
        private RelayCommand<RoutedPropertyChangedEventArgs<object>> _treeviewSelectionChangedCommand;
        private RelayCommand _filterCommand;
        private RelayCommand<object> _openExplorerCommand;
        private RelayCommand _selectUTorrentChangedCommand;

        private Dictionary<string, string> _trackers;

        public MainViewModel()
        {
            this.InitCommands();
            this.UpdateLables();

            this.FindUTorrent();
        }

        private void InitCommands()
        {
            this._loadResumeDataCommand = new RelayCommand(() =>
            {
                var dialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
                dialog.Title = "Please select one resume.dat file to open";
                dialog.Filter = "All Files|*.*";
                dialog.CheckFileExists = true;
                dialog.ShowDialog();
                this.OpenResume(dialog.FileName);
            });

            this._treeviewSelectionChangedCommand = new RelayCommand<RoutedPropertyChangedEventArgs<object>>((args) =>
            {
                if (args.NewValue != null)
                {
                    if (args.NewValue is TreeViewItem)
                    {
                        this.UpdateItems("All");
                        return;
                    }
                    var patt = new Regex(@"(.*) \(\d+\)$");
                    this.UpdateItems(patt.Match(args.NewValue.ToString()).Groups[1].Value);
                }
            });

            this._createLabelsCommand = new RelayCommand(CreateLabels);

            this._resetCommand = new RelayCommand(() =>
            {
                if (this._resume != null)
                {
                    this._resumeItems = this._resume.ResumeItems;
                    this.UpdateLables();
                    this.UpdateItems("All");
                }
            });

            this._saveCommand = new RelayCommand(() =>
            {
                if (this._resumeItems == null)
                {
                    return;
                }
                string backpath = this._resume.Backup();
                if (backpath != null)
                {
                    this._resume.Save(this._resumeItems);
                    MessageBox.Show("Save compolete, the original file has been backup to " + backpath, "ResumeEditor", MessageBoxButton.OK, MessageBoxImage.None);
                }
                else
                {
                    throw new Exception("Cannot backup resume.dat, the change has not been saved.");
                }
            });

            this._filterCommand = new RelayCommand(FilterList);

            this._openExplorerCommand = new RelayCommand<object>((dg) =>
            {
                var datagrid = (DataGrid)dg;
                if (datagrid.SelectedItem != null)
                {
                    var item = (ResumeItem)datagrid.SelectedItem;
                    Process.Start("explorer.exe", item.Path);
                }
            });

            this._selectUTorrentChangedCommand = new RelayCommand(() => OpenResume(this._selectedResume));
        }

        private void OpenResume(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            try
            {
                Resume resume = Resume.Load(this._selectedResume);
                this.Set(() => this.ResumeDataFilePath, ref this._resumeDataFilePath, path);
                this._resume = resume;
                this._resumeItems = resume.ResumeItems;
                this.UpdateLables();
                this.UpdateItems("All");
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }

        }

        private void UpdateLables()
        {
            if (this._displayLabels == null)
            {
                this._displayLabels = new Labels();
            }
            this._displayLabels.Clear();
            if (this._resumeItems == null)
            {
                return;
            }
            this._displayLabels.SetSum(this._resumeItems.Count);
            var labels = this._resumeItems.Select(c => c.Label).Distinct();
            foreach (var label in labels)
            {
                if (label == null)
                {
                    continue;
                }
                this._displayLabels.AddLabel(label, this._resumeItems.Count(c => c.Label == label));
            }
            this._displayLabels.AddOthers();
        }

        private void UpdateItems(string label)
        {
            if (this._resumeItems == null)
            {
                return;
            }
            if (label == "All")
            {
                this.Set(() => this.DisplayItems, ref this._displayItems, this._resumeItems);
            }
            else if (label == "Others")
            {
                this.Set(() => this.DisplayItems, ref this._displayItems,
                    this._resumeItems.Where(c => string.IsNullOrEmpty(c.Label)));
            }
            else
            {
                this.Set(() => this.DisplayItems, ref this._displayItems,
                    this._resumeItems.Where(c => c.Label == label));
            }
            this.FilterList();
        }

        public void CreateLabels()
        {
            switch (this._labelMode)
            {
                case "letter":
                    foreach (var item in this._resumeItems)
                    {
                        item.Label = item.Path.Substring(0, 1).ToUpperInvariant();
                    }
                    break;
                case "label":
                    var drivers = DriveInfo.GetDrives();
                    foreach (var item in this._resumeItems)
                    {
                        var driver =
                            drivers.FirstOrDefault(c => c.RootDirectory.FullName == Path.GetPathRoot(item.Path));
                        if (driver != null)
                        {
                            item.Label = driver.VolumeLabel;
                        }
                        else
                        {
                            item.Label = "";
                        }
                    }
                    break;
                case "tracker":
                    if (this._trackers == null) { LoadTrackers(); }
                    foreach (var item in this._resumeItems)
                    {
                        string tracker = item.Tracker;
                        bool set = false;
                        foreach (var trackerPair in this._trackers)
                        {
                            if (tracker.Contains(trackerPair.Key))
                            {
                                item.Label = trackerPair.Value;
                                set = true;
                                break;
                            }
                        }
                        if (!set)
                        {
                            item.Label = null;
                        }
                    }
                    break;
            }
            this.UpdateLables();
        }

        private void LoadTrackers()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "trackers.txt");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Cannot found tracker list file.", path);
            }
            const string patt = @"^(.*?)\s+(.*)$";
            this._trackers = new Dictionary<string, string>();
            using (var reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        continue;
                    }
                    Match match = Regex.Match(line, patt);
                    if (match.Groups.Count == 3)
                    {
                        this._trackers.Add(match.Groups[2].Value, match.Groups[1].Value);
                    }
                }
            }
        }

        private void FilterList()
        {
            IEnumerable<ResumeItem> filteredItems = this._displayItems;
            if (!string.IsNullOrEmpty(this._titleFileter))
            {
                filteredItems = filteredItems.Where(c => c.Caption.Contains(this._titleFileter));
            }
            if (!string.IsNullOrEmpty(this._trackerFilter))
            {
                filteredItems = filteredItems.Where(c => c.Tracker.Contains(this._trackerFilter));
            }
            this.Set(() => this.DisplayItems, ref this._displayItems, filteredItems);
        }

        private void FindUTorrent()
        {
            this._uTorrents = new List<UTorrent>();
            this._uTorrents.Add(new UTorrent
            {
                Title = "Select from Running uTorrent(s)",
                ResumePath = ""
            });
            foreach (var progess in Process.GetProcesses())
            {
                if (progess.ProcessName.ToUpperInvariant().Contains("UTORRENT"))
                {
                    try
                    {
                        string filename = progess.Modules[0].FileName;
                        string resumePath = Path.Combine(Path.GetDirectoryName(filename), "resume.dat");
                        if (File.Exists(resumePath))
                        {
                            this._uTorrents.Add(new UTorrent()
                            {
                                Title = progess.Modules[0].FileVersionInfo.ProductVersion,
                                ResumePath = resumePath
                            });
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }

        private void ShowError(Exception ex)
        {
            MessageBox.Show("An error occoured." + Environment.NewLine + ex.Message, "ResumeEditor", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public string ResumeDataFilePath
        {
            get { return _resumeDataFilePath; }
            set { _resumeDataFilePath = value; }
        }

        public Dictionary<string, string> LabelModes
        {
            get { return _labelModes; }
            set { _labelModes = value; }
        }

        public string LabelMode
        {
            get { return _labelMode; }
            set { _labelMode = value; }
        }

        public IEnumerable<ResumeItem> DisplayItems
        {
            get { return _displayItems; }
            set { _displayItems = value; }
        }

        public Labels DisplayLabels
        {
            get { return _displayLabels; }
            set { _displayLabels = value; }
        }

        public IList<ResumeItem> ResumeItems
        {
            get { return _resumeItems; }
            set { _resumeItems = value; }
        }

        public RelayCommand LoadResumeDataCommand
        {
            get { return _loadResumeDataCommand; }
            set { _loadResumeDataCommand = value; }
        }

        public RelayCommand CreateLabelsCommand
        {
            get { return _createLabelsCommand; }
            set { _createLabelsCommand = value; }
        }

        public RelayCommand SaveCommand
        {
            get { return _saveCommand; }
            set { _saveCommand = value; }
        }

        public RelayCommand ResetCommand
        {
            get { return _resetCommand; }
            set { _resetCommand = value; }
        }

        public RelayCommand FilterCommand
        {
            get { return _filterCommand; }
            set { _filterCommand = value; }
        }

        public RelayCommand<RoutedPropertyChangedEventArgs<object>> TreeviewSelectionChangedCommand
        {
            get { return _treeviewSelectionChangedCommand; }
            set { _treeviewSelectionChangedCommand = value; }
        }

        public string TitleFileter
        {
            get { return _titleFileter; }
            set { _titleFileter = value; }
        }

        public string SizeFilter
        {
            get { return _sizeFilter; }
            set { _sizeFilter = value; }
        }

        public string TrackerFilter
        {
            get { return _trackerFilter; }
            set { _trackerFilter = value; }
        }

        public RelayCommand<object> OpenExplorerCommand
        {
            get { return _openExplorerCommand; }
            set { _openExplorerCommand = value; }
        }

        public List<UTorrent> UTorrents
        {
            get { return _uTorrents; }
            set { _uTorrents = value; }
        }

        public string SelectedResume
        {
            get { return _selectedResume; }
            set { _selectedResume = value; }
        }

        public RelayCommand SelectUTorrentChangedCommand
        {
            get { return _selectUTorrentChangedCommand; }
            set { _selectUTorrentChangedCommand = value; }
        }
    }
}