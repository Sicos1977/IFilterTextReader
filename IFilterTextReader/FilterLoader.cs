//
// FilterLoader.cs
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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using IFilterTextReader.Exceptions;
using Microsoft.Win32;

namespace IFilterTextReader
{
    /// <summary>
    /// FilterLoader finds the dll and ClassID of the COM object responsible for filtering a specific file extension. 
    /// It then loads that dll, creates the appropriate COM object and returns a pointer to an IFilter instance
    /// </summary>
    internal static class FilterLoader
    {
        #region Private class CacheEntry
        /// <summary>
        /// Used to cache the <see cref="NativeMethods.IFilter">IFilter's</see>
        /// </summary>
        private class CacheEntry
        {
            /// <summary>
            /// Returns the name from the filter DLL
            /// </summary>
            public string DllName { get; }
            
            /// <summary>
            /// Returns the class name
            /// </summary>
            public string ClassName { get; }

            /// <summary>
            /// Makes this object and sets it's needed properties
            /// </summary>
            /// <param name="dllName">The name of the <see cref="NativeMethods.IFilter"/> DLL</param>
            /// <param name="className">The name of the <see cref="NativeMethods.IFilter"/> class</param>
            public CacheEntry(string dllName, string className)
            {
                DllName = dllName;
                ClassName = className;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// Contains cached IFilter lookups
        /// </summary>
        private static readonly Dictionary<string, CacheEntry> FilterCache = new Dictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The <see cref="ComHelpers"/> object
        /// </summary>
        private static readonly ComHelpers ComHelpers = new ComHelpers();
        #endregion

        #region ReadFromHKLM
        /// <summary>
        /// Read an key from the HKLM and returns it as an string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private static string ReadFromHKLM(string key)
        {
            // ReSharper disable once IntroduceOptionalParameters.Local
            return ReadFromHKLM(key, null);
        }

        /// <summary>
        /// Read an key from the HKLM and returns it as an string
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string ReadFromHKLM(string key, string value)
        {
            var registryKey = Registry.LocalMachine.OpenSubKey(key);
            if (registryKey == null)
                return null;

            using (registryKey)
                return (string) registryKey.GetValue(value);
        }
        #endregion

        #region LoadAndInitIFilter
        /// <summary>
        /// Returns an IFilter for the given <paramref name="stream"/>
        /// when there is no filter available
        /// </summary>
        /// <param name="stream">An <see cref="Stream"/></param>
        /// <param name="extension">The file extension</param>
        /// <param name="disableEmbeddedContent">When set to <c>true</c> embedded content is NOT read</param>
        /// <param name="fileName">The name of the file</param>
        /// <param name="readIntoMemory">When set to <c>true</c> the <paramref name="stream"/> is completely read 
        /// into memory first before the iFilters starts to read chunks, when set to <c>false</c> the iFilter reads
        /// directly from the <paramref name="stream"/> and advances reading when the chunks are returned. 
        /// Default set to <c>false</c></param>
        /// <returns><see cref="NativeMethods.IFilter"/> or null when no IFilter DLL is</returns>
        public static NativeMethods.IFilter LoadAndInitIFilter(Stream stream, 
                                                               string extension,
                                                               bool disableEmbeddedContent,
                                                               string fileName = "",
                                                               bool readIntoMemory = false)
        {
            // Find the dll and ClassID
            GetFilterDllAndClass(extension, out var dllName, out var filterPersistClass);
            
            var iFilter = LoadFilterFromDll(dllName, filterPersistClass);

            if (iFilter == null)
                return null;

            var iFlags = NativeMethods.IFILTER_INIT.CANON_HYPHENS |
                         NativeMethods.IFILTER_INIT.CANON_PARAGRAPHS |
                         NativeMethods.IFILTER_INIT.CANON_SPACES |
                         NativeMethods.IFILTER_INIT.APPLY_INDEX_ATTRIBUTES |
                         NativeMethods.IFILTER_INIT.APPLY_CRAWL_ATTRIBUTES |
                         NativeMethods.IFILTER_INIT.APPLY_OTHER_ATTRIBUTES |
                         NativeMethods.IFILTER_INIT.HARD_LINE_BREAKS |
                         NativeMethods.IFILTER_INIT.FILTER_OWNED_VALUE_OK |
                         NativeMethods.IFILTER_INIT.EMIT_FORMATTING;

            if (disableEmbeddedContent)
                iFlags |= NativeMethods.IFILTER_INIT.DISABLE_EMBEDDED;

            // ReSharper disable once SuspiciousTypeConversion.Global

            // IPersistStream is assumed on 64 bits systems
            if (iFilter is NativeMethods.IPersistStream iPersistStream)
            {
                // Create a COM stream
                IStream comStream;

                if (readIntoMemory)
                {
                    // Copy the content to global memory
                    var buffer = new byte[stream.Length];
                    // ReSharper disable once MustUseReturnValue
                    stream.Read(buffer, 0, buffer.Length);
                    var nativePtr = Marshal.AllocHGlobal(buffer.Length);
                    Marshal.Copy(buffer, 0, nativePtr, buffer.Length);
                    NativeMethods.CreateStreamOnHGlobal(nativePtr, true, out comStream);
                }
                else
                    comStream = new IStreamWrapper(stream);

                try
                {
                    iPersistStream.Load(comStream);

                    if (iFilter.Init(iFlags, 0, IntPtr.Zero, out _) == NativeMethods.IFilterReturnCode.S_OK)
                        return iFilter;
                }
                catch (Exception exception)
                {
                    if (string.IsNullOrWhiteSpace(fileName))
                    {
                        Marshal.ReleaseComObject(iFilter);
                        throw new IFOldFilterFormat("An error occurred while trying to load a stream with the IPersistStream interface", exception);
                    }
                }
                finally
                {
                    if (comStream != null && Marshal.IsComObject(comStream))
                        Marshal.ReleaseComObject(comStream);
                }
            }
            
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Marshal.ReleaseComObject(iFilter);
                throw new IFOldFilterFormat("The IFilter does not support the IPersistStream interface, supply a filename to use the IFilter");
            }

            // If we get here we probably are using an old IFilter so try to load it the old way
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (iFilter is IPersistFile persistFile)
            {
                try
                {
                    persistFile.Load(fileName, 0);
                }
                catch (Exception)
                {
                    Marshal.ReleaseComObject(iFilter);
                    throw new IFUnknownFormat($"The file '{fileName}' has an unknown format");
                }

                if (iFilter.Init(iFlags, 0, IntPtr.Zero, out _) == NativeMethods.IFilterReturnCode.S_OK)
                    return iFilter;
            }

            Marshal.ReleaseComObject(iFilter);
            return null;
        }

        /// <summary>
        /// Clears all cached data and frees loaded filter DLLs.
        /// Can only be called when there are no active FilterReader instances exist, e.g. on shutdown.
        /// </summary>
        public static void Clear()
        {
            lock (FilterCache)
            {
                FilterCache.Clear();
                ComHelpers.Dispose();
            }
        }
        #endregion

        #region LoadFilterFromDll
        /// <summary>
        /// Returns the filter interface from the given filter dll
        /// </summary>
        /// <param name="dllName"></param>
        /// <param name="filterPersistClass"></param>
        /// <returns></returns>
        private static NativeMethods.IFilter LoadFilterFromDll(string dllName, string filterPersistClass)
        {
            try
            {
                // Get a classFactory for our classID
                var classFactory = ComHelpers.GetClassFactory(dllName, filterPersistClass);
                if (classFactory == null)
                    return null;

                // And create an IFilter instance using that class factory
                var filterGuid = typeof(NativeMethods.IFilter).GUID;
                classFactory.CreateInstance(null, ref filterGuid, out var ppunk);
//              Marshal.ReleaseComObject(classFactory);
                return ppunk as NativeMethods.IFilter;
            }
            catch (Exception exception)
            {
                throw new Exception($"DLL name '{dllName}'{Environment.NewLine}, class {filterPersistClass}'", exception);
            }
        }
        #endregion

        #region GetFilterDll
        /// <summary>
        /// Loads the IFilter for the given <paramref name="extension"/>
        /// </summary>
        /// <param name="extension">The file extension</param>
        /// <param name="dllName"></param>
        /// <param name="filterPersistClass"></param>
        /// <returns>True when an <see cref="NativeMethods.IFilter"/> dll has been loaded for the given <paramref name="extension"/></returns>
        /// <exception cref="IFFilterNotFound">Raised when no IFilter dll could be found for the given <paramref name="extension"/></exception>
        private static void GetFilterDllAndClass(
            string extension, 
            out string dllName, 
            out string filterPersistClass)
        {
            // Try to get the filter from the cache
            lock (FilterCache)
            {
                if (GetFilterDllAndClassFromCache(extension, out dllName, out filterPersistClass)) return;

                var persistentHandlerClass = GetPersistentHandlerClass(extension, true);

                if (persistentHandlerClass != null)
                    if (!GetFilterDllAndClassFromPersistentHandler(persistentHandlerClass, out dllName, out filterPersistClass))
                        throw new IFFilterNotFound($"Could not find a {(Environment.Is64BitProcess ? "64" : "32")} bits IFilter dll for a file with an '{extension}' extension");

                FilterCache.Add(extension, new CacheEntry(dllName, filterPersistClass));
            }
        }

        /// <summary>
        /// Returns true when an filter dll has been found based on the persistent handler class
        /// </summary>
        /// <param name="persistentHandlerClass"></param>
        /// <param name="dllName">The filter dll</param>
        /// <param name="filterPersistClass">The filter persist class</param>
        /// <returns></returns>
        private static bool GetFilterDllAndClassFromPersistentHandler(
            string persistentHandlerClass, 
            out string dllName,
            out string filterPersistClass)
        {
            dllName = null;

            // Read the CLASS ID of the IFilter persistent handler
            filterPersistClass = ReadFromHKLM($@"Software\Classes\CLSID\{persistentHandlerClass}\PersistentAddinsRegistered\{{89BCB740-6119-101A-BCB7-00DD010655AF}}");

            if (string.IsNullOrEmpty(filterPersistClass))
                return false;

            // Read the dll name 
            dllName = ReadFromHKLM($@"Software\Classes\CLSID\{filterPersistClass}\InprocServer32");

            return 
                !string.IsNullOrEmpty(dllName);
        }
        #endregion

        #region GetPersistentHandlerClass
        /// <summary>
        /// Returns an persistent handler class
        /// </summary>
        /// <param name="extension">The file extension</param>
        /// <param name="searchContentType"></param>
        /// <returns></returns>
        private static string GetPersistentHandlerClass(string extension, bool searchContentType)
        {
            // Try getting the info from the file extension
            var persistentHandlerClass = GetPersistentHandlerClassFromExtension(extension);

            // Try getting the info from the document type 
            if (string.IsNullOrEmpty(persistentHandlerClass))
                persistentHandlerClass = GetPersistentHandlerClassFromDocumentType(extension);

            //Try getting the info from the Content Type
            if (searchContentType && string.IsNullOrEmpty(persistentHandlerClass))
                persistentHandlerClass = GetPersistentHandlerClassFromContentType(extension);

            return persistentHandlerClass;
        }
        #endregion

        #region GetPersistentHandlerClassFromContentType
        /// <summary>
        /// Returns an persistent handler class based on the content type of the file
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        private static string GetPersistentHandlerClassFromContentType(string extension)
        {
            var contentType = ReadFromHKLM($@"Software\Classes\{extension}", "Content Type");
            if (string.IsNullOrEmpty(contentType))
                return null;

            var contentTypeExtension = ReadFromHKLM($@"Software\Classes\MIME\Database\Content Type\{contentType}", "Extension");

            return extension.Equals(contentTypeExtension, StringComparison.CurrentCultureIgnoreCase)
                ? null
                : GetPersistentHandlerClass(contentTypeExtension, false);
        }
        #endregion

        #region GetPersistentHandlerClassFromDocumentType
        /// <summary>
        /// Returns an persistent handler class based on the document type
        /// </summary>
        /// <param name="extension">The file extension</param>
        /// <returns></returns>
        private static string GetPersistentHandlerClassFromDocumentType(string extension)
        {
            // Get the DocumentType of this file extension
            var documentType = ReadFromHKLM($@"Software\Classes\{extension}");
            if (string.IsNullOrEmpty(documentType))
                return null;

            // Get the Class ID for this document type
            var docClass = ReadFromHKLM($@"Software\Classes\{documentType}\CLSID");
           
            // Now get the PersistentHandler for that Class ID
            return string.IsNullOrEmpty(documentType)
                ? null
                : ReadFromHKLM($@"Software\Classes\CLSID\{docClass}\PersistentHandler");
        }
        #endregion

        #region GetPersistentHandlerClassFromExtension
        /// <summary>
        /// Returns an persistent handler class based on the extension of the file
        /// </summary>
        /// <param name="extension">The file extension</param>
        /// <returns></returns>
        private static string GetPersistentHandlerClassFromExtension(string extension)
        {
            return ReadFromHKLM($@"Software\Classes\{extension}\PersistentHandler");
        }
        #endregion

        #region GetFilterDllAndClassFromCache
        /// <summary>
        /// Checks if the DLL lookup is already in the cache and if so returns the cached entry
        /// </summary>
        /// <param name="extension">The file extension</param>
        /// <param name="dllName">The name of the <see cref="NativeMethods.IFilter"/> DLL</param>
        /// <param name="filterPersistClass">The class of the <see cref="NativeMethods.IFilter"/> DLL</param>
        /// <returns>True when the cached entry has been found, otherwise false</returns>
        private static bool GetFilterDllAndClassFromCache(string extension, 
                                                          out string dllName, 
                                                          out string filterPersistClass)
        {
            if (FilterCache.TryGetValue(extension, out var cacheEntry))
            {
                dllName = cacheEntry.DllName;
                filterPersistClass = cacheEntry.ClassName;
                return true;
            }

            dllName = null;
            filterPersistClass = null;
            return false;
        }
        #endregion
    }
}
