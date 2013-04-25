using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Graphics = OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using OpenTK.Input;
using System.Runtime.InteropServices;
using CurveFever3DClient.RenderTools;
using System.Drawing.Imaging;
using OpenTK.Platform;
using System.Reflection;

namespace CurveFever3DClient
{
    class GameData
    {
        public float boundingCubeSize = 80;
        public float pathDiameter = 2;

        public List<Vector3> path = new List<Vector3>(1023);

        //List<int>[, ,] presortedPathIndex;

        /*public GameData()
        {
            presortBlocksPerDimension = (int)Math.Floor(boundingCubeSize / maximumPathDiameter);
            presortBlockWidth = boundingCubeSize / presortBlocksPerDimension;

            presortedPathIndex = new List<int>[presortBlocksPerDimension, presortBlocksPerDimension, presortBlocksPerDimension];
            for (int x = 0; x < presortBlocksPerDimension; x++) for (int y = 0; y < presortBlocksPerDimension; y++) for (int z = 0; z < presortBlocksPerDimension; z++)
                presortedPathIndex[x, y, z] = new List<int>();
        }*/

        /*public bool AddPoint(Vector3 p)
        {
            // movement distance check (did the player move far enough to insert a new point?)
            if (path.Count > 0 && (path[path.Count - 1] - p).LengthFast < pathDiameter / 2)
                return true;

            // collision check
            int index_X = (int)Math.Floor(p.X / presortBlockWidth);
            int index_Y = (int)Math.Floor(p.Y / presortBlockWidth);
            int index_Z = (int)Math.Floor(p.Z / presortBlockWidth);

            // wall collision
            if (index_X < 0 || index_X >= presortBlocksPerDimension || index_Y < 0 || index_Y >= presortBlocksPerDimension || index_Z < 0 || index_Z >= presortBlocksPerDimension)
                return false;


            // path collision
            for (int x = index_X - 1; x <= index_X + 1; x++) for (int y = index_Y - 1; y <= index_Y + 1; y++) for (int z = index_Z - 1; z <= index_Z + 1; z++)
                if (x >= 0 && x < presortBlocksPerDimension && y >= 0 && y < presortBlocksPerDimension && z >= 0 && z < presortBlocksPerDimension)
                    foreach (int pathPointIndex in presortedPathIndex[x,y,z])
                        if (pathPointIndex + 3 < path.Count)
                            if ((path[pathPointIndex] - p).LengthFast < pathDiameter)
                                return false;

            // add path point
            path.Add(p);
            presortedPathIndex[index_X, index_Y, index_Z].Add(path.Count - 1);
            return true;
        }*/
    }
}
