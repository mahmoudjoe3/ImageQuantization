using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }


    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }

        public struct color
        {
           public  RGBPixel val;
            public int index;
        };


        
        /// <summary>
        /// get the distinected color in a list of pair 
        /// 1- counter of the color and
        /// 2- the rgb color itself 
        /// from the rgb pixel array to a Dictionary
        /// </summary>
        /// <param name="ImageMatrix"></param>
        /// <returns>Dictionary of distinected color and its number </returns>
        
        private static List<RGBPixel> List_of_Dstinected_Color(RGBPixel[,] ImageMatrix)
        {
            List<RGBPixel> list_of_dstinected_color = new List<RGBPixel>();
            bool[,,] color_state = new bool[256, 256, 256];
            for(int i=0;i<=255; i++)
            {
                for (int j = 0; j <= 255; j++)
                {
                    for (int k = 0; k <= 255; k++)
                    {
                        color_state[i, j, k] = false;
                    }
                }
            }
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);
            
            for (int i = 0; i < Height; i++)
            {
                for (int j = 0; j < Width; j++)

                {
                    int ii = ImageMatrix[i, j].red;
                    int jj = ImageMatrix[i, j].green;
                    int kk = ImageMatrix[i, j].blue;
                    if (color_state[ii,jj,kk]==false)
                    {
                        list_of_dstinected_color.Add(ImageMatrix[i, j]);
                        color_state[ii, jj, kk] = true;
                    }
                }
            }
                return  list_of_dstinected_color;
        }
        private static List<color> list_of_indexed_color(List<RGBPixel> h)
        {
            List<color> index_color = new List<color>();
            for (int i = 0; i < h.Count; i++)
            {
                color c1 = new color();
                c1.index = i;
                c1.val = h[i];
                index_color.Add(c1);
            }
            return index_color;
        }
        public static long Get_Number_of_color(RGBPixel[,] ImageMatrix)
        {
            return List_of_Dstinected_Color(ImageMatrix).Count;
        }
        
        public static MST MST_Weight(RGBPixel[,] ImageMatrix)
        {
            long size;
            List<RGBPixel> color_list = List_of_Dstinected_Color(ImageMatrix);
            List<color> index_color = list_of_indexed_color(color_list);
            List<Node> tree = new List<Node>();
            heap heap = new heap();

            size = color_list.Count;
            bool[] k = new bool[size];
            int[] p = new int[size];
            double[] d = new double[size];
            
            //
            
            for (int i = 1; i < size; i++)
            {
                k[i] = false;
                d[i] = double.MaxValue;
               
            }
            p[0] = -1; d[0] = 0;
            //node
            Node node0 = new Node();
            node0.vertix = 0;
            node0.key = 0;
            heap.insert(node0);

            while (!heap.empty())
            {
                //node
                Node node = heap.extract_Min();
                tree.Add(node);
                int index = node.vertix;
                if (k[index]==true) continue;
                k[index] = true;
                RGBPixel color1 = index_color[index].val;
                int i = 0;

                foreach (var color2 in color_list)
                {
                    if (i != index)
                    {
                        double Red_Diff = (color1.red - color2.red) * (color1.red - color2.red);
                        double Green_Diff = (color1.green - color2.green) * (color1.green - color2.green);
                        double Blue_Diff = (color1.blue - color2.blue) * (color1.blue - color2.blue);
                        double Total_Diff = Math.Sqrt(Red_Diff + Green_Diff + Blue_Diff);

                        if (k[i] == false && Total_Diff < d[i])
                        {
                            d[i] = Total_Diff;
                            p[i] = index;
                            //
                            Node node1 = new Node();
                            node1.key = d[i];
                            node1.vertix = i;
                           // int indexx = i;
                            heap.insert(node1);
                        }
                    }
                        i++;
                    
                }
            }
            double weight = 0;
            for (int i = 0; i < size; i++)
                weight += d[i];
            MST mST = new MST(weight, tree, p);
            return mST;

        }
    }
}