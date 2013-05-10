using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using System.Data.SQLite;
using System.Windows.Forms;

namespace MinesweeperBot
{
    public class Storage
    {
        // new SQLite
        public static SQLiteConnection SQLite = new SQLiteConnection("Data Source=" + Program.GetBasePath() + "\\db.sqlite" + ";Version=3;Compress=True;");

        public static List<DataPoint> DataPoints = null;
        public static ArtificialNeuralNetwork ANN = null;
        public static PrincipalComponentAnalysis PCA = null;
        public static Preprocessor Preprocessor;
        


        // old XML
        public static Storage s;

        //public List<DataPoint> DataSet;
        public int[] TrainingSetMapping;
        public int[] TestSetMapping;
        public int DataPointDimensions { get { return DataPoints[0].Features.Length; } }
        public int CentroidCount = 30;
        //public string ClassificationSet;
        public CentroidSet CentroidSet;
        //public double learningRate = 0.0001;
        //public double[] NeuronalNetworkParameters;

        //[XmlIgnore]
        //public readonly int[] NeuronalNetworkConfiguration = new int[] { 256, 15, 11 };


        public static void Load( )
        {
            s = new Storage();
            DataPoints = new List<DataPoint>();

            SQLite.Open();
            
            // read datapoints
            var res = new SQLiteCommand("SELECT * FROM LabeledData ORDER BY `ID`", SQLite).ExecuteReader();
            while (res.Read())
            {
                string ID = "";
                string Features = "";
                char Label = '?', PreLabel = '?', Set = '?';

                for (int i = 0; i < res.VisibleFieldCount; i++)
                {
                    switch (res.GetName(i))
                    {
                        case "ID": ID = res.GetString(i); break;
                        case "Features": Features = res.GetString(i); break;
                        case "Label": Label = res.GetString(i)[0]; break;
                        case "PreLabel": PreLabel = res.GetString(i)[0]; break;
                        case "Set": Set = res.GetString(i)[0]; break;
                    }
                }
                DataPoints.Add(new DataPoint(ID, Features, Label, PreLabel, Set));
            }

            // read other serialized objects
            res = new SQLiteCommand("SELECT * FROM Objects", SQLite).ExecuteReader();
            while (res.Read())
            {
                string ID = "";
                string Type = "";
                string Data = "";

                for (int i = 0; i < res.VisibleFieldCount; i++)
                {
                    switch (res.GetName(i))
                    {
                        case "ID": ID = res.GetString(i); break;
                        case "Type": Type = res.GetString(i); break;
                        case "Data": Data = res.GetString(i); break;
                    }
                }
                if (Type == "ArtificialNeuralNetwork")
                {
                    Storage.ANN = new ArtificialNeuralNetwork();
                    Storage.ANN.ID = ID;
                    Storage.ANN.Deserialize(Data);
                }
                /*else if (Type == "PrincipalComponentAnalysis")
                {
                    Storage.PCA = new PrincipalComponentAnalysis();
                    Storage.PCA.ID = ID;
                    Storage.PCA.Deserialize(Data);
                }*/
            }

            SQLite.Close();

            if (Storage.ANN == null)
            {
                Storage.ANN = new ArtificialNeuralNetwork();
                Storage.ANN.Init();
            }
            /*if (Storage.PCA == null)
            {
                Storage.PCA = new PrincipalComponentAnalysis();
                Storage.PCA.PCASetup();
            }*/

            Preprocessor = new Preprocessor();
        }


        public static void Save()
        {
            SQLite.Open();
            new SQLiteCommand("begin", SQLite).ExecuteNonQuery();

            for (int i = 0; i < DataPoints.Count; i++)
            {
                DataPoints[i].SaveToDatabase(SQLite);
            }


            Storage.ANN.SaveToDatabase(SQLite);
            //Storage.PCA.SaveToDatabase(SQLite);


            new SQLiteCommand("DELETE FROM LabeledData WHERE `Label` == 'j'", SQLite).ExecuteReader();
            new SQLiteCommand("end", SQLite).ExecuteNonQuery(); 
            SQLite.Close();
        }
    }
}
