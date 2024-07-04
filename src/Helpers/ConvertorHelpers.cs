using System.Text;

namespace AuthApi.Helpers;

public static class ConvertorHelpers {
    public static string ConvertToBase64(this string input) {
        ArgumentNullException.ThrowIfNull(input);

        // Step 1: Convert the string to a byte array
        var byteArray = Encoding.UTF8.GetBytes(input);

        // Step 2: Convert the byte array to a Base64 string
        var base64String = Convert.ToBase64String(byteArray);

        return base64String;
    }

    public static string ConvertFromBase64(this string base64Input) {
        ArgumentNullException.ThrowIfNull(base64Input);

        // Step 1: Convert the Base64 string to a byte array
        var byteArray = Convert.FromBase64String(base64Input);

        // Step 2: Convert the byte array back to a regular string
        var originalString = Encoding.UTF8.GetString(byteArray);

        return originalString;
    }
}