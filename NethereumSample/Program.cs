using NBitcoin;
using Bitcoin.BIP39;
using System.Text;
using Nethereum.Web3;
using System.Security.Cryptography;

//cV2VrGWeyPp5H5ePtkJeG7FjW1Vmnv2rGEvChBJnmKFAZKB5sVWZ alice 20 tBTC
namespace NethereumSample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hy I'm C3P0 and I will help you to manage or create your wallet");
            Console.Write("Enter your password = ");
            string password = Console.ReadLine();
            if(password == null)
            {
                Console.WriteLine("Please enter a password !");
                password = Console.ReadLine();
            }
            Console.WriteLine("Perfect your password is " + password);
            Console.WriteLine("please Wait ...");
            string path = "password.txt";
            int nmbWordsForSeed = 24;
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            string wordList = string.Empty; 
            foreach (string line in lines)
            {
                int indexCaractrer  = 0;
                bool correctPassword = true;
                string passwordTxt = string.Empty;

                while(line[indexCaractrer] != 32)
                {
                    password += line[indexCaractrer];
                    if(password[indexCaractrer] != line[indexCaractrer])
                    {
                        correctPassword = false;
                        break;
                    }
                    indexCaractrer++;
                }

                if(correctPassword is true)
                {
                    int indexCaractrerInSeed = indexCaractrer+1;

                    for(int indexWords = 0; indexWords < nmbWordsForSeed; indexWords++)
                    {

                        string myseed = string.Empty;
                        while(indexCaractrerInSeed < line.Length && line[indexCaractrerInSeed] != 32)
                        {
                            myseed += line[indexCaractrerInSeed];
                            indexCaractrerInSeed++;
                        }
                        indexCaractrerInSeed++;
                        wordList += myseed + " ";
                    }
                }
            }
            Mnemonic mnemo;
            if(wordList == null)
            {
                Console.WriteLine("you do not have account, let me create your Wallet");
                Console.Write("confirm your password");
                password = Console.ReadLine();
                Console.Write("Perfect your password is " + password);
                mnemo = new Mnemonic(Wordlist.English, WordCount.TwentyFour);
                File.WriteAllText(path, password+" "+ mnemo.ToString());             
            }
            else
            {
                mnemo = new Mnemonic(wordList);
            }
            Console.WriteLine(mnemo.ToString());
            EthereumWallet myETHclient = new EthereumWallet(mnemo, password, "https://sepolia.infura.io/v3/969eda60bf5242dfb2c46f0fa053e7a6");
            BitcoinWallet myBTCClient = new BitcoinWallet(mnemo, Network.TestNet);
            Console.WriteLine(myBTCClient.GetPublicAddressBtc());
            string plainText = "Texte à chiffrer";

            // Générez une clé AES-256 (32 octets) et un IV (16 octets) aléatoires
            
            AES message = new AES("je suis le texte", password);
            Console.WriteLine(message.GetEncryptMessage().ToString());
            Console.WriteLine(message.DecryptStringFromBytesAes());
        }
    }
}