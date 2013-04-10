using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using AForge.Imaging.Filters;
using Accord.Imaging.Converters;
using Accord.Math;
using PhaseShiftUnwrap.Properties;

namespace PhaseShiftUnwrap
{
    public partial class Form1 : Form
    {
        private readonly Bitmap imgOrg1 = Resources.o1;
        private readonly Bitmap imgOrg2 = Resources.o2;
        private readonly Bitmap imgOrg3 = Resources.o3;
        private readonly Bitmap imgOrg4 = Resources.o4;
        //private readonly Bitmap imgOrg5 = Resources.cinque_terre_small;

        private Bitmap imgref1 = Resources.f1;
        private Bitmap imgref2 = Resources.f2;
        private Bitmap imgref3 = Resources.f3;
        private Bitmap imgref4 = Resources.f4;


        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void processTSMI_Click(object sender, EventArgs e)
        {
            
            // resimler ters çevrildi
            imgOrg1.RotateFlip(RotateFlipType.Rotate180FlipX);
            imgOrg2.RotateFlip(RotateFlipType.Rotate180FlipX);
            imgOrg3.RotateFlip(RotateFlipType.Rotate180FlipX);
            imgOrg4.RotateFlip(RotateFlipType.Rotate180FlipX);
            imgref1.RotateFlip(RotateFlipType.Rotate180FlipX);
            imgref2.RotateFlip(RotateFlipType.Rotate180FlipX);
            imgref3.RotateFlip(RotateFlipType.Rotate180FlipX);
            imgref4.RotateFlip(RotateFlipType.Rotate180FlipX);
            
            var filter = new Grayscale(0.2125, 0.7154, 0.0721);

            Bitmap grayorg1 = filter.Apply(imgOrg1);
            Bitmap grayorg2 = filter.Apply(imgOrg2);
            Bitmap grayorg3 = filter.Apply(imgOrg3);
            Bitmap grayorg4 = filter.Apply(imgOrg4);

            Bitmap grayref1 = filter.Apply(imgref1);
            Bitmap grayref2 = filter.Apply(imgref2);
            Bitmap grayref3 = filter.Apply(imgref3);
            Bitmap grayref4 = filter.Apply(imgref4);

            double[,] matrix1;
            double[,] matrix2;
            double[,] matrix3;
            double[,] matrix4;
            double[,] matrixA;
            double[,] matrixB;
            
            
            var conv = new ImageToMatrix(min: 0, max: 255);
            
            conv.Convert(grayorg1, out matrix1);
            conv.Convert(grayorg2, out matrix2);
            conv.Convert(grayorg3, out matrix3);
            conv.Convert(grayorg4, out matrix4);


            matrixA = Matrix.Subtract(matrix4, matrix2);
            matrixB = Matrix.Subtract(matrix1, matrix3);



            //double[,] matrix1;
            //double[,] matrix2;
            //conv.Convert(flipUp1, out matrix1);
            //conv.Convert(flipUp3, out matrix2);
            //double[,] matrixA = Matrix.Subtract(matrix2, matrix1);

            //flipUp.RotateFlip(RotateFlipType.Rotate180FlipX);
            //pbFlipUp.Image = flipUp;
        }
    }
}