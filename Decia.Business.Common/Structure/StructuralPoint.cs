using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Structure
{
    public struct StructuralPoint
    {
        public static readonly StructuralPoint GlobalStructuralPoint = new StructuralPoint(new StructuralCoordinate[] { });

        private Dictionary<StructuralDimension, StructuralCoordinate> m_Coordinates;

        public StructuralPoint(StructuralCoordinate coordinate, Nullable<int> alternateDimensionNumber)
            : this(new StructuralCoordinate[] { coordinate }, alternateDimensionNumber)
        { }

        public StructuralPoint(StructuralCoordinate coordinate)
            : this(new StructuralCoordinate[] { coordinate })
        { }

        public StructuralPoint(IEnumerable<StructuralCoordinate> coordinates, Nullable<int> alternateDimensionNumber)
        {
            if (alternateDimensionNumber.HasValue)
            {
                IEnumerable<int> existingDimensionNumbers = coordinates.Select(dim => dim.EntityDimensionNumber).Distinct().ToList();
                if (existingDimensionNumbers.Count() > 1)
                { throw new InvalidOperationException("Dimension Numbers can only be overridden when all Dimension Nubmers are the same."); }

                coordinates = coordinates.Select(cord => new StructuralCoordinate(cord, (alternateDimensionNumber.HasValue) ? alternateDimensionNumber.Value : cord.EntityDimensionNumber)).ToList();
            }

            Dictionary<StructuralDimension, StructuralCoordinate> tempCoordinates = new Dictionary<StructuralDimension, StructuralCoordinate>();
            foreach (StructuralCoordinate coordinate in coordinates.SortByDimensionType(c => c.DimensionType))
            {
                StructuralCoordinate existingCoordinate;
                bool containsDimAlready = tempCoordinates.TryGetValue(coordinate.Dimension, out existingCoordinate);

                if (!containsDimAlready)
                { tempCoordinates.Add(coordinate.Dimension, coordinate); }
                else if (coordinate.IsNull)
                { continue; }
                else if (existingCoordinate.IsNull)
                { tempCoordinates[coordinate.Dimension] = coordinate; }
                else
                { throw new InvalidOperationException("Unresolved Dimension exists."); }
            }
            m_Coordinates = tempCoordinates;
        }

        public StructuralPoint(IEnumerable<StructuralCoordinate> coordinates)
            : this(coordinates, null)
        { }

        public StructuralPoint(StructuralPoint point, Nullable<int> alternateDimensionNumber)
            : this(point, new StructuralCoordinate[] { }, alternateDimensionNumber)
        { }

        public StructuralPoint(StructuralPoint point, StructuralCoordinate extendedCoordinate, Nullable<int> alternateDimensionNumber)
            : this(point, new StructuralCoordinate[] { extendedCoordinate }, alternateDimensionNumber)
        { }

        public StructuralPoint(StructuralPoint point, StructuralCoordinate extendedCoordinate)
            : this(point, new StructuralCoordinate[] { extendedCoordinate })
        { }

        public StructuralPoint(StructuralPoint point, IEnumerable<StructuralCoordinate> extendedCoordinates, Nullable<int> alternateDimensionNumber)
            : this(point.Coordinates.Union(extendedCoordinates), alternateDimensionNumber)
        { }

        public StructuralPoint(StructuralPoint point, IEnumerable<StructuralCoordinate> extendedCoordinates)
            : this(point.Coordinates.Union(extendedCoordinates))
        { }

        public ICollection<StructuralCoordinate> Coordinates
        {
            get { return m_Coordinates.Values.ToList(); }
        }

        public IDictionary<int, StructuralCoordinate> CoordinatesById
        {
            get { return m_Coordinates.Values.ToDictionary(c => c.CoordinateId, c => c); }
        }

        public IDictionary<StructuralDimension, StructuralCoordinate> CoordinatesByDimension
        {
            get { return m_Coordinates.Values.ToDictionary(c => c.Dimension, c => c); }
        }

        public IDictionary<int, StructuralCoordinate> CoordinatesByDimensionId
        {
            get { return m_Coordinates.Values.ToDictionary(c => c.DimensionId, c => c); }
        }

        public int PointId
        {
            get { return this.GetHashCode(); }
        }

        public StructuralSpace Space
        {
            get { return new StructuralSpace(m_Coordinates.Keys); }
        }

        public int SpaceId
        {
            get { return this.GetHashCode(); }
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            { return false; }

            Type thisType = this.GetType();
            if (!obj.GetType().Equals(thisType))
            { return false; }

            StructuralPoint otherKey = (StructuralPoint)obj;

            if (this.m_Coordinates.Count != otherKey.m_Coordinates.Count)
            { return false; }

            foreach (var bucket in this.m_Coordinates)
            {
                StructuralCoordinate thisCoordinate = bucket.Value;
                StructuralCoordinate otherCoordinate;

                if (!otherKey.m_Coordinates.TryGetValue(bucket.Key, out otherCoordinate))
                { return false; }
                if (!otherCoordinate.Equals(thisCoordinate))
                { return false; }
            }
            return true;
        }

        public override string ToString()
        {
            string value = string.Empty;
            var coordinates_AsOrderedStrings = m_Coordinates.Values.Select(x => x.ToString()).OrderBy(x => x).ToList();

            foreach (var coordinate_AsString in coordinates_AsOrderedStrings)
            {
                if (string.IsNullOrWhiteSpace(value))
                { value += coordinate_AsString; }
                else
                { value += "; " + coordinate_AsString; }
            }
            return value;
        }

        public static bool operator ==(StructuralPoint a, StructuralPoint b)
        { return a.Equals(b); }

        public static bool operator !=(StructuralPoint a, StructuralPoint b)
        { return !(a == b); }

        public static StructuralPoint operator +(StructuralPoint a, StructuralPoint b)
        { return a.Merge(b); }

        public static StructuralPoint operator +(StructuralPoint a, StructuralCoordinate b)
        { return new StructuralPoint(a, b); }
    }
}