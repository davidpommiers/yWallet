using NBitcoin;
using Bitcoin.BIP39;
using System.Text;
using Nethereum.Web3;
using System.Security.Cryptography;

//cV2VrGWeyPp5H5ePtkJeG7FjW1Vmnv2rGEvChBJnmKFAZKB5sVWZ alice 20 tBTC
namespace NethereumSample
{
    public class ConnexionProcess
    {
        public static string Login()
        {
            Console.WriteLine("[C3P0] : Hy I'm C3P0 and I will help you to manage or create your wallet");
            Console.Write("[C3P0] : Enter your password = ");
            string myPassword = string.Empty;
            while(myPassword == "")
            {
                myPassword = Console.ReadLine();
            }
            bool login = false;
            string[] lines = File.ReadAllLines("User.txt");
            try
            {
                string[] result = AES.findIVAndHashInLines(lines[0]);
                string genesisAccountPivateKey = string.Empty;
                Console.WriteLine("result0 = " + result[0]);
                Console.WriteLine("result = " + result[1]);

                while(!login)
                {
                    try
                    {
                        Console.WriteLine("[C3P0] : Connexion...");
                        genesisAccountPivateKey = AES.Decrypt(result[0], myPassword, result[1]);
                        login = true;
                    }
                    catch(CryptographicException)
                    {
                        Console.WriteLine("[C3P0] : /!\\ WrongPassWord /!\\");
                        Console.WriteLine("[C3P0] : Enter your password master");
                        myPassword = Console.ReadLine();
                    }
                }
                Console.WriteLine(genesisAccountPivateKey);
                return genesisAccountPivateKey;
            }
            catch(System.IndexOutOfRangeException)
            {
                
                Console.WriteLine("[C3P0] : It seems like you do not have a paswword");
                return SingUp(myPassword);
            }
        }
        public static string SingUp(string myPassword)
        {
            bool login = false;
            string[] lines = File.ReadAllLines("User.txt");
            Console.Write("[C3P0] : confirm your password ");
            myPassword = Console.ReadLine();
            Console.WriteLine("[C3P0] : Perfect your password is " + myPassword);
            Mnemonic mnemo = new Mnemonic(Wordlist.English, WordCount.TwentyFour);
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] IV = new byte[16];
            rng.GetBytes(IV);
            File.WriteAllText("User.txt", AES.Encypt(mnemo.ToString(), myPassword, IV) + " "+ BitConverter.ToString(IV).Replace("-", ""));
            Console.WriteLine(mnemo.ToString());
            return mnemo.ToString();
            
        }
    }
}