using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.VisualBasic; // Add this using directive


namespace Monke2.Views.Pages
{
	public partial class DataPage : Page
	{
		public DataPage()
		{
			InitializeComponent();
		}

		private void DecompileACB_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "ACB Files (*.acb)|*.acb",
				Title = "Select ACB File"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				string acbFilePath = openFileDialog.FileName;
				RunAcbEditor(acbFilePath);

				// Call UpdateFileList with the path of the selected ACB file
				UpdateFileList(acbFilePath);
			}
		}


		private void UpdateFileList(string acbFilePath)
		{
			FilesListBox.Items.Clear();

			// Extracting the directory where the ACB file is located
			string acbFileDirectory = Path.GetDirectoryName(acbFilePath);

			// Extracting the ACB file name without the extension
			string acbFileNameWithoutExtension = Path.GetFileNameWithoutExtension(acbFilePath);

			// Constructing the path to the subfolder named after the ACB file
			string decompiledFolderPath = Path.Combine(acbFileDirectory, acbFileNameWithoutExtension);

			// Display the path for debugging purposes
			MessageBox.Show($"Looking for files in: {decompiledFolderPath}");

			// Checking if the decompiled folder exists
			if (Directory.Exists(decompiledFolderPath))
			{
				string[] files = Directory.GetFiles(decompiledFolderPath);
				foreach (var file in files)
				{
					FilesListBox.Items.Add(Path.GetFileName(file));
				}
			}
			else
			{
				FilesListBox.Items.Add($"No files found in {decompiledFolderPath}");
			}
		}







		private void RunAcbEditor(string path)
		{
			string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SonicAudioTools", "AcbEditor.exe");

			try
			{
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = exePath,
					Arguments = $"\"{path}\"", // Pass the ACB file path directly
					UseShellExecute = false,
					CreateNoWindow = true
				};

				using (Process process = Process.Start(startInfo))
				{
					process.WaitForExit();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error in launching AcbEditor: " + ex.Message);
			}
		}




		private void CompileACB_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog folderDialog = new OpenFileDialog
			{
				ValidateNames = false,
				CheckFileExists = false,
				CheckPathExists = true,
				FileName = "Folder Selection.", // The user will type the folder or navigate into it
				Filter = "Folder|*.none" // Dummy filter
			};

			if (folderDialog.ShowDialog() == true)
			{
				string folderPath = Path.GetDirectoryName(folderDialog.FileName);
				RunAcbEditor(folderPath);
			}
		}

		private void ACBVolume_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "ACB Files (*.acb)|*.acb",
				Title = "Select ACB File"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				string acbFilePath = openFileDialog.FileName;
				string volumeInput = Interaction.InputBox("Enter the volume level (default is 1):", "Volume Level", "1", -1, -1);

				if (float.TryParse(volumeInput, out float volumeLevel))
				{
					AdjustVolumeInAcbFile(acbFilePath, volumeLevel);
					MessageBox.Show($"Adjusted volume level to {volumeLevel} in file: {acbFilePath}");
				}
				else
				{
					MessageBox.Show("Invalid volume level entered.");
				}
			}
		}

		private void AdjustVolumeInAcbFile(string filePath, float volumeLevel)
		{
			byte[] newSequence = BitConverter.GetBytes(volumeLevel); // Convert new volume level to byte sequence

			// Reverse the byte array if the system is little-endian
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(newSequence);
			}

			byte[] fileContent = File.ReadAllBytes(filePath);

			// Ensure the file is large enough
			if (fileContent.Length >= 0x21A + 4)
			{
				Array.Copy(newSequence, 0, fileContent, 0x217, newSequence.Length);
				File.WriteAllBytes(filePath, fileContent);
			}
			else
			{
				MessageBox.Show("The file is too small for the specified operation.");
			}
		}



		private int FindSequence(byte[] array, byte[] sequence)
		{
			for (int i = 0; i < array.Length - sequence.Length + 1; i++)
			{
				bool found = true;
				for (int j = 0; j < sequence.Length; j++)
				{
					if (array[i + j] != sequence[j])
					{
						found = false;
						break;
					}
				}
				if (found)
				{
					return i;
				}
			}
			return -1;
		}
	}
}
