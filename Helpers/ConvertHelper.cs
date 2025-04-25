namespace BlogAPI.Helper;

public static class ConvertHelper
{
    public static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;

        var stringBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (char.IsUpper(c))
            {
                if (i > 0) stringBuilder.Append('_');
                stringBuilder.Append(char.ToLower(c));
            }
            else
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString();
    }
}