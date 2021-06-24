using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dotnet5_winservice_demo.Utility
{
    /// <summary>
    /// 全局变量
    /// </summary>
    public static class GlobalVariables
    {
        /// <summary>
        /// Json序列化配置
        /// </summary>
        public static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowReadingFromString,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// Json写入配置
        /// </summary>
        public static JsonWriterOptions JsonWriterOptions = new JsonWriterOptions
        {
            Indented = true
        };
    }
}
