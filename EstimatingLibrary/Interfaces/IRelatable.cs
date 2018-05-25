using System;
using System.Collections.Generic;
using System.Linq;

namespace EstimatingLibrary.Interfaces
{
    public interface IRelatable : ITECObject
    {
        SaveableMap PropertyObjects { get; }
        SaveableMap LinkedObjects { get; }
    }

    public static class RelatableExtensions
    {
        /// <summary>
        /// Returns all objects of type T which exist as a direct child
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="relatable"></param>
        /// <returns></returns>
        public static List<T> GetAll<T>(this IRelatable relatable)
        {
            List<T> list = new List<T>();
            if (relatable is T typedObj)
            {
                list.Add(typedObj);
            }
            foreach (var item in relatable.GetDirectChildren())
            {
                if (item is IRelatable rel)
                {
                    list.AddRange(GetAll<T>(rel));
                }
                else if (item is T x)
                {
                    list.Add(x);
                }
            }
            return list;
        }
        
        public static List<ITECObject> GetDirectChildren(this IRelatable relatable)
        {
            List<ITECObject> list = new List<ITECObject>();
            foreach (var item in relatable.PropertyObjects.ChildList().Where(x => !relatable.LinkedObjects.Contains(x.PropertyName)))
            {
                list.Add(item.Child);
            }
            return list;
        }

        public static bool IsDirectChildProperty(this IRelatable relatable, string propertyName)
        {
            return !relatable.LinkedObjects.Contains(propertyName) && relatable.PropertyObjects.Contains(propertyName);
        }

        public static bool IsDirectDescendant(this IRelatable relatable, ITECObject item)
        {
            List<ITECObject> directChildren = relatable.GetDirectChildren();
            if (directChildren.Contains(item))
            {
                return true;
            }

            foreach(ITECObject child in directChildren)
            {
                if (child is IRelatable relatableChild && relatableChild.IsDirectDescendant(item))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<ITECObject> GetObjectPath(this IRelatable parent, ITECObject descendant)
        {
            List<ITECObject> path = new List<ITECObject>();

            if (!parent.IsDirectDescendant(descendant))
            {
                return path;
            }

            path.Add(parent);

            foreach(ITECObject child in parent.GetDirectChildren())
            {
                if (child == descendant)
                {
                    path.Add(child);
                    return path;
                }
                else if (child is IRelatable childRelatable && childRelatable.IsDirectDescendant(descendant))
                {
                    path.AddRange(childRelatable.GetObjectPath(descendant));
                    return path;
                }
            }

            throw new Exception("Parent shown as having the direct descendant, but path to direct descendant not found.");
        }
    }

    public class SaveableMap
    {
        public List<ITECObject> Objects;
        public List<string> PropertyNames;
        Dictionary<string, List<ITECObject>> nameDictionary;

        public SaveableMap()
        {
            Objects = new List<ITECObject>();
            PropertyNames = new List<string>();
            nameDictionary = new Dictionary<string, List<ITECObject>>();
        }

        public bool Contains(ITECObject item)
        {
            return Objects.Contains(item);
        }
        public bool Contains(string name)
        {
            return PropertyNames.Contains(name);
        }
        public object Property(object parent, string propertyName)
        {
            return parent.GetType().GetProperty(propertyName).GetValue(parent);
        }
        public void Add(ITECObject item, string name)
        {
            Objects.Add(item);
            this.Add(name);
            addToDictionary(name, item);
        }
        public void Add(string name)
        {
            PropertyNames.Add(name);
        }
        public void AddRange(IEnumerable<ITECObject> items, string name)
        {
            Objects.AddRange(items);
            this.Add(name);

            foreach(TECObject item in items)
            {
                addToDictionary(name, item);
            }
        }
        public void AddRange(IEnumerable<string> names)
        {
            PropertyNames.AddRange(names);
        }
        public void AddRange(SaveableMap map)
        {
            foreach(KeyValuePair<string, List<ITECObject>> pair in map.nameDictionary)
            {
                if (nameDictionary.ContainsKey(pair.Key))
                {
                    nameDictionary[pair.Key].AddRange(pair.Value);
                    Objects.AddRange(pair.Value);
                }
                else
                {
                    nameDictionary[pair.Key] = pair.Value;
                    Objects.AddRange(pair.Value);
                    PropertyNames.Add(pair.Key);
                }
            }
            foreach(string name in map.PropertyNames)
            {
                if (!PropertyNames.Contains(name))
                {
                    PropertyNames.Add(name);
                }
            }
        }
        /// <summary>
        /// List of all child objects nd the name of their containing property
        /// </summary>
        /// <returns></returns>
        public List<(string PropertyName, ITECObject Child)> ChildList()
        {
            List<(string propertyName, ITECObject child)> outList = new List<(string propertyName, ITECObject child)>();
            foreach(KeyValuePair<string, List<ITECObject>> pair in nameDictionary)
            {
                foreach(TECObject item in pair.Value)
                {
                    outList.Add((pair.Key, item));
                }
            }
            return outList;
        }

        private void addToDictionary(string name, ITECObject item)
        {
            if (!nameDictionary.ContainsKey(name))
            {
                nameDictionary[name] = new List<ITECObject>() { item };
            }
            else
            {
                nameDictionary[name].Add(item);
            }
        }
        
    }
    
}
