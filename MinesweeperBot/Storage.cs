using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;

namespace MinesweeperBot
{
    public class Storage
    {
        public static Storage s;

        public List<DataPoint> DataSet;
        public int[] TrainingSetMapping;
        public int[] TestSetMapping;
        public int DataPointDimensions { get { return DataSet[0].Features.Length; } }
        public int CentroidCount = 30;
        public string ClassificationSet;        
        public CentroidSet CentroidSet;
        public double learningRate = 0.0001;
        public double[] NeuronalNetworkParameters;

        [XmlIgnore]
        public readonly int[] NeuronalNetworkConfiguration = new int[] { 256, 15, 11 };
        
        /*
        public double[][] parameterMatrixSerialize
        {
            get
            {
                if (parameterMatrix == null) return null;
                double[][] res = new double[parameterMatrix.RowCount][];
                for (int k = 0; k < parameterMatrix.RowCount; k++)
                {
                    res[k] = new double[parameterMatrix.ColumnCount];
                    for (int n = 0; n < parameterMatrix.ColumnCount; n++)
                    {
                        res[k][n] = parameterMatrix[k, n];
                    }
                }
                return res;
            }
            set
            {
                parameterMatrix = new DenseMatrix(value.Length, value[0].Length);
                for (int k = 0; k < value.Length; k++)
                {
                    for (int n = 0; n < value[0].Length; n++)
                    {
                        parameterMatrix[k, n] = value[k][n];
                    }
                }

            }
        }*/

        public static void Load(string file)
        {
            XmlSerializer XmlSerializer = new XmlSerializer(typeof(Storage));
            try { using (var r = new StreamReader(file)) s = (Storage)XmlSerializer.Deserialize(r); }
            catch
            {
                s = new Storage();
                s.DataSet = new List<DataPoint>();
            }
        }

        public static void Save(string file)
        {
            XmlSerializer XmlSerializer = new XmlSerializer(typeof(Storage));
            using (var w = new StreamWriter(file))
                XmlSerializer.Serialize(w, s);
        }

        public int NeuronalNetworkParameterCount
        {
            get
            {
                int nnpCount = 0;
                for (int i = 0; i < Storage.s.NeuronalNetworkConfiguration.Length - 1; i++)
                    nnpCount += Storage.s.NeuronalNetworkConfiguration[i + 1] * (Storage.s.NeuronalNetworkConfiguration[i] + 1);
                return nnpCount;
            }
        }
    }
}
