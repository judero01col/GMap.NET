using System;

namespace Jama
{
    [Serializable]
    public class CholeskyDecomposition
    {
        private double[][] L;
        private int n;
        private bool isspd;

        public virtual bool SPD
        {
            get
            {
                return isspd;
            }
        }

        public CholeskyDecomposition(JamaMatrix Arg)
        {
            double[][] array = Arg.Array;
            n = Arg.RowDimension;
            L = new double[n][];
            for (int i = 0; i < n; i++)
            {
                L[i] = new double[n];
            }

            isspd = Arg.ColumnDimension == n;
            for (int j = 0; j < n; j++)
            {
                double[] array2 = L[j];
                double num = 0.0;
                for (int k = 0; k < j; k++)
                {
                    double[] array3 = L[k];
                    double num2 = 0.0;
                    for (int i = 0; i < k; i++)
                    {
                        num2 += array3[i] * array2[i];
                    }

                    num2 = array2[k] = (array[j][k] - num2) / L[k][k];
                    num += num2 * num2;
                    isspd &= array[k][j] == array[j][k];
                }

                num = array[j][j] - num;
                isspd &= num > 0.0;
                L[j][j] = Math.Sqrt(Math.Max(num, 0.0));
                for (int k = j + 1; k < n; k++)
                {
                    L[j][k] = 0.0;
                }
            }
        }

        public virtual JamaMatrix getL()
        {
            return new JamaMatrix(L, n, n);
        }

        public virtual JamaMatrix solve(JamaMatrix B)
        {
            if (B.RowDimension != n)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }

            if (!isspd)
            {
                throw new SystemException("Matrix is not symmetric positive definite.");
            }

            double[][] arrayCopy = B.ArrayCopy;
            int columnDimension = B.ColumnDimension;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < columnDimension; j++)
                {
                    for (int k = 0; k < i; k++)
                    {
                        arrayCopy[i][j] -= arrayCopy[k][j] * L[i][k];
                    }

                    arrayCopy[i][j] /= L[i][i];
                }
            }

            for (int i = n - 1; i >= 0; i--)
            {
                for (int j = 0; j < columnDimension; j++)
                {
                    for (int k = i + 1; k < n; k++)
                    {
                        arrayCopy[i][j] -= arrayCopy[k][j] * L[k][i];
                    }

                    arrayCopy[i][j] /= L[i][i];
                }
            }

            return new JamaMatrix(arrayCopy, n, columnDimension);
        }
    }
}
