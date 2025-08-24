using System.Security.Cryptography;
using System.Text;
using QRCoder;

namespace TotpExample.Helpers
{
    public static class TOTPHelper
    {
        #region Public Static Methods
        public static bool Validate(string code, string secretBase32, int step = 30, int digits = 6, int window = 1)
        {
            long counter = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / step;
            for (int i = -window; i <= window; i++)
            {
                var candidate = Hotp(secretBase32, counter + i, digits);
                if (TimingSafeEquals(candidate, code)) return true;
            }
            return false;
        }

        public static string BuildAuthUri(string issuer, string account, string secretB32, int digits = 6, int period = 30, string algorithm = "SHA1")
        {
            string label = Uri.EscapeDataString($"{issuer}:{account}");
            var qs = new Dictionary<string, string>
            {
                ["secret"] = secretB32.TrimEnd('='),
                ["algorithm"] = algorithm.ToUpperInvariant(),
                ["digits"] = digits.ToString(),
                ["period"] = period.ToString()
            };
            string query = string.Join("&", qs.Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
            return $"otpauth://totp/{label}?{query}";
        }

        public static string GenerateSecret()
        {
            // Generate 20 random bytes for the secret
            byte[] secretBytes = RandomNumberGenerator.GetBytes(20);

            // Convert to Base32 (RFC 4648)
            return ToBase32(secretBytes);
        }

        public static string GenerateQrPng(string content, int pixelsPerModule = 10, QRCodeGenerator.ECCLevel ecc = QRCodeGenerator.ECCLevel.Q)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(content, ecc);
            var png = new PngByteQRCode(data);
            var bytes = png.GetGraphic(pixelsPerModule);

            return Convert.ToBase64String(bytes);
        }
        #endregion

        #region Private Static Methods
        private static string Hotp(string secretBase32, long counter, int digits = 6)
        {
            byte[] key = Base32ToBytes(secretBase32);
            byte[] counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian) Array.Reverse(counterBytes); // big-endian

            using var hmac = new HMACSHA1(key);
            var hash = hmac.ComputeHash(counterBytes);

            int offset = hash[^1] & 0x0f;
            int dbc = ((hash[offset] & 0x7f) << 24) |
                      ((hash[offset + 1] & 0xff) << 16) |
                      ((hash[offset + 2] & 0xff) << 8) |
                      (hash[offset + 3] & 0xff);

            int mod = (int)Math.Pow(10, digits);
            return (dbc % mod).ToString().PadLeft(digits, '0');
        }

        private static bool TimingSafeEquals(string a, string b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }

        private static byte[] Base32ToBytes(string input)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            string s = input.ToUpper().Trim().TrimEnd('=').Replace(" ", "");
            var bits = new StringBuilder();
            foreach (char c in s)
            {
                int val = alphabet.IndexOf(c);
                if (val < 0) throw new FormatException("Invalid Base32 char");
                bits.Append(Convert.ToString(val, 2).PadLeft(5, '0'));
            }
            var bytes = new List<byte>();
            for (int i = 0; i + 8 <= bits.Length; i += 8)
                bytes.Add(Convert.ToByte(bits.ToString(i, 8), 2));
            return bytes.ToArray();
        }

        private static string ToBase32(byte[] data)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            var output = new StringBuilder();
            int bits = 0, value = 0;
            foreach (var b in data)
            {
                value = (value << 8) | b;
                bits += 8;
                while (bits >= 5)
                {
                    output.Append(alphabet[(value >> (bits - 5)) & 31]);
                    bits -= 5;
                }
            }
            if (bits > 0)
            {
                output.Append(alphabet[(value << (5 - bits)) & 31]);
            }
            // Pad to multiple of 8 chars with '='
            while (output.Length % 8 != 0) output.Append('=');
            return output.ToString();
        }
        #endregion
    }
}
