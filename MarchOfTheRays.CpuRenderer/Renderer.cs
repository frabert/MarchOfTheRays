﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading.Tasks;

namespace MarchOfTheRays.CpuRenderer
{
    public class Renderer
    {
        Vector3 cameraOrigin = new Vector3(2.1f, 2.0f, 2.5f);
        Vector3 cameraTarget = Vector3.Zero;
        Vector3 upDirection = Vector3.UnitY;

        const int MAX_ITER = 100; // 100 is a safe number to use, it won't produce too many artifacts and still be quite fast
        const float MAX_DIST = 20.0f; // Make sure you change this if you have objects farther than 20 units away from the camera
        const float EPSILON = 0.001f; // At this distance we are close enough to the object that we have essentially hit it

        void MainRender(float x, float y, float width, float height, out Vector3 outColor, Func<Vector3, float> distFunc)
        {
            var cameraDir = Vector3.Normalize(cameraTarget - cameraOrigin);

            var cameraRight = Vector3.Normalize(Vector3.Cross(upDirection, cameraOrigin));
            var cameraUp = Vector3.Cross(cameraDir, cameraRight);

            var screenPos = new Vector2(-1.0f + 2.0f * x / width, -1.0f + 2.0f * y / height);
            screenPos.X *= width / height; // Correct aspect ratio

            var rayDir = Vector3.Normalize(cameraRight * screenPos.X + cameraUp * screenPos.Y + cameraDir);

            float totalDist = 0.0f;
            var pos = cameraOrigin;
            float dist = EPSILON;

            for (int i = 0; i < MAX_ITER; i++)
            {
                // Either we've hit the object or hit nothing at all, either way we should break out of the loop
                if (dist < EPSILON || totalDist > MAX_DIST)
                    break; // If you use windows and the shader isn't working properly, change this to continue;

                dist = distFunc(pos); // Evalulate the distance at the current point
                totalDist += (dist / 10.0f);
                pos += (dist / 10.0f) * rayDir; // Advance the point forwards in the ray direction by the distance

            }

            if (dist < EPSILON)
            {
                var eps = new Vector2(0.0f, EPSILON);
                var yxx = new Vector3(eps.Y, eps.X, eps.X);
                var xyx = new Vector3(eps.X, eps.Y, eps.X);
                var xxy = new Vector3(eps.X, eps.X, eps.Y);

                var normal = Vector3.Normalize(new Vector3(
                    distFunc(pos + yxx) - distFunc(pos - yxx),
                    distFunc(pos + xyx) - distFunc(pos - xyx),
                    distFunc(pos + xxy) - distFunc(pos - xxy)));

                float diffuse = Math.Max(0.0f, Vector3.Dot(-rayDir, normal));
                float specular = (float)Math.Pow(diffuse, 64.0f);
                outColor = new Vector3(diffuse + specular);
            }
            else
            {
                outColor = Vector3.Zero;
            }
        }

        void RenderChunk(IntPtr scan0, int totalWidth, int totalHeight, int height, int stride, int yoff, Func<Vector3, float> func)
        {
            unsafe
            {
                byte* rawBytes = (byte*)scan0;
                int strideDiff = stride - totalWidth * 3;
                for(int i = 0; i < totalWidth * height; i++)
                {
                    int x = i % totalWidth;
                    int y = (i - x) / totalWidth;

                    int idx = (stride * (y + yoff)) + x * 3;

                    Vector3 color;
                    MainRender(x, y + yoff, totalWidth, totalHeight, out color, func);
                    rawBytes[idx + 0] = (byte)(Math.Min(color.X, 1.0f) * 255);
                    rawBytes[idx + 1] = (byte)(Math.Min(color.Y, 1.0f) * 255);
                    rawBytes[idx + 2] = (byte)(Math.Min(color.Z, 1.0f) * 255);
                }
            }
        }

        public Image RenderImage(int width, int height, Func<Vector3, float> func)
        {
            var img = new Bitmap(width, height);

            var data = img.LockBits(new Rectangle(Point.Empty, new Size(width, height)), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            
            RenderChunk(data.Scan0, width, height, height, data.Stride, 0, func);

            img.UnlockBits(data);

            return img;
        }

        public Image RenderImage(int width, int height, Func<Vector3, float> func, int nthreads)
        {
            var img = new Bitmap(width, height);

            var data = img.LockBits(new Rectangle(Point.Empty, new Size(width, height)), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            Task[] tasks = new Task[nthreads];

            var stripeHeight = height / (float)nthreads;
            for(int i = 0; i < nthreads; i++)
            {
                int start = (int)stripeHeight * i;
                int end = (int)(stripeHeight * (i + 1));
                tasks[i] = Task.Factory.StartNew(() => RenderChunk(data.Scan0, width, height, end - start, data.Stride, start, func));
            }

            Task.WaitAll(tasks);

            img.UnlockBits(data);

            return img;
        }

        public Task<Image> RenderImageAsync(int width, int height, Core.OutputNode node, Func<Vector3, float> func)
        {
            return Task<Image>.Factory.StartNew(() => RenderImage(width, height, func));
        }

        public Task<Image> RenderImageAsync(int width, int height, Core.OutputNode node, Func<Vector3, float> func, int nthreads)
        {
            return Task<Image>.Factory.StartNew(() => RenderImage(width, height, func, nthreads));
        }
    }
}
