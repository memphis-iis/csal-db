using System;

using MongoDB.Bson;

namespace CSALMongo {
    /// <summary>
    /// Simple utilities used in this library that don't really have a
    /// place anywhere else
    /// </summary>
    public static class Util {
        /// <summary>
        /// Given a BSON document and a field, return the BSON document
        /// corresponding to src[fldName].  If that isn't possible (null
        /// values, incorrect type, etc) then do NOT throw an exception
        /// </summary>
        /// <param name="src">BSON doc to examine</param>
        /// <param name="fldName">Property to access in src</param>
        /// <returns>src[fldName] as a BsonDocument. If src or fldName are
        /// null, then return null. If src[fldName] isn't a valid document,
        /// then return an empty/default BsonDocument instance.</returns>
        public static BsonDocument ExtractDoc(BsonDocument src, string fldName) {
            if (src == null || String.IsNullOrWhiteSpace(fldName)) {
                return null;
            }
            
            var val = src.GetValue(fldName, new BsonDocument());
            if (val == null || !val.IsBsonDocument) {
                val = new BsonDocument();
            }

            return val.AsBsonDocument;
        }

        /// <summary>
        /// Given a BSON document and a field, return the BSON array
        /// corresponding to src[fldName].  If that isn't possible (null
        /// values, incorrect type, etc) then do NOT throw an exception
        /// </summary>
        /// <param name="src">BSON doc to examine</param>
        /// <param name="fldName">Property to access in src</param>
        /// <returns>src[fldName] as a BsonArray. If src or fldName are
        /// null, then return null. If src[fldName] isn't a valid array,
        /// then return an empty/default BsonArray instance.</returns>
        public static BsonArray ExtractArray(BsonDocument src, string fldName) {
            if (src == null || String.IsNullOrWhiteSpace(fldName)) {
                return null;
            }

            var val = src.GetValue(fldName, new BsonArray());
            if (val == null || !val.IsBsonArray) {
                val = new BsonArray();
            }

            return val.AsBsonArray;
        }
    }
}
