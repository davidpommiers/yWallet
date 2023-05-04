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
           Mnemonic mnemo =  ConnexionProcess.Login();
           BitcoinWallet newWalletBTC  = new BitcoinWallet(mnemo, Network.Main);
           EthereumWallet newWalletETH = new EthereumWallet(mnemo, "https://mainnet.infura.io/v3/969eda60bf5242dfb2c46f0fa053e7a6");
           newWalletETH.AddAccount("livretA", "Epargne");
           newWalletETH.AddAccount("livretB", "Epargne2");
           newWalletETH.AddAccount("livretC", "Epargne3");
           Console.WriteLine(newWalletETH.GetAccounts()[0].GetPrivateKey());
           Console.WriteLine(newWalletETH.GetAccounts()[1].GetPrivateKey());
           Console.WriteLine(newWalletETH.GetAccounts()[2].GetPrivateKey());

           string[] myAccount0 =InformationUser.FindIVAndHashInLines(InformationUser.ACCOUNTSETH, 0);
           string[] myAccount1 =InformationUser.FindIVAndHashInLines(InformationUser.ACCOUNTSETH, 1);
           string[] myAccount2 =InformationUser.FindIVAndHashInLines(InformationUser.ACCOUNTSETH, 2); 
           Console.WriteLine(AES.Decrypt(myAccount0[0], newWalletETH.GetPrivateKey(), myAccount0[1]));
           Console.WriteLine(AES.Decrypt(myAccount1[0], newWalletETH.GetAccounts()[0].GetPrivateKey(), myAccount1[1]));
           Console.WriteLine(AES.Decrypt(myAccount2[0], newWalletETH.GetAccounts()[1].GetPrivateKey(), myAccount2[1]));
        }
    }
}