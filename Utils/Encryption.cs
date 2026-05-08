using System.Text;

namespace MOZ_UPGRADE.Utils
{
    public class Encryption
    {
        public string Encryptstring(string originaltext)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(originaltext);
            return Convert.ToBase64String(plainTextBytes);
        }
        public string Decryptedstring(string hashtext)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(hashtext);
            return Encoding.UTF8.GetString(base64EncodedBytes);

        }
    }
}
