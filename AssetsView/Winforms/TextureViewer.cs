﻿using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AssetsView.Winforms
{
    public partial class TextureViewer : Form
    {
        Bitmap image;

        bool loaded;
        float x, y;
        int width, height;
        int lx, ly;
        int mx, my;
        float sc;
        bool mouseDown;

        public TextureViewer(AssetsFileInstance inst, AssetTypeValueField baseField)
        {
            InitializeComponent();

            loaded = false;
            TextureFile tf = TextureFile.ReadTextureFile(baseField);
            byte[] texDat = tf.GetTextureData(inst);
            if (texDat != null && texDat.Length > 0)
            {
                string fmtName = ((TextureFormat)tf.m_TextureFormat).ToString().Replace("_", " ");
                Text = $"Texture Viewer [{fmtName}]";

                image = new Bitmap(tf.m_Width, tf.m_Height, tf.m_Width * 4, PixelFormat.Format32bppArgb,
                    Marshal.UnsafeAddrOfPinnedArrayElement(texDat, 0));
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);

                x = 0;
                y = 0;
                width = image.Width;
                height = image.Height;
                sc = 1f;
                mouseDown = false;

                DoubleBuffered = true;
                
                Rectangle workingArea = Screen.PrimaryScreen.WorkingArea;
                int waWidth = workingArea.Width;
                int waHeight = workingArea.Height;
                int cliDiffWidth = Size.Width - ClientSize.Width;
                int cliDiffHeight = Size.Height - ClientSize.Height;
                ClientSize = new Size(Math.Min(width, waWidth - cliDiffWidth), Math.Min(height, waHeight - cliDiffHeight));

                loaded = true;
            }
        }

        private void TextureViewer_MouseWheel(object sender, MouseEventArgs e)
        {
            float oldSc = sc;
            sc *= 1 + (float)e.Delta / 1200;

            float oldImageX = mx / oldSc;
            float oldImageY = my / oldSc;

            float newImageX = mx / sc;
            float newImageY = my / sc;

            x = newImageX - oldImageX + x;
            y = newImageY - oldImageY + y;

            Refresh();
        }

        private void TextureViewer_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (loaded)
            {
                int drawWidth = (int)(width * sc);
                int drawHeight = (int)(height * sc);
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                Matrix oldTfm = g.Transform;
                g.ScaleTransform(sc, sc);
                g.TranslateTransform(x, y);
                g.DrawImage(image, 0, 0);//(int)x, (int)y);//, drawWidth, drawHeight);
                //for the resizey thing on the bottom right (for some reason is affected by this)
                g.Transform = oldTfm;
            }
            else
            {
                using (Font font = new Font(SystemFonts.DefaultFont.FontFamily, 20, FontStyle.Regular))
                {
                    g.DrawString("Unsupported texture format", font, Brushes.Red, 20, 20);
                    g.DrawString("or texture could not be parsed.", font, Brushes.Red, 20, 50);
                }
            }
        }

        private void TextureViewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseDown = false;
            }
        }

        private void TextureViewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseDown = true;
                lx = e.X;
                ly = e.Y;
            }
        }

        private void TextureViewer_MouseMove(object sender, MouseEventArgs e)
        {
            mx = e.X;
            my = e.Y;
            if (mouseDown)
            {
                x += (e.X - lx) / sc;
                y += (e.Y - ly) / sc;
                lx = e.X;
                ly = e.Y;
                Refresh();
            }
        }
    }
}
