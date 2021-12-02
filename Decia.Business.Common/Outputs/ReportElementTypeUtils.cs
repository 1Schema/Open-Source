using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Decia.Business.Common.Outputs
{
    public static class ReportElementTypeUtils
    {
        public static readonly IEnumerable<ReportElementType_New> CreatableElementTypes = new ReportElementType_New[] { ReportElementType_New.Cell, ReportElementType_New.Container, ReportElementType_New.DimensionalTable, ReportElementType_New.DimensionalTable_VariableTitleBox };
        public static readonly IEnumerable<ReportElementType_New> DeletableElementTypes = new ReportElementType_New[] { ReportElementType_New.Cell, ReportElementType_New.Container, ReportElementType_New.DimensionalTable, ReportElementType_New.DimensionalTable_VariableTitleBox };
        public static readonly IEnumerable<ReportElementType_New> DraggableElementTypes = new ReportElementType_New[] { ReportElementType_New.Cell, ReportElementType_New.Container, ReportElementType_New.DimensionalTable };
        public static readonly IEnumerable<ReportElementType_New> OrderableElementTypes = new ReportElementType_New[] { ReportElementType_New.DimensionalTable_VariableTitleBox };
        public static readonly IEnumerable<ReportElementType_New> DropContainerElementTypes = new ReportElementType_New[] { ReportElementType_New.Report, ReportElementType_New.Container, ReportElementType_New.DimensionalTable_TableHeader, ReportElementType_New.DimensionalTable_VariableTitleContainer };
        public static readonly IEnumerable<ReportElementType_New> DropPassThroughElementTypes_Always = new ReportElementType_New[] { ReportElementType_New.Cell, ReportElementType_New.DimensionalTable_VariableTitleBox, ReportElementType_New.DimensionalTable_VariableTitleRange };
        public static readonly IEnumerable<ReportElementType_New> DropPassThroughElementTypes_Sometimes = new ReportElementType_New[] { ReportElementType_New.DimensionalTable_StructuralTitleRange, ReportElementType_New.DimensionalTable_TimeTitleRange };

        public static void AssertIsCreatable(this ReportElementType_New elementType)
        {
            if (!CreatableElementTypes.Contains(elementType))
            { throw new InvalidOperationException("The specified Report Element Type is not directly creatable by the User."); }
        }

        public static void AssertIsDeletable(this ReportElementType_New elementType)
        {
            if (!DeletableElementTypes.Contains(elementType))
            { throw new InvalidOperationException("The specified Report Element Type is not directly deletable by the User."); }
        }

        public static bool IsTypeValid(this ReportElementType_New reportElementType)
        {
            var enumValues = Enum.GetValues(typeof(ReportElementType_New)).Cast<ReportElementType_New>();
            if (!enumValues.Contains(reportElementType))
            { return false; }

            return true;
        }

        public static void AssertTypeIsValid(this ReportElementType_New reportElementType)
        {
            if (!reportElementType.IsTypeValid())
            { throw new InvalidOperationException("The specified Report Element Type is not valid."); }
        }

        public static bool IsNameValid(this ReportElementType_New reportElementType, string name)
        {
            return (!string.IsNullOrWhiteSpace(name));
        }

        public static void AssertNameIsValid(this ReportElementType_New reportElementType, string name)
        {
            if (!reportElementType.IsNameValid(name))
            { throw new InvalidOperationException("The specified Report Element Name is not valid."); }
        }

        public static string GetNameForEnumValue(this ReportElementType_New reportElementType)
        {
            if (reportElementType == ReportElementType_New.Report)
            { return "Report"; }
            else if (reportElementType == ReportElementType_New.Header)
            { return "Header"; }
            else if (reportElementType == ReportElementType_New.Footer)
            { return "Footer"; }
            else if (reportElementType == ReportElementType_New.Cell)
            { return "Cell"; }
            else if (reportElementType == ReportElementType_New.Container)
            { return "Container"; }
            else if (reportElementType == ReportElementType_New.DimensionalTable)
            { return "Dimensional Table"; }
            else if (reportElementType == ReportElementType_New.DimensionalTable_TableHeader)
            { return "Table Header"; }
            else if (reportElementType == ReportElementType_New.DimensionalTable_RowHeader)
            { return "Row Header"; }
            else if (reportElementType == ReportElementType_New.DimensionalTable_ColumnHeader)
            { return "Column Header"; }
            else if (reportElementType == ReportElementType_New.DimensionalTable_DataArea)
            { return "Data Area"; }
            else if (reportElementType == ReportElementType_New.DimensionalTable_CommonTitleContainer)
            { return "Common Title Container"; }
            else if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleContainer)
            { return "Variable Title Container"; }
            else if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataContainer)
            { return "Variable Data Container"; }
            else if (reportElementType == ReportElementType_New.DimensionalTable_CommonTitleBox)
            { return "Common Title Box"; }
            else if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleBox)
            { return "Variable Title Box"; }
            else if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataBox)
            { return "Variable Data Box"; }
            else if (reportElementType == ReportElementType_New.DimensionalTable_StructuralTitleRange)
            { return "Structural Title Range"; }
            else if (reportElementType == ReportElementType_New.DimensionalTable_TimeTitleRange)
            { return "Time Title Range"; }
            else if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleRange)
            { return "Variable Title Range"; }
            else if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataRange)
            { return "Variable Data Range"; }
            else
            { throw new InvalidOperationException("Invalid Report Element Type encountered."); }
        }

        public static bool HasDataBoundMultiplicity(this ReportElementType_New reportElementType)
        {
            if (reportElementType == ReportElementType_New.DimensionalTable_CommonTitleBox)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleBox)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataBox)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_StructuralTitleRange)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_TimeTitleRange)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleRange)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataRange)
            { return true; }
            return false;
        }

        public static void AssertHasDataBoundMultiplicity(this ReportElementType_New reportElementType)
        {
            if (!reportElementType.HasDataBoundMultiplicity())
            { throw new InvalidOperationException("The specified Report Element does not have data-bound Multiplicity."); }
        }

        public static void AssertNotHasDataBoundMultiplicity(this ReportElementType_New reportElementType)
        {
            if (reportElementType.HasDataBoundMultiplicity())
            { throw new InvalidOperationException("The specified Report Element has data-bound Multiplicity."); }
        }

        public static bool IsContainer(this ReportElementType_New reportElementType)
        {
            if (reportElementType == ReportElementType_New.Report)
            { return true; }
            if (reportElementType == ReportElementType_New.Container)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_TableHeader)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_RowHeader)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_ColumnHeader)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_DataArea)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_CommonTitleContainer)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleContainer)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataContainer)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_CommonTitleBox)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleBox)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataBox)
            { return true; }
            return false;
        }

        public static void AssertIsContainer(this ReportElementType_New reportElementType)
        {
            if (!reportElementType.IsContainer())
            { throw new InvalidOperationException("The specified Report Element is not a Container."); }
        }

        public static void AssertNotIsContainer(this ReportElementType_New reportElementType)
        {
            if (reportElementType.IsContainer())
            { throw new InvalidOperationException("The specified Report Element is a Container."); }
        }

        public static bool AreContentsEditable(this ReportElementType_New reportElementType)
        {
            if (reportElementType == ReportElementType_New.DimensionalTable)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_DataArea)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataContainer)
            { return false; }

            return (reportElementType.GetAcceptableContentTypes().Count > 0);
        }

        public static void AssertContentsAreEditable(this ReportElementType_New reportElementType)
        {
            if (!reportElementType.AreContentsEditable())
            { throw new InvalidOperationException("The specified Report Element does not support edits to its contents."); }
        }

        public static List<ReportElementType_New> GetAcceptableContentTypes(this ReportElementType_New reportElementType)
        {
            ReportElementType_New[] acceptableTypes = null;

            if (reportElementType == ReportElementType_New.Report)
            {
                acceptableTypes = new ReportElementType_New[] { 
                    ReportElementType_New.Cell, 
                    ReportElementType_New.DimensionalTable, 
                    ReportElementType_New.Container
                };
            }
            else if (reportElementType == ReportElementType_New.Container)
            {
                acceptableTypes = new ReportElementType_New[] { 
                    ReportElementType_New.Cell, 
                    ReportElementType_New.DimensionalTable, 
                    ReportElementType_New.Container
                };
            }
            else if (reportElementType == ReportElementType_New.DimensionalTable)
            {
                acceptableTypes = new ReportElementType_New[] { 
                    ReportElementType_New.DimensionalTable_TableHeader, 
                    ReportElementType_New.DimensionalTable_RowHeader, 
                    ReportElementType_New.DimensionalTable_ColumnHeader,
                    ReportElementType_New.DimensionalTable_DataArea
                };
            }
            else if (reportElementType == ReportElementType_New.DimensionalTable_TableHeader)
            {
                acceptableTypes = new ReportElementType_New[] { 
                    ReportElementType_New.Cell
                };
            }
            else if (reportElementType == ReportElementType_New.DimensionalTable_RowHeader)
            {
                acceptableTypes = new ReportElementType_New[] { 
                    ReportElementType_New.DimensionalTable_StructuralTitleRange,
                    ReportElementType_New.DimensionalTable_TimeTitleRange,
                    ReportElementType_New.DimensionalTable_VariableTitleContainer
                };
            }
            else if (reportElementType == ReportElementType_New.DimensionalTable_ColumnHeader)
            {
                acceptableTypes = new ReportElementType_New[] { 
                    ReportElementType_New.DimensionalTable_StructuralTitleRange,
                    ReportElementType_New.DimensionalTable_TimeTitleRange,
                    ReportElementType_New.DimensionalTable_VariableTitleContainer
                };
            }
            else if (reportElementType == ReportElementType_New.DimensionalTable_DataArea)
            {
                acceptableTypes = new ReportElementType_New[] { 
                    ReportElementType_New.DimensionalTable_VariableDataContainer
                };
            }
            else if (reportElementType == ReportElementType_New.DimensionalTable_CommonTitleContainer)
            {
                acceptableTypes = new ReportElementType_New[] { 
                    ReportElementType_New.DimensionalTable_CommonTitleBox
                };
            }
            else if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleContainer)
            {
                acceptableTypes = new ReportElementType_New[] { 
                    ReportElementType_New.DimensionalTable_VariableTitleBox
                };
            }
            else if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataContainer)
            {
                acceptableTypes = new ReportElementType_New[] { 
                    ReportElementType_New.DimensionalTable_VariableDataBox
                };
            }
            else if (reportElementType == ReportElementType_New.DimensionalTable_CommonTitleBox)
            {
                acceptableTypes = new ReportElementType_New[] { 
                    ReportElementType_New.DimensionalTable_StructuralTitleRange, 
                    ReportElementType_New.DimensionalTable_TimeTitleRange
                };
            }
            else if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleBox)
            {
                acceptableTypes = new ReportElementType_New[] { 
                    ReportElementType_New.DimensionalTable_VariableTitleRange,
                    ReportElementType_New.DimensionalTable_StructuralTitleRange, 
                    ReportElementType_New.DimensionalTable_TimeTitleRange
                };
            }
            else if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataBox)
            {
                acceptableTypes = new ReportElementType_New[] { 
                    ReportElementType_New.DimensionalTable_VariableDataRange
                };
            }
            else if ((reportElementType == ReportElementType_New.Cell) ||
                (reportElementType == ReportElementType_New.DimensionalTable_StructuralTitleRange) ||
                (reportElementType == ReportElementType_New.DimensionalTable_TimeTitleRange) ||
                (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleRange) ||
                (reportElementType == ReportElementType_New.DimensionalTable_VariableDataRange))
            {
                acceptableTypes = new ReportElementType_New[] { };
            }
            else
            { throw new InvalidOperationException("The specified ReportElementType is not supported."); }

            return acceptableTypes.ToList();
        }

        public static void AssertIsAcceptableContentType(this ReportElementType_New reportElementType, ReportElementType_New childElementType)
        {
            if (!reportElementType.GetAcceptableContentTypes().Contains(childElementType))
            { throw new InvalidOperationException("The specified Report Element cannot contain Children of the specified Content Element Type."); }
        }

        public static bool IsEditable_Positioning(this ReportElementType_New reportElementType)
        {
            if (reportElementType == ReportElementType_New.Header)
            { return false; }
            if (reportElementType == ReportElementType_New.Footer)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_TableHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_RowHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_ColumnHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_DataArea)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_CommonTitleContainer)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_CommonTitleBox)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleContainer)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleBox)
            { return false; }
            return true;
        }

        public static void AssertIsEditable_Positioning(this ReportElementType_New reportElementType)
        {
            if (!reportElementType.IsEditable_Positioning())
            { throw new InvalidOperationException("The specified Report Element does not allow it's Positioning to be changed."); }
        }

        public static bool IsEditable_Sizing(this ReportElementType_New reportElementType)
        {
            if (reportElementType == ReportElementType_New.Header)
            { return false; }
            if (reportElementType == ReportElementType_New.Footer)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_TableHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_RowHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_ColumnHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_DataArea)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_CommonTitleContainer)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleContainer)
            { return false; }
            return true;
        }

        public static void AssertIsEditable_Sizing(this ReportElementType_New reportElementType)
        {
            if (!reportElementType.IsEditable_Sizing())
            { throw new InvalidOperationException("The specified Report Element does not allow it's Sizing to be changed."); }
        }

        public static bool RequiresDelayedSizing(this ReportElementType_New reportElementType)
        {
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataContainer)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataRange)
            { return true; }
            return false;
        }

        public static void AssertDoesNotRequireDelayedSizing(this ReportElementType_New reportElementType)
        {
            if (reportElementType.RequiresDelayedSizing())
            { throw new InvalidOperationException("The specified Report Element requires Delayed Sizing."); }
        }

        public static void AssertRequiresDelayedSizing(this ReportElementType_New reportElementType)
        {
            if (!reportElementType.RequiresDelayedSizing())
            { throw new InvalidOperationException("The specified Report Element does not allow Delayed Sizing."); }
        }

        public static bool IsDirectlyTransposable(this ReportElementType_New reportElementType)
        {
            if (reportElementType == ReportElementType_New.DimensionalTable)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_TimeTitleRange)
            { return true; }
            if (reportElementType == ReportElementType_New.DimensionalTable_StructuralTitleRange)
            { return true; }
            return false;
        }

        public static void AssertIsDirectlyTransposable(this ReportElementType_New reportElementType)
        {
            if (!reportElementType.IsDirectlyTransposable())
            { throw new InvalidOperationException("The specified Report Element cannot be directly Transposed."); }
        }

        public static bool IsEditable_Style(this ReportElementType_New reportElementType)
        {
            return true;
        }

        public static void AssertIsEditable_Style(this ReportElementType_New reportElementType)
        {
            if (!reportElementType.IsEditable_Style())
            { throw new InvalidOperationException("The specified Report Element does not allow it's Style to be changed."); }
        }

        public static bool IsLocked_Default(this ReportElementType_New reportElementType)
        {
            return false;
        }

        public static bool IsUnlockable(this ReportElementType_New reportElementType)
        {
            return true;
        }

        public static void AssertIsUnlockable(this ReportElementType_New reportElementType)
        {
            if (!reportElementType.IsUnlockable())
            { throw new InvalidOperationException("The specified Report Element is not unlockable."); }
        }

        public static bool IsParentEditable_Default(this ReportElementType_New reportElementType)
        {
            if (reportElementType == ReportElementType_New.Report)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_TableHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_RowHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_ColumnHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_DataArea)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataContainer)
            { return false; }
            return true;
        }

        public static bool GetCanEditParentEditability(this ReportElementType_New reportElementType)
        {
            if (reportElementType == ReportElementType_New.Report)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_TableHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_RowHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_ColumnHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_DataArea)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataContainer)
            { return false; }
            return true;
        }

        public static void AssertCanEditParentEditability(this ReportElementType_New reportElementType)
        {
            if (!reportElementType.GetCanEditParentEditability())
            { throw new InvalidOperationException("The specified Report Element cannot change its Parent."); }
        }

        public static bool IsDirectlyDeletable_Default(this ReportElementType_New reportElementType)
        {
            if (reportElementType == ReportElementType_New.DimensionalTable_TableHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_RowHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_ColumnHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_DataArea)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_StructuralTitleRange)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_TimeTitleRange)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_CommonTitleContainer)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_CommonTitleBox)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleContainer)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleRange)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataContainer)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataBox)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataRange)
            { return false; }
            return true;
        }

        public static bool GetCanEditDeletability(this ReportElementType_New reportElementType)
        {
            if (reportElementType == ReportElementType_New.DimensionalTable_TableHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_RowHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_ColumnHeader)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_DataArea)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_StructuralTitleRange)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_TimeTitleRange)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_CommonTitleContainer)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_CommonTitleBox)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleContainer)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableTitleRange)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataContainer)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataBox)
            { return false; }
            if (reportElementType == ReportElementType_New.DimensionalTable_VariableDataRange)
            { return false; }
            return true;
        }

        public static void AssertCanEditDeletability(this ReportElementType_New reportElementType)
        {
            if (!reportElementType.GetCanEditDeletability())
            { throw new InvalidOperationException("The specified Report Element cannot change its Deletability."); }
        }

        public static bool AutoDeletesChildren(this ReportElementType_New reportElementType)
        {
            return true;
        }
    }
}