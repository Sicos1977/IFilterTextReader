//
// ComHelpers.cs
//
// Author: Kees van Spelde <sicos2002@hotmail.com>
//
// Copyright (c) 2013-2023 Magic-Sessions. (www.magic-sessions.com)
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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using IFilterTextReader.Exceptions;

namespace IFilterTextReader;

/// <summary>
///     Utility class to get a Class Factory for a certain Class ID by loading the dll that implements that class
/// </summary>
internal class ComHelpers : IDisposable
{
    #region Fields
    /// <summary>
    ///     Used to track all used DLL's by their handle
    /// </summary>
    private readonly List<IntPtr> _dllHandles = new();
    #endregion

    #region GetClassFactory
    /// <summary>
    ///     Gets a class factory for a specific COM Class ID.
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
    ///     Returns the class factory from a DLL
    /// </summary>
    /// <param name="dllName">The name of the DLL</param>
    /// <param name="filterPersistClass">The persistant class we want</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    private NativeMethods.IClassFactory GetClassFactoryFromDll(string dllName, string filterPersistClass)
    {
        // Don't try to load non-existing dll
        if (dllName == null || filterPersistClass == null)
            return null;

        // Load the dll if it is not already loaded
        var dllHandle = NativeMethods.GetModuleHandle(dllName);
        if (dllHandle == IntPtr.Zero)
        {
            dllHandle = NativeMethods.LoadLibrary(dllName);
            if (dllHandle == IntPtr.Zero)
                return null;

            // Keep a reference to the dll until the process\AppDomain dies
            _dllHandles.Add(dllHandle);
        }

        // Get a pointer to the DllGetClassObject function
        var dllGetClassObjectPtr = NativeMethods.GetProcAddress(dllHandle, "DllGetClassObject");
        if (dllGetClassObjectPtr == IntPtr.Zero)
        {
            _dllHandles.Remove(dllHandle);
            NativeMethods.FreeLibrary(dllHandle);
            throw new IFClassFactoryFailure("Could not get proc address from filter dll");
        }

        // Convert the function pointer to a .net delegate
        var dllGetClassObject =
            Marshal.GetDelegateForFunctionPointer<NativeMethods.DllGetClassObject>(dllGetClassObjectPtr);

        // Call the DllGetClassObject to retrieve a class factory for out Filter class
        var filterPersistGuid = new Guid(filterPersistClass);
        var classFactoryGuid = typeof(NativeMethods.IClassFactory).GUID;

        if (dllGetClassObject(ref filterPersistGuid, ref classFactoryGuid, out var unk) != 0)
            return null;

        // Cast the returned object to IClassFactory
        return unk as NativeMethods.IClassFactory;
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        for (var i = _dllHandles.Count - 1; i >= 0; i--)
        {
            var dllHandle = _dllHandles[i];
            _dllHandles.RemoveAt(i);
            NativeMethods.FreeLibrary(dllHandle);
        }

        GC.SuppressFinalize(this);
    }

    ~ComHelpers()
    {
        Dispose();
    }
    #endregion
}