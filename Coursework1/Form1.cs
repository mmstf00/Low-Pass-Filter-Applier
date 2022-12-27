using System.Collections;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace Coursework1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
        }

        public const string vbNewLine = "";
        OpenFileDialog open = new OpenFileDialog();
        Bitmap CurrentBitmap = new Bitmap(5, 5);
        int CurrentX = 0;
        int CurrentY = 0;
        int ImageHeight = 0;
        int ImageWidth = 0;
        int ImageMaxGreyValue = 255;
        String CurrentFile = "";
        String CurrentFilePath = "";
        String ImageType = "";
        Bitmap Im;
        Bitmap filteredImage;

        private void openToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            open.Filter = "All Supported types |*.jpg;*.jpeg;*.gif;*.bmp;*.pgm|JPEG (*.jpg;*.jpeg)|*.jpg; *.jpeg;|GIF(*.GIF) | *.gif | Bitmap files(*.bmp) | *.bmp | PGM files(*.pgm) | *.pgm";
            if (open.ShowDialog() == DialogResult.OK)
            {
                // проверка дали файла, коjто изкаме да отвориме е с разширение PGM
                if (open.FileName.ToUpper().EndsWith(".PGM"))
                    openPGMfile();
                else
                {
                    // визуализация на изображението
                    pictureBox1.Image = new Bitmap(open.FileName);

                }
            }
        }


        // отваряне на PGM феормат
        private void openPGMfile()
        {
            CurrentFilePath = open.FileName;
            StreamReader ImageStreamReader = new StreamReader(open.FileName);
            String LineBuffer;
            String[] ExtractDimension;
            PurgeGlobalData();
            LineBuffer = "#";
            // определяне на типа на файла
            do
            {
                LineBuffer = ImageStreamReader.ReadLine();
            } while (LineBuffer.StartsWith("#"));
            // проверка за валидноста на файла
            if (LineBuffer.StartsWith("P2"))
            {
                ImageType = "P2";
            }
            else
            {
                ImageStreamReader.Close();
                MessageBox.Show("The image is not the write foramt or is corrupt");
                PurgeGlobalData();


            }
            LineBuffer = "#";
            do
            {
                LineBuffer = ImageStreamReader.ReadLine();
            } while (LineBuffer.StartsWith("#"));
            ExtractDimension = LineBuffer.Split(' ');
            ImageHeight = int.Parse(ExtractDimension[1]);
            ImageWidth = int.Parse(ExtractDimension[0]);
            CurrentBitmap = new Bitmap(ImageWidth, ImageWidth);
            // записване на стойностите на фаила в цтринга LineBufer
            if (ImageType == "P2")
            {
                LineBuffer = "#";
                do
                {
                    LineBuffer = ImageStreamReader.ReadLine();
                } while (LineBuffer.StartsWith("#"));
                ImageMaxGreyValue = int.Parse(LineBuffer);
            }
            CurrentFile = ImageStreamReader.ReadToEnd();
            CurrentFile = CurrentFile.Replace("\r", " ");
            CurrentFile = CurrentFile.Replace("\n", " ");
            ImageStreamReader.Close();
            DrawToCanvas();
        }
        public string Chr(int p_intByte)
        {
            byte[] bytBuffer = BitConverter.GetBytes(p_intByte);
            return Encoding.Unicode.GetString(bytBuffer);
        }
        // изобразяване на фаила
        public void DrawToCanvas()
        {
            try
            {
                String NewString = CurrentFile.Replace(Chr(13), " ");
                String[] ColorArray = NewString.Split(' ');
                ArrayList ColorArrayFiltered = new ArrayList();
                int Counter = 0;
                int NewCounter = 0;
                do
                {
                    if (ColorArray[NewCounter].ToString().Length != 0)
                    {
                        ColorArrayFiltered.Add(ColorArray[NewCounter]);
                    }
                    NewCounter += 1;
                } while (NewCounter != ColorArray.Length);
                if (ImageType == "P2")
                {
                    do
                    {
                        if (CurrentX == CurrentBitmap.Width)
                        {
                            CurrentX = 0;
                            CurrentY += 1;
                        }
                        if (CurrentY == CurrentBitmap.Height)
                        {
                            break;
                        }
                        CurrentBitmap.SetPixel(CurrentX, CurrentY, Color.FromArgb(255,
                       int.Parse(ColorArrayFiltered[Counter].ToString()), int.Parse(ColorArrayFiltered[Counter].ToString()),
                       int.Parse(ColorArrayFiltered[Counter].ToString())));
                        CurrentX += 1;
                        Counter += 1;
                    } while (Counter < ColorArrayFiltered.Count);
                    pictureBox1.Image = CurrentBitmap;
                    Im = CurrentBitmap;
                    img = CurrentBitmap;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("The image is not the write foramt or is corrupt");
                PurgeGlobalData();
            }
        }
        private void PurgeGlobalData()
        {
            CurrentBitmap = new Bitmap(5, 5);
            CurrentX = 0;
            CurrentY = 0;
            ImageHeight = 0;
            ImageWidth = 0;
            ImageMaxGreyValue = 255;
            CurrentFile = "";
            CurrentFilePath = "";
            ImageType = "";

        }

        private void exitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void x3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            applyFilter(3);
        }

        private void x5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            applyFilter(5);
        }

        private void x7ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            applyFilter(7);
        }

        private void applyFilter(int maskSize)
        {
            Bitmap tempImage = Im;

            // Check that the mask size is valid (3, 5, or 7)
            if (maskSize != 3 && maskSize != 5 && maskSize != 7)
            {
                throw new ArgumentException("Invalid mask size. Must be 3, 5, or 7.");
            }

            // Create the low-pass filter mask
            var mask = CreateLowPassFilterMask(maskSize);

            // Create a new bitmap to store the filtered image
            var filteredImage = new Bitmap(tempImage.Width, tempImage.Height);

            // Loop through each pixel in the image
            for (int x = 0; x < tempImage.Width; x++)
            {
                for (int y = 0; y < tempImage.Height; y++)
                {
                    // Apply the low-pass filter to the pixel
                    var filteredPixel = ApplyFilter(tempImage, mask, x, y);

                    // Set the filtered pixel in the new image
                    filteredImage.SetPixel(x, y, filteredPixel);
                }
            }


            // Display the filtered image
            pictureBox2.Image = filteredImage;
        }

        private static float[,] CreateLowPassFilterMask(int maskSize)
        {
            // Create the mask array
            var mask = new float[maskSize, maskSize];

            // Calculate the sum of the mask values
            var sum = 0f;

            // Loop through each value in the mask
            for (int x = 0; x < maskSize; x++)
            {
                for (int y = 0; y < maskSize; y++)
                {
                    // Set the mask value to 1
                    mask[x, y] = 1;

                    // Add the mask value to the sum
                    sum += mask[x, y];
                }
            }

            // Divide each value in the mask by the sum to normalize the values
            for (int x = 0; x < maskSize; x++)
            {
                for (int y = 0; y < maskSize; y++)
                {
                    mask[x, y] /= sum;
                }
            }

            // Return the mask
            return mask;
        }

        private static Color ApplyFilter(Bitmap image, float[,] mask, int x, int y)
        {
            // Get the size of the mask
            var maskSize = mask.GetLength(0);

            // Calculate the half-size of the mask
            var halfSize = maskSize / 2;

            // Initialize the sum of red, green, and blue values to 0
            var redSum = 0f;
            var greenSum = 0f;
            var blueSum = 0f;

            // Loop through the mask
            for (int i = 0; i < maskSize; i++)
            {
                for (int j = 0; j < maskSize; j++)
                {
                    // Calculate the x and y coordinates of the current mask value
                    var maskX = x - halfSize + i;
                    var maskY = y - halfSize + j;

                    // Check if the coordinates are within the bounds of the image
                    if (maskX >= 0 && maskX < image.Width && maskY >= 0 && maskY < image.Height)
                    {
                        // Get the color of the pixel at the coordinates
                        var pixel = image.GetPixel(maskX, maskY);

                        // Get the mask value at the current coordinates
                        var maskValue = mask[i, j];

                        // Multiply the pixel values by the mask value and add them to the sum
                        redSum += pixel.R * maskValue;
                        greenSum += pixel.G * maskValue;
                        blueSum += pixel.B * maskValue;
                    }
                }
            }

            // Create a new color with the calculated red, green, and blue values
            var filteredPixel = Color.FromArgb((int)redSum, (int)greenSum, (int)blueSum);

            // Return the filtered pixel
            return filteredPixel;
        }

    }
}