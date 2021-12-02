using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Decia.Business.Common.Modeling;
using Decia.Business.Common.Presentation;

namespace Decia.Business.Common
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true)]
    public class PropertyDisplayDataAttribute : Attribute, IOrderable
    {
        public const string Default_DisplayName = "";
        public const string Default_Description = "";
        public const bool Default_IsEditable = true;
        public const EditorType Default_EditorType = EditorType.Input;
        public static readonly PopupType? Default_PopupType = null;
        public const long DisplayGroup_BaseNumber = 10000;

        public static readonly string NullSelectionValue = DeciaDataType.UniqueID.ToStringValue(null, true);
        public static readonly string NullSelectionText = NullSelectionValue.ToUpper();

        private long? m_DefaultOrderNumber;

        public PropertyDisplayDataAttribute(bool isVisible)
            : base()
        {
            Id = Guid.NewGuid();

            IsVisible = isVisible;
            DisplayName = Default_DisplayName;
            Description = Default_Description;

            IsEditable = isVisible ? Default_IsEditable : false;
            EditorType = Default_EditorType;
            AvailableOptions = new Dictionary<string, string>();

            PopupType = Default_PopupType;

            UseDisplayGroup = false;
            SupportedViewTypes = PropertyHost_ViewType.None;
            DisplayGroup = PropertyDisplayGroup.General;
            RefreshOnUpdate = false;
        }

        public PropertyDisplayDataAttribute()
            : this(Default_DisplayName)
        { }

        public PropertyDisplayDataAttribute(string displayName)
            : this(displayName, Default_Description)
        { }

        public PropertyDisplayDataAttribute(string displayName, string description)
            : this(displayName, description, Default_IsEditable, Default_EditorType)
        { }

        public PropertyDisplayDataAttribute(string displayName, string description, bool isEditable, EditorType editorType)
            : this(displayName, description, isEditable, editorType, null, Default_PopupType)
        { }

        public PropertyDisplayDataAttribute(string displayName, string description, bool isEditable, EditorType editorType, Type enumTypeToDisplay)
            : this(displayName, description, isEditable, editorType, enumTypeToDisplay, Default_PopupType)
        { }

        public PropertyDisplayDataAttribute(string displayName, string description, bool isEditable, PopupType popupType)
            : this(displayName, description, isEditable, Default_EditorType, null, popupType)
        { }

        protected PropertyDisplayDataAttribute(string displayName, string description, bool isEditable, EditorType editorType, Type enumTypeToDisplay, PopupType? popupType)
            : base()
        {
            Id = Guid.NewGuid();

            IsVisible = true;
            DisplayName = displayName;
            Description = description;

            IsEditable = isEditable;
            EditorType = editorType;
            AvailableOptions = (enumTypeToDisplay != null) ? EnumUtils.GetEnumValuesAsSelectOptions(enumTypeToDisplay) : new Dictionary<string, string>();

            PopupType = popupType;

            UseDisplayGroup = false;
            SupportedViewTypes = PropertyHost_ViewType.None;
            DisplayGroup = PropertyDisplayGroup.General;
            RefreshOnUpdate = false;
        }

        public Guid Id { get; protected set; }
        public PropertyInfo PropertyInfo { get; protected set; }
        public Type DataType { get { return (PropertyInfo != null) ? PropertyInfo.PropertyType : null; } }

        public bool IsVisible { get; protected set; }
        public string DisplayName { get; protected set; }
        public string Description { get; protected set; }

        public bool IsEditable { get; protected set; }
        public EditorType EditorType { get; protected set; }
        public Dictionary<string, string> AvailableOptions { get; protected set; }

        public bool RequiresPopup { get { return PopupType.HasValue; } }
        public PopupType? PopupType { get; protected set; }
        public string PopupData { get { return string.Empty; } }

        public bool UseDisplayGroup { get; set; }
        public PropertyHost_ViewType SupportedViewTypes { get; set; }
        public PropertyDisplayGroup DisplayGroup { get; set; }
        public bool RefreshOnUpdate { get; set; }

        public long OrderNumber_NonNull
        {
            get { return m_DefaultOrderNumber.HasValue ? m_DefaultOrderNumber.Value : long.MinValue; }
            set { m_DefaultOrderNumber = value; }
        }

        public DeciaDataType DeciaDataType
        {
            get
            {
                if (DataType == null)
                { return DeciaDataType.Text; }

                DeciaDataType dataType = Modeling.DeciaDataType.Text;
                var success = DeciaDataTypeUtils.TryGetDataTypeForSystemType(DataType, out dataType);

                return dataType;
            }
        }

        internal void SetPropertyInfo(PropertyInfo propertyInfo)
        {
            if (PropertyInfo != null)
            { throw new InvalidOperationException("This method should only be called on Attributes with no PropertyInfo set."); }

            PropertyInfo = propertyInfo;

            if (!IsVisible)
            { return; }

            if (string.IsNullOrWhiteSpace(DisplayName))
            { DisplayName = propertyInfo.Name; }
        }

        #region Methods - IPropertyDisplayDataOverrider Hooks

        public bool Get_IsVisible_ForObject(object obj, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            if (obj == null)
            { throw new InvalidOperationException("The Object to check must not be null."); }

            var objAsProvider = (obj as IPropertyDisplayDataOverrider);
            if (objAsProvider == null)
            { return this.IsVisible; }

            return objAsProvider.Get_IsVisible_ForProperty(this, modelDataProviderAsObj, reportDataObjectAsObj, args);
        }

        public bool Get_IsEditable_ForObject(object obj, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            if (obj == null)
            { throw new InvalidOperationException("The Object to check must not be null."); }

            var objAsProvider = (obj as IPropertyDisplayDataOverrider);
            if (objAsProvider == null)
            { return this.IsEditable; }

            return objAsProvider.Get_IsEditable_ForProperty(this, modelDataProviderAsObj, reportDataObjectAsObj, args);
        }

        public object Get_Value_ForObject(object obj, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            if (obj == null)
            { throw new InvalidOperationException("The Object to check must not be null."); }

            var objAsProvider = (obj as IPropertyDisplayDataOverrider_WithValue);
            if (objAsProvider == null)
            { return this.PropertyInfo.GetValue(obj, null); ; }

            return objAsProvider.Get_Value_ForProperty(this, modelDataProviderAsObj, reportDataObjectAsObj, args);
        }

        public Dictionary<string, string> Get_AvailableOptions_ForObject(object obj, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            if (obj == null)
            { throw new InvalidOperationException("The Object to check must not be null."); }

            var objAsProvider = (obj as IPropertyDisplayDataOverrider);
            if (objAsProvider == null)
            { return this.AvailableOptions; }

            return objAsProvider.Get_AvailableOptions_ForProperty(this, modelDataProviderAsObj, reportDataObjectAsObj, args);
        }

        public string Get_PopupData_ForObject(object obj, object modelDataProviderAsObj, object reportDataObjectAsObj, object[] args)
        {
            if (obj == null)
            { throw new InvalidOperationException("The Object to check must not be null."); }

            var objAsProvider = (obj as IPropertyDisplayDataOverrider);
            if (objAsProvider == null)
            { return this.PopupData; }

            return objAsProvider.Get_PopupData_ForProperty(this, modelDataProviderAsObj, reportDataObjectAsObj, args);
        }

        #endregion

        #region IOrderable Implementation

        long? IOrderable.OrderNumber
        {
            get
            {
                if (UseDisplayGroup)
                {
                    var displayGroupNumber = (long)DisplayGroup;
                    var displayGroupIndex = (DisplayGroup_BaseNumber * displayGroupNumber);
                    var propertyIndex = m_DefaultOrderNumber.HasValue ? (displayGroupIndex + m_DefaultOrderNumber.Value) : displayGroupIndex;
                    return propertyIndex;
                }
                return m_DefaultOrderNumber;
            }
        }

        string IOrderable.OrderValue
        {
            get { return DisplayName; }
        }

        #endregion
    }
}