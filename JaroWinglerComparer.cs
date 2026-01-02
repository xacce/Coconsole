using System;
using System.Collections.Generic;
using System.Linq;

namespace Coconsole
{
    public class JaroWinglerComparer : IComparer
    {
        public double weightThreshold = 0.7;
        public int numChars = 4;
        public float minWeight = 0.2f;


        public string[] Search(string caseSensitiveValue, string[] values)
        {
            if (String.IsNullOrEmpty(caseSensitiveValue)) return values;
            var value = caseSensitiveValue.ToLower();
            var results = new List<(double, string)>();
            int i = 0;
            foreach (var v in values)
            {
                var distance = Proximity(value, v);
                if (distance > minWeight)
                {
                    results.Add((distance, v));
                }


                i++;
            }

            results.Sort((a, b) => b.Item1.CompareTo(a.Item1));


            return results.Select(x => x.Item2).ToArray();
        }

        public T[] Search<T>(string caseSensitiveValue, string[] values, Func<int, string, double, T> onResult) where T : ISearchResult
        {
            if (String.IsNullOrEmpty(caseSensitiveValue))
            {
                return values.Select((x, index) => onResult(index, x, 0.0)).ToArray();
            }

            var value = caseSensitiveValue.ToLower();
            var results = new List<T>();
            int i = 0;
            foreach (var v in values)
            {
                var distance = Proximity(value, v);
                if (distance > minWeight)
                {
                    results.Add(onResult(i, v, distance));
                }

                i++;
            }

            results.Sort((a, b) => b.distance.CompareTo(a.distance));


            return results.ToArray();
        }

        public double Proximity(string aString1, string caseSensitiveString2)
        {
            int lLen1 = aString1.Length;
            var aString2 = caseSensitiveString2.ToLower();
            int lLen2 = aString2.Length;
            if (lLen1 == 0)
                return lLen2 == 0 ? 1.0 : 0.0;

            int lSearchRange = Math.Max(0, Math.Max(lLen1, lLen2) / 2 - 1);
            // default initialized to false
            bool[] lMatched1 = new bool[lLen1];
            bool[] lMatched2 = new bool[lLen2];

            int lNumCommon = 0;
            for (int i = 0; i < lLen1; ++i)
            {
                int lStart = Math.Max(0, i - lSearchRange);
                int lEnd = Math.Min(i + lSearchRange + 1, lLen2);
                for (int j = lStart; j < lEnd; ++j)
                {
                    if (lMatched2[j]) continue;
                    if (aString1[i] != aString2[j])
                        continue;
                    lMatched1[i] = true;
                    lMatched2[j] = true;
                    ++lNumCommon;
                    break;
                }
            }

            if (lNumCommon == 0) return 0.0;

            int lNumHalfTransposed = 0;
            int k = 0;
            for (int i = 0; i < lLen1; ++i)
            {
                if (!lMatched1[i]) continue;
                while (!lMatched2[k]) ++k;
                if (aString1[i] != aString2[k])
                    ++lNumHalfTransposed;
                ++k;
            }

            // System.Diagnostics.Debug.WriteLine("numHalfTransposed=" + numHalfTransposed);
            int lNumTransposed = lNumHalfTransposed / 2;

            // System.Diagnostics.Debug.WriteLine("numCommon=" + numCommon + " numTransposed=" + numTransposed);
            double lNumCommonD = lNumCommon;
            double lWeight = (lNumCommonD / lLen1
                              + lNumCommonD / lLen2
                              + (lNumCommon - lNumTransposed) / lNumCommonD) / 3.0;

            if (lWeight <= weightThreshold) return lWeight;
            int lMax = Math.Min(numChars, Math.Min(aString1.Length, aString2.Length));
            int lPos = 0;
            while (lPos < lMax && aString1[lPos] == aString2[lPos])
                ++lPos;
            if (lPos == 0) return lWeight;
            return lWeight + 0.1 * lPos * (1.0 - lWeight);
        }
    }
}