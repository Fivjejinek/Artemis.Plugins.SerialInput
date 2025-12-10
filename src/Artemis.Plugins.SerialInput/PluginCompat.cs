using System;
using System.Linq;
using System.Reflection;
using Artemis.Core;

namespace Artemis.Plugins.SerialInput
{
    internal static class PluginCompat
    {
        // Try to get a setting value of type T from the plugin, falling back to defaultValue.
        public static T GetSettingValue<T>(Plugin plugin, string name, T defaultValue)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));

            Type pluginType = plugin.GetType();

            // 1) Try direct method: plugin.GetSetting<T>(name, default)
            var method = pluginType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                   .FirstOrDefault(m => m.Name == "GetSetting" && m.IsGenericMethod);
            if (method != null)
            {
                try
                {
                    var generic = method.MakeGenericMethod(typeof(T));
                    var result = generic.Invoke(plugin, new object[] { name, defaultValue });
                    if (result is T t) return t;
                }
                catch { /* ignore and continue */ }
            }

            // 2) Try property "Configuration" or "Settings" that has GetSetting method
            foreach (var propName in new[] { "Configuration", "Settings", "ConfigurationService" })
            {
                var prop = pluginType.GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (prop != null)
                {
                    var configObj = prop.GetValue(plugin);
                    if (configObj != null)
                    {
                        var cfgType = configObj.GetType();
                        var cfgMethod = cfgType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                               .FirstOrDefault(m => m.Name == "GetSetting" && m.IsGenericMethod);
                        if (cfgMethod != null)
                        {
                            try
                            {
                                var generic = cfgMethod.MakeGenericMethod(typeof(T));
                                var result = generic.Invoke(configObj, new object[] { name, defaultValue });
                                if (result is T t) return t;
                            }
                            catch { /* ignore and continue */ }
                        }

                        // Some versions expose GetSetting non-generic returning a wrapper; try common method signatures
                        var nonGeneric = cfgType.GetMethod("GetSetting", new[] { typeof(string), typeof(object) });
                        if (nonGeneric != null)
                        {
                            try
                            {
                                var result = nonGeneric.Invoke(configObj, new object[] { name, defaultValue });
                                if (result is T t) return t;
                            }
                            catch { /* ignore */ }
                        }
                    }
                }
            }

            // 3) Try plugin.CreateSetting or plugin.CreateSetting<T>
            var createMethod = pluginType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                         .FirstOrDefault(m => m.Name == "CreateSetting" && m.IsGenericMethod);
            if (createMethod != null)
            {
                try
                {
                    var generic = createMethod.MakeGenericMethod(typeof(T));
                    var settingObj = generic.Invoke(plugin, new object[] { name, defaultValue });
                    // Many CreateSetting returns a PluginSetting<T> with Value property
                    if (settingObj != null)
                    {
                        var valProp = settingObj.GetType().GetProperty("Value");
                        if (valProp != null)
                        {
                            var val = valProp.GetValue(settingObj);
                            if (val is T t) return t;
                        }
                    }
                }
                catch { /* ignore */ }
            }

            // 4) Last resort: try to find a field/property on plugin named like the setting
            var fallbackProp = pluginType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fallbackProp != null && fallbackProp.PropertyType == typeof(T))
            {
                var val = fallbackProp.GetValue(plugin);
                if (val is T t) return t;
            }

            return defaultValue;
        }

        // Try to register a module type on the plugin using common method names.
        public static void RegisterModule(Plugin plugin, Type moduleType)
        {
            if (plugin == null) throw new ArgumentNullException(nameof(plugin));
            if (moduleType == null) throw new ArgumentNullException(nameof(moduleType));

            Type pluginType = plugin.GetType();

            // 1) Try plugin.Register<T>()
            var registerGeneric = pluginType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                            .FirstOrDefault(m => m.Name == "Register" && m.IsGenericMethod);
            if (registerGeneric != null)
            {
                try
                {
                    var generic = registerGeneric.MakeGenericMethod(moduleType);
                    generic.Invoke(plugin, Array.Empty<object>());
                    return;
                }
                catch { /* ignore and continue */ }
            }

            // 2) Try plugin.RegisterModule<T>()
            var regModuleGeneric = pluginType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                             .FirstOrDefault(m => m.Name == "RegisterModule" && m.IsGenericMethod);
            if (regModuleGeneric != null)
            {
                try
                {
                    var generic = regModuleGeneric.MakeGenericMethod(moduleType);
                    generic.Invoke(plugin, Array.Empty<object>());
                    return;
                }
                catch { /* ignore */ }
            }

            // 3) Try plugin.RegisterModule(Type)
            var regModuleNonGeneric = pluginType.GetMethod("RegisterModule", new[] { typeof(Type) });
            if (regModuleNonGeneric != null)
            {
                try
                {
                    regModuleNonGeneric.Invoke(plugin, new object[] { moduleType });
                    return;
                }
                catch { /* ignore */ }
            }

            // 4) Try plugin.Modules.RegisterModule(Type)
            var modulesProp = pluginType.GetProperty("Modules", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (modulesProp != null)
            {
                var modulesObj = modulesProp.GetValue(plugin);
                if (modulesObj != null)
                {
                    var modulesType = modulesObj.GetType();
                    var modulesReg = modulesType.GetMethod("RegisterModule", new[] { typeof(Type) });
                    if (modulesReg != null)
                    {
                        try
                        {
                            modulesReg.Invoke(modulesObj, new object[] { moduleType });
                            return;
                        }
                        catch { /* ignore */ }
                    }

                    // generic Modules.RegisterModule<T>()
                    var modulesRegGen = modulesType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                                   .FirstOrDefault(m => m.Name == "RegisterModule" && m.IsGenericMethod);
                    if (modulesRegGen != null)
                    {
                        try
                        {
                            var generic = modulesRegGen.MakeGenericMethod(moduleType);
                            generic.Invoke(modulesObj, Array.Empty<object>());
                            return;
                        }
                        catch { /* ignore */ }
                    }
                }
            }

            throw new InvalidOperationException("Could not find a supported RegisterModule/Register API on the Plugin instance. Inspect the Artemis SDK version and adapt accordingly.");
        }
    }
}
