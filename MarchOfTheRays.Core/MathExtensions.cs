using System;
using System.Numerics;

namespace MarchOfTheRays.Core
{
    public static class MathExtensions
    {
        static Vector2 Map(Func<double, double> f, Vector2 v) => new Vector2((float)f(v.X), (float)f(v.Y));
        static Vector2 Map(Func<double, double, double> f, Vector2 a, Vector2 b) => new Vector2((float)f(a.X, b.X), (float)f(a.Y, b.Y));
        static Vector3 Map(Func<double, double> f, Vector3 v) => new Vector3((float)f(v.X), (float)f(v.Y), (float)f(v.Z));
        static Vector3 Map(Func<double, double, double> f, Vector3 a, Vector3 b) => new Vector3((float)f(a.X, b.X), (float)f(a.Y, b.Y), (float)f(a.Z, b.Z));
        static Vector4 Map(Func<double, double> f, Vector4 v) => new Vector4((float)f(v.X), (float)f(v.Y), (float)f(v.Z), (float)f(v.W));
        static Vector4 Map(Func<double, double, double> f, Vector4 a, Vector4 b) => new Vector4((float)f(a.X, b.X), (float)f(a.Y, b.Y), (float)f(a.Z, b.Z), (float)f(a.W, b.W));

        static float Map(Func<double, double> f, float v) => (float)f(v);
        static float Map(Func<double, double, double> f, float a, float b) => (float)f(a, b);

        public static Vector3 Acos(Vector3 a) => Map(Math.Acos, a);
        public static Vector3 Asin(Vector3 a) => Map(Math.Asin, a);
        public static Vector3 Atan(Vector3 a) => Map(Math.Atan, a);
        public static Vector3 Atan(Vector3 a, Vector3 b) => Map(Math.Atan2, a, b);
        public static Vector3 Ceil(Vector3 a) => Map(Math.Ceiling, a);
        public static Vector3 Floor(Vector3 a) => Map(Math.Floor, a);
        public static Vector3 Cos(Vector3 a) => Map(Math.Cos, a);
        public static Vector3 Exp(Vector3 a) => Map(Math.Exp, a);
        public static Vector3 Log(Vector3 a) => Map(Math.Log, a);
        public static Vector3 Sin(Vector3 a) => Map(Math.Sin, a);
        public static Vector3 Sqrt(Vector3 a) => Vector3.SquareRoot(a);
        public static Vector3 Tan(Vector3 a) => Map(Math.Tan, a);
        public static Vector3 Degrees(Vector3 a) => Map(x => x * (180.0 / Math.PI), a);
        public static Vector3 Radians(Vector3 a) => Map(x => x * (Math.PI / 180.0), a);
        public static Vector3 Mod(Vector3 a, Vector3 b) => Map((x, y) => x - y * (Math.Floor(x / y)), a, b);

        public static float Acos(float a) => Map(Math.Acos, a);
        public static float Asin(float a) => Map(Math.Asin, a);
        public static float Atan(float a) => Map(Math.Atan, a);
        public static float Atan(float a, float b) => Map(Math.Atan2, a, b);
        public static float Ceil(float a) => Map(Math.Ceiling, a);
        public static float Floor(float a) => Map(Math.Floor, a);
        public static float Cos(float a) => Map(Math.Cos, a);
        public static float Exp(float a) => Map(Math.Exp, a);
        public static float Log(float a) => Map(Math.Log, a);
        public static float Sin(float a) => Map(Math.Sin, a);
        public static float Sqrt(float a) => Map(Math.Sqrt, a);
        public static float Tan(float a) => Map(Math.Tan, a);
        public static float Degrees(float rad) => (float)(rad * (180.0 / Math.PI));
        public static float Radians(float deg) => (float)(deg * (Math.PI / 180.0));
        public static float Mod(float a, float b) => a - b * Floor(a / b);
    }
}
