using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AuNoteLib.Util
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
