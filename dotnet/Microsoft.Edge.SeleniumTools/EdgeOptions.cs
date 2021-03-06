﻿// <copyright file="EdgeOptions.cs" company="WebDriver Committers">
//
// Portions Copyright Microsoft 2020
// Licensed under the Apache License, Version 2.0
//
// Licensed to the Software Freedom Conservancy (SFC) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The SFC licenses this file
// to you under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace Microsoft.Edge.SeleniumTools
{
    /// <summary>
    /// Specifies the behavior of waiting for page loads in the Edge driver.
    /// </summary>
    public enum EdgePageLoadStrategy
    {
        /// <summary>
        /// Indicates the behavior is not set.
        /// </summary>
        Default,

        /// <summary>
        /// Waits for pages to load and ready state to be 'complete'.
        /// </summary>
        Normal,

        /// <summary>
        /// Waits for pages to load and for ready state to be 'interactive' or 'complete'.
        /// </summary>
        Eager,

        /// <summary>
        /// Does not wait for pages to load, returning immediately.
        /// </summary>
        None
    }

    /// <summary>
    /// Class to manage options specific to <see cref="EdgeDriver"/>
    /// </summary>
    /// <remarks>
    /// Used with msedgedriver.exe.
    /// </remarks>
    /// <example>
    /// <code>
    /// EdgeOptions options = new EdgeOptions();
    /// options.AddExtensions("\path\to\extension.crx");
    /// options.BinaryLocation = "\path\to\msedge";
    /// </code>
    /// <para></para>
    /// <para>For use with EdgeDriver:</para>
    /// <para></para>
    /// <code>
    /// EdgeDriver driver = new EdgeDriver(options);
    /// </code>
    /// <para></para>
    /// <para>For use with RemoteWebDriver:</para>
    /// <para></para>
    /// <code>
    /// RemoteWebDriver driver = new RemoteWebDriver(new Uri("http://localhost:4444/wd/hub"), options.ToCapabilities());
    /// </code>
    /// </example>
    public class EdgeOptions : DriverOptions
    {
        /// <summary>
        /// Gets the name of the capability used to store Edge options in
        /// a <see cref="DesiredCapabilities"/> object.
        /// </summary>
        public static readonly string Capability = "ms:edgeOptions";

        private const string DefaultBrowserNameValue = "MicrosoftEdge";
        private const string WebViewBrowserNameValue = "webview2";

        // Engine switching
        private const string UseChromiumCapability = "ms:edgeChromium";
        private bool useChromium = false;

        // Edge Legacy options
        private const string UseInPrivateBrowsingCapability = "ms:inPrivate";
        private const string ExtensionPathsCapability = "ms:extensionPaths";
        private const string StartPageCapability = "ms:startPage";

        private EdgePageLoadStrategy pageLoadStrategy = EdgePageLoadStrategy.Default;

        private bool useInPrivateBrowsing;
        private string startPage;
        private List<string> extensionPaths = new List<string>();

        // Edge Chromium options
        private const string ArgumentsEdgeOption = "args";
        private const string BinaryEdgeOption = "binary";
        private const string ExtensionsEdgeOption = "extensions";
        private const string LocalStateEdgeOption = "localState";
        private const string PreferencesEdgeOption = "prefs";
        private const string DetachEdgeOption = "detach";
        private const string DebuggerAddressEdgeOption = "debuggerAddress";
        private const string ExcludeSwitchesEdgeOption = "excludeSwitches";
        private const string MinidumpPathEdgeOption = "minidumpPath";
        private const string MobileEmulationEdgeOption = "mobileEmulation";
        private const string PerformanceLoggingPreferencesEdgeOption = "perfLoggingPrefs";
        private const string WindowTypesEdgeOption = "windowTypes";
        private const string UseSpecCompliantProtocolOption = "w3c";

        private bool leaveBrowserRunning;
        private bool useSpecCompliantProtocol;
        private string binaryLocation;
        private string debuggerAddress;
        private string minidumpPath;
        private List<string> arguments = new List<string>();
        private List<string> extensionFiles = new List<string>();
        private List<string> encodedExtensions = new List<string>();
        private List<string> excludedSwitches = new List<string>();
        private List<string> windowTypes = new List<string>();
        private Dictionary<string, object> additionalCapabilities = new Dictionary<string, object>();
        private Dictionary<string, object> additionalEdgeOptions = new Dictionary<string, object>();
        private Dictionary<string, object> userProfilePreferences;
        private Dictionary<string, object> localStatePreferences;

        private string mobileEmulationDeviceName;
        private EdgeMobileEmulationDeviceSettings mobileEmulationDeviceSettings;
        private EdgePerformanceLoggingPreferences perfLoggingPreferences;

        public EdgeOptions() : base()
        {
            this.BrowserName = DefaultBrowserNameValue;
            this.AddKnownCapabilityName(EdgeOptions.Capability, "current EdgeOptions class instance");
            this.AddKnownCapabilityName(CapabilityType.LoggingPreferences, "SetLoggingPreference method");
            this.AddKnownCapabilityName(EdgeOptions.ArgumentsEdgeOption, "AddArguments method");
            this.AddKnownCapabilityName(EdgeOptions.BinaryEdgeOption, "BinaryLocation property");
            this.AddKnownCapabilityName(EdgeOptions.ExtensionsEdgeOption, "AddExtensions method");
            this.AddKnownCapabilityName(EdgeOptions.LocalStateEdgeOption, "AddLocalStatePreference method");
            this.AddKnownCapabilityName(EdgeOptions.PreferencesEdgeOption, "AddUserProfilePreference method");
            this.AddKnownCapabilityName(EdgeOptions.DetachEdgeOption, "LeaveBrowserRunning property");
            this.AddKnownCapabilityName(EdgeOptions.DebuggerAddressEdgeOption, "DebuggerAddress property");
            this.AddKnownCapabilityName(EdgeOptions.ExcludeSwitchesEdgeOption, "AddExcludedArgument property");
            this.AddKnownCapabilityName(EdgeOptions.MinidumpPathEdgeOption, "MinidumpPath property");
            this.AddKnownCapabilityName(EdgeOptions.MobileEmulationEdgeOption, "EnableMobileEmulation method");
            this.AddKnownCapabilityName(EdgeOptions.PerformanceLoggingPreferencesEdgeOption, "PerformanceLoggingPreferences property");
            this.AddKnownCapabilityName(EdgeOptions.WindowTypesEdgeOption, "AddWindowTypes method");
            this.AddKnownCapabilityName(EdgeOptions.UseSpecCompliantProtocolOption, "UseSpecCompliantProtocol property");
        }

        /// <summary>
        /// Gets or sets a value indicating whether to launch Edge Chromium. Defaults to using Edge Legacy.
        /// </summary>
        public bool UseChromium
        {
            get { return this.useChromium; }
            set { this.useChromium = value; }
        }

        /// <summary>
        /// Gets or sets whether to create a WebView session used for launching an Edge (Chromium) WebView-based app on desktop.
        /// </summary>
        public bool UseWebView
        {
            get { return this.BrowserName == WebViewBrowserNameValue; }
            set { this.BrowserName = value ? WebViewBrowserNameValue : DefaultBrowserNameValue; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the browser should be launched using
        /// InPrivate browsing.
        /// </summary>
        public bool UseInPrivateBrowsing
        {
            get { return this.useInPrivateBrowsing; }
            set { this.useInPrivateBrowsing = value; }
        }

        /// <summary>
        /// Gets or sets the URL of the page with which the browser will be navigated to on launch.
        /// </summary>
        public string StartPage
        {
            get { return this.startPage; }
            set { this.startPage = value; }
        }

        /// <summary>
        /// Gets or sets the location of the Edge browser's binary executable file.
        /// </summary>
        public string BinaryLocation
        {
            get { return this.binaryLocation; }
            set { this.binaryLocation = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Edge should be left running after the
        /// msedgedriver instance is exited. Defaults to <see langword="false"/>.
        /// </summary>
        public bool LeaveBrowserRunning
        {
            get { return this.leaveBrowserRunning; }
            set { this.leaveBrowserRunning = value; }
        }

        /// <summary>
        /// Gets the list of arguments appended to the Edge command line as a string array.
        /// </summary>
        public ReadOnlyCollection<string> Arguments
        {
            get { return this.arguments.AsReadOnly(); }
        }

        /// <summary>
        /// Gets the list of extensions to be installed as an array of base64-encoded strings.
        /// </summary>
        public ReadOnlyCollection<string> Extensions
        {
            get
            {
                List<string> allExtensions = new List<string>(this.encodedExtensions);
                foreach (string extensionFile in this.extensionFiles)
                {
                    byte[] extensionByteArray = File.ReadAllBytes(extensionFile);
                    string encodedExtension = Convert.ToBase64String(extensionByteArray);
                    allExtensions.Add(encodedExtension);
                }

                return allExtensions.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets or sets the address of a Edge debugger server to connect to.
        /// Should be of the form "{hostname|IP address}:port".
        /// </summary>
        public string DebuggerAddress
        {
            get { return this.debuggerAddress; }
            set { this.debuggerAddress = value; }
        }

        /// <summary>
        /// Gets or sets the directory in which to store minidump files.
        /// </summary>
        public string MinidumpPath
        {
            get { return this.minidumpPath; }
            set { this.minidumpPath = value; }
        }

        /// <summary>
        /// Gets or sets the performance logging preferences for the driver.
        /// </summary>
        public EdgePerformanceLoggingPreferences PerformanceLoggingPreferences
        {
            get { return this.perfLoggingPreferences; }
            set { this.perfLoggingPreferences = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="EdgeDriver"/> instance
        /// should use the legacy OSS protocol dialect or a dialect compliant with the W3C
        /// WebDriver Specification.
        /// </summary>
        public bool UseSpecCompliantProtocol
        {
            get { return this.useSpecCompliantProtocol; }
            set { this.useSpecCompliantProtocol = value; }
        }

        /// <summary>
        /// Adds a single argument to the list of arguments to be appended to the msedge.exe command line.
        /// </summary>
        /// <param name="argument">The argument to add.</param>
        public void AddArgument(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentException("argument must not be null or empty", "argument");
            }

            this.AddArguments(argument);
        }

        /// <summary>
        /// Adds arguments to be appended to the msedge.exe command line.
        /// </summary>
        /// <param name="argumentsToAdd">An array of arguments to add.</param>
        public void AddArguments(params string[] argumentsToAdd)
        {
            this.AddArguments(new List<string>(argumentsToAdd));
        }

        /// <summary>
        /// Adds arguments to be appended to the msedge.exe command line.
        /// </summary>
        /// <param name="argumentsToAdd">An <see cref="IEnumerable{T}"/> object of arguments to add.</param>
        public void AddArguments(IEnumerable<string> argumentsToAdd)
        {
            if (argumentsToAdd == null)
            {
                throw new ArgumentNullException("argumentsToAdd", "argumentsToAdd must not be null");
            }

            this.arguments.AddRange(argumentsToAdd);
        }

        /// <summary>
        /// Adds a single argument to be excluded from the list of arguments passed by default
        /// to the msedge.exe command line by msedgedriver.exe.
        /// </summary>
        /// <param name="argument">The argument to exclude.</param>
        public void AddExcludedArgument(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentException("argument must not be null or empty", "argument");
            }

            this.AddExcludedArguments(argument);
        }

        /// <summary>
        /// Adds arguments to be excluded from the list of arguments passed by default
        /// to the msedge.exe command line by msedgedriver.exe.
        /// </summary>
        /// <param name="argumentsToExclude">An array of arguments to exclude.</param>
        public void AddExcludedArguments(params string[] argumentsToExclude)
        {
            this.AddExcludedArguments(new List<string>(argumentsToExclude));
        }

        /// <summary>
        /// Adds arguments to be excluded from the list of arguments passed by default
        /// to the msedge.exe command line by msedgedriver.exe.
        /// </summary>
        /// <param name="argumentsToExclude">An <see cref="IEnumerable{T}"/> object of arguments to exclude.</param>
        public void AddExcludedArguments(IEnumerable<string> argumentsToExclude)
        {
            if (argumentsToExclude == null)
            {
                throw new ArgumentNullException("argumentsToExclude", "argumentsToExclude must not be null");
            }

            this.excludedSwitches.AddRange(argumentsToExclude);
        }

        /// <summary>
        /// Adds a path to a packed Edge extension (.crx file) to the list of extensions
        /// to be installed in the instance of Edge.
        /// </summary>
        /// <param name="pathToExtension">The full path to the extension to add.</param>
        public void AddExtension(string pathToExtension)
        {
            if (string.IsNullOrEmpty(pathToExtension))
            {
                throw new ArgumentException("pathToExtension must not be null or empty", "pathToExtension");
            }

            this.AddExtensions(pathToExtension);
        }

        /// <summary>
        /// Adds a list of paths to packed Edge extensions (.crx files) to be installed
        /// in the instance of Edge.
        /// </summary>
        /// <param name="extensions">An array of full paths to the extensions to add.</param>
        public void AddExtensions(params string[] extensions)
        {
            this.AddExtensions(new List<string>(extensions));
        }

        /// <summary>
        /// Adds a list of paths to packed Edge extensions (.crx files) to be installed
        /// in the instance of Edge.
        /// </summary>
        /// <param name="extensions">An <see cref="IEnumerable{T}"/> of full paths to the extensions to add.</param>
        public void AddExtensions(IEnumerable<string> extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException("extensions", "extensions must not be null");
            }

            foreach (string extension in extensions)
            {
                if (!File.Exists(extension))
                {
                    throw new FileNotFoundException("No extension found at the specified path", extension);
                }

                this.extensionFiles.Add(extension);
            }
        }

        /// <summary>
        /// Adds a base64-encoded string representing a Edge extension to the list of extensions
        /// to be installed in the instance of Edge.
        /// </summary>
        /// <param name="extension">A base64-encoded string representing the extension to add.</param>
        public void AddEncodedExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentException("extension must not be null or empty", "extension");
            }

            this.AddEncodedExtensions(extension);
        }

        /// <summary>
        /// Adds a list of base64-encoded strings representing Edge extensions to the list of extensions
        /// to be installed in the instance of Edge.
        /// </summary>
        /// <param name="extensions">An array of base64-encoded strings representing the extensions to add.</param>
        public void AddEncodedExtensions(params string[] extensions)
        {
            this.AddEncodedExtensions(new List<string>(extensions));
        }

        /// <summary>
        /// Adds a list of base64-encoded strings representing Edge extensions to be installed
        /// in the instance of Edge.
        /// </summary>
        /// <param name="extensions">An <see cref="IEnumerable{T}"/> of base64-encoded strings
        /// representing the extensions to add.</param>
        public void AddEncodedExtensions(IEnumerable<string> extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException("extensions", "extensions must not be null");
            }

            foreach (string extension in extensions)
            {
                // Run the extension through the base64 converter to test that the
                // string is not malformed.
                try
                {
                    Convert.FromBase64String(extension);
                }
                catch (FormatException ex)
                {
                    throw new WebDriverException("Could not properly decode the base64 string", ex);
                }

                this.encodedExtensions.Add(extension);
            }
        }

        /// <summary>
        /// Adds a path to an extension that is to be used with the Edge driver.
        /// </summary>
        /// <param name="extensionPath">The full path and file name of the extension.</param>
        public void AddExtensionPath(string extensionPath)
        {
            if (string.IsNullOrEmpty(extensionPath))
            {
                throw new ArgumentException("extensionPath must not be null or empty", "extensionPath");
            }

            this.AddExtensionPaths(extensionPath);
        }

        /// <summary>
        /// Adds a list of paths to an extensions that are to be used with the Edge driver.
        /// </summary>
        /// <param name="extensionPathsToAdd">An array of full paths with file names of extensions to add.</param>
        public void AddExtensionPaths(params string[] extensionPathsToAdd)
        {
            this.AddExtensionPaths(new List<string>(extensionPathsToAdd));
        }

        /// <summary>
        /// Adds a list of paths to an extensions that are to be used with the Edge driver.
        /// </summary>
        /// <param name="extensionPathsToAdd">An <see cref="IEnumerable{T}"/> of full paths with file names of extensions to add.</param>
        public void AddExtensionPaths(IEnumerable<string> extensionPathsToAdd)
        {
            if (extensionPathsToAdd == null)
            {
                throw new ArgumentNullException("extensionPathsToAdd", "extensionPathsToAdd must not be null");
            }

            this.extensionPaths.AddRange(extensionPathsToAdd);
        }

        /// <summary>
        /// Adds a preference for the user-specific profile or "user data directory."
        /// If the specified preference already exists, it will be overwritten.
        /// </summary>
        /// <param name="preferenceName">The name of the preference to set.</param>
        /// <param name="preferenceValue">The value of the preference to set.</param>
        public void AddUserProfilePreference(string preferenceName, object preferenceValue)
        {
            if (this.userProfilePreferences == null)
            {
                this.userProfilePreferences = new Dictionary<string, object>();
            }

            this.userProfilePreferences[preferenceName] = preferenceValue;
        }

        /// <summary>
        /// Adds a preference for the local state file in the user's data directory for Edge.
        /// If the specified preference already exists, it will be overwritten.
        /// </summary>
        /// <param name="preferenceName">The name of the preference to set.</param>
        /// <param name="preferenceValue">The value of the preference to set.</param>
        public void AddLocalStatePreference(string preferenceName, object preferenceValue)
        {
            if (this.localStatePreferences == null)
            {
                this.localStatePreferences = new Dictionary<string, object>();
            }

            this.localStatePreferences[preferenceName] = preferenceValue;
        }

        /// <summary>
        /// Allows the Edge browser to emulate a mobile device.
        /// </summary>
        /// <param name="deviceName">The name of the device to emulate. The device name must be a
        /// valid device name from the Edge DevTools Emulation panel.</param>
        /// <remarks>Specifying an invalid device name will not throw an exeption, but
        /// will generate an error in Edge when the driver starts. To unset mobile
        /// emulation, call this method with <see langword="null"/> as the argument.</remarks>
        public void EnableMobileEmulation(string deviceName)
        {
            this.mobileEmulationDeviceSettings = null;
            this.mobileEmulationDeviceName = deviceName;
        }

        /// <summary>
        /// Allows the Edge browser to emulate a mobile device.
        /// </summary>
        /// <param name="deviceSettings">The <see cref="EdgeMobileEmulationDeviceSettings"/>
        /// object containing the settings of the device to emulate.</param>
        /// <exception cref="ArgumentException">Thrown if the device settings option does
        /// not have a user agent string set.</exception>
        /// <remarks>Specifying an invalid device name will not throw an exeption, but
        /// will generate an error in Edge when the driver starts. To unset mobile
        /// emulation, call this method with <see langword="null"/> as the argument.</remarks>
        public void EnableMobileEmulation(EdgeMobileEmulationDeviceSettings deviceSettings)
        {
            this.mobileEmulationDeviceName = null;
            if (deviceSettings != null && string.IsNullOrEmpty(deviceSettings.UserAgent))
            {
                throw new ArgumentException("Device settings must include a user agent string.", "deviceSettings");
            }

            this.mobileEmulationDeviceSettings = deviceSettings;
        }

        /// <summary>
        /// Adds a type of window that will be listed in the list of window handles
        /// returned by the Edge driver.
        /// </summary>
        /// <param name="windowType">The name of the window type to add.</param>
        /// <remarks>This method can be used to allow the driver to access {webview}
        /// elements by adding "webview" as a window type.</remarks>
        public void AddWindowType(string windowType)
        {
            if (string.IsNullOrEmpty(windowType))
            {
                throw new ArgumentException("windowType must not be null or empty", "windowType");
            }

            this.AddWindowTypes(windowType);
        }

        /// <summary>
        /// Adds a list of window types that will be listed in the list of window handles
        /// returned by the Edge driver.
        /// </summary>
        /// <param name="windowTypesToAdd">An array of window types to add.</param>
        public void AddWindowTypes(params string[] windowTypesToAdd)
        {
            this.AddWindowTypes(new List<string>(windowTypesToAdd));
        }

        /// <summary>
        /// Adds a list of window types that will be listed in the list of window handles
        /// returned by the Edge driver.
        /// </summary>
        /// <param name="windowTypesToAdd">An <see cref="IEnumerable{T}"/> of window types to add.</param>
        public void AddWindowTypes(IEnumerable<string> windowTypesToAdd)
        {
            if (windowTypesToAdd == null)
            {
                throw new ArgumentNullException("windowTypesToAdd", "windowTypesToAdd must not be null");
            }

            this.windowTypes.AddRange(windowTypesToAdd);
        }

        /// <summary>
        /// Provides a means to add additional capabilities not yet added as type safe options
        /// for the Edge driver.
        /// </summary>
        /// <param name="capabilityName">The name of the capability to add.</param>
        /// <param name="capabilityValue">The value of the capability to add.</param>
        /// <exception cref="ArgumentException">
        /// thrown when attempting to add a capability for which there is already a type safe option, or
        /// when <paramref name="capabilityName"/> is <see langword="null"/> or the empty string.
        /// </exception>
        /// <remarks>Calling <see cref="AddAdditionalCapability(string, object)"/>
        /// where <paramref name="capabilityName"/> has already been added will overwrite the
        /// existing value with the new value in <paramref name="capabilityValue"/>.
        /// Also, by default, calling this method adds capabilities to the options object passed to
        /// msedgedriver.exe.</remarks>
        public override void AddAdditionalCapability(string capabilityName, object capabilityValue)
        {
            // Add the capability to the edgeOptions object by default. This is to handle
            // the 80% case where the msedgedriver team adds a new option in msedgedriver.exe
            // and the bindings have not yet had a type safe option added.
            this.AddAdditionalCapability(capabilityName, capabilityValue, false);
        }

        /// <summary>
        /// Provides a means to add additional capabilities not yet added as type safe options
        /// for the Edge driver.
        /// </summary>
        /// <param name="capabilityName">The name of the capability to add.</param>
        /// <param name="capabilityValue">The value of the capability to add.</param>
        /// <param name="isGlobalCapability">Indicates whether the capability is to be set as a global
        /// capability for the driver instead of a Edge-specific option.</param>
        /// <exception cref="ArgumentException">
        /// thrown when attempting to add a capability for which there is already a type safe option, or
        /// when <paramref name="capabilityName"/> is <see langword="null"/> or the empty string.
        /// </exception>
        /// <remarks>Calling <see cref="AddAdditionalCapability(string, object, bool)"/>
        /// where <paramref name="capabilityName"/> has already been added will overwrite the
        /// existing value with the new value in <paramref name="capabilityValue"/></remarks>
        public void AddAdditionalCapability(string capabilityName, object capabilityValue, bool isGlobalCapability)
        {
            if (this.IsKnownCapabilityName(capabilityName))
            {
                string typeSafeOptionName = this.GetTypeSafeOptionName(capabilityName);
                string message = string.Format(CultureInfo.InvariantCulture, "There is already an option for the {0} capability. Please use the {1} instead.", capabilityName, typeSafeOptionName);

                throw new ArgumentException(message, "capabilityName");
            }

            if (string.IsNullOrEmpty(capabilityName))
            {
                throw new ArgumentException("Capability name may not be null an empty string.", "capabilityName");
            }

            if (isGlobalCapability)
            {
                this.additionalCapabilities[capabilityName] = capabilityValue;
            }
            else
            {
                this.additionalEdgeOptions[capabilityName] = capabilityValue;
            }
        }

        /// <summary>
        /// Returns DesiredCapabilities for Edge with these options included as
        /// capabilities. This does not copy the options. Further changes will be
        /// reflected in the returned capabilities.
        /// </summary>
        /// <returns>The DesiredCapabilities for Edge with these options.</returns>
        public override ICapabilities ToCapabilities()
        {
            return this.useChromium ? ToChromiumCapabilities() : ToLegacyCapabilities();
        }

        private ICapabilities ToChromiumCapabilities()
        {
            Dictionary<string, object> edgeOptions = this.BuildEdgeOptionsDictionary();

            DesiredCapabilities capabilities = this.GenerateDesiredCapabilities(false);
            capabilities.SetCapability(EdgeOptions.UseChromiumCapability, this.useChromium);
            capabilities.SetCapability(EdgeOptions.Capability, edgeOptions);

            Dictionary<string, object> loggingPreferences = this.GenerateLoggingPreferencesDictionary();
            if (loggingPreferences != null)
            {
                capabilities.SetCapability(CapabilityType.LoggingPreferences, loggingPreferences);
            }

            foreach (KeyValuePair<string, object> pair in this.additionalCapabilities)
            {
                capabilities.SetCapability(pair.Key, pair.Value);
            }

            // AsReadOnly is an internal method so we need to use reflection to call it from here.
            Type type = capabilities.GetType();
            MethodInfo method = type.GetMethod("AsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            return method.Invoke(capabilities, new object[] { }) as ICapabilities;
        }

        private ICapabilities ToLegacyCapabilities()
        {
            DesiredCapabilities capabilities = this.GenerateDesiredCapabilities(false);
            capabilities.SetCapability(EdgeOptions.UseChromiumCapability, this.useChromium);

            if (this.useInPrivateBrowsing)
            {
                capabilities.SetCapability(UseInPrivateBrowsingCapability, true);
            }

            if (!string.IsNullOrEmpty(this.startPage))
            {
                capabilities.SetCapability(StartPageCapability, this.startPage);
            }

            if (this.extensionPaths.Count > 0)
            {
                capabilities.SetCapability(ExtensionPathsCapability, this.extensionPaths);
            }

            foreach (KeyValuePair<string, object> pair in this.additionalCapabilities)
            {
                capabilities.SetCapability(pair.Key, pair.Value);
            }

            // Should return capabilities.AsReadOnly(), and will in a future release.
            return capabilities;
        }

        private Dictionary<string, object> BuildEdgeOptionsDictionary()
        {
            Dictionary<string, object> edgeOptions = new Dictionary<string, object>();
            if (this.Arguments.Count > 0)
            {
                edgeOptions[ArgumentsEdgeOption] = this.Arguments;
            }

            if (!string.IsNullOrEmpty(this.binaryLocation))
            {
                edgeOptions[BinaryEdgeOption] = this.binaryLocation;
            }

            ReadOnlyCollection<string> extensions = this.Extensions;
            if (extensions.Count > 0)
            {
                edgeOptions[ExtensionsEdgeOption] = extensions;
            }

            if (this.localStatePreferences != null && this.localStatePreferences.Count > 0)
            {
                edgeOptions[LocalStateEdgeOption] = this.localStatePreferences;
            }

            if (this.userProfilePreferences != null && this.userProfilePreferences.Count > 0)
            {
                edgeOptions[PreferencesEdgeOption] = this.userProfilePreferences;
            }

            if (this.leaveBrowserRunning)
            {
                edgeOptions[DetachEdgeOption] = this.leaveBrowserRunning;
            }

            if (this.useSpecCompliantProtocol)
            {
                edgeOptions[UseSpecCompliantProtocolOption] = this.useSpecCompliantProtocol;
            }

            if (!string.IsNullOrEmpty(this.debuggerAddress))
            {
                edgeOptions[DebuggerAddressEdgeOption] = this.debuggerAddress;
            }

            if (this.excludedSwitches.Count > 0)
            {
                edgeOptions[ExcludeSwitchesEdgeOption] = this.excludedSwitches;
            }

            if (!string.IsNullOrEmpty(this.minidumpPath))
            {
                edgeOptions[MinidumpPathEdgeOption] = this.minidumpPath;
            }

            if (!string.IsNullOrEmpty(this.mobileEmulationDeviceName) || this.mobileEmulationDeviceSettings != null)
            {
                edgeOptions[MobileEmulationEdgeOption] = this.GenerateMobileEmulationSettingsDictionary();
            }

            if (this.perfLoggingPreferences != null)
            {
                edgeOptions[PerformanceLoggingPreferencesEdgeOption] = this.GeneratePerformanceLoggingPreferencesDictionary();
            }

            if (this.windowTypes.Count > 0)
            {
                edgeOptions[WindowTypesEdgeOption] = this.windowTypes;
            }

            foreach (KeyValuePair<string, object> pair in this.additionalEdgeOptions)
            {
                edgeOptions.Add(pair.Key, pair.Value);
            }

            return edgeOptions;
        }

        private Dictionary<string, object> GeneratePerformanceLoggingPreferencesDictionary()
        {
            Dictionary<string, object> perfLoggingPrefsDictionary = new Dictionary<string, object>();
            perfLoggingPrefsDictionary["enableNetwork"] = this.perfLoggingPreferences.IsCollectingNetworkEvents;
            perfLoggingPrefsDictionary["enablePage"] = this.perfLoggingPreferences.IsCollectingPageEvents;

            string tracingCategories = this.perfLoggingPreferences.TracingCategories;
            if (!string.IsNullOrEmpty(tracingCategories))
            {
                perfLoggingPrefsDictionary["traceCategories"] = tracingCategories;
            }

            perfLoggingPrefsDictionary["bufferUsageReportingInterval"] = Convert.ToInt64(this.perfLoggingPreferences.BufferUsageReportingInterval.TotalMilliseconds);

            return perfLoggingPrefsDictionary;
        }

        private Dictionary<string, object> GenerateMobileEmulationSettingsDictionary()
        {
            Dictionary<string, object> mobileEmulationSettings = new Dictionary<string, object>();

            if (!string.IsNullOrEmpty(this.mobileEmulationDeviceName))
            {
                mobileEmulationSettings["deviceName"] = this.mobileEmulationDeviceName;
            }
            else if (this.mobileEmulationDeviceSettings != null)
            {
                mobileEmulationSettings["userAgent"] = this.mobileEmulationDeviceSettings.UserAgent;
                Dictionary<string, object> deviceMetrics = new Dictionary<string, object>();
                deviceMetrics["width"] = this.mobileEmulationDeviceSettings.Width;
                deviceMetrics["height"] = this.mobileEmulationDeviceSettings.Height;
                deviceMetrics["pixelRatio"] = this.mobileEmulationDeviceSettings.PixelRatio;
                if (!this.mobileEmulationDeviceSettings.EnableTouchEvents)
                {
                    deviceMetrics["touch"] = this.mobileEmulationDeviceSettings.EnableTouchEvents;
                }

                mobileEmulationSettings["deviceMetrics"] = deviceMetrics;
            }

            return mobileEmulationSettings;
        }
    }
}