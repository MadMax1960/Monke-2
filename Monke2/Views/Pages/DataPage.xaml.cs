﻿using Microsoft.Win32;
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
				Filter = "uexp Files (*.uexp)|*.uexp|ACB Files (*.acb)|*.acb",
				Title = "Select uexp or ACB File"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				string filePath = openFileDialog.FileName;

				if (filePath.EndsWith(".uexp", StringComparison.OrdinalIgnoreCase))
				{
					// Handle uexp file
					HandleuexpFile(filePath);
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

		private void HandleuexpFile(string filePath)
		{
			try
			{
				// Read the .uexp file as bytes
				byte[] fileBytes = File.ReadAllBytes(filePath);

				// Search for the first occurrence of "@UTF" in the byte array
				int index = FindSequence(fileBytes, new byte[] { 0x40, 0x55, 0x54, 0x46 });

				if (index != -1)
				{
					// Extract the bytes starting from the "@UTF" signature to the end of the file
					byte[] newData = new byte[fileBytes.Length - index];
					Array.Copy(fileBytes, index, newData, 0, newData.Length);

					// Write the extracted bytes to a new file with the extension ".acb"
					string newFilePath = Path.ChangeExtension(filePath, ".acb");
					File.WriteAllBytes(newFilePath, newData);

					// Run the original ACB decompilation code
					RunAcbEditor(newFilePath);
					UpdateFileList(newFilePath);

					MessageBox.Show("Processed .uexp file successfully.");
				}
				else
				{
					MessageBox.Show("The .uexp file does not contain the expected pattern.");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error handling .uexp file: " + ex.Message);
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
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "uexp Files (*.uexp)|*.uexp",
				Title = "Select uexp File"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				string uexpFilePath = openFileDialog.FileName;
				string folderPath = Path.GetDirectoryName(uexpFilePath);
				string folderName = Path.GetFileNameWithoutExtension(uexpFilePath);

				if (Directory.Exists(folderPath))
				{
					// Run AcbEditor.exe with the folder path
					RunAcbEditor(Path.Combine(folderPath, folderName));

					// Find the ACB file with the same name as the uexp file
					string acbFileName = folderName + ".acb";
					string acbFilePath = Path.Combine(folderPath, acbFileName);

					if (File.Exists(acbFilePath))
					{
						// Insert ACB into uexp file
						InsertAcbIntouexp(uexpFilePath, acbFilePath);
					}
					else
					{
						MessageBox.Show($"ACB file '{acbFileName}' not found in the folder.");
					}
				}
				else
				{
					MessageBox.Show($"Folder '{folderName}' not found.");
				}
			}
		}






		private void InsertAcbIntouexp(string uexpFilePath, string acbFilePath)
		{
			try
			{
				byte[] uexpBytes = File.ReadAllBytes(uexpFilePath);
				byte[] acbBytes = File.ReadAllBytes(acbFilePath);

				// Find the index of "@UTF" in the uexp file
				int utfIndex = FindSequence(uexpBytes, new byte[] { 0x40, 0x55, 0x54, 0x46 });

				if (utfIndex != -1)
				{
					// Overwrite bytes in the uexp file with ACB bytes
					Array.Copy(acbBytes, 0, uexpBytes, utfIndex, acbBytes.Length);

					// Write the modified uexp file
					File.WriteAllBytes(uexpFilePath, uexpBytes);

					MessageBox.Show($"ACB file '{Path.GetFileName(acbFilePath)}' inserted into uexp file '{Path.GetFileName(uexpFilePath)}' successfully.");
				}
				else
				{
					MessageBox.Show("Could not find '@UTF' in the uexp file.");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error inserting ACB into uexp file: " + ex.Message);
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
