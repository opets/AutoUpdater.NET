using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows.Forms;
using ZipExtractor.Properties;

namespace ZipExtractor
{
    public partial class FormMain : Form
    {
        private BackgroundWorker _backgroundWorker;
		private static string m_logPath;

		public FormMain()
        {
			InitLog();
			InitializeComponent();
        }

		protected override void OnLoad( EventArgs e ) {
			base.OnLoad( e );
			string[] args = Environment.GetCommandLineArgs();

			foreach( string s in args ) {
				if( s.IndexOf( "silent", StringComparison.OrdinalIgnoreCase ) >= 0 ) {
					WriteLogMessage( $"INFO: running extractor in silent mode" );
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

			try {
				string[] args = Environment.GetCommandLineArgs();
				WriteLogMessage( $"INFO: removing archive file: {args[1]}" );
				var processStartInfo = new ProcessStartInfo {
					FileName = "del",
					UseShellExecute = true,
					Arguments = args[1]
				};
				Process.Start( processStartInfo );

			} catch( Exception ex ) {
				WriteLogMessage( $"ERROR: removing archive file: {ex.Message} - {ex.InnerException?.Message} - {ex.StackTrace}" );
			}
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
      //          foreach (var process in Process.GetProcesses())
      //          {
      //              try
      //              {
      //                  if (process.MainModule.FileName.Equals(args[2]))
      //                  {
						//	WriteLogMessage( $"INFO: Waiting for application to Exit..." );
      //                      labelInformation.Text = @"Waiting for application to Exit...";
      //                      process.WaitForExit();
      //                  }
      //              }
      //              catch (Exception ex)
      //              {
						//WriteLogMessage( $"ERROR: {ex.Message} - {ex.InnerException?.Message} - {ex.StackTrace}" );
      //              }
      //          }

                // Extract all the files.
                _backgroundWorker = new BackgroundWorker
                {
                    WorkerReportsProgress = true,
                    WorkerSupportsCancellation = true
                };

                _backgroundWorker.DoWork += (o, eventArgs) =>
                {
					try {

						var path = Path.GetDirectoryName(args[2]);

						using( ZipStorer zip = ZipStorer.Open( args[1], FileAccess.Read ) ) {

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

					} catch( Exception ex ) {
						WriteLogMessage( $"ERROR: {ex.Message} - {ex.InnerException?.Message} - {ex.StackTrace}" );
						eventArgs.Cancel = true;
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
						string arguments = ( args.Length > 3 ) ? args[3] : null;
						WriteLogMessage( $"INFO: Finished. Run Application {args[2]} {arguments}" );

						try {
                            ProcessStartInfo processStartInfo = new ProcessStartInfo(args[2]);
                            if ( arguments != null)
                            {
                                processStartInfo.Arguments = arguments;
                            }

							WriteLogMessage( $"INFO: Finished" );
                            Process.Start(processStartInfo);

						}
                        catch (Win32Exception ex)
                        {
							if( ex.NativeErrorCode != 1223)
							{
								WriteLogMessage( $"ERROR: {ex.Message} - {ex.InnerException?.Message} - {ex.StackTrace}" );
							}

						}

						Application.Exit();
                    } else {
						WriteLogMessage( $"INFO: Cancelled" );
					}
				};
                _backgroundWorker.RunWorkerAsync();
            }
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            _backgroundWorker?.CancelAsync();
        }

		private static void InitLog() {
			try {
				string currentDirectoryPath = Path.GetDirectoryName( Assembly.GetEntryAssembly().Location ) ?? string.Empty;
				string logDirectoryPath = Path.Combine( currentDirectoryPath, "logs" );
				m_logPath = Path.Combine( logDirectoryPath, "ZipExtractor.txt" );

				if( !Directory.Exists( logDirectoryPath ) ) {
					Directory.CreateDirectory( logDirectoryPath );
				}

				if( !File.Exists( m_logPath ) ) {
					File.WriteAllText( m_logPath, string.Empty );
				}
			} catch( Exception e ) {
				m_logPath = null;
			}
		}

		static void WriteLogMessage( string message ) {
			if( m_logPath == null ) {
				return;
			}
			File.AppendAllText( m_logPath, $@"{DateTime.Now}	:{message}{Environment.NewLine}" );
		}
	}
}