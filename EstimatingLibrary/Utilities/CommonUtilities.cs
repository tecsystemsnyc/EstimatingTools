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

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="otherCollection"></param>
        /// <param name="onReplace">Method to be called upon identify an item to replace. The arguments should be (existing item, new item)</param>
        public static void UnionizeScopeCollection<T>(IList<T> collectionToModify, IList<T> otherCollection, Action<T,T> onReplace = null) where T : ITECObject
        {
            List<T> itemsToRemove = new List<T>();

            foreach (T otherItem in otherCollection)
            {
                foreach (T item in collectionToModify)
                {
                    if (item.Guid == otherItem.Guid)
                    {
                        onReplace?.Invoke(item, otherItem);
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

        public static bool OptionallyReplaceAll<T>(T item, IList<T> collection, T replacement = null) where T : class
        {
            if (!collection.Contains(item)) return false;
            while(collection.Contains(item))
            {
                collection.Remove(item);
                if (replacement != null)
                {
                    collection.Add(replacement);
                }
            }
            return true;
        }

    }
}
