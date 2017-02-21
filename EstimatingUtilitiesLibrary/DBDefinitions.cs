﻿using EstimatingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EstimatingUtilitiesLibrary
{
    #region Table Definitions
    public class BidInfoTable : TableBase
    {
        public static new string TableName = "TECBidInfo";
        public static Type ObjectType = typeof(TECBid);

        public static TableField DBVersion = new TableField("DBVersion", "TEXT", null);
        public static TableField BidName = new TableField("BidName", "TEXT", ObjectType.GetProperty("Name"));
        public static TableField BidInfoID = new TableField("BidInfoID", "TEXT", ObjectType.GetProperty("InfoGuid"));
        public static TableField BidNumber = new TableField("BidNumber", "TEXT", ObjectType.GetProperty("BidNumber"));
        public static TableField DueDate = new TableField("DueDate", "TEXT", ObjectType.GetProperty("DueDateString"));
        public static TableField Salesperson = new TableField("Salesperson", "TEXT", ObjectType.GetProperty("Salesperson"));
        public static TableField Estimator = new TableField("Estimator", "TEXT", ObjectType.GetProperty("Estimator"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            BidInfoID
        };

        public static new List<Type> Types = new List<Type>() {
            ObjectType
        };

    }
    public class TemplatesInfoTable : TableBase
    {
        public static new string TableName = "TECTemplatesInfo";
        public static Type ObjectType = typeof(TECTemplates);

        public static TableField TemplatesInfoID = new TableField("TemplatesInfoID", "TEXT", ObjectType.GetProperty("InfoGuid"));
        public static TableField DBVersion = new TableField("DBVersion", "TEXT", null);

        public static new List<TableField> PrimaryKey = new List<TableField>()
        {
            TemplatesInfoID
        };

        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };
    }
    public class LaborConstantsTable : TableBase
    {
        public static new string TableName = "TECLaborConst";
        public static Type ObjectType = typeof(TECLabor);
        public static  Type ReferenceType = typeof(TECBid);

        public static TableField BidID = new TableField("BidID", "TEXT", ReferenceType.GetProperty("InfoGuid"));

        public static TableField PMCoef = new TableField("PMCoef", "REAL", ObjectType.GetProperty("PMCoef"));
        public static TableField PMRate = new TableField("PMRate", "REAL", ObjectType.GetProperty("PMRate"));

        public static TableField ENGCoef = new TableField("ENGCoef", "REAL", ObjectType.GetProperty("ENGCoef"));
        public static TableField ENGRate = new TableField("ENGRate", "REAL", ObjectType.GetProperty("ENGRate"));

        public static TableField CommCoef = new TableField("CommCoef", "REAL", ObjectType.GetProperty("CommCoef"));
        public static TableField CommRate = new TableField("CommRate", "REAL", ObjectType.GetProperty("CommRate"));

        public static TableField SoftCoef = new TableField("SoftCoef", "REAL", ObjectType.GetProperty("SoftCoef"));
        public static TableField SoftRate = new TableField("SoftRate", "REAL", ObjectType.GetProperty("SoftRate"));

        public static TableField GraphCoef = new TableField("GraphCoef", "REAL", ObjectType.GetProperty("GraphCoef"));
        public static TableField GraphRate = new TableField("GraphRate", "REAL", ObjectType.GetProperty("GraphRate"));

        public static new List<TableField> PrimaryKey = new List<TableField>()
        {
            BidID
        };

        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };
    }
    public class SubcontractorConstantsTable : TableBase
    {
        public static new string TableName = "TECSubcontractorConst";
        public static Type ObjectType = typeof(TECLabor);
        public static  Type ReferenceType = typeof(TECBid);

        public static TableField BidID = new TableField("BidID", "TEXT", ReferenceType.GetProperty("InfoGuid"));

        public static TableField ElectricalRate = new TableField("ElectricalRate", "REAL", ObjectType.GetProperty("ElectricalRate"));
        public static TableField ElectricalSuperRate = new TableField("ElectricalSuperRate", "REAL", ObjectType.GetProperty("ElectricalSuperRate"));

        public static new List<TableField> PrimaryKey = new List<TableField>()
        {
            BidID
        };

        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };
    }
    public class UserAdjustmentsTable : TableBase
    {
        public static new string TableName = "TECUserAdjustments";
        public static Type ObjectType = typeof(TECBid);
        public static Type ReferenceType = typeof(TECLabor);

        public static TableField BidID = new TableField("BidID", "TEXT", ReferenceType.GetProperty("InfoGuid"));

        public static TableField PMExtraHours = new TableField("PMExtraHours", "REAL", ObjectType.GetProperty("PMExtraHours"));
        public static TableField ENGExtraHours = new TableField("ENGExtraHours", "REAL", ObjectType.GetProperty("ENGExtraHours"));
        public static TableField CommExtraHours = new TableField("CommExtraHours", "REAL", ObjectType.GetProperty("CommExtraHours"));
        public static TableField SoftExtraHours = new TableField("SoftExtraHours", "REAL", ObjectType.GetProperty("SoftExtraHours"));
        public static TableField GraphExtraHours = new TableField("GraphExtraHours", "REAL", ObjectType.GetProperty("GraphExtraHours"));

        public static new List<TableField> PrimaryKey = new List<TableField>()
        {
            BidID
        };

        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };

    }
    public class CostAdditionsTable : TableBase
    {
        public static new string TableName = "TECCostAdditions";
        public static Type ObjectType = null;

        public static TableField Name = new TableField("Name", "TEXT", null);
        public static TableField Cost = new TableField("Cost", "REAL", null);

        public static new List<TableField> PrimaryKey = new List<TableField>()
        {
            Name
        };

        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };
    }
    public class NoteTable : TableBase
    {
        public static new string TableName = "TECNote";
        public static Type ObjectType = typeof(TECNote);


        public static TableField NoteID = new TableField("NoteID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField NoteText = new TableField("NoteText", "TEXT", ObjectType.GetProperty("Text"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            NoteID
            };

        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };
    }
    public class ExclusionTable : TableBase
    {
        public static new string TableName = "TECExclusion";
        public static Type ObjectType = typeof(TECExclusion);

        public static TableField ExclusionID = new TableField("ExclusionID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField ExclusionText = new TableField("ExclusionText", "TEXT", ObjectType.GetProperty("Text"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            ExclusionID
            };

        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };

    }
    public class BidScopeBranchTable : TableBase
    {
        public static new string TableName = "TECBidTECScopeBranch";
        public static Type ObjectType = typeof(TECScopeBranch);
        public static Type ReferenceType = typeof(TECBid);

        public static TableField BidID = new TableField("BidID", "TEXT", ReferenceType.GetProperty("InfoGuid"));
        public static TableField ScopeBranchID = new TableField("ScopeBranchID", "TEXT", ObjectType.GetProperty("Guid"));

        public static new List<TableField> PrimaryKey = new List<TableField>()
        {
            BidID,
            ScopeBranchID
        };

        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };
    }
    public class ScopeBranchTable : TableBase
    {
        public static new string TableName = "TECScopeBranch";
        public static Type ObjectType = typeof(TECScopeBranch);

        public static TableField ScopeBranchID = new TableField("ScopeBranchID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField Name = new TableField("Name", "TEXT", ObjectType.GetProperty("Name"));
        public static TableField Description = new TableField("Description", "TEXT", ObjectType.GetProperty("Description"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            ScopeBranchID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };

    }
    public class SystemTable : TableBase
    {
        public static new string TableName = "TECSystem";
        public static Type ObjectType = typeof(TECSystem);

        public static TableField SystemID = new TableField("SystemID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField Name = new TableField("Name", "TEXT", ObjectType.GetProperty("Name"));
        public static TableField Description = new TableField("Description", "TEXT", ObjectType.GetProperty("Description"));
        public static TableField Quantity = new TableField("Quantity", "INTEGER", ObjectType.GetProperty("Quantity"));
        public static TableField BudgetPrice = new TableField("BudgetPrice", "REAL", ObjectType.GetProperty("BudgetPrice"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            SystemID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };

    }
    public class EquipmentTable : TableBase
    {
        public static new string TableName = "TECEquipment";
        public static Type ObjectType = typeof(TECEquipment);

        public static TableField EquipmentID = new TableField("EquipmentID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField Name = new TableField("Name", "TEXT", ObjectType.GetProperty("Name"));
        public static TableField Description = new TableField("Description", "TEXT", ObjectType.GetProperty("Description"));
        public static TableField Quantity = new TableField("Quantity", "INTEGER", ObjectType.GetProperty("Quantity"));
        public static TableField BudgetPrice = new TableField("BudgetPrice", "REAL", ObjectType.GetProperty("BudgetPrice"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            EquipmentID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };

    }
    public class SubScopeTable : TableBase
    {
        public static new string TableName = "TECSubScope";
        public static Type ObjectType = typeof(TECSubScope);

        public static TableField SubScopeID = new TableField("SubScopeID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField Name = new TableField("Name", "TEXT", ObjectType.GetProperty("Name"));
        public static TableField Description = new TableField("Description", "TEXT", ObjectType.GetProperty("Description"));
        public static TableField Quantity = new TableField("Quantity", "INTEGER", ObjectType.GetProperty("Quantity"));
        public static TableField Length = new TableField("Length", "REAL", ObjectType.GetProperty("Length"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            SubScopeID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };

    }
    public class DeviceTable : TableBase
    {
        public static new string TableName = "TECDevice";
        public static Type ObjectType = typeof(TECDevice);

        public static TableField DeviceID = new TableField("DeviceID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField Name = new TableField("Name", "TEXT", ObjectType.GetProperty("Name"));
        public static TableField Description = new TableField("Description", "TEXT", ObjectType.GetProperty("Description"));
        public static TableField Cost = new TableField("Cost", "REAL", ObjectType.GetProperty("Cost"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            DeviceID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };

    }
    public class PointTable : TableBase
    {
        public static new string TableName = "TECPoint";
        public static Type ObjectType = typeof(TECPoint);

        public static TableField PointID = new TableField("PointID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField Name = new TableField("Name", "TEXT", ObjectType.GetProperty("Name"));
        public static TableField Description = new TableField("Description", "TEXT", ObjectType.GetProperty("Description"));
        public static TableField Quantity = new TableField("Quantity", "INTEGER", ObjectType.GetProperty("Quantity"));
        public static TableField Type = new TableField("Type", "TEXT", ObjectType.GetProperty("Type"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            PointID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };

    }
    public class TagTable : TableBase
    {
        public static new string TableName = "TECTag";
        public static Type ObjectType = typeof(TECTag);

        public static TableField TagID = new TableField("TagID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField TagString = new TableField("TagString", "TEXT", ObjectType.GetProperty("Text"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            TagID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };

    }
    public class ManufacturerTable : TableBase
    {
        public static new string TableName = "TECManufacturer";
        public static Type ObjectType = typeof(TECManufacturer);

        public static TableField ManufacturerID = new TableField("ManufacturerID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField Name = new TableField("Name", "TEXT", ObjectType.GetProperty("Name"));
        public static TableField Multiplier = new TableField("Multiplier", "REAL", ObjectType.GetProperty("Multiplier"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            ManufacturerID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };

    }
    public class DrawingTable : TableBase
    {
        public static new string TableName = "TECDrawing";
        public static Type ObjectType = typeof(TECDrawing);

        public static TableField DrawingID = new TableField("DrawingID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField Name = new TableField("Name", "TEXT", ObjectType.GetProperty("Name"));
        public static TableField Description = new TableField("Description", "TEXT", ObjectType.GetProperty("Description"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            DrawingID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };

    }
    public class PageTable : TableBase
    {
        public static new string TableName = "TECPage";
        public static Type ObjectType = typeof(TECPage);

        public static TableField PageID = new TableField("PageID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField Image = new TableField("Image", "BLOB", null);
        public static TableField PageNum = new TableField("PageNum", "INTEGER", ObjectType.GetProperty("PageNum"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            PageID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };

    }
    public class LocationTable : TableBase
    {
        public static new string TableName = "TECLocation";
        public static Type ObjectType = typeof(TECLocation);

        public static TableField LocationID = new TableField("LocationID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField Name = new TableField("Name", "TEXT", ObjectType.GetProperty("Name"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            LocationID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };

    }
    public class VisualScopeTable : TableBase
    {
        public static new string TableName = "TECVisualScope";
        public static Type ObjectType = typeof(TECVisualScope);

        public static TableField VisualScopeID = new TableField("VisualScopeID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField XPos = new TableField("XPos", "REAL", ObjectType.GetProperty("XPos"));
        public static TableField YPos = new TableField("YPos", "REAL", ObjectType.GetProperty("YPos"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            VisualScopeID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };

    }
    public class ProposalScopeTable : TableBase
    {
        public static new string TableName = "TECProposalScope";
        public static Type ObjectType = typeof(TECProposalScope);

        public static TableField ProposalScopeID = new TableField("ProposalScopeID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField IsProposed = new TableField("IsProposed", "INTEGER", ObjectType.GetProperty("IsProposed"));

        public static new List<TableField> PrimaryKey = new List<TableField>()
        {
            ProposalScopeID
        };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };
    }
    public class ConnectionTypeTable : TableBase
    {
        public static new string TableName = "TECConnectionType";
        public static Type ObjectType = typeof(TECConnectionType);
        
        public static TableField ConnectionTypeID = new TableField("ConnectionTypeID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField Name = new TableField("Name", "TEXT", ObjectType.GetProperty("Name"));
        public static TableField Cost = new TableField("Cost", "REAL", ObjectType.GetProperty("Cost"));
        public static TableField Labor = new TableField("Labor", "REAL", ObjectType.GetProperty("Labor"));

        public static new List<TableField> PrimaryKey = new List<TableField>()
        {
            ConnectionTypeID
        };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };
    }
    public class ConduitTypeTable : TableBase
    {
        public static new string TableName = "TECConduitType";
        public static Type ObjectType = typeof(TECConduitType);

        public static TableField ConduitTypeID = new TableField("ConduitTypeID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField Name = new TableField("Name", "TEXT", ObjectType.GetProperty("Name"));
        public static TableField Cost = new TableField("Cost", "REAL", ObjectType.GetProperty("Cost"));
        public static TableField Labor = new TableField("Labor", "REAL", ObjectType.GetProperty("Labor"));

        public static new List<TableField> PrimaryKey = new List<TableField>()
        {
            ConduitTypeID
        };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };
    }
    public class AssociatedCostTable : TableBase
    {
        public static new string TableName = "TECAssociatedCost";
        public static Type ObjectType = typeof(TECAssociatedCost);

        public static TableField AssociatedCostID = new TableField("AssociatedCostID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField Name = new TableField("Name", "TEXT", ObjectType.GetProperty("Name"));
        public static TableField Cost = new TableField("Cost", "REAL", ObjectType.GetProperty("Cost"));

        public static new List<TableField> PrimaryKey = new List<TableField>()
        {
            AssociatedCostID
        };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };
    }
    public class ProposalScopeScopeBranchTable : RelationTableBase
    {
        public static new string TableName = "TECProposalScopeTECScopeBranch";
        public static Type ObjectType = typeof(TECProposalScope);
        public static  Type ReferenceType = typeof(TECScopeBranch);

        public static TableField ProposalScopeID = new TableField("ProposalScopeID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField ScopeBranchID = new TableField("ScopeBranchID", "TEXT", ReferenceType.GetProperty("Guid"));

        public static new List<TableField> PrimaryKey = new List<TableField>()
        {
            ScopeBranchID
        };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };
    }
    public class ConnectionTable : TableBase
    {
        public static new string TableName = "TECConnection";
        public static Type ObjectType = typeof(TECConnection);

        public static TableField ConnectionID = new TableField("ConnectionID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField Length = new TableField("Length", "REAL", ObjectType.GetProperty("Length"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            ConnectionID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };
    }
    public class ControllerTable : TableBase
    {
        public static new string TableName = "TECController";
        public static Type ObjectType = typeof(TECController);

        public static TableField ControllerID = new TableField("ControllerID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField Name = new TableField("Name", "TEXT", ObjectType.GetProperty("Name"));
        public static TableField Description = new TableField("Description", "TEXT", ObjectType.GetProperty("Description"));
        public static TableField Cost = new TableField("Cost", "REAL", ObjectType.GetProperty("Cost"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            ControllerID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType
        };
    }
    public class ControllerIOTypeTable : RelationTableBase
    {
        public static new string TableName = "TECControllerTECIO";
        public static Type ObjectType = typeof(TECController);
        public static Type ReferenceType = typeof(TECIO);
      
        public static TableField ControllerID = new TableField("ControllerID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField IOType = new TableField("IOType", "TEXT", ReferenceType.GetProperty("Type"));
        public static TableField Quantity = new TableField("Quantity", "INTEGER", ReferenceType.GetProperty("Quantity"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            ControllerID,
            IOType
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };
    }
    public class ControllerConnectionTable : RelationTableBase
    {
        public static new string TableName = "TECControllerTECConection";
        public static Type ObjectType = typeof(TECController);
        public static Type ReferenceType = typeof(TECConnection);

        public static TableField ControllerID = new TableField("ControllerID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField ConnectionID = new TableField("ConnectionID", "TEXT", ReferenceType.GetProperty("Guid"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            ControllerID,
            ConnectionID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };
    }
    public class ScopeConnectionTable : RelationTableBase
    {
        public static new string TableName = "TECScopeTECConnection";
        public static Type ObjectType = typeof(TECScope);
        public static Type ReferenceType = typeof(TECConnection);

        public static TableField ScopeID = new TableField("ScopeID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField ConnectionID = new TableField("ConnectionID", "TEXT", ReferenceType.GetProperty("Guid"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            ScopeID,
            ConnectionID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };
    }
    public class ScopeBranchHierarchyTable : RelationTableBase
    {
        public static new string TableName = "TECScopeBranchHierarchy";
        public static Type ObjectType = typeof(TECScopeBranch);
        public static Type ReferenceType = typeof(TECScopeBranch);

        public static TableField ParentID = new TableField("ParentID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField ChildID = new TableField("ChildID", "TEXT", ReferenceType.GetProperty("Guid"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            ParentID,
            ChildID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };

    }
    public class BidSystemTable : RelationTableBase
    {
        public static new string TableName = "TECBidTECSystem";
        public static Type SystemType = typeof(TECSystem);
        public static Type BidType = typeof(TECBid);

        public static Type Helpers = typeof(HelperProperties);
        
        public static TableField Index = new TableField("ScopeIndex", "INTEGER", Helpers.GetProperty("Index"));
        public static TableField SystemID = new TableField("SystemID", "TEXT", SystemType.GetProperty("Guid"));
        public static TableField BidID = new TableField("BidID", "TEXT", BidType.GetProperty("Guid"));
        

        public static new List<TableField> PrimaryKey = new List<TableField>()
        {
            SystemID,
            BidID
        };
        public static new List<Type> Types = new List<Type>()
        {
            BidType,
            SystemType
        };
    }
    public class SystemEquipmentTable : RelationTableBase
    {
        public static new string TableName = "TECSystemTECEquipment";
        public static Type ObjectType = typeof(TECSystem);
        public static Type ReferenceType = typeof(TECEquipment);

        public static TableField SystemID = new TableField("SystemID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField EquipmentID = new TableField("EquipmentID", "TEXT", ReferenceType.GetProperty("Guid"));
        public static TableField ScopeIndex = new TableField("ScopeIndex", "INTEGER", null);

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            SystemID,
            EquipmentID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };

    }
    public class EquipmentSubScopeTable : RelationTableBase
    {
        public static new string TableName = "TECEquipmentTECSubScope";
        public static Type ObjectType = typeof(TECEquipment);
        public static Type ReferenceType = typeof(TECSubScope);

        public static TableField EquipmentID = new TableField("EquipmentID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField SubScopeID = new TableField("SubScopeID", "TEXT", ReferenceType.GetProperty("Guid"));
        public static TableField ScopeIndex = new TableField("ScopeIndex", "INTEGER", null);

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            EquipmentID,
            SubScopeID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };

    }
    public class SubScopeDeviceTable : RelationTableBase
    {
        public static new string TableName = "TECSubScopeTECDevice";
        public static Type ObjectType = typeof(TECSubScope);
        public static Type ReferenceType = typeof(TECDevice);

        public static TableField SubScopeID = new TableField("SubScopeID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField DeviceID = new TableField("DeviceID", "TEXT", ReferenceType.GetProperty("Guid"));
        public static TableField Quantity = new TableField("Quantity", "INTEGER", null);
        public static TableField ScopeIndex = new TableField("ScopeIndex", "INTEGER", null);

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            SubScopeID,
            DeviceID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };

    }
    public class SubScopePointTable : RelationTableBase
    {
        public static new string TableName = "TECSubScopeTECPoint";
        public static Type ObjectType = typeof(TECSubScope);
        public static Type ReferenceType = typeof(TECPoint);

        public static TableField SubScopeID = new TableField("SubScopeID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField PointID = new TableField("PointID", "TEXT", ReferenceType.GetProperty("Guid"));
        public static TableField ScopeIndex = new TableField("ScopeIndex", "INTEGER", null);

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            SubScopeID,
            PointID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };

    }
    public class ScopeTagTable : RelationTableBase
    {
        public static new string TableName = "TECScopeTECTag";
        public static Type ObjectType = typeof(TECScope);
        public static Type ReferenceType = typeof(TECTag);

        public static TableField ScopeID = new TableField("ScopeID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField TagID = new TableField("TagID", "TEXT", ReferenceType.GetProperty("Guid"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            ScopeID,
            TagID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };
    }
    public class DeviceManufacturerTable : RelationTableBase
    {
        public static new string TableName = "TECDeviceTECManufacturer";
        public static Type ObjectType = typeof(TECDevice);
        public static Type ReferenceType = typeof(TECManufacturer);

        public static TableField DeviceID = new TableField("DeviceID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField ManufacturerID = new TableField("ManufacturerID", "TEXT", ReferenceType.GetProperty("Guid"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            DeviceID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };

    }
    public class DeviceConnectionTypeTable : RelationTableBase
    {
        public static new string TableName = "TECDeviceTECConnectionType";
        public static Type ObjectType = typeof(TECDevice);
        public static Type ReferenceType = typeof(TECConnectionType);

        public static TableField DeviceID = new TableField("DeviceID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField TypeID = new TableField("ConnectionTypeID", "TEXT", ReferenceType.GetProperty("Guid"));

        public static new List<TableField> PrimaryKey = new List<TableField>()
        {

        };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };
    }
    public class DrawingPageTable : RelationTableBase
    {
        public static new string TableName = "TECDrawingTECPage";
        public static Type ObjectType = typeof(TECDrawing);
        public static Type ReferenceType = typeof(TECPage);

        public static TableField DrawingID = new TableField("DrawingID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField PageID = new TableField("PageID", "TEXT", ReferenceType.GetProperty("Guid"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            DrawingID,
            PageID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };

    }
    public class PageVisualScopeTable : RelationTableBase
    {
        public static new string TableName = "TECPageTECVisualScope";
        public static Type ObjectType = typeof(TECPage);
        public static Type ReferenceType = typeof(TECVisualScope);

        public static TableField PageID = new TableField("PageID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField VisualScopeID = new TableField("VisualScopeID", "TEXT", ReferenceType.GetProperty("Guid"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            PageID,
            VisualScopeID
            };

        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };

    }
    public class VisualScopeScopeTable : RelationTableBase
    {
        public static new string TableName = "TECVisualScopeTECScope";
        public static Type ObjectType = typeof(TECVisualScope);
        public static Type ReferenceType = typeof(TECScope);

        public static TableField VisualScopeID = new TableField("VisualScopeID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField ScopeID = new TableField("ScopeID", "TEXT", ReferenceType.GetProperty("Guid"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            VisualScopeID,
            ScopeID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };

    }
    public class LocationScopeTable : RelationTableBase
    {
        public static new string TableName = "TECLocationTECScope";
        public static Type ObjectType = typeof(TECScope);
        public static Type ReferenceType = typeof(TECLocation);

        public static TableField LocationID = new TableField("LocationID", "TEXT", ReferenceType.GetProperty("Guid"));
        public static TableField ScopeID = new TableField("ScopeID", "TEXT", ObjectType.GetProperty("Guid"));

        public static new List<TableField> PrimaryKey = new List<TableField>() {
            LocationID,
            ScopeID
            };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };
    }
    public class ScopeAssociatedCostTable : RelationTableBase
    {
        public static new string TableName = "TECScopeTECAssociatedCost";
        public static Type ObjectType = typeof(TECLocation);
        public static Type ReferenceType = typeof(TECScope);

        public static TableField ScopeID = new TableField("ScopeID", "TEXT", ObjectType.GetProperty("Guid"));
        public static TableField AssociatedCostID = new TableField("AssociatedCostID", "TEXT", ReferenceType.GetProperty("Guid"));
        public static TableField Quantity = new TableField("Quantity", "INTEGER", null);

        public static new List<TableField> PrimaryKey = new List<TableField>()
        {
            ScopeID,
            AssociatedCostID
        };
        public static new List<Type> Types = new List<Type>()
        {
            ObjectType,
            ReferenceType
        };
    }

    public static class AllBidTables
    {
        public static List<object> Tables = new List<object>() {
            new BidInfoTable(),
            new LaborConstantsTable(),
            new UserAdjustmentsTable(),
            new SubcontractorConstantsTable(),
            new NoteTable(),
            new ExclusionTable(),
            new ScopeBranchTable(),
            new SystemTable(),
            new EquipmentTable(),
            new SubScopeTable(),
            new DeviceTable(),
            new PointTable(),
            new TagTable(),
            new ManufacturerTable(),
            new DrawingTable(),
            new PageTable(),
            new LocationTable(),
            new VisualScopeTable(),
            new ConnectionTypeTable(),
            new ConduitTypeTable(),
            new ScopeBranchHierarchyTable(),
            new BidSystemTable(),
            new SystemEquipmentTable(),
            new EquipmentSubScopeTable(),
            new SubScopeDeviceTable(),
            new SubScopePointTable(),
            new ScopeTagTable(),
            new DeviceManufacturerTable(),
            new DrawingPageTable(),
            new PageVisualScopeTable(),
            new VisualScopeScopeTable(),
            new LocationScopeTable(),
            new ConnectionTable(),
            new ControllerTable(),
            new AssociatedCostTable(),
            new ControllerConnectionTable(),
            new ControllerIOTypeTable(),
            new ScopeConnectionTable(),
            new ProposalScopeTable(),
            new ProposalScopeScopeBranchTable(),
            new BidScopeBranchTable(),
            new DeviceConnectionTypeTable(),
            new ScopeAssociatedCostTable()
            };
    }

    public static class AllTemplateTables
    {
        public static List<object> Tables = new List<object>()
        {
            new TemplatesInfoTable(),
            new LaborConstantsTable(),
            new SubcontractorConstantsTable(),
            new SystemTable(),
            new EquipmentTable(),
            new SubScopeTable(),
            new DeviceTable(),
            new PointTable(),
            new TagTable(),
            new ManufacturerTable(),
            new ConnectionTypeTable(),
            new ConduitTypeTable(),
            new AssociatedCostTable(),
            new SystemEquipmentTable(),
            new EquipmentSubScopeTable(),
            new SubScopeDeviceTable(),
            new SubScopePointTable(),
            new ScopeTagTable(),
            new ControllerTable(),
            new ControllerIOTypeTable(),
            new ScopeConnectionTable(),
            new DeviceManufacturerTable(),
            new DeviceConnectionTypeTable(),
            new ScopeAssociatedCostTable()
        };
    }

    public static class AllTables
    {
        public static List<object> Tables = new List<object>()
        {
            new BidInfoTable(),
            new TemplatesInfoTable(),
            new LaborConstantsTable(),
            new SubcontractorConstantsTable(),
            new UserAdjustmentsTable(),
            new CostAdditionsTable(),
            new NoteTable(),
            new ExclusionTable(),
            new BidScopeBranchTable(),
            new ScopeBranchTable(),
            new SystemTable(),
            new EquipmentTable(),
            new SubScopeTable(),
            new DeviceTable(),
            new PointTable(),
            new TagTable(),
            new ManufacturerTable(),
            new DrawingTable(),
            new PageTable(),
            new LocationTable(),
            new VisualScopeTable(),
            new ProposalScopeTable(),
            new ConnectionTypeTable(),
            new ConduitTypeTable(),
            new AssociatedCostTable(),
            new ProposalScopeScopeBranchTable(),
            new ConnectionTable(),
            new ControllerTable(),
            new ControllerIOTypeTable(),
            new ControllerConnectionTable(),
            new ScopeConnectionTable(),
            new ScopeBranchHierarchyTable(),
            new BidSystemTable(),
            new SystemEquipmentTable(),
            new EquipmentSubScopeTable(),
            new SubScopeDeviceTable(),
            new SubScopePointTable(),
            new ScopeTagTable(),
            new DeviceManufacturerTable(),
            new DeviceConnectionTypeTable(),
            new DrawingPageTable(),
            new PageVisualScopeTable(),
            new VisualScopeScopeTable(),
            new LocationScopeTable(),
            new ScopeAssociatedCostTable()
        };
    }

    public class TableBase
    {
        public static string TableName;
        public static List<Type> Types;
        
        public static List<TableField> PrimaryKey;
    }

    public class RelationTableBase : TableBase { }

    public class TableField
    {
        public string Name;
        public string FieldType;
        public PropertyInfo Property;

        public TableField(string name, string fieldType, PropertyInfo property)
        {
            Name = name;
            FieldType = fieldType;
            Property = property;
        }
    }
    
    public class HelperProperties
    {
        public bool Index;

        public HelperProperties() { }

    }
    #endregion
}
