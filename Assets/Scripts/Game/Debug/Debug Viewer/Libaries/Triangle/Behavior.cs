﻿// -----------------------------------------------------------------------
// <copyright file="Behavior.cs">
// Original Triangle code by Jonathan Richard Shewchuk, http://www.cs.cmu.edu/~quake/triangle.html
// Triangle.NET code by Christian Woltering, http://triangle.codeplex.com/
// </copyright>
// -----------------------------------------------------------------------

namespace TriangleNet
{
    using System;
    using TriangleNet.Geometry;

    /// <summary>
    /// Controls the behavior of the meshing software.
    /// </summary>
    internal class Behavior
    {
        private bool poly = false;
        private bool quality = false;
        private bool varArea = false;
        private bool convex = false;
        private bool jettison = false;
        private bool boundaryMarkers = true;
        private bool noHoles = false;
        private bool conformDel = false;

        private Func<ITriangle, double, bool> usertest;

        private int noBisect = 0;

        private double minAngle = 0.0;
        private double maxAngle = 0.0;
        private double maxArea = -1.0;

        internal bool fixedArea = false;
        internal bool useSegments = true;
        internal bool useRegions = false;
        internal double goodAngle = 0.0;
        internal double maxGoodAngle = 0.0;
        internal double offconstant = 0.0;

        /// <summary>
        /// Creates an instance of the Behavior class.
        /// </summary>
        public Behavior(bool quality = false, double minAngle = 20.0)
        {
            if (quality)
            {
                this.quality = true;
                this.minAngle = minAngle;

                Update();
            }
        }

        /// <summary>
        /// Update quality options dependencies.
        /// </summary>
        private void Update()
        {
            this.quality = true;

            if (this.minAngle < 0 || this.minAngle > 60)
            {
                this.minAngle = 0;
                this.quality = false;

                Log.Instance.Warning("Invalid quality option (minimum angle).", "Mesh.Behavior");
            }

            if ((this.maxAngle != 0.0) && (this.maxAngle < 60 || this.maxAngle > 180))
            {
                this.maxAngle = 0;
                this.quality = false;

                Log.Instance.Warning("Invalid quality option (maximum angle).", "Mesh.Behavior");
            }

            this.useSegments = this.Poly || this.Quality || this.Convex;
            this.goodAngle = Math.Cos(this.MinAngle * Math.PI / 180.0);
            this.maxGoodAngle = Math.Cos(this.MaxAngle * Math.PI / 180.0);

            if (this.goodAngle == 1.0)
            {
                this.offconstant = 0.0;
            }
            else
            {
                this.offconstant = 0.475 * Math.Sqrt((1.0 + this.goodAngle) / (1.0 - this.goodAngle));
            }

            this.goodAngle *= this.goodAngle;
        }

        #region Static properties

        /// <summary>
        /// No exact arithmetic.
        /// </summary>
        public static bool NoExact { get; set; }

        #endregion

        #region Public properties

        /// <summary>
        /// Quality mesh generation.
        /// </summary>
        public bool Quality
        {
            get => quality;
            set
            {
                quality = value;
                if (quality)
                {
                    Update();
                }
            }
        }

        /// <summary>
        /// Minimum angle constraint.
        /// </summary>
        public double MinAngle
        {
            get => minAngle;
            set { minAngle = value; Update(); }
        }

        /// <summary>
        /// Maximum angle constraint.
        /// </summary>
        public double MaxAngle
        {
            get => maxAngle;
            set { maxAngle = value; Update(); }
        }

        /// <summary>
        /// Maximum area constraint.
        /// </summary>
        public double MaxArea
        {
            get => maxArea;
            set
            {
                maxArea = value;
                fixedArea = value > 0.0;
            }
        }

        /// <summary>
        /// Apply a maximum triangle area constraint.
        /// </summary>
        public bool VarArea
        {
            get => varArea;
            set => varArea = value;
        }

        /// <summary>
        /// Input is a Planar Straight Line Graph.
        /// </summary>
        public bool Poly
        {
            get => poly;
            set => poly = value;
        }

        /// <summary>
        /// Apply a user-defined triangle constraint.
        /// </summary>
        public Func<ITriangle, double, bool> UserTest
        {
            get => usertest;
            set => usertest = value;
        }

        /// <summary>
        /// Enclose the convex hull with segments.
        /// </summary>
        public bool Convex
        {
            get => convex;
            set => convex = value;
        }

        /// <summary>
        /// Conforming Delaunay (all triangles are truly Delaunay).
        /// </summary>
        public bool ConformingDelaunay
        {
            get => conformDel;
            set => conformDel = value;
        }

        /// <summary>
        /// Suppresses boundary segment splitting.
        /// </summary>
        /// <remarks>
        /// 0 = split segments
        /// 1 = no new vertices on the boundary
        /// 2 = prevent all segment splitting, including internal boundaries
        /// </remarks>
        public int NoBisect
        {
            get => noBisect;
            set
            {
                noBisect = value;
                if (noBisect < 0 || noBisect > 2)
                {
                    noBisect = 0;
                }
            }
        }

        /// <summary>
        /// Compute boundary information.
        /// </summary>
        public bool UseBoundaryMarkers
        {
            get => boundaryMarkers;
            set => boundaryMarkers = value;
        }

        /// <summary>
        /// Ignores holes in polygons.
        /// </summary>
        public bool NoHoles
        {
            get => noHoles;
            set => noHoles = value;
        }

        /// <summary>
        /// Jettison unused vertices from output.
        /// </summary>
        public bool Jettison
        {
            get => jettison;
            set => jettison = value;
        }

        #endregion
    }
}
