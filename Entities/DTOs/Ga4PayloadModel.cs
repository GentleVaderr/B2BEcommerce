using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Entities.DTOs
{
    public class Ga4PayloadModel
    {
        [JsonPropertyName("client_id")]
        public string? ClientId { get; set; }

        [JsonPropertyName("user_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
        public string? UserId { get; set; }

        [JsonPropertyName("events")]
        public List<Ga4EventModel> Events { get; set; } = new List<Ga4EventModel>();
    }

    // 2. Etkinlik Modeli
    public class Ga4EventModel
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("params")]
        public object? Params { get; set; } 
    }

    // 3. E-Ticaret Ürün Modeli (Dinamik Parametreler)
    public class Ga4EcommerceItem
    {
        [JsonPropertyName("item_id")] public string? ItemId { get; set; }
        [JsonPropertyName("item_name")] public string? ItemName { get; set; }
        [JsonPropertyName("price")] public decimal Price { get; set; }
        [JsonPropertyName("quantity")] public int Quantity { get; set; }
        [JsonPropertyName("index")] public int Index { get; set; }

        //  Dinamik Alanlar 
        [JsonPropertyName("item_brand")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? ItemBrand { get; set; }
        [JsonPropertyName("item_category")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? ItemCategory { get; set; }
        [JsonPropertyName("coupon")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Coupon { get; set; }
        [JsonPropertyName("discount")][JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public decimal Discount { get; set; }
    }

}
}
