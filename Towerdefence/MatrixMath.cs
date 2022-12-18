using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Towerdefence
{
    internal static class MatrixMath
    {
        public static float[,] GetRotationMatrix2x2(float angle)
        {
            float[,] m = new float[2, 2];
            m[0, 0] = MathF.Cos(angle);
            m[0, 1] = -MathF.Sin(angle);
            m[1, 0] = MathF.Sin(angle);
            m[1, 1] = MathF.Cos(angle);

            return m;
        }
        public static Vector2 TransformVector2x2(float[,] m, Vector2 vec)
        {
            Vector2 v2= new Vector2();
            v2.X = m[0, 0] * vec.X + m[0, 1] * vec.Y;
            v2.Y = m[1, 0] * vec.X + m[1, 1] * vec.Y;
            return v2;
        }
        public static float Determinant3x3(float[,] m)
        {
           return m[0, 0] * (m[1, 1] * m[2, 2] -
                m[2, 1] * m[1, 2]) - m[0, 1] * (m[1, 0] * m[2, 2] -
                m[2, 1] * m[1, 2]) + m[0, 2] * (m[1, 0] * m[2, 1] -
                m[2, 0] * m[1, 1]);
        }
        public static float Determinant2x2(float[,] m)
        {
            return (m[0, 0] * m[1, 1]) - (m[0, 1] * m[1, 0]);
        }
        public static float[,] Inverse3x3(float[,] m)
        {
            float  d= Determinant3x3(m);
            if(d==0)
            {
                return m;
            }
            else
            {
                float[,] inversed = new float[3, 3];
                float[,] submatrix = new float[2, 2];
                float[,] transposed = Transposed3x3(m);
                submatrix[0, 0] = transposed[1, 1];
                submatrix[0, 1] = transposed[2, 1];
                submatrix[1, 0] = transposed[1, 2];
                submatrix[1, 1] = transposed[2, 2];
                float ud00 = MatrixMath.Determinant2x2(submatrix);
                submatrix[0, 0] = transposed[1, 0];
                submatrix[0, 1] = transposed[2, 0];
                submatrix[1, 0] = transposed[1, 2];
                submatrix[1, 1] = transposed[2, 2];
                float ud01 = MatrixMath.Determinant2x2(submatrix)*-1;
                submatrix[0, 0] = transposed[1, 0];
                submatrix[0, 1] = transposed[1, 1];
                submatrix[1, 0] = transposed[0, 2];
                submatrix[1, 1] = transposed[2, 1];
                float ud02 = MatrixMath.Determinant2x2(submatrix);
                submatrix[0, 0] = transposed[0, 1];
                submatrix[0, 1] = transposed[2, 1];
                submatrix[1, 0] = transposed[0, 2];
                submatrix[1, 1] = transposed[2, 2];
                float ud10 = MatrixMath.Determinant2x2(submatrix)*-1;
                submatrix[0, 0] = transposed[0, 0];
                submatrix[0, 1] = transposed[2, 0];
                submatrix[1, 0] = transposed[0, 2];
                submatrix[1, 1] = transposed[2, 2];
                float ud11 = MatrixMath.Determinant2x2(submatrix);
                submatrix[0, 0] = transposed[0, 0];
                submatrix[0, 1] = transposed[2, 0];
                submatrix[1, 0] = transposed[0, 1];
                submatrix[1, 1] = transposed[2, 1];
                float ud12 = MatrixMath.Determinant2x2(submatrix) * -1;
                submatrix[0, 0] = transposed[0, 1];
                submatrix[0, 1] = transposed[0, 2];
                submatrix[1, 0] = transposed[1, 1];
                submatrix[1, 1] = transposed[1, 2];
                float ud20 = MatrixMath.Determinant2x2(submatrix);
                submatrix[0, 0] = transposed[0, 0];
                submatrix[0, 1] = transposed[1, 0];
                submatrix[1, 0] = transposed[0, 2];
                submatrix[1, 1] = transposed[1, 2];
                float ud21 = MatrixMath.Determinant2x2(submatrix) * -1;
                submatrix[0, 0] = transposed[0, 0];
                submatrix[0, 1] = transposed[1, 0];
                submatrix[1, 0] = transposed[0, 1];
                submatrix[1, 1] = transposed[1, 1];
                float ud22 = MatrixMath.Determinant2x2(submatrix);

                inversed[0, 0] = ud00;
                inversed[1, 0] = ud10;
                inversed[2, 0] = ud20;
                inversed[0, 1] = ud01;
                inversed[1, 1] = ud11;
                inversed[2, 1] = ud21;
                inversed[0, 2] = ud02;
                inversed[1, 2] = ud12;
                inversed[2, 2] = ud22;

                inversed = ScaleMatrix(inversed, 1.0f / d);
                return inversed;
            }
        }
        public static float[,] ScaleMatrix(float[,] m, float scalar)
        {
            float[,] scaledM = new float[3, 3];
            scaledM[0, 0] = m[0, 0] * scalar;
            scaledM[1, 0]= m[1, 0]*scalar;
            scaledM[2, 0]=m[2, 0]*scalar;
            scaledM[0, 1]=m[0, 1]*scalar;
            scaledM[1, 1]=m[1, 1]*scalar;
            scaledM[2, 1]=m[2, 1]*scalar;
            scaledM[0, 2]=m[0, 2]*scalar;
            scaledM[1, 2]=m[1, 2]*scalar;
            scaledM[2, 2]=m[2, 2]*scalar;
            return scaledM;
        }
        public static Vector4 TransformVector4x3(float[,] m, Vector3 vec)
        {
            Vector4 v4 = new Vector4();
            v4.X = m[0, 0] * vec.X + m[0, 1] * vec.Y + m[0, 2] * vec.Z;
            v4.Y = m[1, 0] * vec.X + m[1, 1] * vec.Y + m[1, 2] * vec.Z;
            v4.Z = m[2, 0] * vec.X + m[2, 1] * vec.Y + m[2, 2] * vec.Z;
            v4.W = m[3, 0] * vec.X + m[3, 1] * vec.Y + m[3, 2] * vec.Z;
            return v4;
        }
        //Vector 1x12, matrix 12x1
        public static float[,] TransformVector12x1(float[,] m, float[,] vec)
        {
            ////-(J*V+b)/J*M*JT
            float[,] mat = new float[12, 12];

            for(int row=0; row<12; row++)
            {
                for(int col=0; col<12; col++)
                {
                    mat[row, col] = 0;
                    
                }
            }
            for (int row = 0; row < 12; row++)
            {
                for (int col = 0; col < 12; col++)
                {
                 
                    mat[row, col] += m[row, 0] * vec[0, col];
                }
            }
            return mat;
        }
        public static float[,] MultiplyMatrixGeneric(float[,] A, float[,] B)
        {
            ////-(J*V+b)/J*M*JT
           

            int rA = A.GetLength(0);
            int cA = A.GetLength(1);
            int rB = B.GetLength(0);
            int cB = B.GetLength(1);
            if (cA != rB)
            {
                Console.WriteLine("Matrixes can't be multiplied!!");
            }
            float temp = 0;
            float[,] mat = new float[rA, cB];

            for (int i = 0; i < rA; i++)
            {
                for (int j = 0; j < cB; j++)
                {
                    temp = 0;
                    for (int k = 0; k < cA; k++)
                    {
                        temp += A[i, k] * B[k, j];
                    }
                    mat[i, j] = temp;
                }
            }
           
            return mat;
        }
        public static float[,] AddMatrixGeneric(float[,] A, float[,] B)
        {
            ////-(J*V+b)/J*M*JT


            int rA = A.GetLength(0);
            int cA = A.GetLength(1);
            int rB = B.GetLength(0);
            int cB = B.GetLength(1);
            if (cA != cB||rA!=rB)
            {
                Console.WriteLine("Matrixes can't be added!!");
            }

            float[,] mat = new float[rA, cB];

            for (int i = 0; i < rA; i++)
            {
                for (int j = 0; j < cB; j++)
                {
                    mat[i, j] = A[i, j] + B[i, j];
                }
            }

            return mat;
        }
        public static float[,] SubMatrixGeneric(float[,] A, float[,] B)
        {
            ////-(J*V+b)/J*M*JT


            int rA = A.GetLength(0);
            int cA = A.GetLength(1);
            int rB = B.GetLength(0);
            int cB = B.GetLength(1);
            if (cA != cB || rA != rB)
            {
                Console.WriteLine("Matrixes can't be subtracted!!");
            }

            float[,] mat = new float[rA, cB];

            for (int i = 0; i < rA; i++)
            {
                for (int j = 0; j < cB; j++)
                {
                    mat[i, j] = A[i, j] - B[i, j];
                }
            }

            return mat;
        }
        public static float[,] MultiplyMatrix3x3(float[,] ma, float[,]mb)
        {
            float[,] m3x3 = new float[3, 3];

            m3x3[0, 0] = ma[0, 0] * mb[0, 0] + ma[0, 1] * mb[1, 0] + ma[0, 2] * mb[2, 0];
            m3x3[0, 1] = ma[0, 0] * mb[0, 1] + ma[0, 1] * mb[1, 1] + ma[0, 2] * mb[2, 1];
            m3x3[0, 2] = ma[0, 0] * mb[0, 2] + ma[0, 1] * mb[1, 2] + ma[0, 2] * mb[2, 2];

            m3x3[1, 0] = ma[1, 0] * mb[0, 0] + ma[1, 1] * mb[1, 0] + ma[1, 2] * mb[2, 0];
            m3x3[1, 1] = ma[1, 0] * mb[0, 1] + ma[1, 1] * mb[1, 1] + ma[1, 2] * mb[2, 1];
            m3x3[1, 2] = ma[1, 0] * mb[0, 2] + ma[1, 1] * mb[1, 2] + ma[1, 2] * mb[2, 2];

            m3x3[2, 0] = ma[2, 0] * mb[0, 0] + ma[2, 1] * mb[1, 0] + ma[2, 2] * mb[2, 0];
            m3x3[2, 1] = ma[2, 0] * mb[0, 1] + ma[2, 1] * mb[1, 1] + ma[2, 2] * mb[2, 1];
            m3x3[2, 2] = ma[2, 0] * mb[0, 2] + ma[2, 1] * mb[1, 2] + ma[2, 2] * mb[2, 2];

            return m3x3;
        }
        public static float[,] MultiplyMatrix4x3x3x3(float[,] ma, float[,] mb)
        {
            float[,] m4x3 = new float[4, 3];

            m4x3[0, 0] = ma[0, 0] * mb[0, 0] + ma[0, 1] * mb[1, 0] + ma[0, 2] * mb[2, 0];
            m4x3[0, 1] = ma[0, 0] * mb[0, 1] + ma[0, 1] * mb[1, 1] + ma[0, 2] * mb[2, 1];
            m4x3[0, 2] = ma[0, 0] * mb[0, 2] + ma[0, 1] * mb[1, 2] + ma[0, 2] * mb[2, 2];

            m4x3[1, 0] = ma[1, 0] * mb[0, 0] + ma[1, 1] * mb[1, 0] + ma[1, 2] * mb[2, 0];
            m4x3[1, 1] = ma[1, 0] * mb[0, 1] + ma[1, 1] * mb[1, 1] + ma[1, 2] * mb[2, 1];
            m4x3[1, 2] = ma[1, 0] * mb[0, 2] + ma[1, 1] * mb[1, 2] + ma[1, 2] * mb[2, 2];

            m4x3[2, 0] = ma[2, 0] * mb[0, 0] + ma[2, 1] * mb[1, 0] + ma[2, 2] * mb[2, 0];
            m4x3[2, 1] = ma[2, 0] * mb[0, 1] + ma[2, 1] * mb[1, 1] + ma[2, 2] * mb[2, 1];
            m4x3[2, 2] = ma[2, 0] * mb[0, 2] + ma[2, 1] * mb[1, 2] + ma[2, 2] * mb[2, 2];

            m4x3[3, 0] = ma[3, 0] * mb[0, 0] + ma[3, 1] * mb[1, 0] + ma[3, 2] * mb[2, 0];
            m4x3[3, 1] = ma[3, 0] * mb[0, 1] + ma[3, 1] * mb[1, 1] + ma[3, 2] * mb[2, 1];
            m4x3[3, 2] = ma[3, 0] * mb[0, 2] + ma[3, 1] * mb[1, 2] + ma[3, 2] * mb[2, 2];

            return m4x3;
        }
        public static float[,] MultiplyMatrix3x4x4x3(float[,] ma, float[,] mb)
        {
            float[,] m3x3 = new float[3, 3];

            m3x3[0, 0] = ma[0, 0] * mb[0, 0] + ma[0, 1] * mb[1, 0] + ma[0, 2] * mb[2, 0] + ma[0, 3] * mb[3, 0];
            m3x3[0, 1] = ma[0, 0] * mb[0, 1] + ma[0, 1] * mb[1, 1] + ma[0, 2] * mb[2, 1] + ma[0, 3] * mb[3, 1];
            m3x3[0, 2] = ma[0, 0] * mb[0, 2] + ma[0, 1] * mb[1, 2] + ma[0, 2] * mb[2, 2] + ma[0, 3] * mb[3, 2];

            m3x3[1, 0] = ma[1, 0] * mb[0, 0] + ma[1, 1] * mb[1, 0] + ma[1, 2] * mb[2, 0] + ma[1, 3] * mb[3, 0];
            m3x3[1, 1] = ma[1, 0] * mb[0, 1] + ma[1, 1] * mb[1, 1] + ma[1, 2] * mb[2, 1]+ ma[1, 3] * mb[3, 1];
            m3x3[1, 2] = ma[1, 0] * mb[0, 2] + ma[1, 1] * mb[1, 2] + ma[1, 2] * mb[2, 2]+ ma[1, 3] * mb[3, 2];

            m3x3[2, 0] = ma[2, 0] * mb[0, 0] + ma[2, 1] * mb[1, 0] + ma[2, 2] * mb[2, 0] + ma[2, 3] * mb[3, 0];
            m3x3[2, 1] = ma[2, 0] * mb[0, 1] + ma[2, 1] * mb[1, 1] + ma[2, 2] * mb[2, 1]+ ma[2, 3] * mb[3, 1];
            m3x3[2, 2] = ma[2, 0] * mb[0, 2] + ma[2, 1] * mb[1, 2] + ma[2, 2] * mb[2, 2]+ ma[2, 3] * mb[3, 2];

           

            return m3x3;
        }
        public static Vector3 Cross(Vector3 u, Vector3 v)
        {
            //\left(u_2v_3-u_3v_2,\:u_3v_1-u_1v_3,\:u_1v_2-u_2v_1\right)
            Vector3 result = new Vector3();
            result.X = u.Y * v.Z - u.Z * v.Y;
            result.Y = u.Z * v.X - u.X * v.Z;
            result.Z = u.X * v.Y - u.Y * v.X;
            return result;
        }
        public static float[,] Transposed3x3(float[,] m)
        {
            float[,] transposed = new float[3, 3];
            transposed[0, 0] = m[0,0];
            transposed[1, 0] = m[0,1];
            transposed[2, 0] = m[0,2];
            transposed[0, 1] = m[1,0];
            transposed[1, 1] = m[1,1];
            transposed[2, 1] = m[1,2];
            transposed[0, 2] = m[2,0];
            transposed[1, 2] = m[2,1];
            transposed[2, 2] = m[2,2];
            return transposed;
        }
        public static float[,] Transposed4x3(float[,] m)
        {
            float[,] transposed = new float[3, 4];
            transposed[0, 0] = m[0, 0];
            transposed[1, 0] = m[0, 1];
            transposed[2, 0] = m[0, 2];
            transposed[0, 1] = m[1, 0];
            transposed[1, 1] = m[1, 1];
            transposed[2, 1] = m[1, 2];
            transposed[0, 2] = m[2, 0];
            transposed[1, 2] = m[2, 1];
            transposed[2, 2] = m[2, 2];
            transposed[0, 3] = m[3, 0];
            transposed[1, 3] = m[3, 1];
            transposed[2, 3] = m[3, 2];
            return transposed;
        }
        public static float Cross(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }

        // Cross Product between Vector and Scalar - Counterclockwise Direction
        public static Vector2 Cross(Vector2 v, float s)
        {
            return new Vector2(s * v.Y, -s * v.X);
        }
        public static Vector2 TripleCross(Vector2 a, Vector2 b, Vector2 c)
        {

            //ay*(bx*cy-by*cx)
            //ax*(by*cx-bx*cy)
            Vector2 resutl = (b * Vector2.Dot(c, a)) - (a * Vector2.Dot(c, b)); //new Vector2(a.Y * (b.X * c.Y - b.Y * c.X), a.X * (b.Y * c.X - b.X * c.Y));

            return resutl;



        }
    }

    
}
