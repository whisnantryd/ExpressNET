
namespace Debouncehouse.ExpressNET.Helpers
{
    public static class Extensions
    {

        /// <summary>
        /// Converts the string to standardized route path format ('api/v1' -> '/API/V1')
        /// </summary>
        public static string ToRoute(this string obj)
        {
            obj = "/" + obj.Trim(new char[] { '/' }).ToUpper();

            return obj;
        }

        public static bool PathMatch(this string[] obj, string[] other)
        {
            if (obj.Length != other.Length)
                return false;

            var match = true;

            for (int i = 0; i < obj.Length; i++)
                if (!(obj[i] == other[i] || (obj[i].StartsWith(":") || other[i].StartsWith(":"))))
                    match = false;

            return match;
        }

        public static string ToJSON<T>(this T obj)
        {
            var js = new System.Web.Script.Serialization.JavaScriptSerializer();

            return js.Serialize(obj);
        }

    }
}
