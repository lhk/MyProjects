using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinesweeperBot
{
    public class CentroidSet
    {
        public double[][] Centroids;
        public int[] DataCentroidConnection;
        public int DataPointCount { get { return DataCentroidConnection.Length; } }
        public int DataPointDimensions { get { return Centroids[0].Length; } }
        public int CentroidCount { get { return Centroids.Length; } }

        double _cost = -1;

        public CentroidSet(){ }

        public CentroidSet(int _DataPointCount, int _DataPointDimensions, int _CentroidCount)
        {
            DataCentroidConnection = new int[_DataPointCount];
            Centroids = new double[_CentroidCount][];
            for (int i = 0; i < _CentroidCount; i++)
            {
                Centroids[i] = new double[_DataPointDimensions];
            }
        }

        public CentroidSet Clone()
        {
            CentroidSet c = new CentroidSet(DataCentroidConnection.Length, Centroids[0].Length, Centroids.Length);
            Array.Copy(DataCentroidConnection, c.DataCentroidConnection, DataCentroidConnection.Length);
            for (int i = 0; i < Centroids.Length; i++)
            {
                Array.Copy(Centroids[i], c.Centroids[i], Centroids[i].Length);
            }
            return c;
        }

        public double Cost(List<DataPoint> DataSet)
        {
            if (_cost == -1)
            {
                double cost = 0;
                for (int i = 0; i < DataSet.Count; i++)
                    for (int n = 0; n < DataPointDimensions; n++)
                    {
                        double diff = Centroids[DataCentroidConnection[i]][n] - DataSet[i].Features[n];
                        cost += diff * diff;
                    }
                _cost = cost / DataPointDimensions / DataSet.Count;
            }
            return _cost;
        }

        public void MoveStep(List<DataPoint> DataSet)
        {
            int[] centroidAssignedDataCount = new int[Centroids.Length];
            for (int i = 0; i < DataSet.Count; i++)
            {
                for (int n = 0; n < DataPointDimensions; n++)
                    Centroids[DataCentroidConnection[i]][n] += DataSet[i].Features[n];

                centroidAssignedDataCount[DataCentroidConnection[i]]++;
            }
            for (int k = 0; k < Centroids.Length; k++)
            {
                if (centroidAssignedDataCount[k] > 0)
                    for (int n = 0; n < DataPointDimensions; n++)
                        Centroids[k][n] /= centroidAssignedDataCount[k];

                else
                {
                    int index = GenuineRandomGenerator.GetInt(DataSet.Count);
                    for (int n = 0; n < DataPointDimensions; n++)
                        Centroids[k][n] = DataSet[index].Features[n] + .1;
                }
            }
        }

        public void AssignmentStep(List<DataPoint> DataSet)
        {
            for (int i = 0; i < DataSet.Count; i++)
            {
                double minSquaredDistance = double.MaxValue;
                int centroidIndex = -1;

                for (int k = 0; k < Centroids.Length; k++)
                {
                    double squaredDistance = 0;
                    for (int n = 0; n < DataPointDimensions; n++)
                    {
                        double diff = Centroids[k][n] - DataSet[i].Features[n];
                        squaredDistance += diff * diff;
                    }
                    if (minSquaredDistance > squaredDistance)
                    {
                        minSquaredDistance = squaredDistance;
                        centroidIndex = k;
                    }
                }
                DataCentroidConnection[i] = centroidIndex;
            }
        }
    }
}
