using System;

namespace Jama
{
    [Serializable]
    public class LUDecomposition
    {
        private double[][] LU;
        private int m;
        private int n;
        private int pivsign;
        private int[] piv;

        public virtual bool Nonsingular
        {
            get
            {
                bool result;
                for (int i = 0; i < n; i++)
                {
                    if (LU[i][i] == 0.0)
                    {
                        result = false;
                        return result;
                    }
                }

                result = true;
                return result;
            }
        }

        public virtual JamaMatrix L
        {
            get
            {
                JamaMatrix jamaMatrix = new JamaMatrix(m, n);
                double[][] array = jamaMatrix.Array;
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (i > j)
                        {
                            array[i][j] = LU[i][j];
                        }
                        else
                        {
                            if (i == j)
                            {
                                array[i][j] = 1.0;
                            }
                            else
                            {
                                array[i][j] = 0.0;
                            }
                        }
                    }
                }

                return jamaMatrix;
            }
        }

        public virtual JamaMatrix U
        {
            get
            {
                JamaMatrix jamaMatrix = new JamaMatrix(n, n);
                double[][] array = jamaMatrix.Array;
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (i <= j)
                        {
                            array[i][j] = LU[i][j];
                        }
                        else
                        {
                            array[i][j] = 0.0;
                        }
                    }
                }

                return jamaMatrix;
            }
        }

        public virtual int[] Pivot
        {
            get
            {
                int[] array = new int[m];
                for (int i = 0; i < m; i++)
                {
                    array[i] = piv[i];
                }

                return array;
            }
        }

        public virtual double[] DoublePivot
        {
            get
            {
                double[] array = new double[m];
                for (int i = 0; i < m; i++)
                {
                    array[i] = piv[i];
                }

                return array;
            }
        }

        public LUDecomposition(JamaMatrix A)
        {
            LU = A.ArrayCopy;
            m = A.RowDimension;
            n = A.ColumnDimension;
            piv = new int[m];
            for (int i = 0; i < m; i++)
            {
                piv[i] = i;
            }

            pivsign = 1;
            double[] array = new double[m];
            for (int j = 0; j < n; j++)
            {
                for (int i = 0; i < m; i++)
                {
                    array[i] = LU[i][j];
                }

                for (int i = 0; i < m; i++)
                {
                    double[] array2 = LU[i];
                    int num = Math.Min(i, j);
                    double num2 = 0.0;
                    for (int k = 0; k < num; k++)
                    {
                        num2 += array2[k] * array[k];
                    }

                    array2[j] = array[i] -= num2;
                }

                int num3 = j;
                for (int i = j + 1; i < m; i++)
                {
                    if (Math.Abs(array[i]) > Math.Abs(array[num3]))
                    {
                        num3 = i;
                    }
                }

                if (num3 != j)
                {
                    for (int k = 0; k < n; k++)
                    {
                        double num4 = LU[num3][k];
                        LU[num3][k] = LU[j][k];
                        LU[j][k] = num4;
                    }

                    int num5 = piv[num3];
                    piv[num3] = piv[j];
                    piv[j] = num5;
                    pivsign = -pivsign;
                }

                if ((j < m) & (LU[j][j] != 0.0))
                {
                    for (int i = j + 1; i < m; i++)
                    {
                        LU[i][j] /= LU[j][j];
                    }
                }
            }
        }

        public virtual double det()
        {
            if (m != n)
            {
                throw new ArgumentException("Matrix must be square.");
            }

            double num = pivsign;
            for (int i = 0; i < n; i++)
            {
                num *= LU[i][i];
            }

            return num;
        }

        public virtual JamaMatrix solve(JamaMatrix B)
        {
            if (B.RowDimension != m)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }

            if (!Nonsingular)
            {
                throw new CorrespondencesAreSingularException();
            }

            int columnDimension = B.ColumnDimension;
            JamaMatrix matrix = B.getMatrix(piv, 0, columnDimension - 1);
            double[][] array = matrix.Array;
            for (int i = 0; i < n; i++)
            {
                for (int j = i + 1; j < n; j++)
                {
                    for (int k = 0; k < columnDimension; k++)
                    {
                        array[j][k] -= array[i][k] * LU[j][i];
                    }
                }
            }

            for (int i = n - 1; i >= 0; i--)
            {
                for (int k = 0; k < columnDimension; k++)
                {
                    array[i][k] /= LU[i][i];
                }

                for (int j = 0; j < i; j++)
                {
                    for (int k = 0; k < columnDimension; k++)
                    {
                        array[j][k] -= array[i][k] * LU[j][i];
                    }
                }
            }

            return matrix;
        }
    }
}
