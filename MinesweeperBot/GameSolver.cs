using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MinesweeperBot
{
    class GameSolver
    {
        public static Point FindNextClick(char[,] Categorization, Point lastClick)
        {
            int width = Categorization.GetUpperBound(0) + 1;
            int height = Categorization.GetUpperBound(1) + 1;

            // create list of all non-zero && known fields
            List<Point> fields = new List<Point>();
            for (int x1 = 0; x1 < width; x1++)
            {
                for (int y1 = 0; y1 < height; y1++)
                {
                    if (Categorization[x1, y1] != 'x' && Categorization[x1, y1] != 'f' && Categorization[x1, y1] != '0')
                    {
                        fields.Add(new Point(x1, y1));
                    }
                }
            }

            // search list of all n tuples
            for (int n = 2; n <= 3; n++)
            {
                List<Point[]> NTuples = GetAllNTuples(n, fields);
                var itemList = (from t in NTuples
                                orderby SqDist(lastClick,TupleCenter(t))
                                select t).ToArray();

                foreach (var tuple in itemList)
                {
                    Point p = AnalyzeTuple(Categorization, tuple, width, height);
                    if (p.X >= 0) return p;
                }
            }



            return new Point(-1, -1);
        }

        private static List<Point[]> GetAllNTuples(int n, List<Point> fields)
        {
            List<Point[]> tupleList = new List<Point[]>();
            TupleCombinationTree(tupleList, fields, new Point[n], 0, 0);
            return tupleList;
        }

        /*private static List<Point[]> FilterForConnectedTuples(List<Point[]> unfilteredTupleList)
        {
            List<Point[]> filteredTupleList = new List<Point[]>();
            foreach (var tuple in unfilteredTupleList)
            {
                if (TupleIsConnected(tuple)) filteredTupleList.Add(tuple);
            }
            return filteredTupleList;
        }*/

        private static bool TupleIsConnected(Point[] tuple)
        {
            bool[] pointIsConnected = new bool[tuple.Length];
            for (int i = 0; i < pointIsConnected.Length; i++) pointIsConnected[i] = false;
            pointIsConnected[0] = true;

            // iterate over all possible connections
            for (int i = 0; i < tuple.Length - 1; i++)
            {
                for (int j = i + 1; j < tuple.Length; j++)
                {
                    if (pointIsConnected[i] && IsInExtendedPerimeter(tuple[i], tuple[j]))
                        pointIsConnected[j] = true;
                }
            }
            return AllTrue(pointIsConnected);
        }

        private static bool AllTrue(bool[] pointIsConnected)
        {
            foreach (var item in pointIsConnected)if (!item) return false;
            return true;
        }

        private static void TupleCombinationTree(List<Point[]> tupleList, List<Point> fields, Point[] tuple, int startIndex, int currentDepth)
        {
            for (int i = startIndex; i < fields.Count; i++)
            {
                Point[] tuple_next = new Point[tuple.Length];
                Array.Copy(tuple, tuple_next, tuple_next.Length);

                tuple_next[currentDepth] = fields[i];
                if (currentDepth >= tuple.Length - 1)
                {
                    if (TupleIsConnected(tuple_next))
                    {
                        tupleList.Add(tuple_next);
                    }
                }
                else TupleCombinationTree(tupleList, fields, tuple_next, i + 1, currentDepth + 1);
            }
        }


        private static Point AnalyzeTuple(char[,] Categorization, Point[] tuple, int width, int height)
        {

            // list all unkowns connected to the tuple
            List<Point> unknowns = new List<Point>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (Categorization[x, y] == 'x' || Categorization[x, y] == 'f')
                    {
                        Point p = new Point(x, y);
                        for (int i = 0; i < tuple.Length; i++)
                        {
                            if (IsInPerimeter(p, tuple[i]))
                            {
                                unknowns.Add(p);
                                break;
                            }
                        }
                    }


            bool[] unknownIsFree = new bool[unknowns.Count];
            for (int i = 0; i < unknownIsFree.Length; i++) unknownIsFree[i] = true;


            int minimumPossibleMinesInCurrentTuple = 0;
            int maximumPossibleMinesInCurrentTuple = 0;
            for (int i = 0; i < tuple.Length; i++)
            {
                int fieldPerimeterCount = LabelToNumber(Categorization[tuple[i].X, tuple[i].Y]);
                maximumPossibleMinesInCurrentTuple += fieldPerimeterCount;
                if (minimumPossibleMinesInCurrentTuple > fieldPerimeterCount) minimumPossibleMinesInCurrentTuple = fieldPerimeterCount;
            }

            AnalyzePermutations(Categorization, tuple, unknowns, unknownIsFree, minimumPossibleMinesInCurrentTuple, maximumPossibleMinesInCurrentTuple);

            

            // if one unknow is guaranteed to be free, return it as the new point to click on
            for (int i = 0; i < unknowns.Count; i++)
            {
                if (unknownIsFree[i]) return unknowns[i];
            }

            // return that no free point has been found
            return new Point(-1, -1);
        }

        private static bool IsTuplePermutationPossible(char[,] Categorization, Point[] tuple, List<Point> unknowns, long permutation)
        {
            bool isPossiblePermutation = true;
            for (int j = 0; j < tuple.Length && isPossiblePermutation; j++)
            {
                if (perimeterCount(tuple[j], unknowns, permutation) != LabelToNumber(Categorization[tuple[j].X, tuple[j].Y]))
                {
                    isPossiblePermutation = false;
                }
            }
            return isPossiblePermutation;
        }

        private static void AnalyzePermutations(char[,] Categorization, Point[] tuple, List<Point> unknowns, bool[] unknownIsFree, int minimumPossibleMinesInCurrentTuple, int maximumPossibleMinesInCurrentTuple)
        {
            for (int i = minimumPossibleMinesInCurrentTuple; i <= maximumPossibleMinesInCurrentTuple; i++)
			{
                KinN_permutationTree(Categorization, tuple, unknowns, unknownIsFree, i, 0, 0, 0);
			}
        }

        private static void KinN_permutationTree(char[,] Categorization, Point[] tuple, List<Point> unknowns, bool[] unknownIsFree, int wantedTargetSetSize, int currentTargetSetSize, long currentTargetSetBits, int startIndex)
        {
            for (int i = startIndex; i < unknowns.Count; i++)
            {
                long nextTargetSetBits = currentTargetSetBits;
                nextTargetSetBits = SetBit(nextTargetSetBits, true, i);

                if (wantedTargetSetSize == currentTargetSetSize)
                {
                    //permutations.Add(nextTargetSetBits);
                    if (IsTuplePermutationPossible(Categorization, tuple, unknowns, nextTargetSetBits))
                    {
                        for (int j = 0; j < unknowns.Count; j++)
                        {
                            if (GetBit(nextTargetSetBits, j)) unknownIsFree[j] = false;
                        }
                    }
                }
                else KinN_permutationTree(Categorization, tuple, unknowns, unknownIsFree,  wantedTargetSetSize, currentTargetSetSize + 1, nextTargetSetBits, i + 1);
            }
        }

        private static bool IsInField(Point p, int width, int height)
        {
            return p.X >= 0 && p.Y >= 0 && p.X < width && p.Y < height;
        }

        private static bool IsInPerimeter(Point p1, Point p2)
        {
            int d = SqDist(p1.X, p1.Y, p2.X, p2.Y);
            return d > 0 && d <= 2;
        }

        private static bool IsInExtendedPerimeter(Point p1, Point p2)
        {
            int d = SqDist(p1.X, p1.Y, p2.X, p2.Y);
            return d > 0 && d <= 8;
        }

        private static int perimeterCount(Point p1, List<Point> unknowns, long permutation)
        {
            int perimeterCount = 0;
            for (int j = 0; j < unknowns.Count; j++)
            {
                if (IsInPerimeter(new Point(unknowns[j].X, unknowns[j].Y), p1) && GetBit(permutation, j))
                    perimeterCount++;
            }
            return perimeterCount;
        }

        private static int LabelToNumber(char p)
        {
            byte b = (byte)p;
            if (b < 48 || b > 57) return -1;
            return b - 48;
        }

        private static Point TupleCenter(Point[] Tuple)
        {
            Point res = new Point(0,0);
            foreach (var item in Tuple)
            {
                res.X += item.X;
                res.Y += item.Y;
            }
            return new Point(res.X / Tuple.Length, res.Y / Tuple.Length);
        }

        private static int SqDist(Point p1, Point p2)
        {
            return SqDist(p1.X, p1.Y, p2.X, p2.Y);
        }

        private static int SqDist(int x, int y, int x2, int y2)
        {
            return (x - x2) * (x - x2) + (y - y2) * (y - y2);
        }

        private static bool GetBit(long code, int position)
        {
            return ((code >> position) & 1) != 0;
        }

        public static long SetBit(long code, bool bit, int position)
        {
            if (bit) return (((long)1) << position) | code;
            else return (~(((long)1) << position)) & code;
        }

        private static long Pow(long _base, int exponent)
        {
            long p = 1;
            for (int i = 0; i < exponent; i++)
            {
                p *= _base;
            }
            return p;
        }
    }
}
