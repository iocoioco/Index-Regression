//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace New_Tradegy
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;

//    class RegressionInput
//    {
//        public double Price;
//        public double Ratio;
//        public double Sum;
//    }

//    class Program
//    {
//        static void Main()
//        {
//            // Generate sample data
//            var data = GenerateSampleData(1000); // 1,000 rows

//            // Solve regression: y = a1 * x1 + a2 * x2 + b
//            var (a1, a2, b) = SolveLinearRegression2D_WithBias(data);

//            Console.WriteLine($"a1 = {a1:F4}, a2 = {a2:F4}, b = {b:F4}");

//            // Example prediction
//            var latest = data[^1];
//            double predictedDelta = a1 * latest.Ratio + a2 * latest.Sum + b;
//            Console.WriteLine($"Predicted ΔPrice = {predictedDelta:F4}");
//        }

//        static List<RegressionInput> GenerateSampleData(int count)
//        {
//            var rand = new Random(0);
//            var data = new List<RegressionInput>();
//            double price = 100;

//            for (int i = 0; i < count; i++)
//            {
//                double ratio = rand.NextDouble() * 2.0;  // 0.0 to 2.0
//                double sum = rand.NextDouble() * 5.0;    // 0.0 to 5.0

//                // Simulate small price movement related to ratio and sum
//                price += 0.02 * ratio + 0.01 * sum + (rand.NextDouble() - 0.5) * 0.1;

//                data.Add(new RegressionInput
//                {
//                    Price = price,
//                    Ratio = ratio,
//                    Sum = sum
//                });
//            }

//            return data;
//        }

//        public static (double a1, double a2, double b) SolveLinearRegression2D_WithBias(List<RegressionInput> data)
//        {
//            int N = data.Count - 1;
//            double Sx1 = 0, Sx2 = 0, Sy = 0;
//            double Sx1x1 = 0, Sx2x2 = 0, Sx1x2 = 0;
//            double Sx1y = 0, Sx2y = 0;

//            for (int i = 0; i < N; i++)
//            {
//                double x1 = data[i].Ratio;
//                double x2 = data[i].Sum;
//                double y = data[i + 1].Price - data[i].Price;

//                Sx1 += x1;
//                Sx2 += x2;
//                Sy += y;

//                Sx1x1 += x1 * x1;
//                Sx2x2 += x2 * x2;
//                Sx1x2 += x1 * x2;
//                Sx1y += x1 * y;
//                Sx2y += x2 * y;
//            }

//            double[,] A = new double[3, 3]
//            {
//            { Sx1x1, Sx1x2, Sx1 },
//            { Sx1x2, Sx2x2, Sx2 },
//            { Sx1,   Sx2,   N   }
//            };

//            double[] B = new double[3] { Sx1y, Sx2y, Sy };
//            double[] result = Solve3x3(A, B);
//            return (result[0], result[1], result[2]);
//        }

//        public static double[] Solve3x3(double[,] A, double[] B)
//        {
//            double a = A[0, 0], b = A[0, 1], c = A[0, 2];
//            double d = A[1, 0], e = A[1, 1], f = A[1, 2];
//            double g = A[2, 0], h = A[2, 1], i = A[2, 2];

//            double detA =
//                a * (e * i - f * h) -
//                b * (d * i - f * g) +
//                c * (d * h - e * g);

//            if (Math.Abs(detA) < 1e-12)
//                throw new InvalidOperationException("Matrix is singular");

//            double[,] A1 = { { B[0], b, c }, { B[1], e, f }, { B[2], h, i } };
//            double[,] A2 = { { a, B[0], c }, { d, B[1], f }, { g, B[2], i } };
//            double[,] A3 = { { a, b, B[0] }, { d, e, B[1] }, { g, h, B[2] } };

//            double detA1 = Determinant3x3(A1);
//            double detA2 = Determinant3x3(A2);
//            double detA3 = Determinant3x3(A3);

//            return new double[] { detA1 / detA, detA2 / detA, detA3 / detA };
//        }

//        public static double Determinant3x3(double[,] m)
//        {
//            return
//                m[0, 0] * (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1]) -
//                m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0]) +
//                m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]);
//        }
//    }




//    public class RegressionInput
//    {
//        public double Price;        // Price at time t
//        public double Ratio;        // x1 = multiple ratio at time t
//        public double Sum;          // x2 = sum of multiple at time t
//    }

//    internal class TwoDeimensionalRegression
//    {
//        public static (double a1, double a2, double b) SolveLinearRegression2D_WithBias(List<RegressionInput> data)
//        {
//            int N = data.Count - 1; // We lose 1 row for next-price diff
//            if (N < 2) throw new ArgumentException("Not enough data");

//            double Sx1 = 0, Sx2 = 0, Sy = 0;
//            double Sx1x1 = 0, Sx2x2 = 0, Sx1x2 = 0;
//            double Sx1y = 0, Sx2y = 0;

//            for (int i = 0; i < N; i++)
//            {
//                double x1 = data[i].Ratio;
//                double x2 = data[i].Sum;
//                double y = data[i + 1].Price - data[i].Price;

//                Sx1 += x1;
//                Sx2 += x2;
//                Sy += y;

//                Sx1x1 += x1 * x1;
//                Sx2x2 += x2 * x2;
//                Sx1x2 += x1 * x2;

//                Sx1y += x1 * y;
//                Sx2y += x2 * y;
//            }

//            // Build normal equations
//            // [ Sx1x1  Sx1x2  Sx1 ]
//            // [ Sx1x2  Sx2x2  Sx2 ]
//            // [ Sx1    Sx2    N   ]
//            // multiplied by [a1, a2, b] = [Sx1y, Sx2y, Sy]

//            // Solve 3x3 system using Cramer's Rule or matrix inversion
//            // Here we'll use direct matrix inversion for simplicity

//            // Matrix A
//            double[,] A = new double[3, 3]
//            {
//        { Sx1x1, Sx1x2, Sx1 },
//        { Sx1x2, Sx2x2, Sx2 },
//        { Sx1,   Sx2,   N   }
//            };

//            // Vector Y
//            double[] B = new double[3] { Sx1y, Sx2y, Sy };

//            // Invert A and multiply A⁻¹ * B
//            double[] result = Solve3x3(A, B);
//            return (result[0], result[1], result[2]);
//        }

//        public static double[] Solve3x3(double[,] A, double[] B)
//        {
//            double a = A[0, 0], b = A[0, 1], c = A[0, 2];
//            double d = A[1, 0], e = A[1, 1], f = A[1, 2];
//            double g = A[2, 0], h = A[2, 1], i = A[2, 2];

//            double detA =
//                a * (e * i - f * h) -
//                b * (d * i - f * g) +
//                c * (d * h - e * g);

//            if (Math.Abs(detA) < 1e-12)
//                throw new InvalidOperationException("Matrix is singular");

//            // Cramer's rule
//            double[,] A1 = { { B[0], b, c }, { B[1], e, f }, { B[2], h, i } };
//            double[,] A2 = { { a, B[0], c }, { d, B[1], f }, { g, B[2], i } };
//            double[,] A3 = { { a, b, B[0] }, { d, e, B[1] }, { g, h, B[2] } };

//            double detA1 = A1[0, 0] * (A1[1, 1] * A1[2, 2] - A1[1, 2] * A1[2, 1]) - A1[0, 1] * (A1[1, 0] * A1[2, 2] - A1[1, 2] * A1[2, 0]) + A1[0, 2] * (A1[1, 0] * A1[2, 1] - A1[1, 1] * A1[2, 0]);
//            double detA2 = A2[0, 0] * (A2[1, 1] * A2[2, 2] - A2[1, 2] * A2[2, 1]) - A2[0, 1] * (A2[1, 0] * A2[2, 2] - A2[1, 2] * A2[2, 0]) + A2[0, 2] * (A2[1, 0] * A2[2, 1] - A2[1, 1] * A2[2, 0]);
//            double detA3 = A3[0, 0] * (A3[1, 1] * A3[2, 2] - A3[1, 2] * A3[2, 1]) - A3[0, 1] * (A3[1, 0] * A3[2, 2] - A3[1, 2] * A3[2, 0]) + A3[0, 2] * (A3[1, 0] * A3[2, 1] - A3[1, 1] * A3[2, 0]);

//            return new double[] { detA1 / detA, detA2 / detA, detA3 / detA };
//        }

//    }
//}
