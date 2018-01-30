using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using IFilterTextReader.Exceptions;

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
    /// Utility class to get a Class Factory for a certain Class ID by loading the dll that implements that class
    /// </summary>
    internal class ComHelpers : IDisposable
    {
        #region Fields
        /// <summary>
        /// Used to track all used DLL's by their handle
        /// </summary>
        private readonly List<IntPtr> _dllHandles = new List<IntPtr>();
        #endregion

        #region GetClassFactory
        /// <summary>
        /// Gets a class factory for a specific COM Class ID. 
        /// </summary>
        /// <param name="dllName">The dll where the COM class is implemented</param>
        /// <param name="filterPersistClass">The requested Class ID</param>
        /// <returns>IClassFactory instance used to create instances of that class</returns>
        public NativeMethods.IClassFactory GetClassFactory(string dllName, string filterPersistClass)
        {
            // Load the class factory from the dll
            var classFactory = GetClassFactoryFromDll(dllName, filterPersistClass);
            return classFactory;
        }
        #endregion

        #region GetClassFactoryFromDll
        /// <summary>
        /// Returns the class factory from a DLL
        /// </summary>
        /// <param name="dllName">The name of the DLL</param>
        /// <param name="filterPersistClass">The persistant class we want</param>
        /// <returns></returns>
        private NativeMethods.IClassFactory GetClassFactoryFromDll(string dllName, string filterPersistClass)
        {
            // Load the dll
            var dllHandle = NativeMethods.LoadLibrary(dllName);
            if (dllHandle == IntPtr.Zero)
                return null;

            // Keep a reference to the dll until the process\AppDomain dies
            _dllHandles.Add(dllHandle);

            // Get a pointer to the DllGetClassObject function
            var dllGetClassObjectPtr = NativeMethods.GetProcAddress(dllHandle, "DllGetClassObject");
            if (dllGetClassObjectPtr == IntPtr.Zero)
                throw new IFClassFactoryFailure("Could not get proc address from filter dll");

            // Convert the function pointer to a .net delegate
            var dllGetClassObject =
                (NativeMethods.DllGetClassObject)
                    Marshal.GetDelegateForFunctionPointer(dllGetClassObjectPtr, typeof (NativeMethods.DllGetClassObject));

            // Call the DllGetClassObject to retreive a class factory for out Filter class
            var filterPersistGuid = new Guid(filterPersistClass);
            var classFactoryGuid = new Guid("00000001-0000-0000-C000-000000000046"); //IClassFactory class id
            
            Object unk;
            
            if (dllGetClassObject(ref filterPersistGuid, ref classFactoryGuid, out unk) != 0)
                return null;

            // Cast the returned object to IClassFactory
            return (unk as NativeMethods.IClassFactory);
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            foreach (var dllHandle in _dllHandles)
                NativeMethods.FreeLibrary(dllHandle);
        }
        #endregion
    }
}
