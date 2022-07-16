using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DemoSilkX11.Utility
{
    public class Util
    {
		public static byte Clamp(byte n, byte min, byte max)
		{
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}
		public static short Clamp(short n, short min, short max)
		{
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}
		public static int Clamp(int n, int min, int max)
		{
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}
		public static float Clamp(float n, float min, float max)
		{
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}
		public static double Clamp(double n, double min, double max)
		{
			if (n < min) return min;
			if (n > max) return max;
			return n;
		}

		public static float Lerp(float value1, float value2, float amount)
		{
			return value1 + (value2 - value1) * amount;
		}

		public static string ToSentenceCase(string str)
		{
			return Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
		}

		public static string[] EnumFancyNames(Type type)
		{
			return Enum.GetNames(type).Select((x) => Util.ToSentenceCase(x)).ToArray();
		}
		public static string[] EnumFancyNames<TEnum>()
		{
			return Enum.GetNames(typeof(TEnum)).Select((x) => Util.ToSentenceCase(x)).ToArray();
		}

        public static ThreadSafeRandom Random = new ThreadSafeRandom();
	}

    /// <summary>
    /// A threadsafe random object
    /// </summary>
    public class ThreadSafeRandom
    {
        private static readonly Random _global = new Random();
        [ThreadStatic] private static Random? _local;

        /// <returns>A random integer</returns>
        public int Next()
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }

            return _local.Next();
        }

        /// <returns>A random integer from 0 to maxValue</returns>
        public int Next(int maxValue)
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }

            return _local.Next(maxValue);
        }

        /// <returns>A random integer from minValue to maxValue</returns>
        public int Next(int minValue, int maxValue)
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }

            return _local.Next(minValue, maxValue);
        }

        /// <returns>A random double between 0 and 1</returns>
        public double NextDouble()
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }

            return _local.NextDouble();
        }

        /// <returns>A random float between 0 and 1</returns>
        public float NextFloat()
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }

            return (float)_local.NextDouble();
        }

        /// <returns>A random arrat if bytes</returns>
        public void NextBytes(byte[] buffer)
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }

            _local.NextBytes(buffer);
        }
    }
}
