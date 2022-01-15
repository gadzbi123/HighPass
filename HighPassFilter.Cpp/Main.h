// Project: High Pass Filter for bitMaps.
// Author: Kacper Nitkiewicz, Informatyka, year 3, sem. 5, gr. 5, date: 15.01.2021
// Vesion: 1.0.

// 3x3 mask of filter that will be applied on a image
int* MaskMatrix;

// function apllies Mask to fragment of image [1 pixel], works for GRB colors
unsigned char ApplyMaskToRGB(unsigned char* imageFragment);

// external funcion is called in CS, applies the filter into a whole image
extern "C" __declspec(dllexport) unsigned char* __stdcall ApplyHighPassFilterCpp(unsigned char* inputBitmap, int inputBitmapLength, int inputBitmapWidth, int startIndex, int endIndex);