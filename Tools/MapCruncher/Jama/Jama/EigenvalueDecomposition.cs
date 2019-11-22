using System;
using Jama.util;

namespace Jama
{
    [Serializable]
    public class EigenvalueDecomposition
    {
        private int n;
        private bool issymmetric;
        private double[] d;
        private double[] e;
        private double[][] V;
        private double[][] H;
        private double[] ort;
        [NonSerialized] private double cdivr;
        [NonSerialized] private double cdivi;

        public virtual double[] RealEigenvalues
        {
            get
            {
                return d;
            }
        }

        public virtual double[] ImagEigenvalues
        {
            get
            {
                return e;
            }
        }

        public virtual JamaMatrix D
        {
            get
            {
                JamaMatrix jamaMatrix = new JamaMatrix(n, n);
                double[][] array = jamaMatrix.Array;
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        array[i][j] = 0.0;
                    }

                    array[i][i] = d[i];
                    if (e[i] > 0.0)
                    {
                        array[i][i + 1] = e[i];
                    }
                    else
                    {
                        if (e[i] < 0.0)
                        {
                            array[i][i - 1] = e[i];
                        }
                    }
                }

                return jamaMatrix;
            }
        }

        private void tred2()
        {
            for (int i = 0; i < n; i++)
            {
                d[i] = V[n - 1][i];
            }

            for (int j = n - 1; j > 0; j--)
            {
                double num = 0.0;
                double num2 = 0.0;
                for (int k = 0; k < j; k++)
                {
                    num += Math.Abs(d[k]);
                }

                if (num == 0.0)
                {
                    e[j] = d[j - 1];
                    for (int i = 0; i < j; i++)
                    {
                        d[i] = V[j - 1][i];
                        V[j][i] = 0.0;
                        V[i][j] = 0.0;
                    }
                }
                else
                {
                    for (int k = 0; k < j; k++)
                    {
                        d[k] /= num;
                        num2 += d[k] * d[k];
                    }

                    double num3 = d[j - 1];
                    double num4 = Math.Sqrt(num2);
                    if (num3 > 0.0)
                    {
                        num4 = -num4;
                    }

                    e[j] = num * num4;
                    num2 -= num3 * num4;
                    d[j - 1] = num3 - num4;
                    for (int i = 0; i < j; i++)
                    {
                        e[i] = 0.0;
                    }

                    for (int i = 0; i < j; i++)
                    {
                        num3 = d[i];
                        V[i][j] = num3;
                        num4 = e[i] + V[i][i] * num3;
                        for (int k = i + 1; k <= j - 1; k++)
                        {
                            num4 += V[k][i] * d[k];
                            e[k] += V[k][i] * num3;
                        }

                        e[i] = num4;
                    }

                    num3 = 0.0;
                    for (int i = 0; i < j; i++)
                    {
                        e[i] /= num2;
                        num3 += e[i] * d[i];
                    }

                    double num5 = num3 / (num2 + num2);
                    for (int i = 0; i < j; i++)
                    {
                        e[i] -= num5 * d[i];
                    }

                    for (int i = 0; i < j; i++)
                    {
                        num3 = d[i];
                        num4 = e[i];
                        for (int k = i; k <= j - 1; k++)
                        {
                            V[k][i] -= num3 * e[k] + num4 * d[k];
                        }

                        d[i] = V[j - 1][i];
                        V[j][i] = 0.0;
                    }
                }

                d[j] = num2;
            }

            for (int j = 0; j < n - 1; j++)
            {
                V[n - 1][j] = V[j][j];
                V[j][j] = 1.0;
                double num2 = d[j + 1];
                if (num2 != 0.0)
                {
                    for (int k = 0; k <= j; k++)
                    {
                        d[k] = V[k][j + 1] / num2;
                    }

                    for (int i = 0; i <= j; i++)
                    {
                        double num4 = 0.0;
                        for (int k = 0; k <= j; k++)
                        {
                            num4 += V[k][j + 1] * V[k][i];
                        }

                        for (int k = 0; k <= j; k++)
                        {
                            V[k][i] -= num4 * d[k];
                        }
                    }
                }

                for (int k = 0; k <= j; k++)
                {
                    V[k][j + 1] = 0.0;
                }
            }

            for (int i = 0; i < n; i++)
            {
                d[i] = V[n - 1][i];
                V[n - 1][i] = 0.0;
            }

            V[n - 1][n - 1] = 1.0;
            e[0] = 0.0;
        }

        private void tql2()
        {
            for (int i = 1; i < n; i++)
            {
                e[i - 1] = e[i];
            }

            e[n - 1] = 0.0;
            double num = 0.0;
            double num2 = 0.0;
            double num3 = Math.Pow(2.0, -52.0);
            for (int j = 0; j < n; j++)
            {
                num2 = Math.Max(num2, Math.Abs(d[j]) + Math.Abs(e[j]));
                int k;
                for (k = j; k < n; k++)
                {
                    if (Math.Abs(e[k]) <= num3 * num2)
                    {
                        break;
                    }
                }

                if (k > j)
                {
                    int num4 = 0;
                    do
                    {
                        num4++;
                        double num5 = d[j];
                        double num6 = (d[j + 1] - num5) / (2.0 * e[j]);
                        double num7 = Maths.hypot(num6, 1.0);
                        if (num6 < 0.0)
                        {
                            num7 = -num7;
                        }

                        d[j] = e[j] / (num6 + num7);
                        d[j + 1] = e[j] * (num6 + num7);
                        double num8 = d[j + 1];
                        double num9 = num5 - d[j];
                        for (int i = j + 2; i < n; i++)
                        {
                            d[i] -= num9;
                        }

                        num += num9;
                        num6 = d[k];
                        double num10 = 1.0;
                        double num11 = num10;
                        double num12 = num10;
                        double num13 = e[j + 1];
                        double num14 = 0.0;
                        double num15 = 0.0;
                        for (int i = k - 1; i >= j; i--)
                        {
                            num12 = num11;
                            num11 = num10;
                            num15 = num14;
                            num5 = num10 * e[i];
                            num9 = num10 * num6;
                            num7 = Maths.hypot(num6, e[i]);
                            e[i + 1] = num14 * num7;
                            num14 = e[i] / num7;
                            num10 = num6 / num7;
                            num6 = num10 * d[i] - num14 * num5;
                            d[i + 1] = num9 + num14 * (num10 * num5 + num14 * d[i]);
                            for (int l = 0; l < n; l++)
                            {
                                num9 = V[l][i + 1];
                                V[l][i + 1] = num14 * V[l][i] + num10 * num9;
                                V[l][i] = num10 * V[l][i] - num14 * num9;
                            }
                        }

                        num6 = -num14 * num15 * num12 * num13 * e[j] / num8;
                        e[j] = num14 * num6;
                        d[j] = num10 * num6;
                    } while (Math.Abs(e[j]) > num3 * num2);
                }

                d[j] = d[j] + num;
                e[j] = 0.0;
            }

            for (int i = 0; i < n - 1; i++)
            {
                int l = i;
                double num6 = d[i];
                for (int m = i + 1; m < n; m++)
                {
                    if (d[m] < num6)
                    {
                        l = m;
                        num6 = d[m];
                    }
                }

                if (l != i)
                {
                    d[l] = d[i];
                    d[i] = num6;
                    for (int m = 0; m < n; m++)
                    {
                        num6 = V[m][i];
                        V[m][i] = V[m][l];
                        V[m][l] = num6;
                    }
                }
            }
        }

        private void orthes()
        {
            int num = 0;
            int num2 = n - 1;
            for (int i = num + 1; i <= num2 - 1; i++)
            {
                double num3 = 0.0;
                for (int j = i; j <= num2; j++)
                {
                    num3 += Math.Abs(H[j][i - 1]);
                }

                if (num3 != 0.0)
                {
                    double num4 = 0.0;
                    for (int j = num2; j >= i; j--)
                    {
                        ort[j] = H[j][i - 1] / num3;
                        num4 += ort[j] * ort[j];
                    }

                    double num5 = Math.Sqrt(num4);
                    if (ort[i] > 0.0)
                    {
                        num5 = -num5;
                    }

                    num4 -= ort[i] * num5;
                    ort[i] = ort[i] - num5;
                    for (int k = i; k < n; k++)
                    {
                        double num6 = 0.0;
                        for (int j = num2; j >= i; j--)
                        {
                            num6 += ort[j] * H[j][k];
                        }

                        num6 /= num4;
                        for (int j = i; j <= num2; j++)
                        {
                            H[j][k] -= num6 * ort[j];
                        }
                    }

                    for (int j = 0; j <= num2; j++)
                    {
                        double num6 = 0.0;
                        for (int k = num2; k >= i; k--)
                        {
                            num6 += ort[k] * H[j][k];
                        }

                        num6 /= num4;
                        for (int k = i; k <= num2; k++)
                        {
                            H[j][k] -= num6 * ort[k];
                        }
                    }

                    ort[i] = num3 * ort[i];
                    H[i][i - 1] = num3 * num5;
                }
            }

            for (int j = 0; j < n; j++)
            {
                for (int k = 0; k < n; k++)
                {
                    V[j][k] = j == k ? 1.0 : 0.0;
                }
            }

            for (int i = num2 - 1; i >= num + 1; i--)
            {
                if (H[i][i - 1] != 0.0)
                {
                    for (int j = i + 1; j <= num2; j++)
                    {
                        ort[j] = H[j][i - 1];
                    }

                    for (int k = i; k <= num2; k++)
                    {
                        double num5 = 0.0;
                        for (int j = i; j <= num2; j++)
                        {
                            num5 += ort[j] * V[j][k];
                        }

                        num5 = num5 / ort[i] / H[i][i - 1];
                        for (int j = i; j <= num2; j++)
                        {
                            V[j][k] += num5 * ort[j];
                        }
                    }
                }
            }
        }

        private void cdiv(double xr, double xi, double yr, double yi)
        {
            if (Math.Abs(yr) > Math.Abs(yi))
            {
                double num = yi / yr;
                double num2 = yr + num * yi;
                cdivr = (xr + num * xi) / num2;
                cdivi = (xi - num * xr) / num2;
            }
            else
            {
                double num = yr / yi;
                double num2 = yi + num * yr;
                cdivr = (num * xr + xi) / num2;
                cdivi = (num * xi - xr) / num2;
            }
        }

        private void hqr2()
        {
            int num = this.n;
            int i = num - 1;
            int num2 = 0;
            int num3 = num - 1;
            double num4 = Math.Pow(2.0, -52.0);
            double num5 = 0.0;
            double num6 = 0.0;
            double num7 = 0.0;
            double num8 = 0.0;
            double num9 = 0.0;
            double num10 = 0.0;
            double num11 = 0.0;
            for (int j = 0; j < num; j++)
            {
                if ((j < num2) | (j > num3))
                {
                    d[j] = H[j][j];
                    e[j] = 0.0;
                }

                for (int k = Math.Max(j - 1, 0); k < num; k++)
                {
                    num11 += Math.Abs(H[j][k]);
                }
            }

            int num12 = 0;
            while (i >= num2)
            {
                int l;
                for (l = i; l > num2; l--)
                {
                    num9 = Math.Abs(H[l - 1][l - 1]) + Math.Abs(H[l][l]);
                    if (num9 == 0.0)
                    {
                        num9 = num11;
                    }

                    if (Math.Abs(H[l][l - 1]) < num4 * num9)
                    {
                        break;
                    }
                }

                if (l == i)
                {
                    H[i][i] = H[i][i] + num5;
                    d[i] = H[i][i];
                    e[i] = 0.0;
                    i--;
                    num12 = 0;
                }
                else
                {
                    if (l == i - 1)
                    {
                        double num13 = H[i][i - 1] * H[i - 1][i];
                        num6 = (H[i - 1][i - 1] - H[i][i]) / 2.0;
                        num7 = num6 * num6 + num13;
                        num10 = Math.Sqrt(Math.Abs(num7));
                        H[i][i] = H[i][i] + num5;
                        H[i - 1][i - 1] = H[i - 1][i - 1] + num5;
                        double num14 = H[i][i];
                        if (num7 >= 0.0)
                        {
                            if (num6 >= 0.0)
                            {
                                num10 = num6 + num10;
                            }
                            else
                            {
                                num10 = num6 - num10;
                            }

                            d[i - 1] = num14 + num10;
                            d[i] = d[i - 1];
                            if (num10 != 0.0)
                            {
                                d[i] = num14 - num13 / num10;
                            }

                            e[i - 1] = 0.0;
                            e[i] = 0.0;
                            num14 = H[i][i - 1];
                            num9 = Math.Abs(num14) + Math.Abs(num10);
                            num6 = num14 / num9;
                            num7 = num10 / num9;
                            num8 = Math.Sqrt(num6 * num6 + num7 * num7);
                            num6 /= num8;
                            num7 /= num8;
                            for (int k = i - 1; k < num; k++)
                            {
                                num10 = H[i - 1][k];
                                H[i - 1][k] = num7 * num10 + num6 * H[i][k];
                                H[i][k] = num7 * H[i][k] - num6 * num10;
                            }

                            for (int j = 0; j <= i; j++)
                            {
                                num10 = H[j][i - 1];
                                H[j][i - 1] = num7 * num10 + num6 * H[j][i];
                                H[j][i] = num7 * H[j][i] - num6 * num10;
                            }

                            for (int j = num2; j <= num3; j++)
                            {
                                num10 = V[j][i - 1];
                                V[j][i - 1] = num7 * num10 + num6 * V[j][i];
                                V[j][i] = num7 * V[j][i] - num6 * num10;
                            }
                        }
                        else
                        {
                            d[i - 1] = num14 + num6;
                            d[i] = num14 + num6;
                            e[i - 1] = num10;
                            e[i] = -num10;
                        }

                        i -= 2;
                        num12 = 0;
                    }
                    else
                    {
                        double num14 = H[i][i];
                        double num15 = 0.0;
                        double num13 = 0.0;
                        if (l < i)
                        {
                            num15 = H[i - 1][i - 1];
                            num13 = H[i][i - 1] * H[i - 1][i];
                        }

                        if (num12 == 10)
                        {
                            num5 += num14;
                            for (int j = num2; j <= i; j++)
                            {
                                H[j][j] -= num14;
                            }

                            num9 = Math.Abs(H[i][i - 1]) + Math.Abs(H[i - 1][i - 2]);
                            num15 = num14 = 0.75 * num9;
                            num13 = -0.4375 * num9 * num9;
                        }

                        if (num12 == 30)
                        {
                            num9 = (num15 - num14) / 2.0;
                            num9 = num9 * num9 + num13;
                            if (num9 > 0.0)
                            {
                                num9 = Math.Sqrt(num9);
                                if (num15 < num14)
                                {
                                    num9 = -num9;
                                }

                                num9 = num14 - num13 / ((num15 - num14) / 2.0 + num9);
                                for (int j = num2; j <= i; j++)
                                {
                                    H[j][j] -= num9;
                                }

                                num5 += num9;
                                num15 = num14 = num13 = 0.964;
                            }
                        }

                        num12++;
                        int m;
                        for (m = i - 2; m >= l; m--)
                        {
                            num10 = H[m][m];
                            num8 = num14 - num10;
                            num9 = num15 - num10;
                            num6 = (num8 * num9 - num13) / H[m + 1][m] + H[m][m + 1];
                            num7 = H[m + 1][m + 1] - num10 - num8 - num9;
                            num8 = H[m + 2][m + 1];
                            num9 = Math.Abs(num6) + Math.Abs(num7) + Math.Abs(num8);
                            num6 /= num9;
                            num7 /= num9;
                            num8 /= num9;
                            if (m == l)
                            {
                                break;
                            }

                            if (Math.Abs(H[m][m - 1]) * (Math.Abs(num7) + Math.Abs(num8)) < num4 *
                                (Math.Abs(num6) * (Math.Abs(H[m - 1][m - 1]) + Math.Abs(num10) +
                                                   Math.Abs(H[m + 1][m + 1]))))
                            {
                                break;
                            }
                        }

                        for (int j = m + 2; j <= i; j++)
                        {
                            H[j][j - 2] = 0.0;
                            if (j > m + 2)
                            {
                                H[j][j - 3] = 0.0;
                            }
                        }

                        for (int n = m; n <= i - 1; n++)
                        {
                            bool flag = n != i - 1;
                            if (n != m)
                            {
                                num6 = H[n][n - 1];
                                num7 = H[n + 1][n - 1];
                                num8 = flag ? H[n + 2][n - 1] : 0.0;
                                num14 = Math.Abs(num6) + Math.Abs(num7) + Math.Abs(num8);
                                if (num14 != 0.0)
                                {
                                    num6 /= num14;
                                    num7 /= num14;
                                    num8 /= num14;
                                }
                            }

                            if (num14 == 0.0)
                            {
                                break;
                            }

                            num9 = Math.Sqrt(num6 * num6 + num7 * num7 + num8 * num8);
                            if (num6 < 0.0)
                            {
                                num9 = -num9;
                            }

                            if (num9 != 0.0)
                            {
                                if (n != m)
                                {
                                    H[n][n - 1] = -num9 * num14;
                                }
                                else
                                {
                                    if (l != m)
                                    {
                                        H[n][n - 1] = -H[n][n - 1];
                                    }
                                }

                                num6 += num9;
                                num14 = num6 / num9;
                                num15 = num7 / num9;
                                num10 = num8 / num9;
                                num7 /= num6;
                                num8 /= num6;
                                for (int k = n; k < num; k++)
                                {
                                    num6 = H[n][k] + num7 * H[n + 1][k];
                                    if (flag)
                                    {
                                        num6 += num8 * H[n + 2][k];
                                        H[n + 2][k] = H[n + 2][k] - num6 * num10;
                                    }

                                    H[n][k] = H[n][k] - num6 * num14;
                                    H[n + 1][k] = H[n + 1][k] - num6 * num15;
                                }

                                for (int j = 0; j <= Math.Min(i, n + 3); j++)
                                {
                                    num6 = num14 * H[j][n] + num15 * H[j][n + 1];
                                    if (flag)
                                    {
                                        num6 += num10 * H[j][n + 2];
                                        H[j][n + 2] = H[j][n + 2] - num6 * num8;
                                    }

                                    H[j][n] = H[j][n] - num6;
                                    H[j][n + 1] = H[j][n + 1] - num6 * num7;
                                }

                                for (int j = num2; j <= num3; j++)
                                {
                                    num6 = num14 * V[j][n] + num15 * V[j][n + 1];
                                    if (flag)
                                    {
                                        num6 += num10 * V[j][n + 2];
                                        V[j][n + 2] = V[j][n + 2] - num6 * num8;
                                    }

                                    V[j][n] = V[j][n] - num6;
                                    V[j][n + 1] = V[j][n + 1] - num6 * num7;
                                }
                            }
                        }
                    }
                }
            }

            if (num11 != 0.0)
            {
                for (i = num - 1; i >= 0; i--)
                {
                    num6 = d[i];
                    num7 = e[i];
                    if (num7 == 0.0)
                    {
                        int l = i;
                        H[i][i] = 1.0;
                        for (int j = i - 1; j >= 0; j--)
                        {
                            double num13 = H[j][j] - num6;
                            num8 = 0.0;
                            for (int k = l; k <= i; k++)
                            {
                                num8 += H[j][k] * H[k][i];
                            }

                            if (e[j] < 0.0)
                            {
                                num10 = num13;
                                num9 = num8;
                            }
                            else
                            {
                                l = j;
                                double num16;
                                if (e[j] == 0.0)
                                {
                                    if (num13 != 0.0)
                                    {
                                        H[j][i] = -num8 / num13;
                                    }
                                    else
                                    {
                                        H[j][i] = -num8 / (num4 * num11);
                                    }
                                }
                                else
                                {
                                    double num14 = H[j][j + 1];
                                    double num15 = H[j + 1][j];
                                    num7 = (d[j] - num6) * (d[j] - num6) + e[j] * e[j];
                                    num16 = (num14 * num9 - num10 * num8) / num7;
                                    H[j][i] = num16;
                                    if (Math.Abs(num14) > Math.Abs(num10))
                                    {
                                        H[j + 1][i] = (-num8 - num13 * num16) / num14;
                                    }
                                    else
                                    {
                                        H[j + 1][i] = (-num9 - num15 * num16) / num10;
                                    }
                                }

                                num16 = Math.Abs(H[j][i]);
                                if (num4 * num16 * num16 > 1.0)
                                {
                                    for (int k = j; k <= i; k++)
                                    {
                                        H[k][i] = H[k][i] / num16;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (num7 < 0.0)
                        {
                            int l = i - 1;
                            if (Math.Abs(H[i][i - 1]) > Math.Abs(H[i - 1][i]))
                            {
                                H[i - 1][i - 1] = num7 / H[i][i - 1];
                                H[i - 1][i] = -(H[i][i] - num6) / H[i][i - 1];
                            }
                            else
                            {
                                cdiv(0.0, -H[i - 1][i], H[i - 1][i - 1] - num6, num7);
                                H[i - 1][i - 1] = cdivr;
                                H[i - 1][i] = cdivi;
                            }

                            H[i][i - 1] = 0.0;
                            H[i][i] = 1.0;
                            for (int j = i - 2; j >= 0; j--)
                            {
                                double num17 = 0.0;
                                double num18 = 0.0;
                                for (int k = l; k <= i; k++)
                                {
                                    num17 += H[j][k] * H[k][i - 1];
                                    num18 += H[j][k] * H[k][i];
                                }

                                double num13 = H[j][j] - num6;
                                if (e[j] < 0.0)
                                {
                                    num10 = num13;
                                    num8 = num17;
                                    num9 = num18;
                                }
                                else
                                {
                                    l = j;
                                    if (e[j] == 0.0)
                                    {
                                        cdiv(-num17, -num18, num13, num7);
                                        H[j][i - 1] = cdivr;
                                        H[j][i] = cdivi;
                                    }
                                    else
                                    {
                                        double num14 = H[j][j + 1];
                                        double num15 = H[j + 1][j];
                                        double num19 = (d[j] - num6) * (d[j] - num6) + e[j] * e[j] -
                                                       num7 * num7;
                                        double num20 = (d[j] - num6) * 2.0 * num7;
                                        if ((num19 == 0.0) & (num20 == 0.0))
                                        {
                                            num19 = num4 * num11 *
                                                    (Math.Abs(num13) + Math.Abs(num7) + Math.Abs(num14) +
                                                     Math.Abs(num15) + Math.Abs(num10));
                                        }

                                        cdiv(num14 * num8 - num10 * num17 + num7 * num18,
                                            num14 * num9 - num10 * num18 - num7 * num17,
                                            num19,
                                            num20);
                                        H[j][i - 1] = cdivr;
                                        H[j][i] = cdivi;
                                        if (Math.Abs(num14) > Math.Abs(num10) + Math.Abs(num7))
                                        {
                                            H[j + 1][i - 1] =
                                                (-num17 - num13 * H[j][i - 1] + num7 * H[j][i]) / num14;
                                            H[j + 1][i] =
                                                (-num18 - num13 * H[j][i] - num7 * H[j][i - 1]) / num14;
                                        }
                                        else
                                        {
                                            cdiv(-num8 - num15 * H[j][i - 1],
                                                -num9 - num15 * H[j][i],
                                                num10,
                                                num7);
                                            H[j + 1][i - 1] = cdivr;
                                            H[j + 1][i] = cdivi;
                                        }
                                    }

                                    double num16 = Math.Max(Math.Abs(H[j][i - 1]), Math.Abs(H[j][i]));
                                    if (num4 * num16 * num16 > 1.0)
                                    {
                                        for (int k = j; k <= i; k++)
                                        {
                                            H[k][i - 1] = H[k][i - 1] / num16;
                                            H[k][i] = H[k][i] / num16;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                for (int j = 0; j < num; j++)
                {
                    if ((j < num2) | (j > num3))
                    {
                        for (int k = j; k < num; k++)
                        {
                            V[j][k] = H[j][k];
                        }
                    }
                }

                for (int k = num - 1; k >= num2; k--)
                {
                    for (int j = num2; j <= num3; j++)
                    {
                        num10 = 0.0;
                        for (int n = num2; n <= Math.Min(k, num3); n++)
                        {
                            num10 += V[j][n] * H[n][k];
                        }

                        V[j][k] = num10;
                    }
                }
            }
        }

        public EigenvalueDecomposition(JamaMatrix Arg)
        {
            double[][] array = Arg.Array;
            n = Arg.ColumnDimension;
            V = new double[n][];
            for (int i = 0; i < n; i++)
            {
                V[i] = new double[n];
            }

            d = new double[n];
            e = new double[n];
            issymmetric = true;
            int j = 0;
            while ((j < n) & issymmetric)
            {
                int i = 0;
                while ((i < n) & issymmetric)
                {
                    issymmetric = array[i][j] == array[j][i];
                    i++;
                }

                j++;
            }

            if (issymmetric)
            {
                for (int i = 0; i < n; i++)
                {
                    for (j = 0; j < n; j++)
                    {
                        V[i][j] = array[i][j];
                    }
                }

                tred2();
                tql2();
            }
            else
            {
                double[][] array2 = new double[n][];
                for (int k = 0; k < n; k++)
                {
                    array2[k] = new double[n];
                }

                H = array2;
                ort = new double[n];
                for (j = 0; j < n; j++)
                {
                    for (int i = 0; i < n; i++)
                    {
                        H[i][j] = array[i][j];
                    }
                }

                orthes();
                hqr2();
            }
        }

        public virtual JamaMatrix getV()
        {
            return new JamaMatrix(V, n, n);
        }
    }
}
