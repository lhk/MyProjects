using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using Scalar = System.Double;
using System.Drawing;

namespace TrussSim
{
    class HomogeneousProjection
    {
        Matrix<Scalar> projectionMatrix;
        Matrix<Scalar> inverseProjectionMatrix;
        int dimensions;

        public int Dimensions { get { return dimensions; } }

        public Matrix<Scalar> ProjectionMatrix
        {
            get { return projectionMatrix; }
            set
            {
                if (value.RowCount != dimensions + 1 || value.ColumnCount != dimensions + 1)
                    throw new ArgumentException("The matrix must be (" + (dimensions + 1).ToString() + "x" + (dimensions + 1).ToString() + ").");

                if (Math.Abs(value.Determinant()) < Scalar.Epsilon * 16)
                    throw new ArgumentException("The matrix must be invertible.");

                projectionMatrix = value;
                inverseProjectionMatrix = projectionMatrix.Inverse();
            }
        }

        public HomogeneousProjection(int _dimensions)
        {
            dimensions = _dimensions;
        }

        public Vector<Scalar> Project(Vector<Scalar> vector)
        {
            return HomogeneousTransformation(projectionMatrix, vector);
        }

        public Vector<Scalar> UnProject(Vector<Scalar> vector)
        {
            return HomogeneousTransformation(inverseProjectionMatrix, vector);
        }

        Vector<Scalar> HomogeneousTransformation(Matrix<Scalar> HomogeneousProjectionMatrix, Vector<Scalar> CartesianInputVector)
        {
            if (CartesianInputVector.Count != dimensions)
                throw new ArgumentException("The vector must have " + dimensions.ToString() + " elements.");

            Vector<Scalar> HomogeneousInputVector = new DenseVector(dimensions + 1, 1);
            for (int i = 0; i < dimensions; i++)
                HomogeneousInputVector[i] = CartesianInputVector[i];

            Vector<Scalar> HomogeneousOutputVector = HomogeneousProjectionMatrix * HomogeneousInputVector;
            Vector<Scalar> CartesianOutputVector = new DenseVector(dimensions, 0);

            for (int i = 0; i < dimensions; i++)
                CartesianOutputVector[i] = HomogeneousOutputVector[i];

            return CartesianOutputVector;
        }

        public static Matrix<Scalar> Scale(Vector<Scalar> scale)
        {
            int dimensions = scale.Count;

            Matrix<Scalar> output = new DenseMatrix(dimensions + 1);
            for (int i = 0; i < dimensions; i++)
                output[i, i] = scale[i];

            output[dimensions, dimensions] = 1;

            return output;
        }

        public static Matrix<Scalar> Scale(Scalar scale, int dimensions)
        {
            return Scale(new DenseVector(dimensions, scale));
        }

        public static Matrix<Scalar> Translate(Vector<Scalar> translation)
        {
            int dimensions = translation.Count;

            Matrix<Scalar> output = new DenseMatrix(dimensions + 1);
            for (int i = 0; i < dimensions + 1; i++)
                output[i, i] = 1;

            for (int i = 0; i < dimensions; i++)
                output[i, dimensions] = translation[i];


            return output;
        }

        public static Matrix<Scalar> Rotate(Scalar angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);

            return new DenseMatrix(new Scalar[,]{
                 { cos,-sin,  0},
                 { sin, cos,  0},
                 {   0,   0,  1}            
            });
        }

        public static Matrix<Scalar> RotateX(Scalar angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);

            return new DenseMatrix(new Scalar[,]{
                 {    1,    0,    0,    1},
                 {    0,  cos, -sin,    0},
                 {    0,  sin,  cos,    0},
                 {    0,    0,    0,    1}        
            });
        }

        public static Matrix<Scalar> RotateY(Scalar angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);

            return new DenseMatrix(new Scalar[,]{
                 {  cos,    0,  sin,    1},
                 {    0,    1,    0,    0},
                 { -sin,    0,  cos,    0},
                 {    0,    0,    0,    1}        
            });
        }

        public static Matrix<Scalar> RotateZ(Scalar angle)
        {
            double cos = Math.Cos(angle);
            double sin = Math.Sin(angle);

            return new DenseMatrix(new Scalar[,]{
                 { cos,-sin,  0,  0},
                 { sin, cos,  0,  0},
                 {   0,   0,  1,  0},
                 {   0,   0,  0,  1},
            });
        }
    }
}