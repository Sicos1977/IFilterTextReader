using System;

/*
   Copyright 2013-2018 Kees van Spelde

   Licensed under The Code Project Open License (CPOL) 1.02;
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

     http://www.codeproject.com/info/cpol10.aspx

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

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
        public Guid PropertySetGuid { get; private set; }

        /// <summary>
        /// The name/id of the property
        /// </summary>
        public string PropertyName { get; private set; }

        /// <summary>
        /// The canonical name for the property when found in the system, null when not found
        /// </summary>
        public string CanonicalName { get; private set; }

        /// <summary>
        /// The value that has been found with this unmapped property
        /// </summary>
        public string Value { get; private set; }

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
