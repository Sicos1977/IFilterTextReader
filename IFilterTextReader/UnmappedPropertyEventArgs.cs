//
// UnmappedPropertyEventArgs.cs
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

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace IFilterTextReader
{
    /// <summary>
    /// Raised through a delegate when an unmapped property has been found
    /// </summary>
    public class UnmappedPropertyEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="Guid"/> to which property set the unmapped property belongs
        /// </summary>
        public Guid PropertySetGuid { get; }

        /// <summary>
        /// The name/id of the property
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// The canonical name for the property when found in the system, null when not found
        /// </summary>
        public string CanonicalName { get; }

        /// <summary>
        /// The value that has been found with this unmapped property
        /// </summary>
        public string Value { get; }

        #region Constructor
        /// <summary>
        /// Creates this objects and sets its properties
        /// </summary>
        /// <param name="propertySetGuid">The <see cref="Guid"/> to which property set the unmapped property belongs</param>
        /// <param name="propertyName">The name/id of the property</param>
        /// <param name="canonicalName">The canonical name for the property, null when not found</param>
        /// <param name="value"></param>
        internal UnmappedPropertyEventArgs(Guid propertySetGuid,
            string propertyName,
            string canonicalName,
            string value)
        {
            PropertySetGuid = propertySetGuid;
            PropertyName = propertyName;
            CanonicalName = canonicalName;
            Value = value;
        }
        #endregion
    }
}
