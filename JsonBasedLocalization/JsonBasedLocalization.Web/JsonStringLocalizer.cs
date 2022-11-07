using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace JsonBasedLocalization.Web
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly JsonSerializer _Serializer = new JsonSerializer();
        private readonly IDistributedCache _Cache;

        public JsonStringLocalizer(IDistributedCache cache)
        {
            _Cache = cache;
        }
        public LocalizedString this[string name]
        {
            get
            {
                var value = GetString(name);
                return new LocalizedString(name, value);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var actualValue = this[name];
                return !actualValue.ResourceNotFound 
                    ? new LocalizedString(name, String.Format(actualValue.Value, arguments)) 
                    : actualValue;
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            var filePath = $"Resources/{Thread.CurrentThread.CurrentCulture.Name}.json";
            using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using StreamReader sr = new StreamReader(stream);
            using JsonTextReader reader = new JsonTextReader(sr);

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                    continue;

                var key = reader.Value as string;
                reader.Read();

                var value = _Serializer.Deserialize<string>(reader);
                yield return new LocalizedString(key, value);
            }


        }

        private string GetString(string Key)
        {
            var filePath = $"Resources/{Thread.CurrentThread.CurrentCulture.Name}.json";
            var fullFilePath = Path.GetFullPath(filePath);

            if (File.Exists(fullFilePath))
            {
                var casheKey = $"locale_{Thread.CurrentThread.CurrentCulture.Name}_{Key}";
                var casheValue = _Cache.GetString(casheKey);

                if(!string.IsNullOrEmpty(casheValue))
                {
                    return casheValue;
                }

                var result = GetValueFromJson(Key, fullFilePath);

                if (!string.IsNullOrEmpty(result))
                {
                    _Cache.SetString(casheKey, result);
                }
                return result;
            }

            return String.Empty;
        }

        private string GetValueFromJson(string PropertyName, string FilePath)
        {
            if(string.IsNullOrEmpty(FilePath) || string.IsNullOrEmpty(FilePath))
                return string.Empty;

            using FileStream stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            using StreamReader sr = new StreamReader(stream);
            using JsonTextReader reader = new JsonTextReader(sr);

            while (reader.Read())
            {
                if(reader.TokenType == JsonToken.PropertyName && reader.Value as string == PropertyName)
                {
                    reader.Read();
                    return _Serializer.Deserialize<string>(reader);
                }
            }

            return String.Empty;
        }
    }
}
