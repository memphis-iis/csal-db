using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
            return HttpUtility.UrlDecode(HttpUtility.UrlDecode(s));
        }
    }
}