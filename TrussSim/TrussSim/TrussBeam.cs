using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MathNet.Numerics.LinearAlgebra.Generic;
using MathNet.Numerics.LinearAlgebra.Double;
using Scalar = System.Double;

namespace TrussSim
{
    class TrussBeam
    {
        /// <summary>
        /// Unit: Metre
        /// </summary>
        public double Length;

        /// <summary>
        /// Unit: Metre
        /// </summary>
        public double LengthLoaded;

        /// <summary>
        /// Unit: Metre^2
        /// </summary>
        public double CrosssectionArea;

        /// <summary>
        /// Unit: Pascal = Newton / Metre^2
        /// </summary>
        public double YoungsModulus;

        public double ExtensionRatio { get { return (LengthLoaded - Length) / Length; } }

        public double TensileStress { get { return YoungsModulus * ExtensionRatio; } }
    }
}
