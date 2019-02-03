using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using ZipExtractor.Properties;

namespace ZipExtractor
{
    public partial class FormMain : Form
    {
        private BackgroundWorker _backgroundWorker;

        public FormMain()
        {
            InitializeComponent();
        }

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );
			string[] args = Environment.GetCommandLineArgs();

			foreach( string s in args ) {
				if( s.IndexOf( "silent", StringComparison.OrdinalIgnoreCase ) >= 0 ) {
					this.Hide();
					this.WindowState = FormWindowState.Minimized;
					this.Visible = false; // Hide form window.
					this.ShowInTaskbar = false; // Remove from taskbar.
					this.Opacity = 0;
				}
			}
		}

		protected override void OnClosed( EventArgs e ) {
			base.OnClosed( e );

			string[] args = Environment.GetCommandLineArgs();
			var processStartInfo = new ProcessStartInfo {
				FileName = "del",
				UseShellExecute = true,
				Arguments = args[1]
			};
			Process.Start( processStartInfo );
		}

		private void FormMain_Shown(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
			foreach( string s in args ) {
				if( s.IndexOf( "silent", StringComparison.OrdinalIgnoreCase ) >= 0 ) {
					this.Hide();
					this.WindowState = FormWindowState.Minimized;
					this.Visible = false; // Hide form window.
					this.ShowInTaskbar = false; // Remove from taskbar.
					this.Opacity = 0;
				}
			}

			if( args.Length >= 3)
            {
                foreach (var process in Process.GetProcesses())
                {
                    try
                    {
                        if (process.MainModule.FileName.Equals(args[2]))
                        {
                            labelInformation.Text = @"Waiting for application to Exit...";
                            process.WaitForExit();
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception.Message);
                    }
                }

                // Extract all the files.
                _backgroundWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };

                _backgroundWorker.DoWork += (o, eventArgs) =>
                {
                    var path = Path.GetDirectoryName(args[2]);


					using( ZipStorer zip = ZipStorer.Open( args[1], FileAccess.Read ) ) {

						zip.FileName = args[1];

						// Read the central directory collection.
						List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();

	                    for (var index = 0; index < dir.Count; index++)
	                    {
	                        if (_backgroundWorker.CancellationPending)
	                        {
	                            eventArgs.Cancel = true;
	                            zip.Close();
	                            return;
	                        }

	                        ZipStorer.ZipFileEntry entry = dir[index];
	                        zip.ExtractFile(entry, Path.Combine(path, entry.FilenameInZip));
	                        _backgroundWorker.ReportProgress((index + 1) * 100 / dir.Count,
	                            string.Format(Resources.CurrentFileExtracting, entry.FilenameInZip));
	                    }

						zip.Close();
					}

					

				};

                _backgroundWorker.ProgressChanged += (o, eventArgs) =>
                {
                    progressBar.Value = eventArgs.ProgressPercentage;
                    labelInformation.Text = eventArgs.UserState.ToString();
                };

                _backgroundWorker.RunWorkerCompleted += (o, eventArgs) =>
                {
                    if (!eventArgs.Cancelled)
                    {
                        labelInformation.Text = @"Finished";

						try {
                            ProcessStartInfo processStartInfo = new ProcessStartInfo(args[2]);
                            if (args.Length > 3)
                            {
                                processStartInfo.Arguments = args[3];
                            }

                            Process.Start(processStartInfo);

						}
                        catch (Win32Exception exception)
                        {
                            if (exception.NativeErrorCode != 1223)
                                throw;
                        }

                        Application.Exit();
                    }
                };
                _backgroundWorker.RunWorkerAsync();
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _backgroundWorker?.CancelAsync();
        }
    }
}