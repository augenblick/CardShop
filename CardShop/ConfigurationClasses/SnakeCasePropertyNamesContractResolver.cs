using Newtonsoft.Json.Serialization;

namespace CardShop.ConfigurationClasses
{
    public class SnakeCasePropertyNamesContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return ConvertToSnakeCase(propertyName);
        }

        private string ConvertToSnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = char.ToLower(input[0]).ToString();
            for (int i = 1; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]))
                {
                    result += "_";
                    result += char.ToLower(input[i]);
                }
                else
                {
                    result += input[i];
                }
            }
            return result;
        }
    }
}
