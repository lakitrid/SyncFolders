using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SyncFolders.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private string _sourceFolder;
        private string _targetFolders;
        private Visibility _hasError;
        private string _errorMessage;
        private string _resultMessage;
        private int _total;
        private int _current;
        private Visibility _inProcessVis;
        private Visibility _outProcessVis;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            this.SourceFolder = string.Empty;
            this.HasError = Visibility.Collapsed;
            this.InProcessVis = Visibility.Collapsed;
            this.OutProcessVis = Visibility.Visible;
            this.ErrorMessage = string.Empty;
            this.ResultMessage = string.Empty;
            this.Total = int.MaxValue;
            this.Current = int.MinValue;
            this.SyncCommand = new RelayCommand(this.Sync);
            this.SelectSourceCommand = new RelayCommand(this.SelectSource);
            this.SelectTargetCommand = new RelayCommand(this.SelectTarget);
        }

        public string SourceFolder
        {
            get
            {
                return this._sourceFolder;
            }

            set
            {
                this._sourceFolder = value;
                this.RaisePropertyChanged(nameof(SourceFolder));
            }
        }

        public string TargetFolders
        {
            get
            {
                return this._targetFolders;
            }

            set
            {
                this._targetFolders = value;
                this.RaisePropertyChanged(nameof(TargetFolders));
            }
        }

        public string ErrorMessage
        {
            get
            {
                return this._errorMessage;
            }

            set
            {
                this._errorMessage = value;
                this.RaisePropertyChanged(nameof(ErrorMessage));
            }
        }

        public string ResultMessage
        {
            get
            {
                return this._resultMessage;
            }

            set
            {
                this._resultMessage = value;
                this.RaisePropertyChanged(nameof(ResultMessage));
            }
        }

        public int Current
        {
            get
            {
                return this._current;
            }

            set
            {
                this._current = value;
                this.RaisePropertyChanged(nameof(Current));
            }
        }

        public int Total
        {
            get
            {
                return this._total;
            }

            set
            {
                this._total = value;
                this.RaisePropertyChanged(nameof(Total));
            }
        }

        public Visibility HasError
        {
            get
            {
                return this._hasError;
            }

            set
            {
                this._hasError = value;
                this.RaisePropertyChanged(nameof(HasError));
            }
        }

        public Visibility InProcessVis
        {
            get
            {
                return this._inProcessVis;
            }

            set
            {
                this._inProcessVis = value;
                this.RaisePropertyChanged(nameof(InProcessVis));
            }
        }

        public Visibility OutProcessVis
        {
            get
            {
                return this._outProcessVis;
            }

            set
            {
                this._outProcessVis = value;
                this.RaisePropertyChanged(nameof(OutProcessVis));
            }
        }

        public RelayCommand SyncCommand { get; set; }

        public RelayCommand SelectSourceCommand { get; set; }

        public RelayCommand SelectTargetCommand { get; set; }

        public void Sync()
        {
            this.InProcessVis = Visibility.Visible;
            this.OutProcessVis = Visibility.Collapsed;
            Task task = new Task(this.DoSync);
            task.Start();
        }

        private void DoSync()
        {
            this.HasError = Visibility.Collapsed;

            if (!Directory.Exists(this.SourceFolder))
            {
                this.ErrorSync("Source Folder not found");
                return;
            }

            if(!Directory.Exists(this.TargetFolders))
            {
                this.ErrorSync("Target Folder not found");
                return;
            }

            ResultMessage = $"Preparing the  to copy from {this.SourceFolder} to {this.TargetFolders}";

            IEnumerable<string> sourceFiles = Directory.EnumerateFiles(this.SourceFolder, "*", SearchOption.AllDirectories);
            IEnumerable<string> targetFiles = Directory.EnumerateFiles(this.TargetFolders, "*", SearchOption.AllDirectories);

            List<Tuple<string, string>> toCopy = new List<Tuple<string, string>>();
            foreach(string file in sourceFiles)
            {
                FileInfo fileInfo = new FileInfo(file);
                string dest = Path.Combine(this.TargetFolders, fileInfo.Name);

                if(!targetFiles.Contains(dest))
                {
                    toCopy.Add(new Tuple<string, string>(file, dest));
                }
            }

            ResultMessage = $"{toCopy.Count} file to copy from {this.SourceFolder} to {this.TargetFolders}";
            this.Total = toCopy.Count;
            this.Current = 0;

            foreach (Tuple<string, string> file in toCopy)
            {
                File.Copy(file.Item1, file.Item2);
                this.Current++;
                ResultMessage = $"{toCopy.Count - this.Current} file to copy from {this.SourceFolder} to {this.TargetFolders}";
            }

            ResultMessage = $"Successfully copy {toCopy.Count} file from {this.SourceFolder} to {this.TargetFolders}";

            this.InProcessVis = Visibility.Collapsed;
            this.OutProcessVis = Visibility.Visible;
        }

        private void ErrorSync(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
            this.HasError = Visibility.Visible;
            this.InProcessVis = Visibility.Collapsed;
            this.OutProcessVis = Visibility.Visible;
        }

        private void SelectSource()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = "c:";
            fileDialog.Multiselect = false;

            if (fileDialog.ShowDialog() == true)
            {
                FileInfo info = new FileInfo(fileDialog.FileName);
                this.SourceFolder = info.DirectoryName;
            }
        }

        private void SelectTarget()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = "c:";
            fileDialog.Multiselect = false;

            if (fileDialog.ShowDialog() == true)
            {
                FileInfo info = new FileInfo(fileDialog.FileName);
                this.TargetFolders = info.DirectoryName;
            }
        }
    }
}