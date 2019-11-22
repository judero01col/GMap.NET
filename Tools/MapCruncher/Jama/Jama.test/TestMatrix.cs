using System;

namespace Jama.test
{
    public class TestMatrix
    {
        [STAThread]
        public static void Main2(string[] argv)
        {
            int num = 0;
            int value = 0;
            var array = new[] {1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0};
            var y = new[] {1.0, 4.0, 7.0, 10.0, 2.0, 5.0, 8.0, 11.0, 3.0, 6.0, 9.0, 12.0};
            var array2 = new[]
            {
                new[] {1.0, 4.0, 7.0, 10.0}, new[] {2.0, 5.0, 8.0, 11.0},
                new[] {3.0, 6.0, 9.0, 12.0}
            };
            var a = array2;
            var a2 = new[]
            {
                new[] {1.0, 2.0, 3.0}, new[] {4.0, 5.0, 6.0}, new[] {7.0, 8.0, 9.0},
                new[] {10.0, 11.0, 12.0}
            };
            var a3 = new[] {new[] {5.0, 8.0, 11.0}, new[] {6.0, 9.0, 12.0}};
            var a4 = new[]
            {
                new[] {1.0, 4.0, 7.0}, new[] {2.0, 5.0, 8.0, 11.0}, new[] {3.0, 6.0, 9.0, 12.0}
            };
            var a5 = new[]
            {
                new[] {4.0, 1.0, 1.0}, new[] {1.0, 2.0, 3.0}, new[] {1.0, 3.0, 6.0}
            };
            var array3 = new double[3][];
            var arg_1CC_0 = array3;
            int arg_1CC_1 = 0;
            var array4 = new double[4];
            array4[0] = 1.0;
            arg_1CC_0[arg_1CC_1] = array4;
            var arg_1E7_0 = array3;
            int arg_1E7_1 = 1;
            array4 = new double[4];
            array4[1] = 1.0;
            arg_1E7_0[arg_1E7_1] = array4;
            var arg_202_0 = array3;
            int arg_202_1 = 2;
            array4 = new double[4];
            array4[2] = 1.0;
            arg_202_0[arg_202_1] = array4;
            var a6 = array3;
            array3 = new double[4][];
            var arg_229_0 = array3;
            int arg_229_1 = 0;
            array4 = new double[4];
            array4[1] = 1.0;
            arg_229_0[arg_229_1] = array4;
            var arg_251_0 = array3;
            int arg_251_1 = 1;
            array4 = new double[4];
            array4[0] = 1.0;
            array4[2] = 2E-07;
            arg_251_0[arg_251_1] = array4;
            array3[2] = new[] {0.0, -2E-07, 0.0, 1.0};
            var arg_294_0 = array3;
            int arg_294_1 = 3;
            array4 = new double[4];
            array4[2] = 1.0;
            arg_294_0[arg_294_1] = array4;
            var a7 = array3;
            var a8 = new[]
            {
                new[] {166.0, 188.0, 210.0}, new[] {188.0, 214.0, 240.0},
                new[] {210.0, 240.0, 270.0}
            };
            var a9 = new[] {new[] {13.0}, new[] {15.0}};
            var a10 = new[] {new[] {1.0, 3.0}, new[] {7.0, 9.0}};
            int num2 = 3;
            int num3 = 4;
            int m = 5;
            int i = 0;
            int j = 4;
            int m2 = 3;
            int m3 = 4;
            int num4 = 1;
            int num5 = 2;
            int num6 = 1;
            int num7 = 3;
            var r = new[] {1, 2};
            var r2 = new[] {1, 3};
            var c = new[] {1, 2, 3};
            var c2 = new[] {1, 2, 4};
            double y2 = 33.0;
            double y3 = 30.0;
            double y4 = 15.0;
            double d = 650.0;
            print("\nTesting constructors and constructor-like methods...\n");
            JamaMatrix jamaMatrix;
            try
            {
                jamaMatrix = new JamaMatrix(array, m);
                num = try_failure(num,
                    "Catch invalid length in packed constructor... ",
                    "exception not thrown for invalid input");
            }
            catch (ArgumentException ex)
            {
                try_success("Catch invalid length in packed constructor... ", ex.Message);
            }

            double num8;
            try
            {
                jamaMatrix = new JamaMatrix(a4);
                num8 = jamaMatrix.get_Renamed(i, j);
            }
            catch (ArgumentException ex)
            {
                try_success("Catch ragged input to default constructor... ", ex.Message);
            }
            catch (IndexOutOfRangeException)
            {
                num = try_failure(num,
                    "Catch ragged input to constructor... ",
                    "exception not thrown in construction...ArrayIndexOutOfBoundsException thrown later");
            }

            try
            {
                jamaMatrix = JamaMatrix.constructWithCopy(a4);
                num8 = jamaMatrix.get_Renamed(i, j);
            }
            catch (ArgumentException ex)
            {
                try_success("Catch ragged input to constructWithCopy... ", ex.Message);
            }
            catch (IndexOutOfRangeException)
            {
                num = try_failure(num,
                    "Catch ragged input to constructWithCopy... ",
                    "exception not thrown in construction...ArrayIndexOutOfBoundsException thrown later");
            }

            jamaMatrix = new JamaMatrix(array, m2);
            var jamaMatrix2 = new JamaMatrix(array2);
            num8 = jamaMatrix2.get_Renamed(0, 0);
            array2[0][0] = 0.0;
            var jamaMatrix3 = jamaMatrix2.minus(jamaMatrix);
            array2[0][0] = num8;
            jamaMatrix2 = JamaMatrix.constructWithCopy(array2);
            num8 = jamaMatrix2.get_Renamed(0, 0);
            array2[0][0] = 0.0;
            if (num8 - jamaMatrix2.get_Renamed(0, 0) != 0.0)
            {
                num = try_failure(num, "constructWithCopy... ", "copy not effected... data visible outside");
            }
            else
            {
                try_success("constructWithCopy... ", "");
            }

            array2[0][0] = array[0];
            var x = new JamaMatrix(a6);
            try
            {
                check(x, JamaMatrix.identity(3, 4));
                try_success("identity... ", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "identity... ", "identity Matrix not successfully created");
            }

            print("\nTesting access methods...\n");
            jamaMatrix2 = new JamaMatrix(array2);
            if (jamaMatrix2.RowDimension != num2)
            {
                num = try_failure(num, "getRowDimension... ", "");
            }
            else
            {
                try_success("getRowDimension... ", "");
            }

            if (jamaMatrix2.ColumnDimension != num3)
            {
                num = try_failure(num, "getColumnDimension... ", "");
            }
            else
            {
                try_success("getColumnDimension... ", "");
            }

            jamaMatrix2 = new JamaMatrix(array2);
            var array5 = jamaMatrix2.Array;
            if (array5 != array2)
            {
                num = try_failure(num, "getArray... ", "");
            }
            else
            {
                try_success("getArray... ", "");
            }

            array5 = jamaMatrix2.ArrayCopy;
            if (array5 == array2)
            {
                num = try_failure(num, "getArrayCopy... ", "data not (deep) copied");
            }

            try
            {
                check(array5, array2);
                try_success("getArrayCopy... ", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "getArrayCopy... ", "data not successfully (deep) copied");
            }

            var x2 = jamaMatrix2.ColumnPackedCopy;
            try
            {
                check(x2, array);
                try_success("getColumnPackedCopy... ", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "getColumnPackedCopy... ", "data not successfully (deep) copied by columns");
            }

            x2 = jamaMatrix2.RowPackedCopy;
            try
            {
                check(x2, y);
                try_success("getRowPackedCopy... ", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "getRowPackedCopy... ", "data not successfully (deep) copied by rows");
            }

            try
            {
                num8 = jamaMatrix2.get_Renamed(jamaMatrix2.RowDimension, jamaMatrix2.ColumnDimension - 1);
                num = try_failure(num, "get(int,int)... ", "OutOfBoundsException expected but not thrown");
            }
            catch (IndexOutOfRangeException)
            {
                try
                {
                    num8 = jamaMatrix2.get_Renamed(jamaMatrix2.RowDimension - 1, jamaMatrix2.ColumnDimension);
                    num = try_failure(num, "get(int,int)... ", "OutOfBoundsException expected but not thrown");
                }
                catch (IndexOutOfRangeException)
                {
                    try_success("get(int,int)... OutofBoundsException... ", "");
                }
            }
            catch (ArgumentException)
            {
                num = try_failure(num, "get(int,int)... ", "OutOfBoundsException expected but not thrown");
            }

            try
            {
                if (jamaMatrix2.get_Renamed(jamaMatrix2.RowDimension - 1, jamaMatrix2.ColumnDimension - 1) !=
                    array2[jamaMatrix2.RowDimension - 1][jamaMatrix2.ColumnDimension - 1])
                {
                    num = try_failure(num, "get(int,int)... ", "Matrix entry (i,j) not successfully retreived");
                }
                else
                {
                    try_success("get(int,int)... ", "");
                }
            }
            catch (IndexOutOfRangeException)
            {
                num = try_failure(num, "get(int,int)... ", "Unexpected ArrayIndexOutOfBoundsException");
            }

            var jamaMatrix4 = new JamaMatrix(a3);
            JamaMatrix jamaMatrix5;
            try
            {
                jamaMatrix5 = jamaMatrix2.getMatrix(num4, num5 + jamaMatrix2.RowDimension + 1, num6, num7);
                num = try_failure(num,
                    "getMatrix(int,int,int,int)... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }
            catch (IndexOutOfRangeException)
            {
                try
                {
                    jamaMatrix5 = jamaMatrix2.getMatrix(num4, num5, num6, num7 + jamaMatrix2.ColumnDimension + 1);
                    num = try_failure(num,
                        "getMatrix(int,int,int,int)... ",
                        "ArrayIndexOutOfBoundsException expected but not thrown");
                }
                catch (IndexOutOfRangeException)
                {
                    try_success("getMatrix(int,int,int,int)... ArrayIndexOutOfBoundsException... ", "");
                }
            }
            catch (ArgumentException)
            {
                num = try_failure(num,
                    "getMatrix(int,int,int,int)... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }

            try
            {
                jamaMatrix5 = jamaMatrix2.getMatrix(num4, num5, num6, num7);
                try
                {
                    check(jamaMatrix4, jamaMatrix5);
                    try_success("getMatrix(int,int,int,int)... ", "");
                }
                catch (SystemException)
                {
                    num = try_failure(num, "getMatrix(int,int,int,int)... ", "submatrix not successfully retreived");
                }
            }
            catch (IndexOutOfRangeException)
            {
                num = try_failure(num, "getMatrix(int,int,int,int)... ", "Unexpected ArrayIndexOutOfBoundsException");
            }

            try
            {
                jamaMatrix5 = jamaMatrix2.getMatrix(num4, num5, c2);
                num = try_failure(num,
                    "getMatrix(int,int,int[])... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }
            catch (IndexOutOfRangeException)
            {
                try
                {
                    jamaMatrix5 = jamaMatrix2.getMatrix(num4, num5 + jamaMatrix2.RowDimension + 1, c);
                    num = try_failure(num,
                        "getMatrix(int,int,int[])... ",
                        "ArrayIndexOutOfBoundsException expected but not thrown");
                }
                catch (IndexOutOfRangeException)
                {
                    try_success("getMatrix(int,int,int[])... ArrayIndexOutOfBoundsException... ", "");
                }
            }
            catch (ArgumentException)
            {
                num = try_failure(num,
                    "getMatrix(int,int,int[])... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }

            try
            {
                jamaMatrix5 = jamaMatrix2.getMatrix(num4, num5, c);
                try
                {
                    check(jamaMatrix4, jamaMatrix5);
                    try_success("getMatrix(int,int,int[])... ", "");
                }
                catch (SystemException)
                {
                    num = try_failure(num, "getMatrix(int,int,int[])... ", "submatrix not successfully retreived");
                }
            }
            catch (IndexOutOfRangeException)
            {
                num = try_failure(num, "getMatrix(int,int,int[])... ", "Unexpected ArrayIndexOutOfBoundsException");
            }

            try
            {
                jamaMatrix5 = jamaMatrix2.getMatrix(r2, num6, num7);
                num = try_failure(num,
                    "getMatrix(int[],int,int)... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }
            catch (IndexOutOfRangeException)
            {
                try
                {
                    jamaMatrix5 = jamaMatrix2.getMatrix(r, num6, num7 + jamaMatrix2.ColumnDimension + 1);
                    num = try_failure(num,
                        "getMatrix(int[],int,int)... ",
                        "ArrayIndexOutOfBoundsException expected but not thrown");
                }
                catch (IndexOutOfRangeException)
                {
                    try_success("getMatrix(int[],int,int)... ArrayIndexOutOfBoundsException... ", "");
                }
            }
            catch (ArgumentException)
            {
                num = try_failure(num,
                    "getMatrix(int[],int,int)... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }

            try
            {
                jamaMatrix5 = jamaMatrix2.getMatrix(r, num6, num7);
                try
                {
                    check(jamaMatrix4, jamaMatrix5);
                    try_success("getMatrix(int[],int,int)... ", "");
                }
                catch (SystemException)
                {
                    num = try_failure(num, "getMatrix(int[],int,int)... ", "submatrix not successfully retreived");
                }
            }
            catch (IndexOutOfRangeException)
            {
                num = try_failure(num, "getMatrix(int[],int,int)... ", "Unexpected ArrayIndexOutOfBoundsException");
            }

            try
            {
                jamaMatrix5 = jamaMatrix2.getMatrix(r2, c);
                num = try_failure(num,
                    "getMatrix(int[],int[])... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }
            catch (IndexOutOfRangeException)
            {
                try
                {
                    jamaMatrix5 = jamaMatrix2.getMatrix(r, c2);
                    num = try_failure(num,
                        "getMatrix(int[],int[])... ",
                        "ArrayIndexOutOfBoundsException expected but not thrown");
                }
                catch (IndexOutOfRangeException)
                {
                    try_success("getMatrix(int[],int[])... ArrayIndexOutOfBoundsException... ", "");
                }
            }
            catch (ArgumentException)
            {
                num = try_failure(num,
                    "getMatrix(int[],int[])... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }

            try
            {
                jamaMatrix5 = jamaMatrix2.getMatrix(r, c);
                try
                {
                    check(jamaMatrix4, jamaMatrix5);
                    try_success("getMatrix(int[],int[])... ", "");
                }
                catch (SystemException)
                {
                    num = try_failure(num, "getMatrix(int[],int[])... ", "submatrix not successfully retreived");
                }
            }
            catch (IndexOutOfRangeException)
            {
                num = try_failure(num, "getMatrix(int[],int[])... ", "Unexpected ArrayIndexOutOfBoundsException");
            }

            try
            {
                jamaMatrix2.set_Renamed(jamaMatrix2.RowDimension, jamaMatrix2.ColumnDimension - 1, 0.0);
                num = try_failure(num, "set(int,int,double)... ", "OutOfBoundsException expected but not thrown");
            }
            catch (IndexOutOfRangeException)
            {
                try
                {
                    jamaMatrix2.set_Renamed(jamaMatrix2.RowDimension - 1, jamaMatrix2.ColumnDimension, 0.0);
                    num = try_failure(num, "set(int,int,double)... ", "OutOfBoundsException expected but not thrown");
                }
                catch (IndexOutOfRangeException)
                {
                    try_success("set(int,int,double)... OutofBoundsException... ", "");
                }
            }
            catch (ArgumentException)
            {
                num = try_failure(num, "set(int,int,double)... ", "OutOfBoundsException expected but not thrown");
            }

            try
            {
                jamaMatrix2.set_Renamed(num4, num6, 0.0);
                num8 = jamaMatrix2.get_Renamed(num4, num6);
                try
                {
                    check(num8, 0.0);
                    try_success("set(int,int,double)... ", "");
                }
                catch (SystemException)
                {
                    num = try_failure(num, "set(int,int,double)... ", "Matrix element not successfully set");
                }
            }
            catch (IndexOutOfRangeException)
            {
                num = try_failure(num, "set(int,int,double)... ", "Unexpected ArrayIndexOutOfBoundsException");
            }

            jamaMatrix5 = new JamaMatrix(2, 3, 0.0);
            try
            {
                jamaMatrix2.setMatrix(num4, num5 + jamaMatrix2.RowDimension + 1, num6, num7, jamaMatrix5);
                num = try_failure(num,
                    "setMatrix(int,int,int,int,Matrix)... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }
            catch (IndexOutOfRangeException)
            {
                try
                {
                    jamaMatrix2.setMatrix(num4, num5, num6, num7 + jamaMatrix2.ColumnDimension + 1, jamaMatrix5);
                    num = try_failure(num,
                        "setMatrix(int,int,int,int,Matrix)... ",
                        "ArrayIndexOutOfBoundsException expected but not thrown");
                }
                catch (IndexOutOfRangeException)
                {
                    try_success("setMatrix(int,int,int,int,Matrix)... ArrayIndexOutOfBoundsException... ", "");
                }
            }
            catch (ArgumentException)
            {
                num = try_failure(num,
                    "setMatrix(int,int,int,int,Matrix)... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }

            try
            {
                jamaMatrix2.setMatrix(num4, num5, num6, num7, jamaMatrix5);
                try
                {
                    check(jamaMatrix5.minus(jamaMatrix2.getMatrix(num4, num5, num6, num7)), jamaMatrix5);
                    try_success("setMatrix(int,int,int,int,Matrix)... ", "");
                }
                catch (SystemException)
                {
                    num = try_failure(num, "setMatrix(int,int,int,int,Matrix)... ", "submatrix not successfully set");
                }

                jamaMatrix2.setMatrix(num4, num5, num6, num7, jamaMatrix4);
            }
            catch (IndexOutOfRangeException)
            {
                num = try_failure(num,
                    "setMatrix(int,int,int,int,Matrix)... ",
                    "Unexpected ArrayIndexOutOfBoundsException");
            }

            try
            {
                jamaMatrix2.setMatrix(num4, num5 + jamaMatrix2.RowDimension + 1, c, jamaMatrix5);
                num = try_failure(num,
                    "setMatrix(int,int,int[],Matrix)... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }
            catch (IndexOutOfRangeException)
            {
                try
                {
                    jamaMatrix2.setMatrix(num4, num5, c2, jamaMatrix5);
                    num = try_failure(num,
                        "setMatrix(int,int,int[],Matrix)... ",
                        "ArrayIndexOutOfBoundsException expected but not thrown");
                }
                catch (IndexOutOfRangeException)
                {
                    try_success("setMatrix(int,int,int[],Matrix)... ArrayIndexOutOfBoundsException... ", "");
                }
            }
            catch (ArgumentException)
            {
                num = try_failure(num,
                    "setMatrix(int,int,int[],Matrix)... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }

            try
            {
                jamaMatrix2.setMatrix(num4, num5, c, jamaMatrix5);
                try
                {
                    check(jamaMatrix5.minus(jamaMatrix2.getMatrix(num4, num5, c)), jamaMatrix5);
                    try_success("setMatrix(int,int,int[],Matrix)... ", "");
                }
                catch (SystemException)
                {
                    num = try_failure(num, "setMatrix(int,int,int[],Matrix)... ", "submatrix not successfully set");
                }

                jamaMatrix2.setMatrix(num4, num5, num6, num7, jamaMatrix4);
            }
            catch (IndexOutOfRangeException)
            {
                num = try_failure(num,
                    "setMatrix(int,int,int[],Matrix)... ",
                    "Unexpected ArrayIndexOutOfBoundsException");
            }

            try
            {
                jamaMatrix2.setMatrix(r, num6, num7 + jamaMatrix2.ColumnDimension + 1, jamaMatrix5);
                num = try_failure(num,
                    "setMatrix(int[],int,int,Matrix)... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }
            catch (IndexOutOfRangeException)
            {
                try
                {
                    jamaMatrix2.setMatrix(r2, num6, num7, jamaMatrix5);
                    num = try_failure(num,
                        "setMatrix(int[],int,int,Matrix)... ",
                        "ArrayIndexOutOfBoundsException expected but not thrown");
                }
                catch (IndexOutOfRangeException)
                {
                    try_success("setMatrix(int[],int,int,Matrix)... ArrayIndexOutOfBoundsException... ", "");
                }
            }
            catch (ArgumentException)
            {
                num = try_failure(num,
                    "setMatrix(int[],int,int,Matrix)... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }

            try
            {
                jamaMatrix2.setMatrix(r, num6, num7, jamaMatrix5);
                try
                {
                    check(jamaMatrix5.minus(jamaMatrix2.getMatrix(r, num6, num7)), jamaMatrix5);
                    try_success("setMatrix(int[],int,int,Matrix)... ", "");
                }
                catch (SystemException)
                {
                    num = try_failure(num, "setMatrix(int[],int,int,Matrix)... ", "submatrix not successfully set");
                }

                jamaMatrix2.setMatrix(num4, num5, num6, num7, jamaMatrix4);
            }
            catch (IndexOutOfRangeException)
            {
                num = try_failure(num,
                    "setMatrix(int[],int,int,Matrix)... ",
                    "Unexpected ArrayIndexOutOfBoundsException");
            }

            try
            {
                jamaMatrix2.setMatrix(r, c2, jamaMatrix5);
                num = try_failure(num,
                    "setMatrix(int[],int[],Matrix)... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }
            catch (IndexOutOfRangeException)
            {
                try
                {
                    jamaMatrix2.setMatrix(r2, c, jamaMatrix5);
                    num = try_failure(num,
                        "setMatrix(int[],int[],Matrix)... ",
                        "ArrayIndexOutOfBoundsException expected but not thrown");
                }
                catch (IndexOutOfRangeException)
                {
                    try_success("setMatrix(int[],int[],Matrix)... ArrayIndexOutOfBoundsException... ", "");
                }
            }
            catch (ArgumentException)
            {
                num = try_failure(num,
                    "setMatrix(int[],int[],Matrix)... ",
                    "ArrayIndexOutOfBoundsException expected but not thrown");
            }

            try
            {
                jamaMatrix2.setMatrix(r, c, jamaMatrix5);
                try
                {
                    check(jamaMatrix5.minus(jamaMatrix2.getMatrix(r, c)), jamaMatrix5);
                    try_success("setMatrix(int[],int[],Matrix)... ", "");
                }
                catch (SystemException)
                {
                    num = try_failure(num, "setMatrix(int[],int[],Matrix)... ", "submatrix not successfully set");
                }
            }
            catch (IndexOutOfRangeException)
            {
                num = try_failure(num,
                    "setMatrix(int[],int[],Matrix)... ",
                    "Unexpected ArrayIndexOutOfBoundsException");
            }

            print("\nTesting array-like methods...\n");
            var b = new JamaMatrix(array, m3);
            var jamaMatrix6 = JamaMatrix.random(jamaMatrix.RowDimension, jamaMatrix.ColumnDimension);
            jamaMatrix = jamaMatrix6;
            try
            {
                b = jamaMatrix.minus(b);
                num = try_failure(num, "minus conformance check... ", "nonconformance not raised");
            }
            catch (ArgumentException)
            {
                try_success("minus conformance check... ", "");
            }

            if (jamaMatrix.minus(jamaMatrix6).norm1() != 0.0)
            {
                num = try_failure(num,
                    "minus... ",
                    "(difference of identical Matrices is nonzero,\nSubsequent use of minus should be suspect)");
            }
            else
            {
                try_success("minus... ", "");
            }

            jamaMatrix = jamaMatrix6.copy();
            jamaMatrix.minusEquals(jamaMatrix6);
            var jamaMatrix7 = new JamaMatrix(jamaMatrix.RowDimension, jamaMatrix.ColumnDimension);
            try
            {
                jamaMatrix.minusEquals(b);
                num = try_failure(num, "minusEquals conformance check... ", "nonconformance not raised");
            }
            catch (ArgumentException)
            {
                try_success("minusEquals conformance check... ", "");
            }

            if (jamaMatrix.minus(jamaMatrix7).norm1() != 0.0)
            {
                num = try_failure(num,
                    "minusEquals... ",
                    "(difference of identical Matrices is nonzero,\nSubsequent use of minus should be suspect)");
            }
            else
            {
                try_success("minusEquals... ", "");
            }

            jamaMatrix = jamaMatrix6.copy();
            jamaMatrix2 = JamaMatrix.random(jamaMatrix.RowDimension, jamaMatrix.ColumnDimension);
            jamaMatrix3 = jamaMatrix.minus(jamaMatrix2);
            try
            {
                b = jamaMatrix.plus(b);
                num = try_failure(num, "plus conformance check... ", "nonconformance not raised");
            }
            catch (ArgumentException)
            {
                try_success("plus conformance check... ", "");
            }

            try
            {
                check(jamaMatrix3.plus(jamaMatrix2), jamaMatrix);
                try_success("plus... ", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "plus... ", "(C = A - B, but C + B != A)");
            }

            jamaMatrix3 = jamaMatrix.minus(jamaMatrix2);
            jamaMatrix3.plusEquals(jamaMatrix2);
            try
            {
                jamaMatrix.plusEquals(b);
                num = try_failure(num, "plusEquals conformance check... ", "nonconformance not raised");
            }
            catch (ArgumentException)
            {
                try_success("plusEquals conformance check... ", "");
            }

            try
            {
                check(jamaMatrix3, jamaMatrix);
                try_success("plusEquals... ", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "plusEquals... ", "(C = A - B, but C = C + B != A)");
            }

            jamaMatrix = jamaMatrix6.uminus();
            try
            {
                check(jamaMatrix.plus(jamaMatrix6), jamaMatrix7);
                try_success("uminus... ", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "uminus... ", "(-A + A != zeros)");
            }

            jamaMatrix = jamaMatrix6.copy();
            var y5 = new JamaMatrix(jamaMatrix.RowDimension, jamaMatrix.ColumnDimension, 1.0);
            jamaMatrix3 = jamaMatrix.arrayLeftDivide(jamaMatrix6);
            try
            {
                b = jamaMatrix.arrayLeftDivide(b);
                num = try_failure(num, "arrayLeftDivide conformance check... ", "nonconformance not raised");
            }
            catch (ArgumentException)
            {
                try_success("arrayLeftDivide conformance check... ", "");
            }

            try
            {
                check(jamaMatrix3, y5);
                try_success("arrayLeftDivide... ", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "arrayLeftDivide... ", "(M.\\M != ones)");
            }

            try
            {
                jamaMatrix.arrayLeftDivideEquals(b);
                num = try_failure(num, "arrayLeftDivideEquals conformance check... ", "nonconformance not raised");
            }
            catch (ArgumentException)
            {
                try_success("arrayLeftDivideEquals conformance check... ", "");
            }

            jamaMatrix.arrayLeftDivideEquals(jamaMatrix6);
            try
            {
                check(jamaMatrix, y5);
                try_success("arrayLeftDivideEquals... ", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "arrayLeftDivideEquals... ", "(M.\\M != ones)");
            }

            jamaMatrix = jamaMatrix6.copy();
            try
            {
                jamaMatrix.arrayRightDivide(b);
                num = try_failure(num, "arrayRightDivide conformance check... ", "nonconformance not raised");
            }
            catch (ArgumentException)
            {
                try_success("arrayRightDivide conformance check... ", "");
            }

            jamaMatrix3 = jamaMatrix.arrayRightDivide(jamaMatrix6);
            try
            {
                check(jamaMatrix3, y5);
                try_success("arrayRightDivide... ", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "arrayRightDivide... ", "(M./M != ones)");
            }

            try
            {
                jamaMatrix.arrayRightDivideEquals(b);
                num = try_failure(num, "arrayRightDivideEquals conformance check... ", "nonconformance not raised");
            }
            catch (ArgumentException)
            {
                try_success("arrayRightDivideEquals conformance check... ", "");
            }

            jamaMatrix.arrayRightDivideEquals(jamaMatrix6);
            try
            {
                check(jamaMatrix, y5);
                try_success("arrayRightDivideEquals... ", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "arrayRightDivideEquals... ", "(M./M != ones)");
            }

            jamaMatrix = jamaMatrix6.copy();
            jamaMatrix2 = JamaMatrix.random(jamaMatrix.RowDimension, jamaMatrix.ColumnDimension);
            try
            {
                b = jamaMatrix.arrayTimes(b);
                num = try_failure(num, "arrayTimes conformance check... ", "nonconformance not raised");
            }
            catch (ArgumentException)
            {
                try_success("arrayTimes conformance check... ", "");
            }

            jamaMatrix3 = jamaMatrix.arrayTimes(jamaMatrix2);
            try
            {
                check(jamaMatrix3.arrayRightDivideEquals(jamaMatrix2), jamaMatrix);
                try_success("arrayTimes... ", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "arrayTimes... ", "(A = R, C = A.*B, but C./B != A)");
            }

            try
            {
                jamaMatrix.arrayTimesEquals(b);
                num = try_failure(num, "arrayTimesEquals conformance check... ", "nonconformance not raised");
            }
            catch (ArgumentException)
            {
                try_success("arrayTimesEquals conformance check... ", "");
            }

            jamaMatrix.arrayTimesEquals(jamaMatrix2);
            try
            {
                check(jamaMatrix.arrayRightDivideEquals(jamaMatrix2), jamaMatrix6);
                try_success("arrayTimesEquals... ", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "arrayTimesEquals... ", "(A = R, A = A.*B, but A./B != R)");
            }

            print("\nTesting I/O methods...\n");
            print("\nTesting linear algebra methods...\n");
            jamaMatrix = new JamaMatrix(array, 3);
            var y6 = new JamaMatrix(a2);
            y6 = jamaMatrix.transpose();
            try
            {
                check(jamaMatrix.transpose(), y6);
                try_success("transpose...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "transpose()...", "transpose unsuccessful");
            }

            jamaMatrix.transpose();
            try
            {
                check(jamaMatrix.norm1(), y2);
                try_success("norm1...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "norm1()...", "incorrect norm calculation");
            }

            try
            {
                check(jamaMatrix.normInf(), y3);
                try_success("normInf()...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "normInf()...", "incorrect norm calculation");
            }

            try
            {
                check(jamaMatrix.normF(), Math.Sqrt(d));
                try_success("normF...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "normF()...", "incorrect norm calculation");
            }

            try
            {
                check(jamaMatrix.trace(), y4);
                try_success("trace()...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "trace()...", "incorrect trace calculation");
            }

            try
            {
                check(jamaMatrix.getMatrix(0, jamaMatrix.RowDimension - 1, 0, jamaMatrix.RowDimension - 1).det(), 0.0);
                try_success("det()...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "det()...", "incorrect determinant calculation");
            }

            var jamaMatrix8 = new JamaMatrix(a8);
            try
            {
                check(jamaMatrix.times(jamaMatrix.transpose()), jamaMatrix8);
                try_success("times(Matrix)...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "times(Matrix)...", "incorrect Matrix-Matrix product calculation");
            }

            try
            {
                check(jamaMatrix.times(0.0), jamaMatrix7);
                try_success("times(double)...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "times(double)...", "incorrect Matrix-scalar product calculation");
            }

            jamaMatrix = new JamaMatrix(array, 4);
            var qRDecomposition = jamaMatrix.qr();
            jamaMatrix6 = qRDecomposition.R;
            try
            {
                check(jamaMatrix, qRDecomposition.Q.times(jamaMatrix6));
                try_success("QRDecomposition...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "QRDecomposition...", "incorrect QR decomposition calculation");
            }

            var singularValueDecomposition = jamaMatrix.svd();
            try
            {
                check(jamaMatrix,
                    singularValueDecomposition.getU()
                        .times(singularValueDecomposition.S.times(singularValueDecomposition.getV().transpose())));
                try_success("SingularValueDecomposition...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num,
                    "SingularValueDecomposition...",
                    "incorrect singular value decomposition calculation");
            }

            var jamaMatrix9 = new JamaMatrix(a);
            try
            {
                check(jamaMatrix9.rank(),
                    Math.Min(jamaMatrix9.RowDimension, jamaMatrix9.ColumnDimension) - 1);
                try_success("rank()...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "rank()...", "incorrect rank calculation");
            }

            jamaMatrix2 = new JamaMatrix(a10);
            singularValueDecomposition = jamaMatrix2.svd();
            var singularValues = singularValueDecomposition.SingularValues;
            try
            {
                check(jamaMatrix2.cond(),
                    singularValues[0] /
                    singularValues[Math.Min(jamaMatrix2.RowDimension, jamaMatrix2.ColumnDimension) - 1]);
                try_success("cond()...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "cond()...", "incorrect condition number calculation");
            }

            int columnDimension = jamaMatrix.ColumnDimension;
            jamaMatrix = jamaMatrix.getMatrix(0, columnDimension - 1, 0, columnDimension - 1);
            jamaMatrix.set_Renamed(0, 0, 0.0);
            var lUDecomposition = jamaMatrix.lu();
            try
            {
                check(jamaMatrix.getMatrix(lUDecomposition.Pivot, 0, columnDimension - 1),
                    lUDecomposition.L.times(lUDecomposition.U));
                try_success("LUDecomposition...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "LUDecomposition...", "incorrect LU decomposition calculation");
            }

            var b2 = jamaMatrix.inverse();
            try
            {
                check(jamaMatrix.times(b2), JamaMatrix.identity(3, 3));
                try_success("inverse()...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "inverse()...", "incorrect inverse calculation");
            }

            y5 = new JamaMatrix(jamaMatrix4.RowDimension, 1, 1.0);
            var b3 = new JamaMatrix(a9);
            jamaMatrix8 = jamaMatrix4.getMatrix(0, jamaMatrix4.RowDimension - 1, 0, jamaMatrix4.RowDimension - 1);
            try
            {
                check(jamaMatrix8.solve(b3), y5);
                try_success("solve()...", "");
            }
            catch (ArgumentException ex2)
            {
                num = try_failure(num, "solve()...", ex2.Message);
            }
            catch (SystemException ex3)
            {
                num = try_failure(num, "solve()...", ex3.Message);
            }

            jamaMatrix = new JamaMatrix(a5);
            var choleskyDecomposition = jamaMatrix.chol();
            var l = choleskyDecomposition.getL();
            try
            {
                check(jamaMatrix, l.times(l.transpose()));
                try_success("CholeskyDecomposition...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num, "CholeskyDecomposition...", "incorrect Cholesky decomposition calculation");
            }

            b2 = choleskyDecomposition.solve(JamaMatrix.identity(3, 3));
            try
            {
                check(jamaMatrix.times(b2), JamaMatrix.identity(3, 3));
                try_success("CholeskyDecomposition solve()...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num,
                    "CholeskyDecomposition solve()...",
                    "incorrect Choleskydecomposition solve calculation");
            }

            var eigenvalueDecomposition = jamaMatrix.eig();
            var d2 = eigenvalueDecomposition.D;
            var v = eigenvalueDecomposition.getV();
            try
            {
                check(jamaMatrix.times(v), v.times(d2));
                try_success("EigenvalueDecomposition (symmetric)...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num,
                    "EigenvalueDecomposition (symmetric)...",
                    "incorrect symmetric Eigenvalue decomposition calculation");
            }

            jamaMatrix = new JamaMatrix(a7);
            eigenvalueDecomposition = jamaMatrix.eig();
            d2 = eigenvalueDecomposition.D;
            v = eigenvalueDecomposition.getV();
            try
            {
                check(jamaMatrix.times(v), v.times(d2));
                try_success("EigenvalueDecomposition (nonsymmetric)...", "");
            }
            catch (SystemException)
            {
                num = try_failure(num,
                    "EigenvalueDecomposition (nonsymmetric)...",
                    "incorrect nonsymmetric Eigenvalue decomposition calculation");
            }

            print("\nTestMatrix completed.\n");
            print("Total errors reported: " + Convert.ToString(num) + "\n");
            print("Total warnings reported: " + Convert.ToString(value) + "\n");
        }

        private static void check(double x, double y)
        {
            double num = Math.Pow(2.0, -52.0);
            if (!((x == 0.0) & (Math.Abs(y) < 10.0 * num)))
            {
                if (!((y == 0.0) & (Math.Abs(x) < 10.0 * num)))
                {
                    if (Math.Abs(x - y) > 10.0 * num * Math.Max(Math.Abs(x), Math.Abs(y)))
                    {
                        throw new SystemException("The difference x-y is too large: x = " + x.ToString() + "  y = " +
                                                  y.ToString());
                    }
                }
            }
        }

        private static void check(double[] x, double[] y)
        {
            if (x.Length == y.Length)
            {
                for (int i = 0; i < x.Length; i++)
                {
                    check(x[i], y[i]);
                }

                return;
            }

            throw new SystemException("Attempt to compare vectors of different lengths");
        }

        private static void check(double[][] x, double[][] y)
        {
            var x2 = new JamaMatrix(x);
            var y2 = new JamaMatrix(y);
            check(x2, y2);
        }

        private static void check(JamaMatrix X, JamaMatrix Y)
        {
            double num = Math.Pow(2.0, -52.0);
            if (!((X.norm1() == 0.0) & (Y.norm1() < 10.0 * num)))
            {
                if (!((Y.norm1() == 0.0) & (X.norm1() < 10.0 * num)))
                {
                    if (X.minus(Y).norm1() > 1000.0 * num * Math.Max(X.norm1(), Y.norm1()))
                    {
                        throw new SystemException("The norm of (X-Y) is too large: " + X.minus(Y).norm1().ToString());
                    }
                }
            }
        }

        private static void print(string s)
        {
            Console.Out.Write(s);
        }

        private static void try_success(string s, string e)
        {
            print(">    " + s + "success\n");
            if (e != "")
            {
                print(">      Message: " + e + "\n");
            }
        }

        private static int try_failure(int count, string s, string e)
        {
            print(string.Concat(new[] {">    ", s, "*** failure ***\n>      Message: ", e, "\n"}));
            return ++count;
        }

        private static int try_warning(int count, string s, string e)
        {
            print(string.Concat(new[] {">    ", s, "*** warning ***\n>      Message: ", e, "\n"}));
            return ++count;
        }

        private static void print(double[] x, int w, int d)
        {
            Console.Out.Write("\n");
            new JamaMatrix(x, 1).print(w, d);
            print("\n");
        }
    }
}
