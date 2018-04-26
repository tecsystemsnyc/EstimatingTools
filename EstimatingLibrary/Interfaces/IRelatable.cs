﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace EstimatingLibrary.Interfaces
{
    public interface IRelatable
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

        public static List<TECObject> GetDirectChildren(this IRelatable relatable)
        {
            List<TECObject> list = new List<TECObject>();
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
        
    }

    public class SaveableMap
    {
        public List<TECObject> Objects;
        public List<string> PropertyNames;
        Dictionary<string, List<TECObject>> nameDictionary;

        public SaveableMap()
        {
            Objects = new List<TECObject>();
            PropertyNames = new List<string>();
            nameDictionary = new Dictionary<string, List<TECObject>>();
        }

        public bool Contains(TECObject item)
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
        public void Add(TECObject item, string name)
        {
            Objects.Add(item);
            this.Add(name);
            addToDictionary(name, item);
        }
        public void Add(string name)
        {
            PropertyNames.Add(name);
        }
        public void AddRange(IEnumerable<TECObject> items, string name)
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
            foreach(KeyValuePair<string, List<TECObject>> pair in map.nameDictionary)
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
        public List<(string PropertyName, TECObject Child)> ChildList()
        {
            List<(string propertyName, TECObject child)> outList = new List<(string propertyName, TECObject child)>();
            foreach(KeyValuePair<string, List<TECObject>> pair in nameDictionary)
            {
                foreach(TECObject item in pair.Value)
                {
                    outList.Add((pair.Key, item));
                }
            }
            return outList;
        }

        private void addToDictionary(string name, TECObject item)
        {
            if (!nameDictionary.ContainsKey(name))
            {
                nameDictionary[name] = new List<TECObject>() { item };
            }
            else
            {
                nameDictionary[name].Add(item);
            }
        }
        
    }
    
}
