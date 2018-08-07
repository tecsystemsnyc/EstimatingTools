using EstimatingLibrary.Interfaces;
using System;
using System.Collections.Generic;

namespace EstimatingLibrary.Utilities
{
    public class CostBatch
    {
        private Dictionary<CostType, CostObject> typeDictionary;

        public CostBatch()
        {
            typeDictionary = new Dictionary<CostType, CostObject>();
        }
        public CostBatch(double cost, double labor, CostType type) : this()
        {
            typeDictionary.Add(type, new CostObject(cost, labor));
        }
        public CostBatch(ICost cost) : this()
        {
            typeDictionary.Add(cost.Type, new CostObject(cost));
        }
        public CostBatch(List<ICost> costs) : this()
        {
            foreach(ICost cost in costs)
            {
                AddCost(cost);
            }
        }
        public CostBatch(CostBatch cb)
        {
            this.typeDictionary = new Dictionary<CostType, CostObject>();
            foreach(KeyValuePair<CostType, CostObject> cost in cb.typeDictionary)
            {
                this.typeDictionary.Add(cost.Key, cost.Value);
            }
        }

        public static CostBatch operator +(CostBatch left, CostBatch right)
        {
            CostBatch newCostBatch = new CostBatch(left);
            foreach(KeyValuePair<CostType, CostObject> type in right.typeDictionary)
            {
                if (newCostBatch.typeDictionary.ContainsKey(type.Key))
                {
                    newCostBatch.typeDictionary[type.Key] += right.typeDictionary[type.Key];
                }
                else
                {
                    newCostBatch.typeDictionary.Add(type.Key, type.Value);
                }
            }
            return newCostBatch;
        }
        public static CostBatch operator -(CostBatch costBatch)
        {
            CostBatch negativeCostBatch = new CostBatch();
            foreach(KeyValuePair<CostType, CostObject> type in costBatch.typeDictionary)
            {
                negativeCostBatch.typeDictionary[type.Key] = (-type.Value);
            }
            return negativeCostBatch;
        }
        public static CostBatch operator -(CostBatch left, CostBatch right)
        {
            CostBatch newCostBatch = new CostBatch(left);
            foreach (KeyValuePair<CostType, CostObject> type in right.typeDictionary)
            {
                if (newCostBatch.typeDictionary.ContainsKey(type.Key))
                {
                    newCostBatch.typeDictionary[type.Key] -= right.typeDictionary[type.Key];
                }
                else
                {
                    newCostBatch.typeDictionary.Add(type.Key, -type.Value);
                }
            }
            return newCostBatch;
        }
        public static CostBatch operator *(CostBatch left, double right)
        {
            CostBatch newCostBatch = new CostBatch();
            foreach(KeyValuePair<CostType, CostObject> type in left.typeDictionary)
            {
                newCostBatch.typeDictionary[type.Key] = (type.Value * right);
            }
            return newCostBatch;
        }

        public double GetCost(CostType type)
        {
            if (typeDictionary.ContainsKey(type))
            {
                CostObject obj = typeDictionary[type];
                return obj.Cost;
            }
            else
            {
                return 0;
            }
        }
        public double GetLabor(CostType type)
        {
            if (typeDictionary.ContainsKey(type))
            {
                CostObject obj = typeDictionary[type];
                return obj.Labor;
            }
            else
            {
                return 0;
            }
        }

        public void AddCost(ICost cost)
        {
            if (typeDictionary.ContainsKey(cost.Type))
            {
                typeDictionary[cost.Type] += new CostObject(cost);
            }
            else
            {
                typeDictionary.Add(cost.Type, new CostObject(cost));
            }
        }
        public void RemoveCost(ICost cost)
        {
            if (typeDictionary.ContainsKey(cost.Type))
            {
                typeDictionary[cost.Type] -= new CostObject(cost);
            }
            else
            {
                typeDictionary.Add(cost.Type, (new CostObject(cost) * -1));
            }
        }

        public void Add(CostType type, double cost, double labor)
        {
            if (typeDictionary.ContainsKey(type))
            {
                typeDictionary[type] += new CostObject(cost, labor);
            }
            else
            {
                typeDictionary.Add(type, new CostObject(cost, labor));
            }
        }
        public void Remove(CostType type, double cost, double labor)
        {
            if (typeDictionary.ContainsKey(type))
            {
                typeDictionary[type] -= new CostObject(cost, labor);
            }
            else
            {
                typeDictionary.Add(type, -(new CostObject(cost, labor)));
            }
        }

        public bool CostsEqual(CostBatch other)
        {
            foreach(var pair in typeDictionary)
            {
                if (!other.typeDictionary.ContainsKey(pair.Key))
                {
                    return false;
                }
                else if (pair.Value != other.typeDictionary[pair.Key]){
                    return false;
                }
            }
            return true;
        }

        private struct CostObject
        {
            public double Cost, Labor;

            public CostObject(double cost, double labor)
            {
                Cost = cost;
                Labor = labor;
            }

            public CostObject(ICost cost)
            {
                Cost = cost.Cost;
                Labor = cost.Labor;
            }

            public static CostObject operator +(CostObject left, CostObject right)
            {
                return new CostObject((left.Cost + right.Cost), (left.Labor + right.Labor));
            }

            public static CostObject operator -(CostObject costObject)
            {
                return new CostObject(-costObject.Cost, -costObject.Labor);
            }
            public static CostObject operator -(CostObject left, CostObject right)
            {
                return new CostObject((left.Cost - right.Cost), (left.Labor - right.Labor));
            }

            public static CostObject operator *(CostObject left, double right)
            {
                return new CostObject(left.Cost * right, left.Labor * right);
            }

            public static bool operator ==(CostObject left, CostObject right)
            {
                return left.Cost == right.Cost && left.Labor == right.Labor;
            }
            public static bool operator !=(CostObject left, CostObject right)
            {
                return !(left == right);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                if(obj is CostObject costObj)
                {
                    return this == costObj;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    
}
