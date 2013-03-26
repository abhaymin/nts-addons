using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Operation
{
    /**
    * Computes the length along a LineString to the point on the line nearest a given point.
    */

    internal class LengthToPoint
    {
        private double _locationLength;
        private double _minDistanceToPoint;

        public LengthToPoint(ILineString line, Coordinate inputPt)
        {
            ComputeLength(line, inputPt);
        }

        public static double LengthAlongSegment(LineSegment seg, Coordinate pt)
        {
            double projFactor = seg.ProjectionFactor(pt);
            double len;
            if (projFactor <= 0.0)
                len = 0.0;
            else if (projFactor <= 1.0)
                len = projFactor*seg.Length;
            else
                len = seg.Length;
            return len;
        }

        /**
       * Computes the length along a LineString to the point on the line nearest a given point.
       */

        public static double Length(ILineString line, Coordinate inputPt)
        {
            var lp = new LengthToPoint(line, inputPt);
            return lp.GetLength();
        }

        public double GetLength()
        {
            return _locationLength;
        }

        private void ComputeLength(ILineString line, Coordinate inputPt)
        {
            _minDistanceToPoint = Double.MaxValue;
            double baseLocationDistance = 0.0;
            Coordinate[] pts = line.Coordinates;
            var seg = new LineSegment();
            for (int i = 0; i < pts.Length - 1; i++)
            {
                seg.P0 = pts[i];
                seg.P1 = pts[i + 1];
                UpdateLength(seg, inputPt, baseLocationDistance);
                baseLocationDistance += seg.Length;
            }
        }

        private void UpdateLength(LineSegment seg, Coordinate inputPt, double segStartLocationDistance)
        {
            double dist = seg.Distance(inputPt);
            if (dist > _minDistanceToPoint) return;
            _minDistanceToPoint = dist;
            // found new minimum, so compute location distance of point
            double projFactor = seg.ProjectionFactor(inputPt);
            if (projFactor <= 0.0)
                _locationLength = segStartLocationDistance;
            else if (projFactor <= 1.0)
                _locationLength = segStartLocationDistance + projFactor*seg.Length;
            else
                _locationLength = segStartLocationDistance + seg.Length;
        }
    }
}