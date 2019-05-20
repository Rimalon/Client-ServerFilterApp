using System;
using System.Drawing;
using System.ServiceModel;

namespace ServerLib
{
    public class FilterManager
    {
        public volatile bool isWorking = false;

        public Bitmap ApplyingFilterToBitmap(Bitmap bmp, string filter)
        {
            switch (filter)
            {
                case "Average":
                    {
                        float[,] average = new float[3, 3];
                        float k = 1f / 9f;

                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                average[i, j] = k;
                            }
                        }

                        return Gauss3x3(bmp, average);
                    }
                case "Gauss":
                    {
                        float[,] kernel = new float[3, 3];
                        const float sigma = 1;
                        float sigmaSqr = 2 * sigma * sigma;

                        for (int i = -1; i <= 1; i++)
                        {
                            for (int j = -1; j <= 1; j++)
                            {
                                kernel[i + 1, j + 1] = (float)(Math.Exp(-(i * i + j * j) / sigmaSqr) / (Math.PI * sigmaSqr));
                            }
                        }

                        return Gauss3x3(bmp, kernel);
                    }
                case "GrayScale":
                    {
                        return Grayscale(bmp);
                    }
                case "SobelX":
                    {
                        float[,] maskx =
                        {
                            { -1f, 0f, 1f },
                            { -2f, 0f, 2f },
                            { -1f, 0f, 1f }
                        };

                        return Sobel(bmp, maskx);
                    }
                case "SobelY":
                    {
                        float[,] masky =
                        {
                            { -1f, -2f, -1f },
                            { 0f, 0f, 0f },
                            { 1f, 2f, 1f }
                        };

                        return Sobel(bmp, masky);
                    }
                case "Haar":
                    {
                        return Haar(bmp);
                    }
                default:
                    {
                        throw new ArgumentException("Incorrect filterName");
                    }
            }
        }

        private Bitmap Gauss3x3(Bitmap bmp, float[,] kernel)
        {
            int width = bmp.Width;
            int height = bmp.Height;

            Bitmap result = new Bitmap(bmp);

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    float sum = 0;
                    float r = 0, g = 0, b = 0;

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            Color pixel = bmp.GetPixel(x + i, y + j);
                            float k = kernel[i + 1, j + 1];

                            r += pixel.R * k;
                            g += pixel.G * k;
                            b += pixel.B * k;

                            sum += k;
                        }
                    }

                    if (sum != 0)
                    {
                        result.SetPixel(x, y, Color.FromArgb(
                            ToByte(r / sum),
                            ToByte(g / sum),
                            ToByte(b / sum)));

                    }
                }
                if (isWorking)
                {
                    OperationContext.Current.GetCallbackChannel<IServerServiceCallback>().GetProgress((100 * x / width));
                }
                else
                {
                    return null;
                }
            }
            return result;
        }

        private Bitmap Sobel(Bitmap bmp, float[,] mask)
        {
            int width = bmp.Width;
            int height = bmp.Height;

            Bitmap result = new Bitmap(bmp);

            Color[,] temp = new Color[width, height];

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    int sum = 0;

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            byte cur = GrayscaledPixel(result.GetPixel(x + i, y + j));
                            sum += (int)(cur * mask[i + 1, j + 1]);
                        }
                    }

                    byte clamped = ToByte(sum);
                    temp[x, y] = Color.FromArgb(clamped, clamped, clamped);
                }
                if (isWorking)
                {
                    OperationContext.Current.GetCallbackChannel<IServerServiceCallback>().GetProgress((100 * x / width));
                }
                else
                {
                    return null;
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result.SetPixel(x, y, temp[x, y]);
                }
            }
            return result;
        }

        private Bitmap Grayscale(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            Bitmap result = new Bitmap(bmp);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    byte gray = GrayscaledPixel(result.GetPixel(x, y));
                    result.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
                if (isWorking)
                {
                    OperationContext.Current.GetCallbackChannel<IServerServiceCallback>().GetProgress((100 * x / width));
                }
                else
                {
                    return null;
                }
            }
            return result;
        }

        private byte GrayscaledPixel(Color pixel)
        {
            return ToByte((pixel.R + pixel.G + pixel.B) / 3);
        }

        private Bitmap Haar(Bitmap bmp)
        {
            int width = bmp.Width;
            int height = bmp.Height;
            Bitmap result = new Bitmap(bmp);

            Color[,] temp = new Color[width, height];


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height - 1; y += 2)
                {
                    Color halfSum = Color.FromArgb(
                        ToByte((result.GetPixel(x, y).R + result.GetPixel(x, y + 1).R) / 2),
                        ToByte((result.GetPixel(x, y).G + result.GetPixel(x, y + 1).G) / 2),
                        ToByte((result.GetPixel(x, y).B + result.GetPixel(x, y + 1).B) / 2));
                    Color halfDifference = Color.FromArgb(
                        ToByte((result.GetPixel(x, y).R - result.GetPixel(x, y + 1).R) / 2),
                        ToByte((result.GetPixel(x, y).G - result.GetPixel(x, y + 1).G) / 2),
                        ToByte((result.GetPixel(x, y).B - result.GetPixel(x, y + 1).B) / 2));
                    temp[x, y / 2] = halfSum;
                    temp[x, (height + y) / 2] = halfDifference;
                }
                if (isWorking)
                {
                    OperationContext.Current.GetCallbackChannel<IServerServiceCallback>().GetProgress(50 * x / width);
                }
                else
                {
                    return null;
                }
            }
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result.SetPixel(x, y, temp[x, y]);
                }
            }

            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x += 2)
                {
                    Color halfSum = Color.FromArgb(
                        ToByte((result.GetPixel(x, y).R + result.GetPixel(x, y + 1).R) / 2),
                        ToByte((result.GetPixel(x, y).G + result.GetPixel(x, y + 1).G) / 2),
                        ToByte((result.GetPixel(x, y).B + result.GetPixel(x, y + 1).B) / 2));
                    Color halfDifference = Color.FromArgb(
                        ToByte((result.GetPixel(x, y).R - result.GetPixel(x, y + 1).R) / 2),
                        ToByte((result.GetPixel(x, y).G - result.GetPixel(x, y + 1).G) / 2),
                        ToByte((result.GetPixel(x, y).B - result.GetPixel(x, y + 1).B) / 2));
                    temp[x / 2, y] = halfDifference;
                    temp[(width + x) / 2, y] = halfSum;
                }
                if (isWorking)
                {
                    OperationContext.Current.GetCallbackChannel<IServerServiceCallback>().GetProgress(50 + (50 * y / height));
                }
                else
                {
                    return null;
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    result.SetPixel(x, y, temp[x, y]);
                }
            }
            return result;
        }



        private byte ToByte(float f)
        {
            return f - (int)f >= 0.5f ?
                (byte)(f + 1) :
                (byte)f;
        }

        private byte ToByte(int s)
        {
            return s < 0 ? (byte)0 :
                s > 255 ? (byte)255 :
                (byte)s;
        }
    }
}
