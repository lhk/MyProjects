using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using Scalar = System.Double;

namespace TrussSim
{
    class TrussNode
    {
        /// <summary>
        /// Absolute position.
        /// Unit: Metres
        /// </summary>
        public Vector<Scalar> Position;

        /// <summary>
        /// Change in position due to strain and defromation.
        /// Unit: Metres
        /// </summary>
        public Vector<Scalar> Displacement;

        /// <summary>
        /// Sum of Position and Displacement.
        /// Unit: Metres
        /// </summary>
        public Vector<Scalar> DisplacedPosition { get { return Position + Displacement; } }

        public bool Fixed = false;

        public Vector<Scalar> ExternalLoad;
    }
}
