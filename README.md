# ImageQuantization
Problem Definition What is Image Quantization? The idea of color quantization is to reduce the number of colors in a full resolution digital color image (24 bits per pixel) to a smaller set of representative colors called color palette. Reduction should be performed so that the quantized image differs as little as possible from the original image. Algorithmic optimization task is to find such a color palette that the overall distortion is minimized. An example of color quantization is depicted in the following Figure. First, a color palette is found by using clustering algorithm and then the original image values are replaced by their closest values in the palette.

Some Usages

Target different devices: color quantization is critical for displaying images with many colors on devices that can only display a limited number of colors, usually due to memory limitations.
Image compression: by reducing number of bits per pixels without affecting the image view. It’s used as a step in the compression pipeline of most common formats like JPEG and MPEG.
Image segmentation: is the process extracting useful objects from an image. It usually done by assigning a label to every pixel in an image such that pixels with the same label share certain characteristics (e.g. same colors). Examples are shown in the figure below:
