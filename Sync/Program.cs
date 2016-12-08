using System;
using System.IO;

namespace Sync
{
	internal class Program
	{
		#region Public Methods

		public static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine("Usage: sync.exe directory1 directory2");
				return;
			}

			try
			{
				watcher1 = StartWatcher(args[0]);
				watcher2 = StartWatcher(args[1]);

				Console.WriteLine("Press Esc for exit.");
				while (Console.ReadKey().Key != ConsoleKey.Escape) ;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		#endregion Public Methods

		#region Private Fields

		private static FileSystemWatcher watcher1;

		private static FileSystemWatcher watcher2;

		#endregion Private Fields

		#region Private Methods

		private static void ChangedHandler(object sender, FileSystemEventArgs e)
		{
			try
			{
				Console.WriteLine("File {0} changed.", e.FullPath);

				FileSystemWatcher watcher = (FileSystemWatcher)sender;
				FileSystemWatcher otherWatcher = GetOtherWatcher(watcher);
				string targetDir = otherWatcher.Path;

				otherWatcher.EnableRaisingEvents = false;
				try
				{
					File.Copy(e.FullPath, Path.Combine(targetDir, e.Name), true);
				}
				finally
				{
					otherWatcher.EnableRaisingEvents = true;
				}

				Console.WriteLine("Success.");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		private static void CreatedHandler(object sender, FileSystemEventArgs e)
		{
			try
			{
				Console.WriteLine("File {0} created.", e.FullPath);

				FileSystemWatcher watcher = (FileSystemWatcher)sender;
				FileSystemWatcher otherWatcher = GetOtherWatcher(watcher);
				string targetDir = otherWatcher.Path;

				otherWatcher.EnableRaisingEvents = false;
				try
				{
					File.Copy(e.FullPath, Path.Combine(targetDir, e.Name), true);
					Console.WriteLine("Success.");
				}
				catch (IOException ex)
				{
					Console.WriteLine("The file is occupied with other process");
				}
				finally
				{
					otherWatcher.EnableRaisingEvents = true;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		private static void DeletedHandler(object sender, FileSystemEventArgs e)
		{
			try
			{
				Console.WriteLine("File {0} deleted.", e.FullPath);

				FileSystemWatcher watcher = (FileSystemWatcher)sender;
				FileSystemWatcher otherWatcher = GetOtherWatcher(watcher);
				string targetDir = otherWatcher.Path;

				otherWatcher.EnableRaisingEvents = false;
				try
				{
					File.Delete(Path.Combine(targetDir, e.Name));
				}
				finally
				{
					otherWatcher.EnableRaisingEvents = true;
				}

				Console.WriteLine("Success.");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		private static FileSystemWatcher GetOtherWatcher(FileSystemWatcher watcher)
		{
			if (ReferenceEquals(watcher, watcher1))
				return watcher2;
			else
				return watcher1;
		}

		private static void RenamedHandler(object sender, RenamedEventArgs e)
		{
			try
			{
				Console.WriteLine("File {0} renamed.", e.FullPath);

				FileSystemWatcher watcher = (FileSystemWatcher)sender;
				FileSystemWatcher otherWatcher = GetOtherWatcher(watcher);
				string targetDir = otherWatcher.Path;

				otherWatcher.EnableRaisingEvents = false;
				try
				{
					File.Move(
						Path.Combine(targetDir, e.OldName),
						Path.Combine(targetDir, e.Name)
					);
				}
				finally
				{
					otherWatcher.EnableRaisingEvents = true;
				}

				Console.WriteLine("Success.");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		private static FileSystemWatcher StartWatcher(string dir)
		{
			FileSystemWatcher watcher = new FileSystemWatcher();
			watcher.Path = dir;
			watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
			SubscribeWatcherEvents(watcher);
			watcher.EnableRaisingEvents = true;
			return watcher;
		}

		private static void SubscribeWatcherEvents(FileSystemWatcher watcher)
		{
			watcher.Changed += ChangedHandler;
			watcher.Created += CreatedHandler;
			watcher.Deleted += DeletedHandler;
			watcher.Renamed += RenamedHandler;
		}

		#endregion Private Methods
	}
}