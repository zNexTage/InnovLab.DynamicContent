// See https://aka.ms/new-console-template for more information

using InnovLab.DynamicContent.Processor;

var cancelationTokenSource = new CancellationTokenSource();
var cancelationToken = cancelationTokenSource.Token;

var json = @"
{
  ""version"": ""1.0"",
  ""brand"": ""#brand#"",
  ""branch"": ""#branch#"",
  ""metadata"": {
    ""title"": ""#brand# - Unidade #branch#"",
    ""description"": ""Confira as ofertas da #brand# na unidade #branch#."",
    ""language"": ""pt-BR"",
    ""generatedAt"": """"
  },
  ""header"": {
    ""logo"": {
      ""url"": ""#path:logo#"",
      ""alt"": ""Logo #brand#""
    },
    ""greeting"": """",
    ""menu"": [
      { ""label"": ""Início"", ""action"": { ""actionLink"": ""#path:home#"" } },
      { ""label"": ""Ofertas"", ""action"": { ""actionLink"": ""#path:ofertas#"" } },
      { ""label"": ""Contato"", ""action"": { ""actionLink"": ""#path:contato#"" } }
    ]
  },
  ""sections"": {
    ""hero"": {
      ""type"": ""banner"",
      ""visible"": false,
      ""headline"": """",
      ""image"": ""#path:hero#"",
      ""discountPercent"": 0,
      ""action"": {
        ""text"": ""Ver ofertas"",
        ""actionLink"": ""#path:ofertas#""
      }
    },
    ""products"": {
      ""type"": ""product-list"",
      ""visible"": true,
      ""title"": ""Destaques da #branch#"",
      ""items"": [
        {
          ""sku"": ""PRD-001"",
          ""name"": ""Produto Premium"",
          ""image"": ""#path:produto-premium#"",
          ""action"": { ""actionLink"": ""#path:produto-premium#"" }
        },
        {
          ""sku"": ""PRD-002"",
          ""name"": ""Produto Essencial"",
          ""image"": ""#path:produto-essencial#"",
          ""action"": { ""actionLink"": ""#path:produto-essencial#"" }
        }
      ]
    },
    ""cart"": {
      ""type"": ""summary"",
      ""visible"": false,
      ""itemsCount"": 0,
      ""total"": 0.0,
      ""message"": """"
    },
    ""contact"": {
      ""type"": ""contact-info"",
      ""visible"": true,
      ""email"": ""contato.#branch#@#brand#.com.br"",
      ""phone"": """",
      ""action"": { ""actionLink"": ""#path:contato#"" }
    }
  },
  ""tracking"": {
    ""campaignId"": """",
    ""userId"": """",
    ""segments"": []
  },
  ""footer"": {
    ""text"": ""© #brand# - Unidade #branch#."",
    ""terms"": { ""actionLink"": ""#path:termos#"" }
  }
}";

var processor = await DynamicContentProcessor
    .CreateAsync(json, async (config, ct) =>
{
    config
          .WithBrand("InnovLab")
          .WithBranch("Main");

    await config
        .WithPathResolverAsync(async (name, token) =>
        {
            // Simula uma chamada assíncrona para resolver o caminho
            await Task.Delay(100, token); // Simula algum tempo de processamento
            return name switch
            {
                "logo" => $"https://cdn.exemplo.com/InnovLab/logo.png",
                "hero" => $"https://cdn.exemplo.com/InnovLab/Main/hero.jpg",
                _ => $"https://cdn.exemplo.com/InnovLab/Main/{name}.jpg"
            };
        }, cancelationToken);
});

processor
    .Set("metadata.generatedAt", DateTime.UtcNow.ToString("o"))
    .Set("tracking.campaignId", Guid.NewGuid().ToString())
    .Set("caminho.que.nao.existe", "teste");

Console.WriteLine(processor.ToJson());

Console.ReadKey();
