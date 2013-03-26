﻿using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries.Operation
{
    /**
 * Computes the Euclidean distance (L2 metric) from a Point to a Geometry.
 * Also computes two points which are separated by the distance.
 */
    public class EuclideanDistanceToPoint
    {

        // used for point-line distance calculation
        private static readonly LineSegment TempSegment = new LineSegment();

        public static void ComputeDistance(IGeometry geom, Coordinate pt, PointPairDistance ptDist)
        {
            if (geom == null) throw new ArgumentNullException("geom");
            if (pt == null) throw new ArgumentNullException("pt");
            if (ptDist == null) throw new ArgumentNullException("ptDist");
            if (geom is ILineString) 
            {
                ComputeDistance((ILineString) geom, pt, ptDist);
            }
            else if (geom is IPolygon) 
            {
                ComputeDistance((IPolygon) geom, pt, ptDist);
            }
            else if (geom is IGeometryCollection) 
            {
                var gc = (IGeometryCollection) geom;
                for (int i = 0; i < gc.NumGeometries; i++) 
                {
                    IGeometry g = gc.GetGeometryN(i);
                    ComputeDistance(g, pt, ptDist);
                }
            }
            else 
            { // assume geometry is Point
                ptDist.SetMinimum(geom.Coordinate, pt);
            }
        }

        public static void ComputeDistance(ILineString line, Coordinate pt, PointPairDistance ptDist)
        {
            Coordinate[] coords = line.Coordinates;
            for (int i = 0; i < coords.Length - 1; i++)
            {
                TempSegment.SetCoordinates(coords[i], coords[i + 1]);
                // this is somewhat inefficient - could do better
                Coordinate closestPt = TempSegment.ClosestPoint(pt);
                ptDist.SetMinimum(closestPt, pt);
            }
        }

        public static void ComputeDistance(LineSegment segment, Coordinate pt, PointPairDistance ptDist)
        {
            Coordinate closestPt = segment.ClosestPoint(pt);
            ptDist.SetMinimum(closestPt, pt);
        }

        public static void ComputeDistance(IPolygon poly, Coordinate pt, PointPairDistance ptDist)
        {
            ComputeDistance(poly.ExteriorRing, pt, ptDist);
            for (int i = 0; i < poly.NumInteriorRings; i++)
            {
                ComputeDistance(poly.GetInteriorRingN(i), pt, ptDist);
            }
        }
    }
}