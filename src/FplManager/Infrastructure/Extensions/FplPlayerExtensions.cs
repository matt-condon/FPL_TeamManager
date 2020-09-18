using FplClient.Data;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FplManager.Infrastructure.Extensions
{
    public static class FplPlayerExtensions
    {
        public static string GetFullPlayerString(this FplPlayer player)
        {
            var properties = player.GetType().GetProperties();

            var sb = new StringBuilder();

            foreach (var info in properties)
            {
                var value = info.GetValue(player, null) ?? "(null)";
                sb.AppendLine(info.Name + ": " + value.ToString());
            }

            return sb.ToString();
        }

        public static string GetPartialPlayerString(this FplPlayer player)
        {
            var properties = player.GetKeyProperties();

            var sb = new StringBuilder();

            foreach (var info in properties)
            {
                var value = info.GetValue(player, null) ?? "(null)";
                sb.AppendLine(info.Name + ": " + value.ToString());
            }

            return sb.ToString();
        }

        private static PropertyInfo[] GetKeyProperties(this FplPlayer player)
        {
            var keyProperties = new List<string>() { "FirstName", "SecondName", "TotalPoints", "NowCost", "ValueSeason", "EpNext"};
            return player.GetType().GetProperties().Where(p => keyProperties.Contains(p.Name.ToString())).ToArray();
        }
    }
}
