﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TECUserControlLibrary.Utilities
{
    public class QuantityCollection<T> : ObservableCollection<QuantityItem<T>>
    {
        private Dictionary<T, QuantityItem<T>> itemDictionary;

        public QuantityCollection() : base()
        {
            itemDictionary = new Dictionary<T, QuantityItem<T>>();
        }
        public QuantityCollection(IEnumerable<T> items) : this()
        {
            foreach (T item in items)
            {
                Add(item);
            }
        }

        public event Action<T, int, int> QuantityChanged;

        public void Add(T item, int quantity = 1)
        {
            if (quantity < 0)
            {
                throw new ArgumentOutOfRangeException("Quantity cannot be less than 0");
            }
            else if (quantity == 0)
            {
                return;
            }

            if (itemDictionary.ContainsKey(item))
            {
                itemDictionary[item].Quantity += quantity;
            }
            else
            {
                QuantityItem<T> quantItem = new QuantityItem<T>(item, quantity);
                quantItem.QuantityChanged += quantityChanged;
                itemDictionary.Add(item, quantItem);
                base.Add(quantItem);
            }
        }

        private void quantityChanged(T arg1, int arg2, int arg3)
        {
            QuantityChanged?.Invoke(arg1, arg2, arg3);
        }

        public bool Remove(T item, int quantity = 1)
        {
            if (quantity < 0)
            {
                throw new ArgumentOutOfRangeException("Quantity cannot be less than 0");
            }
            else if (quantity == 0)
            {
                return true;
            }

            if (itemDictionary.ContainsKey(item))
            {
                QuantityItem<T> quantItem = itemDictionary[item];
                quantItem.Quantity -= quantity;
                if (quantItem.Quantity < 1)
                {
                    itemDictionary.Remove(item);
                    base.Remove(quantItem);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool RemoveAll(T item)
        {
            if (itemDictionary.ContainsKey(item))
            {
                QuantityItem<T> quantityItem = itemDictionary[item];
                itemDictionary.Remove(item);
                base.Remove(quantityItem);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ObservablyClear()
        {
            foreach(QuantityItem<T> item in new List<QuantityItem<T>>(this))
            {
                RemoveAll(item.Item);
            }
        }
    }

    public class QuantityItem<T> : INotifyPropertyChanged
    {
        private int _quantity;

        public T Item { get; }
        public int Quantity
        {
            get
            {
                return _quantity;
            }
            set
            {
                QuantityChanged?.Invoke(Item, _quantity, value);
                _quantity = value;
                raisePropertyChanged("Quantity");
            }
        }

        public QuantityItem(T item, int quantity = 1)
        {
            Item = item;
            _quantity = quantity;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event Action<T, int, int> QuantityChanged;

        private void raisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}