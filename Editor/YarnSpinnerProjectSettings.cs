namespace Yarn.Unity.Editor
{
    using System.IO;
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
#endif

    /// <summary>
    /// Basic data class of unity settings that impact Yarn Spinner.
    /// </summary>
    /// <remarks>
    /// Currently this only supports disabling the automatic reimport of Yarn Projects when locale assets change, but other settings will eventually end up here.
    /// </remarks>
    class YarnSpinnerProjectSettings
    {
        public static string YarnSpinnerProjectSettingsPath => Path.Combine("ProjectSettings", "Packages", "dev.yarnspinner", "YarnSpinnerProjectSettings.json");

        public bool autoRefreshLocalisedAssets = true;
        public bool automaticallyLinkAttributedYarnCommandsAndFunctions = true;

        // need to make it os the output can be passed in also so it can log
        internal static YarnSpinnerProjectSettings GetOrCreateSettings(string path = null, Yarn.Unity.ILogger iLogger = null)
        {
            var settingsPath = YarnSpinnerProjectSettingsPath;
            if (path != null)
            {
                settingsPath = Path.Combine(path, YarnSpinnerProjectSettingsPath);
            }
            var logger = ValidLogger(iLogger);

            YarnSpinnerProjectSettings settings = new YarnSpinnerProjectSettings();
            if (File.Exists(settingsPath))
            {
                try
                {
                    var settingsData = File.ReadAllText(settingsPath);
                    settings = YarnSpinnerProjectSettings.FromJson(settingsData, logger);

                    return settings;
                }
                catch (System.Exception e)
                {
                    logger.WriteLine($"Failed to load Yarn Spinner project settings at {settingsPath}: {e.Message}");
                }
            }
            else
            {
                logger.WriteLine($"No settings file exists at {settingsPath}, will fallback to default settings");
            }

            settings.autoRefreshLocalisedAssets = true;
            settings.automaticallyLinkAttributedYarnCommandsAndFunctions = true;
            settings.WriteSettings(path, logger);

            return settings;
        }

        private static YarnSpinnerProjectSettings FromJson(string jsonString, Yarn.Unity.ILogger iLogger = null)
        {
            var logger = ValidLogger(iLogger);
            
            YarnSpinnerProjectSettings settings = new YarnSpinnerProjectSettings();
            var jsonDict = Json.Deserialize(jsonString) as System.Collections.Generic.Dictionary<string, object>;
            if (jsonDict == null)
            {
                logger.WriteLine($"Failed to parse Yarn Spinner project settings JSON");
                return settings;
            }

            bool automaticallyLinkAttributedYarnCommandsAndFunctions = true;
            bool autoRefreshLocalisedAssets = true;
            try
            {
                automaticallyLinkAttributedYarnCommandsAndFunctions = (bool)jsonDict["automaticallyLinkAttributedYarnCommandsAndFunctions"];
                autoRefreshLocalisedAssets = (bool)jsonDict["autoRefreshLocalisedAssets"];
            }
            catch (System.Exception e)
            {
                logger.WriteLine($"Failed to parse Yarn Spinner project settings: {e.Message}");
            }

            settings.automaticallyLinkAttributedYarnCommandsAndFunctions = automaticallyLinkAttributedYarnCommandsAndFunctions;
            settings.autoRefreshLocalisedAssets = autoRefreshLocalisedAssets;

            return settings;
        }

        internal void WriteSettings(string path = null, Yarn.Unity.ILogger iLogger = null)
        {
            var logger = ValidLogger(iLogger);

            var settingsPath = YarnSpinnerProjectSettingsPath;
            if (path != null)
            {
                settingsPath = Path.Combine(path, settingsPath);
            }

            // var jsonValue = EditorJsonUtility.ToJson(this);
            var dictForm = new System.Collections.Generic.Dictionary<string, bool>();
            dictForm["automaticallyLinkAttributedYarnCommandsAndFunctions"] = this.automaticallyLinkAttributedYarnCommandsAndFunctions;
            dictForm["autoRefreshLocalisedAssets"] = this.autoRefreshLocalisedAssets;

            var jsonValue = Json.Serialize(dictForm);

            var folder = Path.GetDirectoryName(YarnSpinnerProjectSettingsPath);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            try
            {
                File.WriteAllText(YarnSpinnerProjectSettingsPath, jsonValue);
            }
            catch (System.Exception e)
            {
                logger.WriteLine($"Failed to save Yarn Spinner project settings to {YarnSpinnerProjectSettingsPath}: {e.Message}");
            }
        }

        // if the provided logger is valid just return it
        // otherwise return the default logger
        private static Yarn.Unity.ILogger ValidLogger(Yarn.Unity.ILogger iLogger)
        {
            var logger = iLogger;
            if (logger == null)
            {
#if UNITY_EDITOR
                logger = new UnityLogger();
#else
                logger = new NullLogger();
#endif
            }
            return logger;
        }
    }
}
