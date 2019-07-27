using System;
using System.Reflection;

namespace RemoteTech.Common.Utils
{
    public static class GameUtil
    {
        public static bool IsGameScenario
            =>
                HighLogic.CurrentGame != null &&
                (HighLogic.CurrentGame.Mode == Game.Modes.SCENARIO ||
                 HighLogic.CurrentGame.Mode == Game.Modes.SCENARIO_NON_RESUMABLE);

        public static int KSPMajorVersion
        {
            get { return Versioning.version_major; }
        }

        public static int KSPMinorVersion
        {
            get { return Versioning.version_minor; }
        }

        /// <summary>
        /// Get a non-public field value from an object instance through reflection.
        /// </summary>
        /// <param name="type">The type of the object instance from which to obtain the field.</param>
        /// <param name="instance">The object instance</param>
        /// <param name="fieldName">The field name in the object instance, from which to obtain the value.</param>
        /// <returns>The value of the <paramref name="fieldName"/> instance or null if no such field exist in the instance.</returns>
        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                           | BindingFlags.Static;
            var field = type.GetField(fieldName, bindFlags);
            return field?.GetValue(instance);
        }

        /// <summary>
        /// Set a non-public field value of an object instance through reflection.
        /// </summary>
        /// <param name="type">The type of the object instance in which to update the field.</param>
        /// <param name="instance">The object instance</param>
        /// <param name="fieldName">The field name in the object instance, in which to change the value.</param>
        /// <returns>The success flag of the update operation.</returns>
        internal static bool SetInstanceField(Type type, object instance, string fieldName, object newValue)
        {
            const BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                                           | BindingFlags.Static;

            try
            {
                var field = type.GetField(fieldName, bindFlags);
                field.SetValue(instance, newValue);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}