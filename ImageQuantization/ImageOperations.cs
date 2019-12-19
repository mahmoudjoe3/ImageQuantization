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
    public struct Color
    {
        public RGBPixel val;
        public int index;
    };

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
            for (int i = 0; i <= 255; i++)
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
                    if (color_state[ii, jj, kk] == false)
                    {
                        list_of_dstinected_color.Add(ImageMatrix[i, j]);
                        color_state[ii, jj, kk] = true;
                    }
                }
            }
            return list_of_dstinected_color;
        }
        private static List<Color> list_of_indexed_color(List<RGBPixel> h)
        {
            List<Color> index_color = new List<Color>();
            for (int i = 0; i < h.Count; i++)
            {
                Color c1 = new Color();
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
            List<Color> index_color = list_of_indexed_color(color_list);
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
            node0.index = 0;
            node0.weight = 0;
            heap.insert(node0);

            while (!heap.empty())
            {
                //node
                Node node = heap.extract_Min();

                int index = node.index;
                if (k[index] == true) continue;
                k[index] = true;
                RGBPixel color1 = index_color[index].val;
                int i = 0;
                tree.Add(node);
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
                            node1.weight = d[i];
                            node1.index = i;
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
            MST mST = new MST(weight, tree, p, index_color);
            return mST;

        }

        public static RGBPixelD[] Extract_color_palette(MST mst, int Num_of_clusters)
        {
            List<Node> tree = mst.tree;
            int[] parent = mst.parent;
            int[] roots = new int[Num_of_clusters];
            RGBPixelD[] Color_palette = new RGBPixelD[Num_of_clusters];
            for (int l = 0; l < Num_of_clusters - 1; l++)  //E(K)
            {
                int i = 0;
                int index = 0;
                double W = double.MinValue;
                foreach (Node color in tree)   //E(D)
                {
                    if (W < color.weight)
                    {
                        W = color.weight;
                        index = i;
                    }
                    i++;
                }
                tree[index].weight = 0;
                parent[index] = -1;
                roots[l] = index;
            }                                        //E(D*K)
            // clusters of the tree
            MST clustered_mst = new MST(mst.Weight, tree, parent, mst.index_color);
            for (int r = 0; r < roots.Length; r++)  //E(K*V^2)
                Color_palette[r] = avgcolor_BFS(clustered_mst, roots[r]);

            return Color_palette;
        }


        public static RGBPixelD avgcolor_BFS(MST mst, int Pindex)//E(V^2)
        {
            List<Node> tree = mst.tree;
            int[] parent = mst.parent;
            int count = 1;
            RGBPixelD avg = new RGBPixelD();
            Queue<int> Q = new Queue<int>();
            Q.Enqueue(Pindex);
            while (Q.Count != 0) //E(V)
            {
                int root = tree[Q.Dequeue()].index;
                avg.red += mst.index_color[root].val.red;
                avg.blue += mst.index_color[root].val.blue;
                avg.green += mst.index_color[root].val.green;
                for (int r = 0; r < parent.Length; r++) //E(V)
                {
                    if (root == parent[r])
                    {
                        Q.Enqueue(r);
                        count++;
                    }
                }
            }
            avg.red /= count;
            avg.blue /= count;
            avg.green /= count;
            return avg;
        }


        public static RGBPixelD avgcolor_DFS(MST mst, int Pindex, int oldindex, RGBPixelD avg, int count)
        {
            List<Node> tree = mst.tree;
            int[] parent = mst.parent;
            int root = tree[Pindex].index;
            bool found = false;
            avg.red += mst.index_color[root].val.red;
            avg.blue += mst.index_color[root].val.blue;
            avg.green += mst.index_color[root].val.green;
            for (int r = 0; r < parent.Length; r++)
            {
                if (root == parent[r])
                {
                    oldindex = Pindex;
                    Pindex = r;
                    parent[Pindex] = -2;
                    found = true;
                    return avgcolor_DFS(mst, Pindex, oldindex, avg, ++count);
                }
            }
            if (found == false)
            {
                for (int r = 0; r < parent.Length; r++)
                {
                    if (tree[oldindex].index == parent[r])
                    {
                        avg.red -= mst.index_color[tree[oldindex].index].val.red;
                        avg.blue -= mst.index_color[tree[oldindex].index].val.blue;
                        avg.green -= mst.index_color[tree[oldindex].index].val.green;
                        return avgcolor_DFS(mst, oldindex, Pindex, avg, count);
                    }
                }

            }
            avg.red /= count;
            avg.blue /= count;
            avg.green /= count;
            return avg;


        }
        public static RGBPixel[,] Quntization(RGBPixel[,] ImageMatrix, RGBPixelD[] pallete)
        {
            int width = ImageMatrix.GetLength(1);
            int height = ImageMatrix.GetLength(0);
            RGBPixel[,] Image = new RGBPixel[height, width];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double min = double.MaxValue;
                    byte r = 0;
                    byte g = 0;
                    byte b = 0;
                    RGBPixel col = new RGBPixel();
                    for (int p = 0; p < pallete.Length; p++)
                    {
                        r = (byte)pallete[p].red;
                        g = (byte)pallete[p].green;
                        b = (byte)pallete[p].blue;
                        byte rr = ImageMatrix[i, j].red;
                        byte gg = ImageMatrix[i, j].green;
                        byte bb = ImageMatrix[i, j].blue;
                        double dis = Math.Sqrt(Math.Pow(r - rr, 2) + Math.Pow(g - gg, 2) + Math.Pow(b - bb, 2));


                        if (dis < min)
                        {
                            min = dis;
                            col.red = (byte)pallete[p].red;
                            col.green = (byte)pallete[p].green;
                            col.blue = (byte)pallete[p].blue;
                        }

                    }
                    Image[i, j].red = col.red;
                    Image[i, j].green = col.green;
                    Image[i, j].blue = col.blue;
                }
            }

            return Image;
        }
    }
}