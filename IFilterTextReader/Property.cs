using System;

namespace IFilterTextReader
{
    #region Enum PropertyType
    /// <summary>
    /// The type of a <see cref="Property"/>
    /// </summary>
    public enum PropertyType
    {
        /// <summary>
        /// The property is a string value
        /// </summary>
        String,

        /// <summary>
        /// The property is a number
        /// </summary>
        Number,

        /// <summary>
        /// The property is a datetime value
        /// </summary>
        DateTime
    }
    #endregion
    
    /// <summary>
    /// Contains information about a metadata property
    /// </summary>
    public class Property
    {
        #region Properties
        /// <summary>
        /// The <see cref="Guid"/> to which property set the unmapped property belongs
        /// </summary>
        public Guid PropertySetGuid { get; private set; }

        /// <summary>
        /// The name/id of the property
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// The friendly name of the property, e.g. From, To, CC, etc...
        /// </summary>
        public string FriendlyName { get; private set; }

        /// <summary>
        /// The <see cref="MetadataPropertyType">type</see> of the property
        /// </summary>
        public PropertyType Type { get; private set; }

        /// <summary>
        /// The mask to use for the property, e.g. YYYY-mm-dd HH:MM:SS
        /// </summary>
        public string Mask { get; private set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Creates this objects and sets its properties
        /// </summary>
        /// <param name="propertySetGuid">The <see cref="Guid"/> to which property set the unmapped property belongs</param>
        /// <param name="propertyName">The name/id of the property</param>
        /// <param name="friendlyName">The friendly name for the property, e.g. Document type</param>
        /// <param name="type">The <see cref="PropertyType">type</see> of the property</param>
        /// <param name="mask">The mask to use for the property, e.g. YYYY-mm-dd HH:MM:SS</param>
        internal Property(Guid propertySetGuid,
            string propertyName,
            string friendlyName,
            PropertyType type = PropertyType.String,
            string mask = "")
        {
            PropertySetGuid = propertySetGuid;
            PropertyName = propertyName;
            FriendlyName = friendlyName;
            Type = type;
            Mask = mask;
        }
        #endregion
    }
}
