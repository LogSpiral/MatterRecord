using System.Collections.Generic;
using System.Reflection;
using Terraria.ID;

namespace MatterRecord.Contents.EternalWine   
{
    public static class ImmunityHelper
    {
        private static int[] _allCooldownIDs;

        public static int[] GetAllImmunityCooldownIDs()
        {
            if (_allCooldownIDs == null)
            {
                var fields = typeof(ImmunityCooldownID).GetFields(BindingFlags.Public | BindingFlags.Static);
                var list = new List<int>();
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(int))
                        list.Add((int)field.GetValue(null));
                }
                _allCooldownIDs = list.ToArray();
            }
            return _allCooldownIDs;
        }
    }
}