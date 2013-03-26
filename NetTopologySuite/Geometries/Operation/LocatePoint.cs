using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Operation
{
    /**
 * Provides various ways of computing the actual value
 * of a point a given length along a line.
 */

    public class LocatePoint
    {
        /**
         * Computes the location of a point a given length along a {@link LineSegment}.
         * If the length exceeds the length of the line segment the last
         * point of the segment is returned.
         * If the length is negative the first point
         * of the segment is returned.
         *
         * @param seg the line segment
         * @param length the length to the desired point
         * @return the {@link Coordinate} of the desired point
         */

        private Int32 _index;
        private Coordinate _pt;

        public LocatePoint(ILineString line, double length)
        {
            Compute(line, length);
        }

        public static Coordinate PointAlongSegment(LineSegment seg, double length)
        {
            return PointAlongSegment(seg.P0, seg.P1, length);
        }

        /**
         * Computes the location of a point a given length along a line segment.
         * If the length exceeds the length of the line segment the last
         * point of the segment is returned.
         * If the length is negative the first point
         * of the segment is returned.
         *
         * @param p0 the first point of the line segment
         * @param p1 the last point of the line segment
         * @param length the length to the desired point
         * @return the {@link Coordinate} of the desired point
         */

        public static Coordinate PointAlongSegment(Coordinate p0, Coordinate p1, double length)
        {
            double segLen = p1.Distance(p0);
            double frac = length/segLen;
            if (frac <= 0.0) return p0;
            if (frac >= 1.0) return p1;

            double x = (p1.X - p0.X)*frac + p0.X;
            double y = (p1.Y - p0.Y)*frac + p0.Y;
            return new Coordinate(x, y);
        }

        /**
         * Computes the location of a point a given fraction along a line segment.
         * If the fraction exceeds 1 the last point of the segment is returned.
         * If the fraction is negative the first point of the segment is returned.
         *
         * @param p0 the first point of the line segment
         * @param p1 the last point of the line segment
         * @param frac the fraction of the segment to the desired point
         * @return the {@link Coordinate} of the desired point
         */

        public static Coordinate PointAlongSegmentByFraction(Coordinate p0, Coordinate p1, double frac)
        {
            if (frac <= 0.0) return p0;
            if (frac >= 1.0) return p1;

            double x = (p1.X - p0.X)*frac + p0.X;
            double y = (p1.Y - p0.Y)*frac + p0.Y;
            return new Coordinate(x, y);
        }

        /**
         * Computes the {@link Coordinate} of the point a given length
         * along a {@link LineString}.
         *
         * @param line
         * @param length
         * @return the {@link Coordinate} of the desired point
         */

        public static Coordinate PointAlongLine(ILineString line, double length)
        {
            var loc = new LocatePoint(line, length);
            return loc.GetPoint();
        }

        private void Compute(ILineString line, double length)
        {
            // <TODO> handle negative distances (measure from opposite end of line)
            double totalLength = 0.0;
            Coordinate[] coord = line.Coordinates;
            for (int i = 0; i < coord.Length - 1; i++)
            {
                Coordinate p0 = coord[i];
                Coordinate p1 = coord[i + 1];
                double segLen = p1.Distance(p0);
                if (totalLength + segLen > length)
                {
                    _pt = PointAlongSegment(p0, p1, length - totalLength);
                    _index = i;
                    return;
                }
                totalLength += segLen;
            }
            // distance is greater than line length
            _pt = new Coordinate(coord[coord.Length - 1]);
            _index = coord.Length;
        }

        public Coordinate GetPoint()
        {
            return _pt;
        }

        /**
         * Returns the index of the segment containing the computed point
         */

        public int GetIndex()
        {
            return _index;
        }
    }
}