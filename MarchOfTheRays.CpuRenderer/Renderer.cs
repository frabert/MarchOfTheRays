using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace MarchOfTheRays.CpuRenderer
{
    public class Renderer
    {
        Vector3 cameraOrigin = new Vector3(2.1f, 2.0f, 2.5f);
        Vector3 cameraTarget = Vector3.Zero;
        Vector3 upDirection = Vector3.UnitY;
        Vector3 lightDirection = Vector3.Normalize(new Vector3(0.3f, 1, 0));

        float ambient_f = 0.1f;

        Vector3 ambient_color = new Vector3(0.9f, 0.9f, 1.0f);

        float diffuse_f = 0.4f;
        float specular_f = 0.1f;

        int MAX_ITER = 100; // 100 is a safe number to use, it won't produce too many artifacts and still be quite fast
        int MAX_SHADOW_ITER = 200;
        float MAX_DIST = 20.0f; // Make sure you change this if you have objects farther than 20 units away from the camera
        float EPSILON = 0.001f; // At this distance we are close enough to the object that we have essentially hit it
        float STEP_SIZE = 1.0f;

        float softShadow(Vector3 ro, Vector3 rd, float start, float end, float k, Func<Vector3, float> distFunc)
        {
            float shade = 1.0f;

            // The "start" value, or minimum, should be set to something more than the stop-threshold, so as to avoid a collision with 
            // the surface the ray is setting out from. It doesn't matter how many times I write shadow code, I always seem to forget this.
            // If adding shadows seems to make everything look dark, that tends to be the problem.
            float dist = start;
            float stepDist = end / MAX_SHADOW_ITER;

            // Max shadow iterations - More iterations make nicer shadows, but slow things down. Obviously, the lowest 
            // number to give a decent shadow is the best one to choose. 
            for (int i = 0; i < MAX_SHADOW_ITER; i++)
            {
                // End, or maximum, should be set to the distance from the light to surface point. If you go beyond that
                // you may hit a surface not between the surface and the light.
                float h = distFunc(ro + rd * dist);
                shade = Math.Min(shade, k * h / dist);

                // What h combination you add to the distance depends on speed, accuracy, etc. To be honest, I find it impossible to find 
                // the perfect balance. Faster GPUs give you more options, because more shadow iterations always produce better results.
                // Anyway, here's some posibilities. Which one you use, depends on the situation:
                // +=h, +=clamp( h, 0.01, 0.25 ), +=min( h, 0.1 ), +=stepDist, +=min(h, stepDist*2.), etc.
                dist += Math.Min(h * STEP_SIZE, stepDist * 2.0f); // The best of both worlds... I think. 

                // Early exits from accumulative distance function calls tend to be a good thing.
                if (h < 0.001f || dist > end) break;
            }

            // I've added 0.3 to the final shade value, which lightens the shadow a bit. It's a preference thing. Really dark shadows look 
            // too brutal to me.
            return Math.Min(Math.Max(shade, 0) + 0.3f, 1.0f);
        }

        public Renderer(Vector3 cameraOrigin, Vector3 cameraTarget, Vector3 upDirection, int maxIter, float maxDist, float Epsilon, float stepSize)
        {
            this.cameraOrigin = cameraOrigin;
            this.cameraTarget = cameraTarget;
            this.upDirection = upDirection;

            MAX_ITER = maxIter;
            MAX_DIST = maxDist;
            EPSILON = Epsilon;
            STEP_SIZE = stepSize;
        }

        static float clamp(float a, float min, float max)
        {
            return a < min ? min : a > max ? max : a;
        }

        float calculateAmbientOcclusion(Vector3 pos, Vector3 norm, Func<Vector3, float> distFunc)
        {
            const int AO_SAMPLES = 6;
            float r = 0.0f;
            float w = 1.0f;
            for (int i = 1; i < AO_SAMPLES; i++)
            {
                float d0 = i * 0.2f;
                r += w * (d0 - distFunc(pos + norm * d0));
                w *= 0.5f;
            }
            return 1.0f - clamp(r, 0.0f, 1.0f);
        }

        Vector3 reflect(Vector3 I, Vector3 N)
        {
            return I - 2.0f * Vector3.Dot(N, I) * N;
        }

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
                totalDist += (dist * STEP_SIZE) * STEP_SIZE;
                pos += (dist * STEP_SIZE) * rayDir; // Advance the point forwards in the ray direction by the distance

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

                
                float diffuse = Math.Max(0.0f, Vector3.Dot(lightDirection, normal));

                var reflectDir = reflect(-lightDirection, rayDir);

                float specular = (float)Math.Pow(Math.Max(0.0f, Vector3.Dot(rayDir, reflectDir)), 32.0f) * specular_f;
                float ao = calculateAmbientOcclusion(pos, normal, distFunc);
                float shade = softShadow(pos, lightDirection, EPSILON * 2, MAX_DIST, 8, distFunc);
                outColor = (new Vector3(diffuse * shade + specular) + ambient_color * ambient_f) * ao;
            }
            else
            {
                outColor = Vector3.Zero;
            }
        }

        bool RenderChunk(IntPtr scan0, int totalWidth, int totalHeight, int height, int stride, int yoff, Func<Vector3, float> func, CancellationToken token, IProgress<int> progress)
        {
            unsafe
            {
                byte* rawBytes = (byte*)scan0;
                int strideDiff = stride - totalWidth * 3;
                for(int i = 0; i < totalWidth * height; i++)
                {
                    if (token.IsCancellationRequested)
                        return false;
                    int x = i % totalWidth;
                    int y = (i - x) / totalWidth;

                    int idx = (stride * (y + yoff)) + x * 3;

                    Vector3 color;
                    MainRender(x, y + yoff, totalWidth, totalHeight, out color, func);
                    rawBytes[idx + 0] = (byte)(Math.Min(color.Z, 1.0f) * 255);
                    rawBytes[idx + 1] = (byte)(Math.Min(color.Y, 1.0f) * 255);
                    rawBytes[idx + 2] = (byte)(Math.Min(color.X, 1.0f) * 255);
                    if(i % 100 == 99) progress?.Report(100);
                }
            }
            return true;
        }

        public async Task<Image> RenderImageAsync(int width, int height, Func<Vector3, float> func, int nthreads, CancellationToken token, IProgress<int> progress)
        {
            var img = new Bitmap(width, height);

            var data = img.LockBits(new Rectangle(Point.Empty, new Size(width, height)), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            Task<bool>[] tasks = new Task<bool>[nthreads];

            var stripeHeight = height / (float)nthreads;
            for(int i = 0; i < nthreads; i++)
            {
                int start = (int)stripeHeight * i;
                int end = (int)(stripeHeight * (i + 1));
                tasks[i] = Task.Factory.StartNew(() => RenderChunk(data.Scan0, width, height, end - start, data.Stride, start, func, token, progress));
            }

            bool imageComplete = true;

            foreach(var task in tasks)
            {
                imageComplete &= await task;
            }

            img.UnlockBits(data);

            return imageComplete ? img : null;
        }

        public async Task<Image> RenderImageAsync(int width, int height, Func<Vector3, float> func, int nthreads)
        {
            return await RenderImageAsync(width, height, func, nthreads, CancellationToken.None, null);
        }
    }
}
