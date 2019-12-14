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

            //my role
            double before = System.Environment.TickCount;
            Operations();
            double after = System.Environment.TickCount;
            double result = after - before;
            result /= 1000;
            time.Text = result.ToString()+" sec";

        }

       
        private void Operations()
        {
            numberofcolor.Text = ImageOperations.Get_Number_of_color(ImageMatrix).ToString();
            MST mst= ImageOperations.MST_Weight(ImageMatrix);
            MST_Sum.Text = mst.Weight.ToString();
            
        }
       
    }
}