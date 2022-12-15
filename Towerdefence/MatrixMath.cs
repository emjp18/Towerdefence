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
            scaledM[0, 0] = scaledM[0, 0] * scalar;
            scaledM[1, 0]=scaledM[1, 0]*scalar;
            scaledM[2, 0]=scaledM[2, 0]*scalar;
            scaledM[0, 1]=scaledM[0, 1]*scalar;
            scaledM[1, 1]=scaledM[1, 1]*scalar;
            scaledM[2, 1]=scaledM[2, 1]*scalar;
            scaledM[0, 2]=scaledM[0, 2]*scalar;
            scaledM[1, 2]=scaledM[1, 2]*scalar;
            scaledM[2, 2]=scaledM[2, 2]*scalar;
            return scaledM;
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
    }
}
