// Project: High Pass Filter for bitMaps.
// Author: Kacper Nitkiewicz, Informatyka, year 3, sem. 5, gr. 5, date: 15.01.2021
// Vesion: 1.0.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HighPassFilter.CS
{
	public class CallingFilter
	{
		[DllImport(@"C:\!Programing\HighPassFilter\x64\Release\HighPassFilter.Cpp.dll")]
		public static extern IntPtr ApplyHighPassFilterCpp(IntPtr inputBitmap, int inputBitmapLength, int inputBitmapWidth, int startIndex, int endIndex);

		private static volatile List<(int Id, byte[] Part)> Results = new List<(int, byte[])>();
		private static object Lock = new object();

		//tworzenie watków do Cpp i przekazanie danych
		public static async Task<byte[]> Cpp(byte[] inputBitmap, int inputBitmapWidth, int threadCount)
		{
			List<Task> listOfThreads = new List<Task>();

			Results.Clear();

			int index = 0;
			int bytesPerPart = inputBitmap.Length / threadCount;

			bytesPerPart -= bytesPerPart % 3;

			for (int i = 0; i < threadCount; i++)
			{
				int startIndex = index;
				int endIndex = startIndex + bytesPerPart - 1;

				if (i + 1 == threadCount)
				{
					endIndex = inputBitmap.Length - 1;
				}

				index = endIndex + 1;

				byte[] filteredFragment = new byte[endIndex - startIndex + 1];

				for (int x = 0; x < endIndex - startIndex + 1; x++)
				{
					filteredFragment[x] = inputBitmap[startIndex + x];
				}

				int taskId = i;

				Task task = Task.Run(() =>
				{
					byte[] output = null;

					unsafe
					{
						fixed (byte* pointerToInputArray = &inputBitmap[0])
						{
							IntPtr inputBitmapPtr = new IntPtr(pointerToInputArray);

							IntPtr result = ApplyHighPassFilterCpp(inputBitmapPtr, inputBitmap.Length, inputBitmapWidth, startIndex, endIndex);

							byte* resultPointer = (byte*)result;

							output = new byte[endIndex - startIndex + 1];

							for (int i = 0; i < output.Length; i++)
							{
								output[i] = resultPointer[i];
							}
						}
					}

					lock (Lock)
					{
						Results.Add((taskId, output));
					}
				});

				listOfThreads.Add(task);

			}

			await Task.WhenAll(listOfThreads).ConfigureAwait(false);

			byte[] output = new byte[inputBitmap.Length]; ;

			index = 0;

			Results = Results.OrderBy(x => x.Id).ToList<(int, byte[])>();

			for (int i = 0; i < Results.Count; i++)
			{
				var resultPart = Results[i];

				for (int j = 0; j < resultPart.Part.Length; j++)
				{
					output[index++] = resultPart.Part[j];
				}
			}

			return output;
		}

		[DllImport(@"C:\!Programing\HighPassFilter\x64\Release\HighPassFilter.Asm.dll")]
		public static extern IntPtr ApplyHighPassFilterAsm(IntPtr bitmapBytes, int bitmapBytesLength, int bitmapWidth, int startIndex, int endIndex, IntPtr filteredFragment, IntPtr helperR, IntPtr helperG, IntPtr helperB);

		public static async Task<byte[]> Asm(byte[] inputBitmap, int inputBitmapWidth, int threadCount)
		{
			List<Task> listOfTasks = new List<Task>();

			Results.Clear();

			int index = 0;
			int bytesPerPart = inputBitmap.Length / threadCount;

			bytesPerPart -= bytesPerPart % 3;

			for (int i = 0; i < threadCount; i++)
			{
				int startIndex = index;
				int endIndex = startIndex + bytesPerPart - 1;

				if (i + 1 == threadCount)
				{
					endIndex = inputBitmap.Length - 1;
				}

				index = endIndex + 1;

				byte[] filteredFragment = new byte[endIndex - startIndex + 1];

				for (int x = 0; x < endIndex - startIndex + 1; x++)
				{
					filteredFragment[x] = inputBitmap[startIndex + x];
				}

				int taskId = i;

				Task task = Task.Run(() =>
				{
					byte[] output = null;
					byte[] helperR = new byte[9];
					byte[] helperG = new byte[9];
					byte[] helperB = new byte[9];

					//* nie da sie
					unsafe
					{
						//fixed zostawia te wsklazniki w tym samym adresie
						fixed (byte* pointerToInputArray = &inputBitmap[0])
						fixed (byte* pointerToFilteredFragmentArray = &filteredFragment[0])
						fixed (byte* pointerToHelperR = &helperR[0])
						fixed (byte* pointerToHelperG = &helperG[0])
						fixed (byte* pointerToHelperB = &helperB[0])
						{
							IntPtr inputBitmapPtr = new IntPtr(pointerToInputArray);
							IntPtr filteredFragmentPtr = new IntPtr(pointerToFilteredFragmentArray);

							IntPtr helperRPtr = new IntPtr(pointerToHelperR);
							IntPtr helperGPtr = new IntPtr(pointerToHelperG);
							IntPtr helperBPtr = new IntPtr(pointerToHelperB);

							IntPtr result = ApplyHighPassFilterAsm(inputBitmapPtr, inputBitmap.Length, inputBitmapWidth, startIndex, endIndex, filteredFragmentPtr, helperRPtr, helperGPtr, helperBPtr);

							byte* resultPointer = (byte*)result;

							output = new byte[endIndex - startIndex + 1];

							for (int i = 0; i < output.Length; i++)
							{
								output[i] = resultPointer[i];
							}
						}
					}

					lock (Lock)
					{
						Results.Add((taskId, output));
					}
				});

				listOfTasks.Add(task);
			}

			await Task.WhenAll(listOfTasks).ConfigureAwait(false);

			byte[] output = new byte[inputBitmap.Length];

			index = 0;

			Results = Results.OrderBy(x => x.Id).ToList<(int, byte[])>();

			for (int i = 0; i < Results.Count; i++)
			{
				var resultPart = Results[i];

				for (int j = 0; j < resultPart.Part.Length; j++)
				{
					output[index++] = resultPart.Part[j];
				}
			}

			return output;
		}
	}
}
