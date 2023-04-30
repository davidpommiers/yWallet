using System.Security.Cryptography;
using System.Text;

public class AES
{
    private  byte[] IV = new byte[16]; 
    private  string toEncrypt;
    private  byte[] password = new byte[32];
    private byte[] encryptMessage = new byte[32];
    public AES(string toEncrypt, string password)
    {
        this.toEncrypt = toEncrypt;
        this.password = StringToAes256Key(password);
        Console.WriteLine(this.password[0]);
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        rng.GetBytes(this.IV);
        this.encryptMessage = EncryptStringToBytesAes(toEncrypt, this.password, this.IV);
    }

    public byte[] GetIV()
    {
        return this.IV;
    }

    public string GetToEncrypt()
    {
        return this.toEncrypt;
    }

    public byte[] GetPassword()
    {
        return this.password;
    }

    public byte[] GetEncryptMessage()
    {
        return this.encryptMessage;
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

    public  string DecryptStringFromBytesAes()
    {
        // Check arguments.
        if (this.encryptMessage == null || this.encryptMessage.Length <= 0)
            throw new ArgumentNullException("cipherText");
        if (this.password == null || this.password.Length <= 0)
            throw new ArgumentNullException("Key");
        if (this.IV == null || this.IV.Length <= 0)
            throw new ArgumentNullException("IV");

        // Declare the string used to hold
        // the decrypted text.
        string plaintext = string.Empty;

        // Create an Aes object
        // with the specified key and IV.
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = this.password;
            aesAlg.IV = IV;

            // Create a decryptor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption.
            using (MemoryStream msDecrypt = new MemoryStream(this.encryptMessage))
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
}