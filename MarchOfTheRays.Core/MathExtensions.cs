using System;
using System.Numerics;

namespace MarchOfTheRays.Core
{
    /// <summary>
    /// Static methods to perform various mathematical operations on float values and vectors
    /// </summary>
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

        public static Vector2 Acos(Vector2 a) => Map(Math.Acos, a);
        public static Vector2 Asin(Vector2 a) => Map(Math.Asin, a);
        public static Vector2 Atan(Vector2 a) => Map(Math.Atan, a);
        public static Vector2 Atan(Vector2 a, Vector2 b) => Map(Math.Atan2, a, b);
        public static Vector2 Ceil(Vector2 a) => Map(Math.Ceiling, a);
        public static Vector2 Floor(Vector2 a) => Map(Math.Floor, a);
        public static Vector2 Cos(Vector2 a) => Map(Math.Cos, a);
        public static Vector2 Exp(Vector2 a) => Map(Math.Exp, a);
        public static Vector2 Log(Vector2 a) => Map(Math.Log, a);
        public static Vector2 Sin(Vector2 a) => Map(Math.Sin, a);
        public static Vector2 Sqrt(Vector2 a) => Vector2.SquareRoot(a);
        public static Vector2 Tan(Vector2 a) => Map(Math.Tan, a);
        public static Vector2 Degrees(Vector2 a) => Map(x => x * (180.0 / Math.PI), a);
        public static Vector2 Radians(Vector2 a) => Map(x => x * (Math.PI / 180.0), a);
        public static Vector2 Mod(Vector2 a, Vector2 b) => Map((x, y) => x - y * (Math.Floor(x / y)), a, b);

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

        public static Vector4 Acos(Vector4 a) => Map(Math.Acos, a);
        public static Vector4 Asin(Vector4 a) => Map(Math.Asin, a);
        public static Vector4 Atan(Vector4 a) => Map(Math.Atan, a);
        public static Vector4 Atan(Vector4 a, Vector4 b) => Map(Math.Atan2, a, b);
        public static Vector4 Ceil(Vector4 a) => Map(Math.Ceiling, a);
        public static Vector4 Floor(Vector4 a) => Map(Math.Floor, a);
        public static Vector4 Cos(Vector4 a) => Map(Math.Cos, a);
        public static Vector4 Exp(Vector4 a) => Map(Math.Exp, a);
        public static Vector4 Log(Vector4 a) => Map(Math.Log, a);
        public static Vector4 Sin(Vector4 a) => Map(Math.Sin, a);
        public static Vector4 Sqrt(Vector4 a) => Vector4.SquareRoot(a);
        public static Vector4 Tan(Vector4 a) => Map(Math.Tan, a);
        public static Vector4 Degrees(Vector4 a) => Map(x => x * (180.0 / Math.PI), a);
        public static Vector4 Radians(Vector4 a) => Map(x => x * (Math.PI / 180.0), a);
        public static Vector4 Mod(Vector4 a, Vector4 b) => Map((x, y) => x - y * (Math.Floor(x / y)), a, b);

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
