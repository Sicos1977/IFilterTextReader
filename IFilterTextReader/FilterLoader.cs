using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Win32;

namespace Email2Storage.Modules.Readers.IFilterTextReader
{
    /// <summary>
    /// FilterLoader finds the dll and ClassID of the COM object responsible  
    /// for filtering a specific file extension. 
    /// It then loads that dll, creates the appropriate COM object and returns 
    /// a pointer to an IFilter instance
    /// </summary>
    internal static class FilterLoader
    {
        #region Private class CacheEntry
        private class CacheEntry
        {
            public string DllName { get; private set; }
            public string ClassName { get; private set; }

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
        private static readonly Dictionary<string, CacheEntry> FilterCache = new Dictionary<string, CacheEntry>();

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
        /// <returns></returns>
        private static string ReadFromHKLM(string key, string value)
        {
            var registryKey = Registry.LocalMachine.OpenSubKey(key);
            if (registryKey == null)
                return null;

            using (registryKey)
            {
                return (string) registryKey.GetValue(value);
            }
        }
        #endregion

        #region LoadAndInitFilter
        /// <summary>
        /// Returns an  IFilter implementation for a file type or throws an <see cref="FilterException"/>
        /// when there is no filter available
        /// </summary>
        /// <param name="ext">The extension of the file</param>
        /// <returns></returns>
        private static NativeMethods.IFilter LoadIFilter(string ext)
        {
            string dllName, filterPersistClass;

            // Find the dll and ClassID
            return GetFilterDllAndClass(ext, out dllName, out filterPersistClass)
                ? LoadFilterFromDll(dllName, filterPersistClass)
                : null;
        }

        /// <summary>
        /// Returns an IFilter for the given <see cref="fileName"/> or raises an <see cref="FilterException"/> 
        /// when there is no filter available
        /// </summary>
        /// <param name="fileName">The filename</param>
        /// <returns></returns>
        public static NativeMethods.IFilter LoadAndInitIFilter(string fileName)
        {
            return LoadAndInitIFilter(fileName, Path.GetExtension(fileName));
        }

        /// <summary>
        /// Returns an IFilter for the given <see cref="fileName"/> or raises an <see cref="FilterException"/> 
        /// when there is no filter available
        /// </summary>
        /// <param name="fileName">The filename</param>
        /// <param name="extension">The file extension</param>
        /// <returns></returns>
        internal static NativeMethods.IFilter LoadAndInitIFilter(string fileName, string extension)
        {
            var filter = LoadIFilter(extension);
            if (filter == null)
                return null;

            var persistFile = (filter as IPersistFile);
            if (persistFile != null)
            {
                persistFile.Load(fileName, 0);
                const NativeMethods.IFILTER_INIT iflags = NativeMethods.IFILTER_INIT.CANON_HYPHENS |
                                                          NativeMethods.IFILTER_INIT.CANON_PARAGRAPHS |
                                                          NativeMethods.IFILTER_INIT.CANON_SPACES |
                                                          NativeMethods.IFILTER_INIT.APPLY_INDEX_ATTRIBUTES |
                                                          NativeMethods.IFILTER_INIT.HARD_LINE_BREAKS |
                                                          NativeMethods.IFILTER_INIT.FILTER_OWNED_VALUE_OK;

                // ReSharper disable once SuspiciousTypeConversion.Global
                var iPersistStream = filter as NativeMethods.IPersistStream;
                if (iPersistStream != null)
                    using (Stream fileStream = new FileStream(fileName, FileMode.Open))
                    {
                        var streamWrapper = new StreamWrapper(fileStream);
                        iPersistStream.Load(streamWrapper);

                        NativeMethods.IFILTER_FLAGS flags;
                        if (filter.Init(iflags, 0, IntPtr.Zero, out flags) == NativeMethods.IFilterReturnCode.S_OK)
                            return filter;
                    }
            }

            // If we failed to retreive an IPersistFile interface or to initialize 
            // the filter, we release it and return null.
            Marshal.ReleaseComObject(filter);
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
        #endregion

        #region GetFilterDll
        /// <summary>
        /// Returns the filter dll based on the extension or throws an <see cref="FilterException"/> when not found
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="dllName"></param>
        /// <param name="filterPersistClass"></param>
        /// <returns></returns>
        private static bool GetFilterDllAndClass(string extension, out string dllName, out string filterPersistClass)
        {
            // Try to get the filter from the cache
            if (GetFilterDllAndClassFromCache(extension, out dllName, out filterPersistClass))
                return (dllName != null && filterPersistClass != null);
            
            var persistentHandlerClass = GetPersistentHandlerClass(extension, true);
            
            if (persistentHandlerClass != null)
                if (!GetFilterDllAndClassFromPersistentHandler(persistentHandlerClass, out dllName, out filterPersistClass))
                    throw new FilterException("Could not find an IFilter dll for a file with an '" + extension + "' extension");

            FilterCache.Add(extension.ToUpperInvariant(), new CacheEntry(dllName, filterPersistClass));
            return (dllName != null && filterPersistClass != null);
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

            if (String.IsNullOrEmpty(filterPersistClass))
                return false;

            // Read the dll name 
            dllName = ReadFromHKLM(@"Software\Classes\CLSID\" + filterPersistClass + @"\InprocServer32");
            return (!String.IsNullOrEmpty(dllName));
        }
        #endregion

        #region GetPersistentHandlerClass
        /// <summary>
        /// Returns an persistant handler class
        /// </summary>
        /// <param name="ext"></param>
        /// <param name="searchContentType"></param>
        /// <returns></returns>
        private static string GetPersistentHandlerClass(string ext, bool searchContentType)
        {
            // Try getting the info from the file extension
            var persistentHandlerClass = GetPersistentHandlerClassFromExtension(ext);

            // Try getting the info from the document type 
            if (String.IsNullOrEmpty(persistentHandlerClass))
                persistentHandlerClass = GetPersistentHandlerClassFromDocumentType(ext);

            //Try getting the info from the Content Type
            if (searchContentType && String.IsNullOrEmpty(persistentHandlerClass))
                persistentHandlerClass = GetPersistentHandlerClassFromContentType(ext);

            return persistentHandlerClass;
        }
        #endregion

        #region GetPersistentHandlerClassFromContentType
        /// <summary>
        /// Returns an persistant handler class based on the contenttype of the file
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        private static string GetPersistentHandlerClassFromContentType(string ext)
        {
            var contentType = ReadFromHKLM(@"Software\Classes\" + ext, "Content Type");
            if (String.IsNullOrEmpty(contentType))
                return null;

            var contentTypeExtension = ReadFromHKLM(@"Software\Classes\MIME\Database\Content Type\" + contentType, "Extension");

            return ext.Equals(contentTypeExtension, StringComparison.CurrentCultureIgnoreCase)
                ? null
                : GetPersistentHandlerClass(contentTypeExtension, false);
        }
        #endregion

        #region GetPersistentHandlerClassFromDocumentType
        /// <summary>
        /// Returns an persistant handler class based on the document type
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        private static string GetPersistentHandlerClassFromDocumentType(string ext)
        {
            // Get the DocumentType of this file extension
            var docType = ReadFromHKLM(@"Software\Classes\" + ext);
            if (String.IsNullOrEmpty(docType))
                return null;

            // Get the Class ID for this document type
            var docClass = ReadFromHKLM(@"Software\Classes\" + docType + @"\CLSID");
            return String.IsNullOrEmpty(docType)
                ? null
                : ReadFromHKLM(@"Software\Classes\CLSID\" + docClass + @"\PersistentHandler");
                // Now get the PersistentHandler for that Class ID
        }
        #endregion

        #region GetPersistentHandlerClassFromExtension
        /// <summary>
        /// Returns an persistant handler class based on the extension of the file
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        private static string GetPersistentHandlerClassFromExtension(string ext)
        {
            return ReadFromHKLM(@"Software\Classes\" + ext + @"\PersistentHandler");
        }
        #endregion

        #region GetFilterDllAndClassFromCache
        /// <summary>
        /// Checks if the DLL lookup is already in the cache and if so returns the cached entry
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="dllName"></param>
        /// <param name="filterPersistClass"></param>
        /// <returns></returns>
        private static bool GetFilterDllAndClassFromCache(string extension, out string dllName, out string filterPersistClass)
        {
            var ext = extension.ToUpperInvariant();

            CacheEntry cacheEntry;
            if (FilterCache.TryGetValue(ext, out cacheEntry))
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
