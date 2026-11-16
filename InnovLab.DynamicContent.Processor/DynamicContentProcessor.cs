using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace InnovLab.DynamicContent.Processor
{
    public class DynamicContentPreprocessor
    {
        private string Json;

        private static readonly Regex PathToken = new(@"#path:(?<name>[^#]+)#", RegexOptions.Compiled);

        public DynamicContentPreprocessor(string json)
        {
            Json = json;
        }

        public DynamicContentPreprocessor WithBrand(string brand)
        {
            SetPlaceholderField("#brand#", brand);
            return this;
        }

        public DynamicContentPreprocessor WithBranch(string branch)
        {
            SetPlaceholderField("#branch#", branch);
            return this;
        }

        private void SetPlaceholderField(string placeholder, object value)
        {
            var escaped = JsonEncodedText.Encode(value?.ToString() ?? string.Empty).ToString();
            Json = Json.Replace(placeholder, escaped);
        }

        public async Task<DynamicContentPreprocessor> WithPathResolverAsync(Func<string, CancellationToken, Task<string>> resolver, CancellationToken cancellationToken)
        {
            var names = PathToken.Matches(Json)
              .Select(m => m.Groups["name"].Value)
              .Distinct();

            var resolved = new Dictionary<string, string>();
            foreach (var name in names)
                resolved[name] = await resolver(name, cancellationToken);

            // 4) Troca todas as ocorrências
            Json = PathToken.Replace(Json, m =>
                JsonEncodedText.Encode(resolved[m.Groups["name"].Value]).ToString());

            return this;
        }

        public string GetProcessedJson() => Json;
    }

    public class DynamicContentProcessor
    {
        private readonly JsonNode Root;
        public DynamicContentProcessor(JsonNode root)
        {
            Root = root;
        }

        public static async Task<DynamicContentProcessor> CreateAsync(
            string json,
            Func<DynamicContentPreprocessor, CancellationToken, Task> configure,
            CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(json);
            ArgumentNullException.ThrowIfNull(configure);

            // 1) #brand#, #branch# e demais placeholders fixos
            var config = new DynamicContentPreprocessor(json);
            await configure(config, ct);

            var processedJson = config.GetProcessedJson();

            try
            {
                // 2) Parse
                var root = JsonNode.Parse(processedJson) ?? throw new InvalidOperationException("JSON vazio ou nulo.");
                return new DynamicContentProcessor(root);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("Falha ao interpretar o JSON.", ex);
            }
        }

        public DynamicContentProcessor Set(string path, JsonNode? value)
        {
            var segments = path.Split('.');
            JsonNode? current = Root;

            foreach (string segment in segments[..^1])
            {
                current = (current as JsonObject)?[segment];
                if (current is null)
                {
                    return this;
                }
            }

            if (current is JsonObject parent)
            {
                parent[segments[^1]] = value;
            }

            return this;
        }

        public JsonNode Build() => Root;

        public string ToJson(bool indented = true) => Root.ToJsonString(new JsonSerializerOptions { WriteIndented = indented });
    }
}
