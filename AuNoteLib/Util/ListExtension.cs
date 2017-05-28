using System.Collections.Generic;

namespace FulltextStorageLib.Util
{
   public static class ListExtension
    {
       public static void AddIfNotNull<T>(this IList<T> list, T item) where T : class
       {
           if (item != null)
                list.Add(item);
       }
    }
}
