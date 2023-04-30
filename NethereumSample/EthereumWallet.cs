using System.Numerics;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using NBitcoin;
using Nethereum.HdWallet;
using Nethereum.Hex.HexTypes;
using Bitcoin.BIP39;

namespace NethereumSample
{
    /// <summary>
    /// The EthereumWallet class is a C# class that provides an interface to the Ethereum blockchain. 
    /// It allows users to manage Ethereum accounts and interact with the Ethereum network.
    /// </summary>
    public class EthereumWallet
    {
        private Account genesisAccount;
        //This property represents the number of accounts in the Ethereum wallet.
        private int numberOfAccount;
        //This property represents a list of Account objects associated with the Ethereum wallet.
        private  List<Account> Accounts = new List<Account>();
        
        //This property represents the Web3 object for connecting to the Ethereum network, infura
        private Web3 network;

        private string privateKey;
        private string publicAdress;

        /// <summary>
        /// This constructor creates a new Ethereum wallet using a given seed phrase and password. 
        /// The seedPhrase parameter is a Mnemonic object that represents the seed phrase for the wallet, and the password parameter is a string that represents the password for the wallet.
        /// </summary>
        /// <param name="seedPhrase"></param>
        /// <param name="password"></param>
        public EthereumWallet(Mnemonic seedPhrase, string password, string network)
        {
            Wallet myWalletEth = new Wallet(seedPhrase+"", password);
            this.genesisAccount = myWalletEth.GetAccount(0);
            this.privateKey = this.genesisAccount.PrivateKey;
            this.publicAdress = this.genesisAccount.Address;
            this.numberOfAccount = 0;
            this.network = new Web3(network);
        }

        public EthereumWallet(string privateKey, string password, string network)
        {
            Wallet myWalletEth = new Wallet(privateKey, password);
            this.genesisAccount = myWalletEth.GetAccount(0);
            this.privateKey = this.genesisAccount.PrivateKey;
            this.publicAdress = this.genesisAccount.Address;
            this.numberOfAccount = 0;
            this.network = new Web3(this.genesisAccount, network);
        }

        public int GetNumberOfAccount()
        {
            return this.numberOfAccount;
        }

        public List<Account> GetAccounts()
        {
            return this.Accounts;
        }

        public Web3 GetConnector()
        {
            return this.network;
        } 
       
        /// <summary>
        /// This method returns the balance of an Ethereum account. 
        /// The account parameter is an Account object that represents the Ethereum account to query.
        /// </summary>
        /// <param name="account"></param>
        /// <returns>the balance og the account</returns>
        public  async Task<decimal> Balance(Account account)
        {
            HexBigInteger balance;
            try
            {
                balance = await this.network.Eth.GetBalance.SendRequestAsync(account.Address);
                var etherAmount = Web3.Convert.FromWei(balance.Value);
                return etherAmount;
            }
            catch(System.Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
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
        public  async Task<string> sendEther(string To, decimal amount, Account account, decimal gwei = 0)
        {

            try
            {
                Nethereum.RPC.Eth.DTOs.TransactionReceipt transaction;
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