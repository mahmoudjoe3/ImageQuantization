using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ImageQuantization
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;

        private void btnOpen_Click(object sender, EventArgs e)
        {
            
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Open the browsed image and display it
                string OpenedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);
            }
            txtWidth.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
            txtHeight.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
           mailestoneOne();
        }
        MST mst;
        private void mailestoneOne()
        {
            numberofcolor.Text = ImageOperations.Get_Number_of_color(ImageMatrix).ToString();
            mst = ImageOperations.MST_Weight(ImageMatrix);
            MST_Sum.Text = mst.Weight.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double before = System.Environment.TickCount;

            ImageMatrix = ImageOperations.ReduceImageColor(ImageMatrix,Convert.ToInt32(clusterTxT.Text));
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);

            double after = System.Environment.TickCount;
            double result = after - before;
            msec.Text = result.ToString() + " M-Sec";
            result /= 1000;
            sec.Text = result.ToString() + " Sec";
        }
       

        
        private void button1_Click(object sender, EventArgs e)
        {
            Image image = pictureBox2.Image;
            SaveImageCapture(image);
        }

        public static void SaveImageCapture(System.Drawing.Image image)
        {
            SaveFileDialog s = new SaveFileDialog();
            s.FileName = "Image";
            s.DefaultExt = ".Jpg";
            s.Filter = "Image (.jpg)|*.jpg";
            s.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            s.RestoreDirectory = true;
            if (s.ShowDialog() == DialogResult.OK)
            {
                string filename = s.FileName;
                using (System.IO.FileStream fstream = new System.IO.FileStream(filename, System.IO.FileMode.Create))
                {
                    image.Save(fstream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    fstream.Close();
                }
            }
        }

       
    }
}