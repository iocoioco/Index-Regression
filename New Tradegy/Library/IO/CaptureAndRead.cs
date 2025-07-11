﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Tesseract;
using New_Tradegy.Library.Models;

namespace New_Tradegy.Library.IO
{
    internal class CaptureAndRead
    {
        private static double lastIndex;
        private static bool first = true;
        public static async Task NasdaqIndex()
        {
            while (true)
            {
                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();

                // 1920 1080 vs 3840 2160
                // Define the area to capture (adjust as needed)
                Rectangle captureArea = new Rectangle(-3840, 1860, 500, 200); // x, y, width, height

                // Capture the screen
                Bitmap bitmap = CaptureScreen(captureArea);


                // Convert to grayscale and increase resolution
                Bitmap grayscaleBitmap = ConvertToGrayscale(bitmap);
                //Bitmap highResBitmap = IncreaseResolution(grayscaleBitmap, 2);

                // Optionally binarize the image
                //Bitmap binaryBitmap = BinarizeImage(highResBitmap);

                //string savePath = @"C:\병신\capture.png"; // Change the path as needed
                //File.Delete(savePath);
                //binaryBitmap.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
                //Console.WriteLine("Image saved to: " + savePath);

                // Perform OCR on the image
                //var enhancedBitmap = ThresholdAndEnhance(grayscaleBitmap);
                //string text = ReadTextFromImage(enhancedBitmap);

                string text = ReadTextFromImage(grayscaleBitmap);


                // 1. Remove characters { . , ( )
                char[] toRemove = { '{', '.', ',', '(', ')' };
                string cleaned = new string(text.Where(c => !toRemove.Contains(c)).ToArray());

                // 2. Take the first 5 characters
                string firstFive = cleaned.Length >= 5 ? cleaned.Substring(0, 5) : cleaned;

                // 3. Convert to double
                double.TryParse(firstFive, out double currentNasdaq);

                if(first)
                {
                    lastIndex = currentNasdaq;
                    first = false;
                }
                else
                {
                    if (Math.Abs(currentNasdaq - lastIndex) > 200)
                    {
                        Thread.Sleep(1500);
                        continue;
                    }
                }
                
                lastIndex = currentNasdaq;

                MajorIndex.Instance.NasdaqIndex = (float)((currentNasdaq - g.NasdaqBasis) / g.NasdaqBasis * 
                    g.HUNDRED);
                // Update the global data table

                //Console.WriteLine($" Nasdaq index {MajorIndex.Instance.NasdaqIndex}");

                if (g.controlPane.GetCellValue(1, 2) != MajorIndex.Instance.NasdaqIndex.ToString())
                    g.controlPane.SetCellValue(1, 2, MajorIndex.Instance.NasdaqIndex.ToString("F3"));

                // update the Nasdaq Index value
                DateTime date = DateTime.Now;
                int HHmm = Convert.ToInt32(date.ToString("HHmm"));

                // Check if the task should be performed based on time and day
                //if (wk.isWorkingHour())
                //{
                //    AppendOrReplaceNasdaqIndex();
                //}
                // elaspedMilliSeconds = 180
                //stopwatch.Stop();
                //double elapsedMilliseconds = stopwatch.Elapsed.TotalMilliseconds;

                Thread.Sleep(1500);

                
            }
        }

        // Capture screen logic
        static Bitmap CaptureScreen(Rectangle area)
        {
            Bitmap bitmap = new Bitmap(area.Width, area.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(area.Location, Point.Empty, area.Size);
            }
            return bitmap;
        }

        // Convert image to grayscale
        static Bitmap ConvertToGrayscale(Bitmap bitmap)
        {
            Bitmap grayscaleBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            using (Graphics g = Graphics.FromImage(grayscaleBitmap))
            {
                ColorMatrix colorMatrix = new ColorMatrix(
                   new float[][]
                   {
                   new float[] {0.3f, 0.3f, 0.3f, 0, 0},
                   new float[] {0.59f, 0.59f, 0.59f, 0, 0},
                   new float[] {0.11f, 0.11f, 0.11f, 0, 0},
                   new float[] {0, 0, 0, 1, 0},
                   new float[] {0, 0, 0, 0, 1}
                   });
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);

                g.DrawImage(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                            0, 0, bitmap.Width, bitmap.Height, GraphicsUnit.Pixel, attributes);
            }
            return grayscaleBitmap;
        }

        // Increase resolution for better OCR accuracy
        static Bitmap IncreaseResolution(Bitmap bitmap, int scaleFactor)
        {
            int newWidth = bitmap.Width * scaleFactor;
            int newHeight = bitmap.Height * scaleFactor;
            Bitmap highResBitmap = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(highResBitmap))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(bitmap, new Rectangle(0, 0, newWidth, newHeight));
            }
            return highResBitmap;
        }

        // Binarize image for sharper contrast
        static Bitmap BinarizeImage(Bitmap bitmap)
        {
            Bitmap binaryBitmap = new Bitmap(bitmap.Width, bitmap.Height);
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color color = bitmap.GetPixel(x, y);
                    int gray = (int)(0.3 * color.R + 0.59 * color.G + 0.11 * color.B); // Convert to grayscale
                    Color newColor = gray > 128 ? Color.White : Color.Black; // Threshold to black or white
                    binaryBitmap.SetPixel(x, y, newColor);
                }
            }
            return binaryBitmap;
        }

        // Perform OCR on the processed image
        static string ReadTextFromImage(Bitmap bitmap)
        {
            string text = string.Empty;

            using (var engine = new TesseractEngine(@"C:\Program Files\Tesseract-OCR\tessdata", "eng+osd", EngineMode.Default))
            {
                // Set PSM (Page Segmentation Mode)
                engine.DefaultPageSegMode = PageSegMode.SingleBlock;
                // PageSegMode.SingleColumn -> multiple row but only one column faster
                // PageSegMode.Auto -> slower
                // PageSegMode.SingleBlockVertText
                // PageSegMode.SparseText
                // PageSegMode.SingleBlock
                // PageSegMode.SingleChar

                // Set the whitelist to digits only
                engine.SetVariable("tessedit_char_whitelist", "(1234567890.-");

                using (var img = BitmapToPix(bitmap))
                {
                    using (var page = engine.Process(img))
                    {
                        text = page.GetText();
                    }
                }
            }

            string value = new string(text.Where(c => char.IsDigit(c) || c == '.').ToArray());
            return value;
        }

        // Convert Bitmap to Pix format for Tesseract
        static Pix BitmapToPix(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png); // ImageFormat is part of System.Drawing.Imaging
                stream.Position = 0;
                return Pix.LoadFromMemory(stream.ToArray());
            }
        }

        static Bitmap ThresholdAndEnhance(Bitmap bmp)
        {
            Bitmap result = new Bitmap(bmp.Width, bmp.Height);
            for (int y = 0; y < bmp.Height; y++)
            {
                for (int x = 0; x < bmp.Width; x++)
                {
                    Color pixel = bmp.GetPixel(x, y);
                    int gray = (pixel.R + pixel.G + pixel.B) / 3;
                    int binary = gray < 160 ? 0 : 255;
                    result.SetPixel(x, y, Color.FromArgb(binary, binary, binary));
                }
            }
            return result;
        }

        public static void AppendOrReplaceNasdaqIndex()
        {
            int time_now_6int = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));

            foreach (var item in g.StockManager.IndexList)
            {
                var data = g.StockRepository.TryGetDataOrNull(item);
                if (data == null)
                {
                    continue;
                }

                int time_befr_6int = data.Api.x[data.Api.nrow - 1, 0];
                bool append;

                // 초는 포함하지 않는 시간 비교
                if (time_now_6int / 100 != time_befr_6int / 100) // times differ or time is 859, append
                    append = true;
                else
                    append = false;

                int append_or_replace_row;
                if (append)
                    append_or_replace_row = data.Api.nrow;
                else
                    append_or_replace_row = data.Api.nrow - 1;

                if (append_or_replace_row >= g.RealMaximumRow)
                    return;

                data.Api.x[append_or_replace_row, 10] = (int)(MajorIndex.Instance.NasdaqIndex * 100); // AAA teethed pattern
            }
        }
    }
}
