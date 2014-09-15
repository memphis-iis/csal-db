using System;

using MongoDB.Bson;

namespace CSALMongo {
    public static class Util {
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
