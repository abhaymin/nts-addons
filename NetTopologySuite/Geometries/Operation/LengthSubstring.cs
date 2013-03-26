using System;
using GeoAPI.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Geometries.Operation
{
    /**
 * Computes a substring of a {@link LineString}
 * between given distances along the line.
 * <ul>
 * <li>The distances are clipped to the actual line length
 * <li>If the start distance is equal to the end distance,
 * a zero-length line with two identical points is returned
 * <li>FUTURE: If the start distance is greater than the end distance,
 * an inverted section of the line is returned
 * </ul>
 * <p>
 * FUTURE: should handle startLength > endLength, and flip the returned
 * linestring. Also should handle negative lengths (they are measured from end
 * of line backwards).
 */
    // Martin made a decision to create this duplicate of a class from JCS. 
    // [Jon Aquino 2004-10-25]
    public class LengthSubstring
    {
        public static ILineString getSubstring(ILineString line, double startLength,
                                              double endLength)
        {
            LengthSubstring ls = new LengthSubstring(line);
            return ls.getSubstring(startLength, endLength);
        }

        private ILineString line;

        public LengthSubstring(ILineString line)
        {
            this.line = line;
        }

        public ILineString getSubstring(double startDistance, double endDistance)
        {
            // future: if start > end, flip values and return an inverted line
            Assert.IsTrue(startDistance <= endDistance, "inverted distances not currently supported");

            Coordinate[] coordinates = line.Coordinates;
            // check for a zero-length segment and handle appropriately
            if (endDistance <= 0.0)
            {
                return line.Factory.CreateLineString(
                    new Coordinate[] { coordinates[0], coordinates[0] });
            }
            if (startDistance >= line.Length)
            {
                return line.Factory
                    .CreateLineString(
                    new Coordinate[] { coordinates[coordinates.Length - 1],
    coordinates[coordinates.Length - 1]});
            }
            if (startDistance < 0.0)
            {
                startDistance = 0.0;
            }
            return computeSubstring(startDistance, endDistance);
        }

        /**
         * Assumes input is strictly valid (e.g. startDist < endDistance)
         *
         * @param startDistance
         * @param endDistance
         * @return
         */
        private ILineString computeSubstring(double startDistance, double endDistance)
        {
            Coordinate[] coordinates = line.Coordinates;
            CoordinateList newCoordinates = new CoordinateList();
            Double segmentStartDistance = 0.0;
            Double segmentEndDistance = 0.0;
            //Boolean started = false;
            Int32 i = 0;
            LineSegment segment = new LineSegment();
            while (i < coordinates.Length - 1 && endDistance > segmentEndDistance)
            {
                segment.P0 = coordinates[i];
                segment.P1 = coordinates[i + 1];
                i++;
                segmentStartDistance = segmentEndDistance;
                segmentEndDistance = segmentStartDistance + segment.Length;

                if (startDistance > segmentEndDistance)
                    continue;
                if (startDistance >= segmentStartDistance
                    && startDistance < segmentEndDistance)
                {
                    newCoordinates.Add(LocatePoint.PointAlongSegment(segment.P0, segment.P1,
                        startDistance - segmentStartDistance), false);
                }
                /*
                if (startDistance >= segmentStartDistance
                    && startDistance == segmentEndDistance) {
                  newCoordinates.add(new Coordinate(segment.p1), false);
                }
                */
                if (endDistance >= segmentEndDistance)
                {
                    newCoordinates.Add(new Coordinate(segment.P1), false);
                }
                if (endDistance >= segmentStartDistance
                    && endDistance < segmentEndDistance)
                {
                    newCoordinates.Add(LocatePoint.PointAlongSegment(segment.P0, segment.P1,
                        endDistance - segmentStartDistance), false);
                }
            }
            Coordinate[] newCoordinateArray = newCoordinates.ToArray();
            /**
             * Ensure there is enough coordinates to build a valid line. Make a
             * 2-point line with duplicate coordinates, if necessary There will
             * always be at least one coordinate in the coordList.
             */
            if (newCoordinateArray.Length <= 1)
            {
                newCoordinateArray = new Coordinate[] { newCoordinateArray[0], newCoordinateArray[0] };
            }
            return line.Factory.CreateLineString(newCoordinateArray);
        }
    }
}