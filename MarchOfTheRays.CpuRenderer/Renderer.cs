﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace MarchOfTheRays.CpuRenderer
{
    public class Renderer
    {
        Vector3 cameraOrigin = new Vector3(2.1f, 2.0f, 2.5f);
        Vector3 cameraTarget = Vector3.Zero;
        Vector3 upDirection = Vector3.UnitY;
        Vector3 lightDirection = Vector3.Normalize(new Vector3(0.3f, 1, 0));
        Vector3 skyColor = Vector3.Zero;

        float ambient_f = 0.1f;

        Vector3 light_color = new Vector3(0.9f, 0.9f, 1.0f);

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
        
        struct Material
        {
            public Vector3 diffuse_color;
            public float specular_strength;
            public float reflection_strength;
        }

        Material GetMaterial(Vector3 pos)
        {
            var diffuseColor = Core.MathExtensions.Mod(pos, Vector3.One);
            var vx = diffuseColor.X > 0.5f;
            var vy = diffuseColor.Y > 0.5f;
            var vz = diffuseColor.Z > 0.5f;
            var diffuseValue = vx ^ vy ^ vz;

            diffuseColor = diffuseValue ? Vector3.One : Vector3.Zero;
            var reflectivity = diffuseValue ? 0.01f : 0.025f;
            var specular = diffuseValue ? 0 : 0.3f;

            return new Material { diffuse_color = diffuseColor, specular_strength = specular, reflection_strength = reflectivity };
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

        float logistic(float x, float L, float x0, float k)
        {
            return L / (1 + (float)Math.Exp(-k * (x - x0)));
        }

        Vector3 rayMarch(Vector3 start, Vector3 rayDir, Func<Vector3, float> distFunc, int iteration = 0)
        {
            if (iteration > 4) return Vector3.Zero;

            float totalDist = 0.0f;
            var pos = start;
            float dist = EPSILON;

            for (int i = 0; i < MAX_ITER; i++)
            {
                // Either we've hit the object or hit nothing at all, either way we should break out of the loop
                if (dist < EPSILON || totalDist > MAX_DIST)
                    break;

                dist = distFunc(pos); // Evaluate the distance at the current point
                totalDist += (dist * STEP_SIZE) * STEP_SIZE;
                pos += (dist * STEP_SIZE) * rayDir; // Advance the point forwards in the ray direction by the distance

            }

            if (dist < EPSILON)
            {
                var eps_x = Vector3.UnitX * EPSILON;
                var eps_y = Vector3.UnitY * EPSILON;
                var eps_z = Vector3.UnitZ * EPSILON;

                var normal = Vector3.Normalize(new Vector3(
                    distFunc(pos + eps_x) - distFunc(pos - eps_x),
                    distFunc(pos + eps_y) - distFunc(pos - eps_y),
                    distFunc(pos + eps_z) - distFunc(pos - eps_z)));


                float diffuse_value = Math.Max(0.0f, Vector3.Dot(lightDirection, normal));

                var specularDir = Vector3.Reflect(-lightDirection, rayDir);
                var reflectionDir = Vector3.Reflect(rayDir, normal);

                float specular_value = (float)Math.Pow(Math.Max(0.0f, Vector3.Dot(rayDir, specularDir)), 32);
                float ao = calculateAmbientOcclusion(pos, normal, distFunc);
                float shade = softShadow(pos, lightDirection, EPSILON * 2, MAX_DIST, 8, distFunc);
                var reflection_color = rayMarch(pos + reflectionDir * EPSILON * 2, reflectionDir, distFunc, iteration + 1);

                var mat = GetMaterial(pos);

                var ambient = light_color * ambient_f;
                var diffuse = light_color * ((diffuse_value * shade + 0.1f) * mat.diffuse_color);
                var specular = light_color * (specular_value * mat.specular_strength);
                var reflection = reflection_color * mat.reflection_strength;

                var distance_rolloff = logistic(-totalDist, 1.0f, -MAX_DIST / 2, 1.0f);

                return (ambient + diffuse + specular + reflection) * ao * distance_rolloff + skyColor * (1 - distance_rolloff);
            }
            else
            {
                return skyColor;
            }
        }

        void MainRender(float x, float y, float width, float height, out Vector3 outColor, Func<Vector3, float> distFunc)
        {
            var cameraDir = Vector3.Normalize(cameraTarget - cameraOrigin);

            var cameraRight = Vector3.Normalize(Vector3.Cross(upDirection, cameraOrigin));
            var cameraUp = Vector3.Cross(cameraDir, cameraRight);

            outColor = Vector3.Zero;

            var offsets = new (Vector2 offset, int weight)[]
            {
                (new Vector2(EPSILON, EPSILON), 1),
                (new Vector2(-EPSILON, EPSILON), 1),
                (new Vector2(-EPSILON, -EPSILON), 1),
                (new Vector2(EPSILON, -EPSILON), 1),
                (new Vector2(EPSILON, 0), 2),
                (new Vector2(0, EPSILON), 2),
                (new Vector2(-EPSILON, 0), 2),
                (new Vector2(0, -EPSILON), 2),
                (Vector2.Zero, 4)
            };

            foreach(var offs in offsets)
            {
                var screenPos = new Vector2(-1.0f + 2.0f * x / width, -1.0f + 2.0f * y / height);
                screenPos += offs.offset;
                screenPos.X *= width / height; // Correct aspect ratio

                var rayDir = Vector3.Normalize(cameraRight * screenPos.X + cameraUp * screenPos.Y + cameraDir);

                outColor += rayMarch(cameraOrigin, rayDir, distFunc) * offs.weight;
            }

            outColor /= offsets.Sum(a => a.weight);
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
