//
// Property.cs
//
// Author: Kees van Spelde <sicos2002@hotmail.com>
//
// Copyright (c) 2013-2022 Magic-Sessions. (www.magic-sessions.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NON INFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
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
        public Guid PropertySetGuid { get; }

        /// <summary>
        /// The name/id of the property
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// The friendly name of the property, e.g. From, To, CC, etc...
        /// </summary>
        public string FriendlyName { get; }

        /// <summary>
        /// The <see cref="PropertyType">type</see> of the property
        /// </summary>
        public PropertyType Type { get; }

        /// <summary>
        /// The mask to use for the property, e.g. YYYY-mm-dd HH:MM:SS
        /// </summary>
        public string Mask { get; }
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
