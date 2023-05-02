using System.Linq;
using System.Security.Cryptography;
using System.Text;

public class AES
{
    public static string[] findIVAndHashInLines(string line)
    {
        string[] result = new string[2];
        int indexSapce = 0;
        foreach (var carac in line)
        {
            if(carac == 32)
            {
                indexSapce++;
                break;
            }
            else
            {
                result[0] += carac;
            }
            indexSapce++;
        }

        for (int i = indexSapce; i < line.Length; i++)
        {
            result[1] += line[i];
        }

        return result;
    }
    public static byte[] StringToByteArray(string hex)
    {
        int length = hex.Length;
        byte[] bytes = new byte[length / 2];
        for (int i = 0; i < length; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }
    public static byte[] StringToAes256Key(string keyString)
    {
        SHA256 sha256 = SHA256.Create();
        byte[] keyBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString));
        return keyBytes;
    }
    
    public static byte[] EncryptStringToBytesAes(string plainText, byte[] Key, byte[] IV)
    {
        // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

        // Return the encrypted bytes from the memory stream.
        return encrypted;
    }

    public static string DecryptStringFromBytesAes(byte[] cipherText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException("cipherText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        // Declare the string used to hold
        // the decrypted text.
        string plaintext = null;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {

                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plaintext;
    }

    public static string Encypt(string toEncrypt, string password, byte[] IV)
    {
        byte[] passwordSHA256 = StringToAes256Key(password);
        byte[] encryptMessage = new byte[32];
        encryptMessage = EncryptStringToBytesAes(toEncrypt, passwordSHA256, IV);
        string de = DecryptStringFromBytesAes(encryptMessage, passwordSHA256, IV);
        return BitConverter.ToString(encryptMessage).Replace("-", "");
    }
    public static string Decrypt(string message, string password, string IV)
    {
        byte[] messageArray = StringToByteArray(message);
        byte[] passwordSHA256 = StringToAes256Key(password);
        byte[] IVArray = new byte[16];
        Console.WriteLine("IV" + IV);
        IVArray = StringToByteArray(IV);
        string encryptMessage = string.Empty;
        encryptMessage = DecryptStringFromBytesAes(messageArray, passwordSHA256, IVArray);
        return encryptMessage;
    }
}