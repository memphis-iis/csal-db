using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace CSALMongoWebAPI.Util {
    /// <summary>
    /// Provide helper static functions for rendering views
    /// </summary>
    public static class RenderHelp {
        /// <summary>
        /// Because our ID's (especially lesson ID's) have embedded illegal
        /// characters, we will be using a special encoding scheme for ID's
        /// </summary>
        /// <param name="s">value to encode</param>
        /// <returns>encoded value</returns>
        /// <seealso cref="URIDecode"/>
        public static string URIEncode(string s) {
            //Currently we just double escape
            if (String.IsNullOrEmpty(s))
                return s;
            return Uri.EscapeDataString(Uri.EscapeDataString(s));
        }

        /// <summary>
        /// Because our ID's (especially lesson ID's) have embedded illegal
        /// characters, we will be using a special encoding scheme for ID's.
        /// </summary>
        /// <param name="s">value to decode (from URIEncode)</param>
        /// <returns>decoded value</returns>
        /// <seealso cref="URIEncode"/>
        public static string URIDecode(string s) {
            if (String.IsNullOrEmpty(s))
                return s;
            return HttpUtility.UrlDecode(HttpUtility.UrlDecode(s));
        }

        /// <summary>
        /// Create a human-readable version of the given number of milliseconds.
        /// For instance, the CurrentTotalTime method from the CSAL Mongo model
        /// </summary>
        /// <param name="dur">Number of milliseconds to translate</param>
        /// <returns></returns>
        public static string HumanDuration(double dur) {
            if (dur < 0.001) {
                return "";
            }

            dur /= 1000.0; //First xlate to secs

            if (dur < 60.0) {
                return "< 1 min";
            }

            string units = "mins";
            dur /= 60.0;
            
            //This is probably test data, but make it readable
            if (dur > 180.0) {
                units = "hrs";
                dur /= 60.0;
            }

            return String.Format("{0:N0} {1}", Math.Floor(dur), units);
        }

        /// <summary>
        /// Used for changing anonymous/dynamic objects into expando objects for MVC Razor
        /// </summary>
        /// <param name="anonymousObject"></param>
        /// <returns></returns>
        public static ExpandoObject ToExpando(object anonymousObject) {
            IDictionary<string, object> anonymousDictionary = new RouteValueDictionary(anonymousObject);
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (var item in anonymousDictionary)
                expando.Add(item);
            return (ExpandoObject)expando;
        }
    }
}