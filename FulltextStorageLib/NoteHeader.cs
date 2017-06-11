// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2016-05-04
// Comment		
// **********************************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using FulltextStorageLib.Util;

namespace FulltextStorageLib
{
    public class NoteHeader : INoteHeader, IEquatable<INoteHeader>
    {
        private int _hashCode;

        public string Id { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public string Name { get; set; }

        public override string ToString()
        {
            return $"{Name}#{Id}";
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            if (_hashCode != 0)
                return _hashCode;

            _hashCode = HashHelper.GetHashCode(string.IsNullOrEmpty(Id) ? Name : Id);

            return _hashCode;
        }

        public bool Equals(INoteHeader other)
        {
            if (other == null)
                return false;

            return Id == other.Id;
        }
    }
}