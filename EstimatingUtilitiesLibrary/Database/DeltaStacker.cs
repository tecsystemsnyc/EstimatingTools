using EstimatingLibrary;
using EstimatingLibrary.Interfaces;
using EstimatingLibrary.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EstimatingUtilitiesLibrary.Database
{
    public class DeltaStacker
    {
        private List<UpdateItem> stack;
        private DBType dbType;

        public DeltaStacker(ChangeWatcher changeWatcher, TECScopeManager manager)
        {
            dbType = DBType.Bid;
            if (manager is TECTemplates)
            {
                dbType = DBType.Templates;
            }
            changeWatcher.Changed += handleChange;
            stack = new List<UpdateItem>();
        }
        public List<UpdateItem> CleansedStack()
        {
            List<UpdateItem> outStack = new List<UpdateItem>();
            Dictionary<(string, string), UpdateItem> editData = new Dictionary<(string, string), UpdateItem>();
            foreach(var item in stack)
            {
                if(item.Change == Change.Add || item.Change == Change.Remove)
                {
                    outStack.Add(item);
                }
                else
                {
                    var key = (item.PrimaryKey.Item2, item.Table);
                    if (!editData.ContainsKey(key))
                    {
                        editData[key] = item;
                    } else
                    {
                        foreach(var pair in item.FieldData)
                        {
                            editData[key].FieldData[pair.Key] = pair.Value;
                        }
                    }
                }
            }
            foreach(var pair in editData)
            {
                outStack.Add(pair.Value);
            }
            return outStack;
        }
        
        public static List<UpdateItem> ChildStack(Change change, IRelatable item, DBType type)
        {
            List<UpdateItem> outStack = new List<UpdateItem>();
            foreach (var saveItem in item.PropertyObjects.ChildList())
            {
                outStack.AddRange(addRemoveStack(change, saveItem.PropertyName, item as ITECObject , saveItem.Child, type, true));
            }
            if (item is TECTypical system)
            {
                outStack.AddRange(typicalInstanceStack(change, system, type));
            }
            else if(item is TECTemplates templates)
            {
                outStack.AddRange(templatesReferencesStack(change, templates));
            }

            return outStack;
        }

        private static IEnumerable<UpdateItem> templatesReferencesStack(Change change, TECTemplates templates)
        {
            List<UpdateItem> outStack = new List<UpdateItem>();
            foreach (KeyValuePair<TECSubScope, List<TECSubScope>> pair in templates.SubScopeSynchronizer.GetFullDictionary())
            {
                foreach (TECSubScope item in pair.Value)
                {
                    outStack.AddRange(addRemoveStack(change, "TemplateRelationship", pair.Key, item, DBType.Templates));
                }
            }
            foreach (KeyValuePair<TECEquipment, List<TECEquipment>> pair in templates.EquipmentSynchronizer.GetFullDictionary())
            {
                foreach (TECEquipment item in pair.Value)
                {
                    outStack.AddRange(addRemoveStack(change, "TemplateRelationship", pair.Key, item, DBType.Templates));
                }
            }
            return outStack;
        }
        
        private static List<UpdateItem> addRemoveStack(Change change, string propertyName, ITECObject sender, ITECObject item, DBType type, bool fromParent = false)
        {
            List<UpdateItem> outStack = new List<UpdateItem>();
            List<TableBase> tables;
            if(sender is IRelatable parent && parent.IsDirectChildProperty(propertyName))
            {
                tables = DatabaseHelper.GetTables(new List<ITECObject>() { item }, propertyName, type);
                outStack.AddRange(tableObjectStack(change, tables, item, fromParent: fromParent));
                if (item is IRelatable saveable)
                {
                    outStack.AddRange(ChildStack(change, saveable, type));
                }
            }
            tables = DatabaseHelper.GetTables(new List<ITECObject>() { sender, item }, propertyName, type);
            outStack.AddRange(tableObjectStack(change, tables, sender, item, fromParent: fromParent));

            return outStack;
        }
        private static List<UpdateItem> editStack(ITECObject  sender, string propertyName, 
            object value, object oldValue, DBType type)
        {
            List<UpdateItem> outStack = new List<UpdateItem>();

            if(value is IList listValue && oldValue is IList)
            {
                if(listValue.Count == 0)
                {
                    return new List<UpdateItem>();
                }
                var tables = DatabaseHelper.GetTables(new List<ITECObject >() { sender, listValue[0] as ITECObject  }, propertyName, type);
                foreach(var table in tables.Where(x => x.IndexString != ""))
                {
                    List<(Dictionary<string, string> data, Tuple<string, string> keyData)> dataList = DatabaseHelper.PrepareIndexData(sender, propertyName, table, type);
                    foreach(var item in dataList)
                    {
                        outStack.Add(new UpdateItem(Change.Edit, table.NameString, item.data, item.keyData));
                    }
                }
                return outStack;
            }
            else if(!(value is ITECObject ) && !(oldValue is ITECObject ))
            {
                List<TableBase> tables = DatabaseHelper.GetTables(sender, type);
                foreach (TableBase table in tables)
                {
                    var fields = table.Fields;
                    var data = DatabaseHelper.PrepareDataForEditObject(fields, sender, propertyName, value);
                    var keyData = DatabaseHelper.PrimaryKeyData(table, sender);
                    if (data != null)
                    {
                        outStack.Add(new UpdateItem(Change.Edit, table.NameString, data, keyData));
                    }
                }
                return outStack;
            }
            else
            {
                if (oldValue != null && oldValue is ITECObject  oldObject)
                {
                    outStack.AddRange(addRemoveStack(Change.Remove, propertyName, sender, oldObject, type));
                }
                if (value != null && value is ITECObject  newObject)
                {
                    outStack.AddRange(addRemoveStack(Change.Add, propertyName, sender, newObject, type));
                }
                return outStack;
            }
            
        }
        
        private static List<UpdateItem> tableObjectStack(Change change, List<TableBase> tables, ITECObject  item, ITECObject child = null, bool fromParent = false)
        {
            List<UpdateItem> outStack = new List<UpdateItem>();
            foreach (TableBase table in tables)
            {
                if(table.QuantityString != "" && !fromParent)
                {
                    var qtyField = table.Fields.First(x => x.Name == table.QuantityString);
                    var qty = (int)DatabaseHelper.HelperObject(qtyField, item, child);
                    if (qty > 0)
                    {
                        change = Change.Add;
                    }
                }
                List<TableField> fields = new List<TableField>();
                if (change == Change.Remove)
                {
                    fields.AddRange(table.PrimaryKeys);
                }
                else
                {
                    fields.AddRange(table.Fields);
                }
                
                Dictionary<string, string> data = new Dictionary<string, string>();
                if(table.Types.Count > 1)
                {
                    data = DatabaseHelper.PrepareDataForRelationTable(fields, item, child);
                }
                else
                {
                    data = DatabaseHelper.PrepareDataForObjectTable(fields, item);
                }
                outStack.Add(new UpdateItem(change, table.NameString, data));
                
            }
            return outStack;
        }
        private static List<UpdateItem> typicalInstanceStack(Change change, TECTypical system, DBType type)
        {
            List<UpdateItem> outStack = new List<UpdateItem>();
            foreach (KeyValuePair<ITECObject , List<ITECObject >> pair in system.TypicalInstanceDictionary.GetFullDictionary())
            {
                foreach(ITECObject  item in pair.Value)
                {
                    outStack.AddRange(addRemoveStack(change, "TypicalInstanceDictionary", pair.Key, item, type));
                }
            }

            return outStack;
        }

        private void handleChange(TECChangedEventArgs e)
        {
            if (e.Change == Change.Add || e.Change == Change.Remove)
            {               
                stack.AddRange(addRemoveStack(e.Change, e.PropertyName, e.Sender, e.Value as ITECObject, dbType));
            }
            else if (e.Change == Change.Edit)
            {
                stack.AddRange(editStack(e.Sender as ITECObject , e.PropertyName, e.Value, e.OldValue, dbType));
            }
        }
    }
}
