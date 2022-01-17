// Project: High Pass Filter for bitMaps.
// Author: Kacper Nitkiewicz, Informatyka, year 3, sem. 5, gr. 5, date: 15.01.2021
// Vesion: 1.0.

#include "pch.h"
#include "Main.h"

unsigned char ApplyMaskToRGB(unsigned char* imageFragment)
{
	int pixelValue = 0;

	for (auto y = 0; y < 3; y++)
	{
		for (auto x = 0; x < 3; x++)
		{
			int element = MaskMatrix[x + y * 3] * imageFragment[x + y * 3];

			pixelValue += element;
		}
	}

	if (pixelValue < 0)
	{
		pixelValue = 0;
	}
	else if (pixelValue > 255)
	{
		pixelValue = 255;
	}

	return pixelValue;
}

extern "C" __declspec(dllexport) unsigned char* __stdcall ApplyHighPassFilterCpp(unsigned char* inputBitmap, int inputBitmapLength, int inputBitmapWidth, int startIndex, int endIndex)
{
	MaskMatrix = new int[] { 1, -2, 1, -2, 5, -2, 1, -2, 1}; 



	unsigned char* filteredFragment = new unsigned char[endIndex - startIndex + 1];

	for (auto i = 0; i < endIndex - startIndex + 1; i++)
	{
		filteredFragment[i] = inputBitmap[startIndex + i];
	}

	for (auto i = startIndex; i <= endIndex; i += 3)
	{
		unsigned char* r = new unsigned char[9];
		unsigned char* g = new unsigned char[9];
		unsigned char* b = new unsigned char[9];

		if (!((i < inputBitmapWidth) || (i % inputBitmapWidth == 0) || (i >= inputBitmapLength - inputBitmapWidth) || ((i + 2 + 1) % inputBitmapWidth == 0)))
		{
			for (auto y = 0; y < 3; y++)
			{
				for (auto x = 0; x < 3; x++)
				{
					int index = i + (inputBitmapWidth * (y - 1) + (x - 1) * 3);

					r[x + y * 3] = inputBitmap[index];
					g[x + y * 3] = inputBitmap[index + 1];
					b[x + y * 3] = inputBitmap[index + 2];
				}
			}

			filteredFragment[i - startIndex] = ApplyMaskToRGB(r);
			filteredFragment[i - startIndex + 1] = ApplyMaskToRGB(g);
			filteredFragment[i - startIndex + 2] = ApplyMaskToRGB(b);
		}
	}

	return filteredFragment;
}