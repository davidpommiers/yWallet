using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Signer;
using Nethereum.RPC.Eth.DTOs;

namespace NethereumSample
{
    /// <summary>
    /// The EthereumWallet class is a C# class that provides an interface to the Ethereum blockchain. 
    /// It allows users to manage Ethereum accounts and interact with the Ethereum network.
    /// </summary>
    public class AccountETH
    {
        //This property represents a list of Account objects associated with the Ethereum wallet.
        private  Account account;
        //This property represents the password for the Ethereum wallet.
        private string privateKey;
        //This property represents the Web3 object for connecting to the Ethereum network, infura
        private Web3 network;

        private string publicAddress;

        private decimal myNmbEth;

        private string category;
        private string name;


        /// <summary>
        /// This constructor creates a new Ethereum wallet using a given seed phrase and password. 
        /// The seedPhrase parameter is a Mnemonic object that represents the seed phrase for the wallet, and the password parameter is a string that represents the password for the wallet.
        /// </summary>
        /// <param name="seedPhrase"></param>
        /// <param name="password"></param>
        public AccountETH(Web3 network, string name, string category, string path, string password)
        {
            this.name = name;
            this.category = category;
            this.network = network;
            EthECKey ecKey = EthECKey.GenerateKey();
            this.privateKey = ecKey.GetPrivateKey();
            this.publicAddress = ecKey.GetPublicAddress();
            this.account = new Account(this.privateKey);
            string filePath = path;
            (string, string) lineToAdd = AES.Encypt(this.privateKey, password);
            InformationUser.SaveInfo(lineToAdd.Item1 + " " + lineToAdd.Item2, InformationUser.ACCOUNTSETH);            
        }

        public string GetPrivateKey()
        {
            return this.privateKey;
        }

        public string GetPublicAddress()
        {
            return this.publicAddress;
        }
        public string GetCategory()
        {
            return this.category;
        }
        public string GetName()
        {
            return this.name;
        }

        public Web3 GetConnector()
        {
            return this.network;
        } 

        public decimal GetMyNmbEth()
        {
            return this.myNmbEth;
        } 
        
        
        /// <summary>
        /// This method sends Ether to a specified Ethereum address. 
        /// The To parameter is a string that represents the recipient Ethereum address, 
        /// the amount parameter is a decimal that represents the amount of Ether to send,
        /// the account parameter is an Account object that represents the sender's Ethereum account, 
        /// and the optional gwei parameter is a decimal that represents the gas price for the transaction.
        /// </summary>
        /// <param name="To"></param>
        /// <param name="amount"></param>
        /// <param name="account"></param>
        /// <param name="gwei"></param>
        /// <returns>the hash of the transaction</returns>
        public  async Task<string> sendEther(string To, decimal amount, decimal gwei = 0)
        {
            try
            {
                TransactionReceipt transaction;
                if(gwei != 0)
                {
                    transaction = await this.network.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(To, amount, gwei);
                }
                else
                {
                    transaction = await this.network.Eth.GetEtherTransferService().TransferEtherAndWaitForReceiptAsync(To, amount);
                }
                return transaction.BlockHash;
            }
            catch(System.Exception e)
            {
                Console.WriteLine(e);
                return "";
            }
        }    
    }
}