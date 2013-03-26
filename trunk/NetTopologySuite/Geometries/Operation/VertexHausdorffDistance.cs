using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Operation
{
    /**
     * Implements algorithm for computing a distance metric
     * which can be thought of as the "Vertex Hausdorff Distance".
     * This is the Hausdorff distance restricted to vertices for
     * one of the geometries.
     * Also computes two points of the Geometries which are separated by the computed distance.
     * <p>
     * <b>NOTE: This algorithm does NOT compute the full Hausdorff distance correctly, but
     * an approximation that is correct for a large subset of useful cases.
     * One important part of this subset is Linestrings that are roughly parallel to each other,
     * and roughly equal in length - just what is needed for line matching.
     * </b>
     */

    public class VertexHausdorffDistance
    {
        private readonly PointPairDistance _ptDist = new PointPairDistance();

        public VertexHausdorffDistance(IGeometry g0, IGeometry g1)
        {
            Compute(g0, g1);
        }

        public VertexHausdorffDistance(LineSegment seg0, LineSegment seg1)
        {
            Compute(seg0, seg1);
        }

        public Coordinate[] Coordinates
        {
            get { return _ptDist.GetCoordinates(); }
        }

        public static double Distance(IGeometry g0, IGeometry g1)
        {
            var vhd = new VertexHausdorffDistance(g0, g1);
            return vhd.Distance();
        }

        public double Distance()
        {
            return _ptDist.GetDistance();
        }

        private void Compute(LineSegment seg0, LineSegment seg1)
        {
            computeMaxPointDistance(seg0, seg1, _ptDist);
            computeMaxPointDistance(seg1, seg0, _ptDist);
        }

        /**
         * Computes the maximum oriented distance between two line segments,
         * as well as the point pair separated by that distance.
         *
         * @param seg0 the line segment containing the furthest point
         * @param seg1 the line segment containing the closest point
         * @param ptDist the point pair and distance to be updated
         */

        private void computeMaxPointDistance(LineSegment seg0, LineSegment seg1, PointPairDistance ptDist)
        {
            Coordinate closestPt0 = seg0.ClosestPoint(seg1.P0);
            ptDist.SetMaximum(closestPt0, seg1.P0);
            Coordinate closestPt1 = seg0.ClosestPoint(seg1.P1);
            ptDist.SetMaximum(closestPt1, seg1.P1);
        }

        private void Compute(IGeometry g0, IGeometry g1)
        {
            computeMaxPointDistance(g0, g1, _ptDist);
            computeMaxPointDistance(g1, g0, _ptDist);
        }

        private void computeMaxPointDistance(IGeometry pointGeom, IGeometry geom, PointPairDistance ptDist)
        {
            var distFilter = new MaxPointDistanceFilter(geom);
            pointGeom.Apply(distFilter);
            ptDist.SetMaximum(distFilter.GetMaxPointDistance());
        }

        public class MaxPointDistanceFilter : ICoordinateFilter
        {
            private readonly IGeometry _geom;
            private readonly PointPairDistance _maxPtDist = new PointPairDistance();
            private readonly PointPairDistance _minPtDist = new PointPairDistance();

            public MaxPointDistanceFilter(IGeometry geom)
            {
                _geom = geom;
            }

            public PointPairDistance GetMaxPointDistance()
            {
                return _maxPtDist;
            }

            #region ICoordinateFilter Members

            public void Filter(Coordinate coord)
            {
                _minPtDist.Initialize();
                EuclideanDistanceToPoint.ComputeDistance(_geom, coord, _minPtDist);
                _maxPtDist.SetMaximum(_minPtDist);
            }

            #endregion
        }
    }
}