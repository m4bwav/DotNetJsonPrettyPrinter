using System;
using System.Collections.Generic;

namespace JsonPrettyPrinterPlusTests
{
    public class SimpleObject
    {
        public string Hammer { get; set; }

        public override bool Equals(object obj)
        {
            var otherObject = obj as SimpleObject;

            return otherObject != null && otherObject.Hammer == Hammer;
        }
    }

    public class TestLeafObject
    {
        public DateTime CreatedDate { get; set; }
        public Guid Id { get; set; }
        public string[] Names { get; set; }

        public override bool Equals(object obj)
        {
            var otherDomainObject = obj as TestLeafObject;

            if (otherDomainObject == null)
                return false;

            return otherDomainObject.Id.Equals(Id)
                   && otherDomainObject.Names.Length.Equals(Names.Length)
                   && AreStartDatesEqual(otherDomainObject);
        }

        private bool AreStartDatesEqual(TestLeafObject otherLeafObject)
        {
            return otherLeafObject.CreatedDate.Year.Equals(CreatedDate.Year);
        }
    }


    public class TestRootObject
    {
        public IList<TestLeafObject> Leaves { get; set; }
        public DateTime CreatedDate { get; set; }
        public Guid Id { get; set; }
        public string[] Titles { get; set; }
        public TestLeafObject Friend { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (TestRootObject)) return false;
            return Equals((TestRootObject) obj);
        }

        public bool Equals(TestRootObject other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            if (!MatchLeafNodes(other)) return false;

            return AreStartDatesEqual(other)
                   && other.Id.Equals(Id)
                   && Equals(other.Titles.Length, Titles.Length)
                   && Equals(other.Friend, Friend);
        }

        private bool MatchLeafNodes(TestRootObject other)
        {
            return other.Leaves.Count == Leaves.Count;
        }

        private bool AreStartDatesEqual(TestRootObject otherRootObject)
        {
            return otherRootObject.CreatedDate.Year.Equals(CreatedDate.Year);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = (Leaves != null ? Leaves.GetHashCode() : 0);
                result = (result*397) ^ CreatedDate.GetHashCode();
                result = (result*397) ^ Id.GetHashCode();
                result = (result*397) ^ (Titles != null ? Titles.GetHashCode() : 0);
                result = (result*397) ^ (Friend != null ? Friend.GetHashCode() : 0);
                return result;
            }
        }
    }
}