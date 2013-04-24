using System;
using System.Drawing;
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

        private readonly Bitmap imgref1 = Resources.f1;
        private readonly Bitmap imgref2 = Resources.f2;
        private readonly Bitmap imgref3 = Resources.f3;
        private readonly Bitmap imgref4 = Resources.f4;


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
            var matrixC = new double[grayorg1.Height,grayorg1.Width];
            var matrixD = new double[grayorg1.Height,grayorg1.Width];

            var matrixUnwOrg = new double[grayorg1.Height,grayorg1.Width];
            var matrixUnwRef = new double[grayorg1.Height,grayorg1.Width];
            var matrixUnwOrgImg = new double[grayorg1.Height,grayorg1.Width];
            var matrixUnwRefImg = new double[grayorg1.Height,grayorg1.Width];


            var conv = new ImageToMatrix(min: 0, max: 255);

            conv.Convert(grayorg1, out matrix1);
            conv.Convert(grayorg2, out matrix2);
            conv.Convert(grayorg3, out matrix3);
            conv.Convert(grayorg4, out matrix4);


            matrixA = matrix4.Subtract(matrix2);
            matrixB = matrix1.Subtract(matrix3);


            // Bu for döngüsünü paralel hale getirebiliriz.
            for (int clm = 0; clm < grayorg1.Width; clm++)
            {
                for (int row = 0; row < grayorg1.Height; row++)
                {
                    matrixC[row, clm] = Math.Atan2(matrixA[row, clm], matrixB[row, clm]);
                }
            }

            matrixUnwOrg = matrixC;
            matrixUnwOrgImg = UnwrapImage(matrixUnwOrg);
            // MatrixC matlab da phase_map görüntüsüne denk 
            // bu durumda MatrixC yi resme dönüştürmek lazım !
            Bitmap phase_map;

            var conMat2Img = new MatrixToImage(min: 0, max: 255);

            conMat2Img.Convert(matrixC, out phase_map);

            pbFlipUp.Image = phase_map;

            conv.Convert(grayref1, out matrix1);
            conv.Convert(grayref2, out matrix2);
            conv.Convert(grayref3, out matrix3);
            conv.Convert(grayref4, out matrix4);

            matrixA = matrix4.Subtract(matrix2);
            matrixB = matrix1.Subtract(matrix3);


            for (int clm = 0; clm < grayorg1.Width; clm++)
            {
                for (int row = 0; row < grayorg1.Height; row++)
                {
                    matrixD[row, clm] = Math.Atan2(matrixA[row, clm], matrixB[row, clm]);
                }
            }


            matrixUnwRef = matrixD;
            matrixUnwRefImg = UnwrapImage(matrixUnwRef);

            double[,] realObjectPlane = matrixUnwOrgImg.Subtract(matrixUnwRefImg);

            #region Height map elde ediliyor

            // todo: Paralel çevir
            // todo: MatLab dan biraz faklı sonuçlar elde ettim kontro et !


            var Height_Map = new double[768,1024];

            double fringeSpacing = 164;
            double Scanning_area_vert = 17;

            fringeSpacing *= Scanning_area_vert/768;

            for (int i = 0; i < 1024; i++)
            {
                for (int j = 0; j < 768; j++)
                {
                    Height_Map[j, i] = -fringeSpacing*realObjectPlane[j, i]/(2*Math.PI*Math.Tan(Math.PI/36));
                }
            }

            #endregion

            var smoothFiltered = new double[768,1024];

            smoothFiltered = AverageFilter(Height_Map);

            var reverse = new double[768,1024];

            for (int i = 0; i < 1024 - 1; i++)
            {
                for (int j = 0; j < 768 - 1; j++)
                {
                    reverse[j, i] = 126;
                }
            }

            var clmVector = new double[768];

            reverse.Subtract(smoothFiltered);


            Bitmap phase_map_ref;
            Bitmap YukseklikResmi;

            var conMat2Imgref = new MatrixToImage(0.0, 255.0);

            conMat2Img.Convert(reverse, out YukseklikResmi);

            pbRef.Image = YukseklikResmi;

            conv.Convert(YukseklikResmi, out matrixUnwOrg);


            //DataGridBox.Show(smoothFiltered, nonBlocking: true)
            //           .SetAutoSizeColumns(DataGridViewAutoSizeColumnsMode.Fill)
            //           .SetAutoSizeRows(DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders)
            //           .SetDefaultFontSize(7)
            //           .WaitForClose();
        }


        private double[,] AverageFilter(double[,] Height_Map)
        {
            var rowVector = new double[770];
            var columnVector = new double[1024];
            Height_Map = Height_Map.InsertRow(columnVector, 0);
            Height_Map = Height_Map.InsertRow(columnVector, 769);
            Height_Map = Height_Map.InsertColumn(rowVector, 0);
            Height_Map = Height_Map.InsertColumn(rowVector, 1025);


            var result = new double[768,1024];
            for (int m = 1; m < Height_Map.GetColumn(0).Length - 2; m++)
            {
                for (int n = 1; n < Height_Map.GetRow(0).Length - 2; n++)
                {
                    result[m - 1, n - 1] = (Height_Map[m - 1, n - 1] + Height_Map[m - 1, n] + Height_Map[m - 1, n + 1] +
                                            Height_Map[m, n - 1] + Height_Map[m, n] + Height_Map[m, n + 1] +
                                            Height_Map[m + 1, n - 1] + Height_Map[m + 1, n] + Height_Map[m + 1, n + 1])/
                                           9;
                }
            }

            return result;
        }


        private double[,] UnwrapImage(double[,] matrixUnwOrg)
        {
            var unwPhaseImg = new double[768,1024];
            var yeni = new double[768,1024];

            double K = 0;
            for (int j = 0; j < 1024; j++)
            {
                for (int i = 1; i < 768; i++)
                {
                    double difference = matrixUnwOrg[i, j] - matrixUnwOrg[i - 1, j];

                    if (difference > Math.PI)
                    {
                        K = K - 2*Math.PI;
                    }
                    else if (difference < -Math.PI)
                    {
                        K = K + 2*Math.PI;
                    }

                    yeni[i, j] = K;
                }
            }

            unwPhaseImg = matrixUnwOrg.Add(yeni);

            return unwPhaseImg;
        }

        protected double[,] UnwrapImage(Bitmap phaseMap)
        {
            double[,] phase_map;
            var unwPhaseImg = new double[phaseMap.Height,phaseMap.Width];


            var conv = new ImageToMatrix(min: 0, max: 255);

            conv.Convert(phaseMap, out phase_map);

            var yeni = new double[phaseMap.Height,phaseMap.Width];

            for (int i = 0; i < phaseMap.Height; i++)
            {
                for (int j = 1; j < phaseMap.Width; j++)
                {
                    double K = 0;

                    double difference = phase_map[i, j] - phase_map[i, j - 1];

                    if (difference > Math.PI)
                    {
                        K = K - 2*Math.PI;
                    }
                    else if (difference < Math.PI)
                    {
                        K = K + 2*Math.PI;
                    }

                    yeni[i, j] = K;
                }
            }


            unwPhaseImg = phase_map.Add(yeni);

            return unwPhaseImg;
        }
    }
}