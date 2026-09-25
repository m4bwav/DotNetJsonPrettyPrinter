using System;
using System.Collections.Generic;
using System.Linq;

namespace JsonPrettyPrinterPlusTests
{
    public sealed class SimpleObject : IEquatable<SimpleObject>
    {
        public string? Hammer { get; set; }

        public bool Equals(SimpleObject? other)
        {
            return other != null && other.Hammer == Hammer;
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as SimpleObject);
        }

        public override int GetHashCode()
        {
            return Hammer == null ? 0 : StringComparer.Ordinal.GetHashCode(Hammer);
        }
    }

    public sealed class TestLeafObject : IEquatable<TestLeafObject>
    {
        public DateTime CreatedDate { get; set; }
        public Guid Id { get; set; }
        public string[]? Names { get; set; }

        public bool Equals(TestLeafObject? other)
        {
            return other != null
                   && other.Id == Id
                   && other.CreatedDate == CreatedDate
                   && SequenceEquals(other.Names, Names);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as TestLeafObject);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (CreatedDate.GetHashCode() * 397 ^ Id.GetHashCode()) * 397 ^ (Names?.Length ?? 0);
            }
        }

        internal static bool SequenceEquals<T>(IEnumerable<T>? left, IEnumerable<T>? right)
        {
            if (left == null || right == null)
                return left == null && right == null;

            return left.SequenceEqual(right);
        }
    }

    public sealed class TestRootObject : IEquatable<TestRootObject>
    {
        public IList<TestLeafObject>? Leaves { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid Id { get; set; }
        public string[]? Titles { get; set; }
        public TestLeafObject? Friend { get; set; }

        public bool Equals(TestRootObject? other)
        {
            return other != null
                   && other.Id == Id
                   && other.CreatedDate == CreatedDate
                   && TestLeafObject.SequenceEquals(other.Leaves, Leaves)
                   && TestLeafObject.SequenceEquals(other.Titles, Titles)
                   && Equals(other.Friend, Friend);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as TestRootObject);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = CreatedDate.GetHashCode() * 397 ^ Id.GetHashCode();
                hash = hash * 397 ^ (Leaves?.Count ?? 0);
                hash = hash * 397 ^ (Titles?.Length ?? 0);
                return hash * 397 ^ (Friend?.GetHashCode() ?? 0);
            }
        }
    }
}
