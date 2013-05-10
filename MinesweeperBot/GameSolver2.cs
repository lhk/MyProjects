using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MinesweeperBot
{
    class GameSolver2
    {
        //char[,] PreviousCategorization;
        public List<Point> knownFreeFields = new List<Point>();
        public List<Point> knownMinedFields = new List<Point>();

        public void Analyze(char[,] Categorization)
        {
            knownFreeFields.Clear();
            knownMinedFields.Clear();

            int width = Categorization.GetUpperBound(0) + 1;
            int height = Categorization.GetUpperBound(1) + 1;

            List<Point> ignoreList = new List<Point>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    if (LabelToNumber(Categorization[x, y]) > 0)
                    {
                        int perimeterCount = 0;
                        PerimeterIterator(new Point(x, y), width, height, p => { if (Categorization[p.X, p.Y] == 'f') perimeterCount++; });

                        if (LabelToNumber(Categorization[x, y]) == perimeterCount)
                        {
                            ignoreList.Add(new Point(x, y));
                            PerimeterIterator(new Point(x, y), width, height, p => { if (Categorization[p.X, p.Y] == 'x') knownFreeFields.Add(p); });
                        }
                    }
                }


            List<Point> FreeFields = new List<Point>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (LabelToNumber(Categorization[x, y]) > 0 && !ignoreList.Contains(new Point(x, y)))// && (PreviousCategorization == null || LabelToNumber(PreviousCategorization[x, y]) <= 0))
                        FreeFields.Add(new Point(x, y));

            /*int firstIterationFieldListLimit = FreeFields.Count;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (LabelToNumber(Categorization[x, y]) > 0 && !FreeFields.Contains(new Point(x, y)))
                        FreeFields.Add(new Point(x, y));*/

            // get n tupel list
            for (int n = 1; n < 4; n++)
            {
                List<Point[]> tupleList = new List<Point[]>();
                TupleCombinationTree(tupleList, FreeFields, new Point[n], 0, 0);

                for (int i = 0; i < tupleList.Count; i++)
                {
                    if (tupleList[i].Contains(new Point(14, 9)) && tupleList[i].Contains(new Point(14, 10)))
                    {

                    }

                    AnalyzeTuple(Categorization, tupleList[i], width, height);
                }
            }
        }

        void PerimeterIterator(Point point, int width, int height, Action<Point> func)
        {
            for (int x = point.X - 1; x <= point.X + 1; x++)
            {
                for (int y = point.Y - 1; y <= point.Y + 1; y++)
                {
                    if (x >= 0 && y >= 0 && x < width && y < height && (x != point.X || y != point.Y))
                    {
                        func(new Point(x, y));
                    }
                }
            }
        }

        void AnalyzeTuple(char[,] Categorization, Point[] tuple, int width, int height)
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
            bool[] unknownIsMined = new bool[unknowns.Count];
            for (int i = 0; i < unknownIsFree.Length; i++)
            {
                unknownIsFree[i] = true;
                unknownIsMined[i] = true;
            }

            int minimumPossibleMinesInCurrentTuple = 0;
            int maximumPossibleMinesInCurrentTuple = 0;
            for (int i = 0; i < tuple.Length; i++)
            {
                int fieldPerimeterCount = LabelToNumber(Categorization[tuple[i].X, tuple[i].Y]);
                maximumPossibleMinesInCurrentTuple += fieldPerimeterCount;
                if (minimumPossibleMinesInCurrentTuple > fieldPerimeterCount) minimumPossibleMinesInCurrentTuple = fieldPerimeterCount;
            }

            AnalyzePermutations(Categorization, tuple, unknowns, unknownIsFree, unknownIsMined, minimumPossibleMinesInCurrentTuple, maximumPossibleMinesInCurrentTuple);



            // if one unknow is guaranteed to be free, return it as the new point to click on
            for (int i = 0; i < unknowns.Count; i++)
            {
                if (unknownIsFree[i] && !knownFreeFields.Contains(unknowns[i]))
                    knownFreeFields.Add(unknowns[i]);

                if (unknownIsMined[i] && !knownMinedFields.Contains(unknowns[i]))
                    knownMinedFields.Add(unknowns[i]);
            }
        }

        void AnalyzePermutations(char[,] Categorization, Point[] tuple, List<Point> unknowns, bool[] unknownIsFree, bool[] unknownIsMined, int minimumPossibleMinesInCurrentTuple, int maximumPossibleMinesInCurrentTuple)
        {
            for (int i = minimumPossibleMinesInCurrentTuple; i <= maximumPossibleMinesInCurrentTuple; i++)
            {
                KinN_permutationTree(Categorization, tuple, unknowns, unknownIsFree, unknownIsMined, i, 0, 0, 0);
            }
        }

        void KinN_permutationTree(char[,] Categorization, Point[] tuple, List<Point> unknowns, bool[] unknownIsFree, bool[] unknownIsMined, int wantedTargetSetSize, int currentTargetSetSize, long currentTargetSetBits, int startIndex)
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
                            if (GetBit(nextTargetSetBits, j)) 
                                unknownIsFree[j] = false;
                            else
                                unknownIsMined[j] = false;
                        }
                    }
                }
                else KinN_permutationTree(Categorization, tuple, unknowns, unknownIsFree, unknownIsMined, wantedTargetSetSize, currentTargetSetSize + 1, nextTargetSetBits, i + 1);
            }
        }

        bool IsTuplePermutationPossible(char[,] Categorization, Point[] tuple, List<Point> unknowns, long permutation)
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

        int perimeterCount(Point p1, List<Point> unknowns, long permutation)
        {
            int perimeterCount = 0;
            for (int j = 0; j < unknowns.Count; j++)
            {
                if (IsInPerimeter(new Point(unknowns[j].X, unknowns[j].Y), p1) && GetBit(permutation, j))
                    perimeterCount++;
            }
            return perimeterCount;
        }

        void TupleCombinationTree(List<Point[]> tupleList, List<Point> fields, Point[] tuple, int startIndex, int currentDepth)
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

        bool AllEqual(char[,] PreviousCategorization, char[,] Categorization)
        {
            int width = Categorization.GetUpperBound(0) + 1;
            int height = Categorization.GetUpperBound(1) + 1;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (PreviousCategorization[x, y] != Categorization[x, y]) return false;
            return true;
        }

        bool TupleIsConnected(Point[] tuple)
        {
            bool[] fieldIsConnected = new bool[tuple.Length];
            for (int i = 0; i < fieldIsConnected.Length; i++) fieldIsConnected[i] = false;
            fieldIsConnected[0] = true;

            // iterate over all possible connections
            for (int i = 0; i < tuple.Length - 1; i++)
            {
                for (int j = i + 1; j < tuple.Length; j++)
                {
                    if (fieldIsConnected[i] && IsInExtendedPerimeter(tuple[i], tuple[j]))
                        fieldIsConnected[j] = true;
                }
            }
            return AllTrue(fieldIsConnected);
        }

        bool AllTrue(bool[] pointIsConnected)
        {
            foreach (var item in pointIsConnected) if (!item) return false;
            return true;
        }

        int LabelToNumber(char p)
        {
            byte b = (byte)p;
            if (b < 48 || b > 57) return -1;
            return b - 48;
        }

        bool IsInPerimeter(Point p1, Point p2)
        {
            int d = SqDist(p1, p2);
            return d > 0 && d <= 2;
        }

        bool IsInExtendedPerimeter(Point p1, Point p2)
        {
            int d = SqDist(p1, p2);
            return d > 0 && d <= 8;
        }

        int SqDist(Point p1, Point p2)
        {
            return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
        }

        bool GetBit(long code, int position)
        {
            return ((code >> position) & 1) != 0;
        }

        long SetBit(long code, bool bit, int position)
        {
            if (bit) return (((long)1) << position) | code;
            else return (~(((long)1) << position)) & code;
        }
    }
}
