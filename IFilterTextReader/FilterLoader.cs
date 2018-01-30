using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using IFilterTextReader.Exceptions;
using Microsoft.Win32;

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
            public string DllName { get; private set; }
            public string ClassName { get; private set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="dllName">The name of the <see cref="NativeMethods.IFilter"/> DL</param>
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
        private readonly static Dictionary<string, CacheEntry> FilterCache = new Dictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// The <see cref="ComHelpers"/> object
        /// </summary>
        private readonly static ComHelpers ComHelpers = new ComHelpers();
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
            string dllName, filterPersistClass;

            // Find the dll and ClassID
            GetFilterDllAndClass(extension, out dllName, out filterPersistClass);
            
            var iFilter = LoadFilterFromDll(dllName, filterPersistClass);

            if (iFilter == null)
                return null;

            var iflags = NativeMethods.IFILTER_INIT.CANON_HYPHENS |
                         NativeMethods.IFILTER_INIT.CANON_PARAGRAPHS |
                         NativeMethods.IFILTER_INIT.CANON_SPACES |
                         NativeMethods.IFILTER_INIT.APPLY_INDEX_ATTRIBUTES |
                         NativeMethods.IFILTER_INIT.APPLY_CRAWL_ATTRIBUTES |
                         NativeMethods.IFILTER_INIT.APPLY_OTHER_ATTRIBUTES |
                         NativeMethods.IFILTER_INIT.HARD_LINE_BREAKS |
                         NativeMethods.IFILTER_INIT.FILTER_OWNED_VALUE_OK |
                         NativeMethods.IFILTER_INIT.EMIT_FORMATTING;

            if (disableEmbeddedContent)
                iflags = iflags | NativeMethods.IFILTER_INIT.DISABLE_EMBEDDED;

            // ReSharper disable once SuspiciousTypeConversion.Global
            var iPersistStream = iFilter as NativeMethods.IPersistStream;

            // IPersistStream is asumed on 64 bits systems
            if (iPersistStream != null)
            {
                // Create a COM stream
                IStream comStream;

                if (readIntoMemory)
                {
                    // Copy the content to global memory
                    var buffer = new byte[stream.Length];
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

                    NativeMethods.IFILTER_FLAGS flags;
                    if (iFilter.Init(iflags, 0, IntPtr.Zero, out flags) == NativeMethods.IFilterReturnCode.S_OK)
                        return iFilter;
                }
                catch (Exception exception)
                {
                    throw new IFOldFilterFormat("An error occured while trying to load a stream with the IPersistStream interface", exception);
                }
            }
            
            if (string.IsNullOrWhiteSpace(fileName))
                throw new IFOldFilterFormat("The IFilter does not support the IPersistStream interface, supply a filename to use the IFilter");

            // If we get here we probably are using an old IFilter so try to load it the old way
            // ReSharper disable once SuspiciousTypeConversion.Global
            var persistFile = iFilter as IPersistFile;
            if (persistFile != null)
            {
                persistFile.Load(fileName, 0);
                NativeMethods.IFILTER_FLAGS flags;
                if (iFilter.Init(iflags, 0, IntPtr.Zero, out flags) == NativeMethods.IFilterReturnCode.S_OK)
                    return iFilter;
            }
            
            // If we failed to retreive an IPersistStream or IPersistFile interface or to initialize
            // the filter, we release it and return null.
            Marshal.ReleaseComObject(iFilter);
            return null;
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
                var filterGuid = new Guid("89BCB740-6119-101A-BCB7-00DD010655AF");
                Object ppunk;
                classFactory.CreateInstance(null, ref filterGuid, out ppunk);
                return (ppunk as NativeMethods.IFilter);
            }
            catch (Exception exception)
            {
                throw new Exception("DLL name: '" + dllName + "'" + Environment.NewLine +
                                    "Class: " + filterPersistClass + "'", exception);
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
        private static void GetFilterDllAndClass(string extension, out string dllName, out string filterPersistClass)
        {
            // Try to get the filter from the cache
            lock (FilterCache)
            {
                if (GetFilterDllAndClassFromCache(extension, out dllName, out filterPersistClass)) return;

                var persistentHandlerClass = GetPersistentHandlerClass(extension, true);

                if (persistentHandlerClass != null)
                    if (
                        !GetFilterDllAndClassFromPersistentHandler(persistentHandlerClass, out dllName,
                            out filterPersistClass))
                        throw new IFFilterNotFound("Could not find a " +
                                                   (Environment.Is64BitProcess ? "64" : "32") +
                                                   " bits IFilter dll for a file with an '" + extension +
                                                   "' extension");

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
        private static bool GetFilterDllAndClassFromPersistentHandler(string persistentHandlerClass, out string dllName,
            out string filterPersistClass)
        {
            dllName = null;

            // Read the CLASS ID of the IFilter persistent handler
            filterPersistClass = ReadFromHKLM(@"Software\Classes\CLSID\" + persistentHandlerClass +
                                                 @"\PersistentAddinsRegistered\{89BCB740-6119-101A-BCB7-00DD010655AF}");

            if (string.IsNullOrEmpty(filterPersistClass))
                return false;

            // Read the dll name 
            dllName = ReadFromHKLM(@"Software\Classes\CLSID\" + filterPersistClass + @"\InprocServer32");
            return (!string.IsNullOrEmpty(dllName));
        }
        #endregion

        #region GetPersistentHandlerClass
        /// <summary>
        /// Returns an persistant handler class
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
        /// Returns an persistant handler class based on the contenttype of the file
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        private static string GetPersistentHandlerClassFromContentType(string extension)
        {
            var contentType = ReadFromHKLM(@"Software\Classes\" + extension, "Content Type");
            if (string.IsNullOrEmpty(contentType))
                return null;

            var contentTypeExtension = ReadFromHKLM(@"Software\Classes\MIME\Database\Content Type\" + contentType, "Extension");

            return extension.Equals(contentTypeExtension, StringComparison.CurrentCultureIgnoreCase)
                ? null
                : GetPersistentHandlerClass(contentTypeExtension, false);
        }
        #endregion

        #region GetPersistentHandlerClassFromDocumentType
        /// <summary>
        /// Returns an persistant handler class based on the document type
        /// </summary>
        /// <param name="extension">The file extension</param>
        /// <returns></returns>
        private static string GetPersistentHandlerClassFromDocumentType(string extension)
        {
            // Get the DocumentType of this file extension
            var documentType = ReadFromHKLM(@"Software\Classes\" + extension);
            if (String.IsNullOrEmpty(documentType))
                return null;

            // Get the Class ID for this document type
            var docClass = ReadFromHKLM(@"Software\Classes\" + documentType + @"\CLSID");
           
            // Now get the PersistentHandler for that Class ID
            return string.IsNullOrEmpty(documentType)
                ? null
                : ReadFromHKLM(@"Software\Classes\CLSID\" + docClass + @"\PersistentHandler");
        }
        #endregion

        #region GetPersistentHandlerClassFromExtension
        /// <summary>
        /// Returns an persistant handler class based on the extension of the file
        /// </summary>
        /// <param name="extension">The file extension</param>
        /// <returns></returns>
        private static string GetPersistentHandlerClassFromExtension(string extension)
        {
            return ReadFromHKLM(@"Software\Classes\" + extension + @"\PersistentHandler");
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

            CacheEntry cacheEntry;
            if (FilterCache.TryGetValue(extension, out cacheEntry))
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
