﻿using System;
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

        public static List<DataPoint> DataPoints;
        public static SQLiteConnection SQLite = new SQLiteConnection("Data Source=" + Program.GetBasePath() + "\\db.sqlite" + ";Version=3;Compress=True;");


        // old XML
        public static Storage s;

        //public List<DataPoint> DataSet;
        public int[] TrainingSetMapping;
        public int[] TestSetMapping;
        public int DataPointDimensions { get { return DataPoints[0].Features.Length; } }
        public int CentroidCount = 30;
        public string ClassificationSet;
        public CentroidSet CentroidSet;
        public double learningRate = 0.0001;
        public double[] NeuronalNetworkParameters;

        [XmlIgnore]
        public readonly int[] NeuronalNetworkConfiguration = new int[] { 256, 15, 11 };


        public static void Load( )
        {
            s = new Storage();

            DataPoints = new List<DataPoint>();


            SQLite.Open();
            var res = new SQLiteCommand("SELECT * FROM LabeledData", SQLite).ExecuteReader();
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
            SQLite.Close();


            /*foreach (var item in s.DataSet)
            {
                string features = FormatHelper.DoubleArrayToString(item.Features,3);
                string id = FormatHelper.hash(features);

                DataPointDB p = new DataPointDB(id,features,item.Label,'?','?');
                Storage.DataPoints.Add(p);
            }*/
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

        internal static void Save()
        {
            SQLite.Open();
            new SQLiteCommand("begin", SQLite).ExecuteNonQuery();

            for (int i = 0; i < DataPoints.Count; i++)
            {
                if (i == 5088)
                {
                }
                DataPoints[i].SaveToDatabase(SQLite, false);
            }

            new SQLiteCommand("end", SQLite).ExecuteNonQuery(); 
            SQLite.Close();
        }
    }
}
