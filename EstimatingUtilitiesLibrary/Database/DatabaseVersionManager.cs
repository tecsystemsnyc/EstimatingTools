using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EstimatingUtilitiesLibrary.Database
{
    internal class DatabaseVersionManager
    {
        public enum UpdateStatus { Updated = 1, NotUpdated, Incompatible }
        public static UpdateStatus CheckAndUpdate(string path, DataTable versionDefintion)
        {
            SQLiteDatabase db = new SQLiteDatabase(path);
            int vDiff = versionDifference(db);
            if (vDiff > 0)
            {
                updateDatabase(db, versionDefintion);
                db.Connection.Close();
                return UpdateStatus.Updated;
            }
            else if (vDiff < 0)
            {
                db.Connection.Close();
                return UpdateStatus.Incompatible;
            }
            else
            {
                return UpdateStatus.NotUpdated;
            }
        }

        #region Database Version Update Methods
        static private int versionDifference(SQLiteDatabase db)
        {
            int currentVersion = Properties.Settings.Default.Version;
            DataTable infoDT = db.GetDataFromTable(MetadataTable.TableName);

            if (infoDT.Rows.Count < 1)
            {
                throw new DataException("Could not load database data.");
            }
            else if (infoDT.Rows.Count == 1)
            {
                DataRow infoRow = infoDT.Rows[0];
                if (infoDT.Columns.Contains(MetadataTable.Version.Name))
                {
                    int version = infoRow[MetadataTable.Version.Name].ToString().ToInt();
                    return (currentVersion - version);
                }
                else
                { return 1; }
            }
            else
            {
                throw new DataException("Improperly formatted database data.");
            }
        }
        static private void updateDatabase(SQLiteDatabase db, DataTable versionDefinition)
        {
            Dictionary<string, string> tableMap = new Dictionary<string, string>();
            List<string> tableNames = DatabaseHelper.TableNames(db);
            List<TableBase> databaseTableList = new List<TableBase>();
            if (tableNames.Contains(BidInfoTable.TableName) || tableNames.Contains("TECBidInfo"))
            { databaseTableList = AllBidTables.Tables; }
            else if (tableNames.Contains(TemplatesInfoTable.TableName) || tableNames.Contains("TECTemplatesInfo"))
            { databaseTableList = AllTemplateTables.Tables; }
            else
            { throw new ArgumentException("updateDatabase() can't determine db type"); }
            foreach (TableBase table in databaseTableList)
            {
                tableMap[table.NameString] = DatabaseGenerator.CreateTempTableFromDefinition(table, db);
            }
            int updateVerison = Properties.Settings.Default.Version;
            DataTable infoDT = db.GetDataFromTable(MetadataTable.TableName);
            int originalVerion = infoDT.Rows[0][MetadataTable.Version.Name].ToString().ToInt();
            updateToVersion(versionDefinition, db, originalVerion, updateVerison, tableMap, 
                tableNames, databaseTableList);
            removeOldTables(tableNames, db);
            foreach (TableBase table in databaseTableList.Where(table => table.NameString != MetadataTable.TableName))
            {
                DatabaseGenerator.CreateTableFromDefinition(table, db);
            }
            migrateFromTempTables(tableMap, db);
            UpdateVersionNumber(db);
        }

        private static void updateToVersion(DataTable dataTable, SQLiteDatabase db, int originalVersion,
            int updateVersion, Dictionary<string, string> tempMap, List<string> currentTables, List<TableBase> AllTables)
        {
            TableMapList mapList = buildMap(dataTable, originalVersion, updateVersion, tempMap, currentTables);
            foreach(TableMap map in mapList)
            {
                if (isSingleRow(map))
                {
                    DataTable dt = combinedTable(map, db);
                    string insertColumns = DatabaseHelper.FieldsString(map.UpdateFields);
                    foreach (DataRow row in dt.Rows)
                    {
                        List<string> values = new List<string>();
                        foreach (var field in map.OriginalFields)
                        {
                            values.Add(row[field].ToString());
                        }
                        string insertValues = DatabaseHelper.ValuesString(values);
                        string command = String.Format("insert into {0} ({1}) values ({2})", map.UpdateTableName, insertColumns, insertValues);
                        db.NonQueryCommand(command);
                    }

                }
                else
                {
                    foreach (string table in map.OriginalTableNames)
                    {
                        migrateData(table, DatabaseHelper.FieldsString(map.TableFieldsDictionary[table]),
                        map.UpdateTableName, DatabaseHelper.FieldsString(map.UpdateFields), db);
                    }
                }

                foreach (TableBase table in AllTables)
                {
                    if(tempMap[table.NameString] == map.UpdateTableName)
                    {
                        foreach(TableField field in table.Fields)
                        {
                            if (!map.UpdateFields.Contains(field.Name))
                            {
                                string command = String.Format("update {0} set {1} = \'{2}\' ",
                                    tempMap[table.NameString], field.Name, field.DefaultValue);
                                db.NonQueryCommand(command);
                            }
                        }
                    }
                }
            }
        }
        static private void migrateFromTempTables(Dictionary<string, string> tableMap, SQLiteDatabase db)
        {
            string commandString;
            foreach (KeyValuePair<string, string> tablePair in tableMap)
            {
                string tableName = tablePair.Key;
                string tempName = tablePair.Value ;
                commandString = "insert into '" + tableName + "' select * from '" + tempName + "'";
                db.NonQueryCommand(commandString);
                commandString = "drop table '" + tempName + "'";
                db.NonQueryCommand(commandString);
            }
        }
        static private void removeOldTables(List<string> tableNames, SQLiteDatabase db)
        {
            foreach(string table in tableNames.Where(table => table != MetadataTable.TableName))
            {
                string commandString = "drop table '" + table + "'";
                db.NonQueryCommand(commandString);
            }
        }
        public static void UpdateVersionNumber(SQLiteDatabase db)
        {
            string commandString = "update " + MetadataTable.TableName + " set " + MetadataTable.Version.Name + " = '" + Properties.Settings.Default.Version + "' ";
            db.NonQueryCommand(commandString);
        }
        private static void migrateData(string originalTable, string originalFields,
            string updateTable, string updateFields, SQLiteDatabase db)
        {
            string commandString = String.Format("insert into {0} ({1}) select {2} from '{3}'", 
                updateTable, updateFields, originalFields, originalTable);
            db.NonQueryCommand(commandString);
        }
        private static string fieldString(int version)
        {
            return "Field " + version;
        }
        private static string tableString(int version)
        {
            return "Table " + version;
        }

        private static DataTable combinedTable(TableMap map, SQLiteDatabase db)
        {
            string command = String.Format("select {0} from {1}", selectString(map), joinString(map.OriginalTableNames));
            DataTable outTable = db.GetDataFromCommand(command);
            return outTable;
        }
        private static bool isSingleRow(TableMap map)
        {
            bool outBool = true;
            foreach (KeyValuePair<string, List<string>> pair in map.TableFieldsDictionary)
            {
                if (pair.Value.Count == map.UpdateFields.Count)
                {
                    outBool = false;
                }
                else
                {
                    outBool = true;
                }
            }
            return outBool;
        }

        private static string joinString(List<string> tables)
        {
            string outString = "";
            int x = 0;
            foreach (var table in tables)
            {
                if(x < tables.Count -1)
                {
                    outString += table + " inner join ";
                    x++;
                } else
                {
                    outString += table;
                }
                
            }
            return outString;
            
        }
        private static string selectString(TableMap map)
        {
            string outString = "";
            int x = 0;
            foreach(var pair in map.TableFieldsDictionary)
            {
                int y = 0;
                foreach(var field in pair.Value)
                {
                    outString += String.Format("{0} as '{0}'", field);

                    if (x < map.TableFieldsDictionary.Count - 1 || y < pair.Value.Count - 1)
                    {
                        outString += ", ";
                    }
                    else
                    {
                        outString += " ";
                    }
                    y++;
                }
                x++;
            }
            return outString;
        }

        private static TableMapList buildMap(DataTable versionData, int originalVersion,
            int updateVersion, Dictionary<string, string> tempMap, List<string> currentTables)
        {
            string originalTableColumn = tableString(originalVersion);
            string originalFieldColumn = fieldString(originalVersion);
            string updateTableColumn = tableString(updateVersion);
            string updateFieldColumn = fieldString(updateVersion);

            TableMapList mapList = new TableMapList();
            foreach(DataRow row in versionData.Rows)
            {
                if (tempMap.ContainsKey(row[updateTableColumn].ToString()) && currentTables.Contains(row[originalTableColumn]))
                {
                    string originalTable = row[originalTableColumn].ToString();
                    string originalField = originalTable + "." + row[originalFieldColumn].ToString();
                    string updateTempTable = tempMap[row[updateTableColumn].ToString()];
                    string updateField = row[updateFieldColumn].ToString();

                    if (row[originalFieldColumn].ToString() != "NONE" || row[updateFieldColumn].ToString() != "NONE")
                    {
                        if (mapList.ContainsTable(updateTempTable))
                        {
                            TableMap map = mapList.GetMap(updateTempTable);
                            map.Add(originalTable, originalField, updateField);

                        }
                        else
                        {
                            TableMap map = new TableMap(updateTempTable);
                            map.Add(originalTable, originalField, updateField);
                            mapList.Add(map);
                        }
                    }
                }
            }
            return mapList;
        }
        
        private class TableMapList : IEnumerable
        {
            List<TableMap> mapList;
            Dictionary<string, TableMap> dictionary;
            public TableMapList()
            {
                mapList = new List<TableMap>();
                dictionary = new Dictionary<string, TableMap>();
            }
            public TableMap this[int index]
            {
                get { return mapList[index]; }
            }

            public IEnumerator GetEnumerator()
            {
                return mapList.GetEnumerator();
            }
            
            public void Add(TableMap item)
            {
                mapList.Add(item);
                dictionary[item.UpdateTableName] = item;
            }

            public bool ContainsTable(string tableName)
            {
                return dictionary.ContainsKey(tableName);
            }

            public TableMap GetMap(string tableName)
            {
                return dictionary[tableName];
            }
        }
        private class TableMap
        {
            public string UpdateTableName;
            public List<string> OriginalTableNames;
            public List<string> UpdateFields;
            public List<string> OriginalFields;
            public Dictionary<string, List<string>> TableFieldsDictionary;

            public TableMap(string updateTable)
            {
                UpdateTableName = updateTable;
                OriginalFields = new List<string>();
                OriginalTableNames = new List<string>();
                UpdateFields = new List<string>();
                TableFieldsDictionary = new Dictionary<string, List<string>>();
            }

            public void Add(string originalTable, string originalField, string updateField)
            {
                if (OriginalTableNames.Contains(originalTable))
                {
                    if (!UpdateFields.Contains(updateField))
                    {
                        UpdateFields.Add(updateField);
                    }
                    if (!OriginalFields.Contains(originalField))
                    {
                        OriginalFields.Add(originalField);
                    }
                    if (!TableFieldsDictionary[originalTable].Contains(originalField))
                    {
                        TableFieldsDictionary[originalTable].Add(originalField);
                    }
                }
                else
                {
                    if (!UpdateFields.Contains(updateField))
                    {
                        UpdateFields.Add(updateField);
                    }
                    if (!OriginalFields.Contains(originalField))
                    {
                        OriginalFields.Add(originalField);
                    }
                    TableFieldsDictionary[originalTable] = new List<string>() { originalField };
                    OriginalTableNames.Add(originalTable);
                }
            }
        }
        #endregion
    }
}
