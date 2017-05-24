// /**********************************************************************************************
// Author:		Vasily Kabanov
// Created		2013-05-30
// Comment		
// **********************************************************************************************/

using System.Collections.Generic;

namespace AuNoteLib.Util
{
    // http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
	public static class HashHelper
	{
	    public static int GetHashCode<T>(T arg)
            //where T: class
	    {
	        //if (null == arg)
	        if (EqualityComparer<T>.Default.Equals(default(T), arg))
	        {
	            return 29;
	        }
	        return arg.GetHashCode();
	    }

	    public static int GetHashCode<T1, T2>(T1 arg1, T2 arg2)
		{
            return 0.CombineHashCode(arg1).CombineHashCode(arg2);
		}

		public static int GetHashCode<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3)
		{
            return GetHashCode(arg1, arg2).CombineHashCode(arg3);
		}

		public static int GetHashCode<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
            return GetHashCode(arg1, arg2, arg3).CombineHashCode(arg4);
        }

		public static int GetHashCode<T>(params T[] list)
		{
			unchecked
			{
				var hash = 0;
				foreach (var item in list)
				{
					hash = hash.CombineHashCode(item);
				}
				return hash;
			}
		}

		public static int GetHashCode<T>(IEnumerable<T> list)
		{
			unchecked
			{
				var hash = 0;
				foreach (var item in list)
				{
                    hash = hash.CombineHashCode(item);
                }
				return hash;
			}
		}

		/// <summary>
		/// Gets a hashcode for a collection for that the order of items 
		/// does not matter.
		/// So {1, 2, 3} and {3, 2, 1} will get same hash code.
		/// </summary>
		public static int GetHashCodeForOrderNoMatterCollection<T>(IEnumerable<T> list)
		{
			unchecked
			{
				var hash = 0;
				var count = 0;
				foreach (var item in list)
				{
					hash += item.GetHashCode();
					count++;
				}
				return 31 * hash + count.GetHashCode();
			}
		}

		/// <summary>
		/// Alternative way to get a hashcode is to use a fluent 
		/// interface like this:<br />
		/// return 0.CombineHashCode(field1).CombineHashCode(field2).
		///     CombineHashCode(field3);
		/// </summary>
		public static int CombineHashCode<T>(this int hashCode, T arg)
		{
			unchecked
			{
				return 31 * hashCode + GetHashCode(arg);
			}
		}
	}
}
