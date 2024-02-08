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
				Filter = "ACB Files (*.acb)|*.acb|UASSET Files (*.uasset)|*.uasset",
				Title = "Select ACB or UASSET File"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				string filePath = openFileDialog.FileName;

				if (filePath.EndsWith(".uasset", StringComparison.OrdinalIgnoreCase))
				{
					// Handle UASSET file
					HandleUASSETFile(filePath);
				}
				else if (filePath.EndsWith(".acb", StringComparison.OrdinalIgnoreCase))
				{
					// Handle ACB file
					RunAcbEditor(filePath);
					UpdateFileList(filePath);
				}
				else
				{
					MessageBox.Show("Unsupported file format.");
				}
			}
		}

		private void HandleUASSETFile(string filePath)
		{
			try
			{
				// Step 1: Read the .uasset file as bytes
				byte[] fileBytes = File.ReadAllBytes(filePath);

				// Step 2: Search for the first occurrence of "@UTF" in the byte array
				int index = FindSequence(fileBytes, new byte[] { 0x40, 0x55, 0x54, 0x46 });

				if (index != -1)
				{
					// Step 3: Extract the bytes before the found "@UTF" signature
					byte[] newData = new byte[index];
					Array.Copy(fileBytes, 0, newData, 0, index);

					// Step 4: Write the extracted bytes to a new file with the extension ".uasset.h"
					string newFilePath = Path.ChangeExtension(filePath, ".uasset.h");
					File.WriteAllBytes(newFilePath, newData);

					// Step 5: Delete the extracted bytes from the original .uasset file
					byte[] remainingBytes = new byte[fileBytes.Length - index];
					Array.Copy(fileBytes, index, remainingBytes, 0, remainingBytes.Length);
					File.WriteAllBytes(filePath, remainingBytes);

					// Step 6: Rename the original .uasset file to have a new extension ".acb"
					string renamedFilePath = Path.ChangeExtension(filePath, ".acb");
					File.Move(filePath, renamedFilePath);

					// Step 7: Run the original ACB decompilation code
					RunAcbEditor(renamedFilePath);
					UpdateFileList(renamedFilePath);

					MessageBox.Show("Processed .uasset file successfully.");
				}
				else
				{
					MessageBox.Show("The .uasset file does not contain the expected pattern.");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error handling .uasset file: " + ex.Message);
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
