
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace HighPassFilter.CS
{
	public partial class MainWindow : Window
	{
		private string filePath;
		private byte[] inputBitmap;
		private byte[] outputBitmap;

		public MainWindow()
		{
			InitializeComponent();

			CppAlgorithmBox.IsChecked = true;
		}

		private void AsmAlgorithmBox_Unchecked(object sender, RoutedEventArgs e)
		{
			CppAlgorithmBox.IsChecked = true;
		}

		private void CsAlgorithmBox_Unchecked(object sender, RoutedEventArgs e)
		{
			AsmAlgorithmBox.IsChecked = true;
		}

		private void AsmAlgorithmBox_Checked(object sender, RoutedEventArgs e)
		{
			CppAlgorithmBox.IsChecked = false;
		}

		private void CsAlgorithmBox_Checked(object sender, RoutedEventArgs e)
		{
			AsmAlgorithmBox.IsChecked = false;
		}

		private void SaveBitmapButton_Click(object sender, RoutedEventArgs e)
		{
			if (outputBitmap != null)
			{
				File.WriteAllBytes("../../../../OutputFiltered.bmp", outputBitmap);
			}
		}

		//Adding path of file to program
		private void BrowseForBitmapButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dialog = new OpenFileDialog()
			{
				Filter = "Bitmap files (*.bmp)|*.bmp|All files (*.*)|*.*"
			};

			if (dialog.ShowDialog() == true)
			{
				string fileName = dialog.FileName;

				if (Path.GetExtension(fileName) != ".bmp")
				{
					return;
				}

				BitmapPanel.Children.Clear();
				ExecutionTimeBlock.Text = "";

				filePath = fileName;

				byte[] bitmapArrayFromFile = File.ReadAllBytes(filePath);
				inputBitmap = bitmapArrayFromFile;
			}

		}

		//After fliter click
		private void FilterBitmapButton_Click(object sender, RoutedEventArgs e)
		{
			if (inputBitmap != null)
			{
				Stopwatch stopwatch = new Stopwatch();
				int threadCount = int.Parse(ThreadCountBox.Text);
				bool asmAlgorithm = AsmAlgorithmBox.IsChecked.Value;

				if (threadCount <= 0)
				{
					return;
				}

				//take first 54 bytes of bmp 
				byte[] bitmapHeader = inputBitmap.Take(54).ToArray();

				byte[] bitmapWithoutHeader = new byte[inputBitmap.Length - 54];

				for (int i = 54; i < inputBitmap.Length; i++)
				{
					bitmapWithoutHeader[i - 54] = inputBitmap[i];
				}
				
				int bitmapWidth = BitConverter.ToInt32(bitmapHeader.Skip(18).Take(4).ToArray(), 0) * 3;

				stopwatch.Start();

				byte[] result = null;

				if (asmAlgorithm)
				{
					result = CallingFilter.Asm(bitmapWithoutHeader, bitmapWidth, threadCount).Result;
				}
				else
				{
					result = CallingFilter.Cpp(bitmapWithoutHeader, bitmapWidth, threadCount).Result;
				}

				stopwatch.Stop();

				string executionTime = "Execution time: " + stopwatch.Elapsed.ToString(@"mm\:ss\.fff");
				ExecutionTimeBlock.Text = executionTime;

				byte[] outputBitmapComplete = new byte[inputBitmap.Length];
				int x = 0;

				for (x = 0; x < 54; x++)
				{
					outputBitmapComplete[x] = bitmapHeader[x];
				}

				for (x = 54; x < outputBitmapComplete.Length; x++)
				{
					outputBitmapComplete[x] = result[x - 54];
				}

				BitmapImage bitmapImage = ConvertBitmapBytesToImageSource(outputBitmapComplete);
				Image image = new Image()
				{
					Source = bitmapImage,
					Width = 400,
					Height = 400
				};

				BitmapPanel.Children.Clear();
				BitmapPanel.Children.Add(image);

				outputBitmap = outputBitmapComplete;
			}
		}
		
		//Converts the bytes that were filtered to Image that will be seen in the app
		private static BitmapImage ConvertBitmapBytesToImageSource(byte[] bitmapBytes)
		{
			if (bitmapBytes == null)
			{
				return null;
			}

			byte[] copy = new byte[bitmapBytes.Length];

			for (int i = 0; i < bitmapBytes.Length; i++)
			{
				copy[i] = bitmapBytes[i];
			}

			BitmapImage bmp = new BitmapImage();
			MemoryStream ms = new MemoryStream(bitmapBytes);
			bmp.BeginInit();
			bmp.StreamSource = ms;
			bmp.CacheOption = BitmapCacheOption.OnLoad;
			bmp.EndInit();

			return bmp;
		}
	}
}
