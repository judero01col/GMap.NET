using System;
using Jama.util;

namespace Jama
{
    [Serializable]
    public class QRDecomposition
    {
        private double[][] QR;
        private int m;
        private int n;
        private double[] Rdiag;

        public virtual bool FullRank
        {
            get
            {
                bool result;
                for (int i = 0; i < n; i++)
                {
                    if (Rdiag[i] == 0.0)
                    {
                        result = false;
                        return result;
                    }
                }

                result = true;
                return result;
            }
        }

        public virtual JamaMatrix H
        {
            get
            {
                JamaMatrix jamaMatrix = new JamaMatrix(m, n);
                double[][] array = jamaMatrix.Array;
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (i >= j)
                        {
                            array[i][j] = QR[i][j];
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

        public virtual JamaMatrix R
        {
            get
            {
                JamaMatrix jamaMatrix = new JamaMatrix(n, n);
                double[][] array = jamaMatrix.Array;
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (i < j)
                        {
                            array[i][j] = QR[i][j];
                        }
                        else
                        {
                            if (i == j)
                            {
                                array[i][j] = Rdiag[i];
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

        public virtual JamaMatrix Q
        {
            get
            {
                JamaMatrix jamaMatrix = new JamaMatrix(m, n);
                double[][] array = jamaMatrix.Array;
                for (int i = n - 1; i >= 0; i--)
                {
                    for (int j = 0; j < m; j++)
                    {
                        array[j][i] = 0.0;
                    }

                    array[i][i] = 1.0;
                    for (int k = i; k < n; k++)
                    {
                        if (QR[i][i] != 0.0)
                        {
                            double num = 0.0;
                            for (int j = i; j < m; j++)
                            {
                                num += QR[j][i] * array[j][k];
                            }

                            num = -num / QR[i][i];
                            for (int j = i; j < m; j++)
                            {
                                array[j][k] += num * QR[j][i];
                            }
                        }
                    }
                }

                return jamaMatrix;
            }
        }

        public QRDecomposition(JamaMatrix A)
        {
            QR = A.ArrayCopy;
            m = A.RowDimension;
            n = A.ColumnDimension;
            Rdiag = new double[n];
            for (int i = 0; i < n; i++)
            {
                double num = 0.0;
                for (int j = i; j < m; j++)
                {
                    num = Maths.hypot(num, QR[j][i]);
                }

                if (num != 0.0)
                {
                    if (QR[i][i] < 0.0)
                    {
                        num = -num;
                    }

                    for (int j = i; j < m; j++)
                    {
                        QR[j][i] /= num;
                    }

                    QR[i][i] += 1.0;
                    for (int k = i + 1; k < n; k++)
                    {
                        double num2 = 0.0;
                        for (int j = i; j < m; j++)
                        {
                            num2 += QR[j][i] * QR[j][k];
                        }

                        num2 = -num2 / QR[i][i];
                        for (int j = i; j < m; j++)
                        {
                            QR[j][k] += num2 * QR[j][i];
                        }
                    }
                }

                Rdiag[i] = -num;
            }
        }

        public virtual JamaMatrix solve(JamaMatrix B)
        {
            if (B.RowDimension != m)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }

            if (!FullRank)
            {
                throw new SystemException("Matrix is rank deficient.");
            }

            int columnDimension = B.ColumnDimension;
            double[][] arrayCopy = B.ArrayCopy;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < columnDimension; j++)
                {
                    double num = 0.0;
                    for (int k = i; k < m; k++)
                    {
                        num += QR[k][i] * arrayCopy[k][j];
                    }

                    num = -num / QR[i][i];
                    for (int k = i; k < m; k++)
                    {
                        arrayCopy[k][j] += num * QR[k][i];
                    }
                }
            }

            for (int i = n - 1; i >= 0; i--)
            {
                for (int j = 0; j < columnDimension; j++)
                {
                    arrayCopy[i][j] /= Rdiag[i];
                }

                for (int k = 0; k < i; k++)
                {
                    for (int j = 0; j < columnDimension; j++)
                    {
                        arrayCopy[k][j] -= arrayCopy[i][j] * QR[k][i];
                    }
                }
            }

            return new JamaMatrix(arrayCopy, n, columnDimension).getMatrix(0, n - 1, 0, columnDimension - 1);
        }
    }
}
