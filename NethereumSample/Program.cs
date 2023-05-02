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
            ConnexionProcess.Login();
        }
    }
}