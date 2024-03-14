using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;

namespace Monke2.ViewModels.Pages
{
	public partial class DashboardViewModel : ObservableObject
	{

		// Properties for TextBox data binding
		[ObservableProperty]
		private string numberInput1;

		[ObservableProperty]
		private string numberInput2;

		private string _acbNameInput;

		public string ACBNameInput
		{
			get => _acbNameInput;
			set => SetProperty(ref _acbNameInput, value);
		}

		public ICommand HCAConversionCommand { get; }
		public ICommand EnterACBNameCommand { get; }

		public ICommand CreateConfigCommand { get; }

		[ObservableProperty]
		private string _selectedFolderPath;

		[ObservableProperty]
		private string _selectedFilePath;

		[RelayCommand]
		private void SelectFolder()
		{
			var dialog = new OpenFileDialog
			{
				ValidateNames = false,
				CheckFileExists = false,
				CheckPathExists = true,
				FileName = "Folder Selection."
			};

			if (dialog.ShowDialog() == true)
			{
				SelectedFolderPath = System.IO.Path.GetDirectoryName(dialog.FileName);
			}
		}

		private double _conversionProgress;
		public double ConversionProgress
		{
			get => _conversionProgress;
			set => SetProperty(ref _conversionProgress, value);
		}

		[RelayCommand]
		private void SelectFile()
		{
			var dialog = new OpenFileDialog();
			if (dialog.ShowDialog() == true)
			{
				SelectedFilePath = dialog.FileName;
			}
		}

		// ICommand for Batch HCA Conversion
		public ICommand BatchHCAConversionCommand { get; }

		public DashboardViewModel()
		{
			// Initialize BatchHCAConversionCommand to a RelayCommand or DelegateCommand
			BatchHCAConversionCommand = new RelayCommand(BatchHCAConversion);
			// Initialize HCAConversionCommand
			HCAConversionCommand = new RelayCommand(HCAConversion);
			// Initialize EnterACBNameCommand to a RelayCommand or DelegateCommand
			EnterACBNameCommand = new RelayCommand(EnterACBName);
			CreateConfigCommand = new RelayCommand(CreateConfig);
		}

		private async void BatchHCAConversion()
		{
			await Task.Run(() =>
			{
				string selectedFolderPath = SelectedFolderPath;
				string[] wavFiles = Directory.GetFiles(selectedFolderPath, "*.wav", SearchOption.AllDirectories);
				int totalFiles = wavFiles.Length;
				string vgaudiocliPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vgaudiocli.exe");

				for (int i = 0; i < totalFiles; i++)
				{
					string wavFile = wavFiles[i];
					string hcaFileName = Path.ChangeExtension(wavFile, ".hca");
					string arguments = $"\"{wavFile}\" \"{hcaFileName}\" --keycode 11918920";

					ProcessStartInfo startInfo = new ProcessStartInfo
					{
						FileName = vgaudiocliPath,
						Arguments = arguments,
						RedirectStandardOutput = true,
						UseShellExecute = false,
						CreateNoWindow = true
					};

					try
					{
						using (Process process = Process.Start(startInfo))
						{
							process.WaitForExit();
						}
					}
					catch (Exception ex)
					{
					}

					// Update UI thread with the progress
					Application.Current.Dispatcher.Invoke(() =>
					{
						ConversionProgress = (i + 1) * 100.0 / totalFiles;
					});
				}

				// After all files are converted, prompt user for ACB name
				Application.Current.Dispatcher.Invoke(() =>
				{
					EnterACBName();
				});
			});

			MessageBox.Show("Monke has made your HCAs.");
		}

		private void EnterACBName()
		{
			// Prompt user for ACB name
			string acbName = Microsoft.VisualBasic.Interaction.InputBox("Enter ACB Name:", "ACB Name", "");

			// Use the entered ACB name as needed
			if (!string.IsNullOrEmpty(acbName))
			{
				// Do something with the entered ACB name
				MessageBox.Show($"ACB Name entered: {acbName}");

				// Call CreateConfig method after ACB name is entered
				ACBNameInput = acbName; // Set ACBNameInput property to bind to the input box
				CreateConfig(); // Call CreateConfig method here
			}
			else
			{
				MessageBox.Show("ACB Name not entered.");
			}
		}


		private void CreateConfig()
		{
			//MessageBox.Show($"hi");

			if (!string.IsNullOrEmpty(ACBNameInput))
			{
				string folderPath = SelectedFolderPath;
				string configFileName = "config.yaml";
				string configFilePath = Path.Combine(folderPath, configFileName);

				try
				{
					// Create the content for the config.yml file
					string configContent = $"acb_name: {ACBNameInput}{Environment.NewLine}" +
										   $"player_id: -1{Environment.NewLine}" +
										   $"category_ids: [2, 9, 15]{Environment.NewLine}" +
										   $"volume: 1";

					// Write the content to the config.yml file
					File.WriteAllText(configFilePath, configContent);

					//MessageBox.Show($"config.yml created successfully at: {configFilePath}");
				}
				catch (Exception ex)
				{
					MessageBox.Show("Error creating config.yml: " + ex.Message);
				}
			}
			else
			{
				MessageBox.Show("Please enter an ACB name.");
			}
		}



		private void HCAConversion()
		{
			if (!string.IsNullOrEmpty(SelectedFilePath))
			{
				string exePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "VGAudioCli.exe");
				string outputFileName = Path.ChangeExtension(SelectedFilePath, ".hca");

				// Build the command arguments
				string arguments = $"\"{SelectedFilePath}\" \"{outputFileName}\"";

				// Check if both text boxes have numbers and append the loop parameters
				if (int.TryParse(NumberInput1, out int loopStart) && int.TryParse(NumberInput2, out int loopEnd))
				{
					arguments += $" -l {loopStart}-{loopEnd} --keycode 11918920";
				}

				RunVGAudioCli(exePath, arguments);
			}
			else
			{
				// Handle the case where no file is selected
				MessageBox.Show("Please select a file first.");
			}
		}

		private void RunVGAudioCli(string exePath, string arguments)
		{
			try
			{
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = exePath,
					Arguments = arguments,
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				};

				using (Process process = Process.Start(startInfo))
				{
					process.WaitForExit();
				}

				// Optionally, handle the process completion or output here
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error in processing: " + ex.Message);
			}
		}
	}
}
