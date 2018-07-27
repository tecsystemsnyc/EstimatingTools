using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingLibrary.Utilities
{
    public static class CommonUtilities
    {
        public static void ObservablyClear<T>(this ObservableCollection<T> collection)
        {
            List<T> toRemove = new List<T>();
            toRemove.AddRange(collection);
            foreach(T item in toRemove)
            {
                collection.Remove(item);
            }
        }

        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> range)
        {
            foreach(T item in range)
            {
                collection.Add(item);
            }
        }

        public static bool Matches<T>(this IEnumerable<T> first, IEnumerable<T> second)
        {
            return (first.Except(second).Count() == 0)
                && (second.Except(first).Count() == 0);
        }

        public static void FillScopeCollection<T>(IList<T> collectionToModify, IList<T> otherCollection) where T : ITECObject
        {
            foreach (T otherItem in otherCollection)
            {
                if (!collectionToModify.Any(item => item.Guid == otherItem.Guid))
                {
                    collectionToModify.Add(otherItem);
                }
            }
        }

        public static void UnionizeScopeCollection<T>(IList<T> collectionToModify, IList<T> otherCollection) where T : ITECObject
        {
            List<T> itemsToRemove = new List<T>();

            foreach (T otherItem in otherCollection)
            {
                foreach (T item in collectionToModify)
                {
                    if (item.Guid == otherItem.Guid)
                    {
                        itemsToRemove.Add(item);
                    }
                }
            }
            foreach (T item in itemsToRemove)
            {
                collectionToModify.Remove(item);
            }
            foreach (T item in otherCollection)
            {
                collectionToModify.Add(item);
            }
        }

    }
}
