using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using NBitcoin;
using Nethereum.HdWallet;
using Nethereum.Hex.HexTypes;

namespace NethereumSample
{
    /// <summary>
    /// The EthereumWallet class is a C# class that provides an interface to the Ethereum blockchain. 
    /// It allows users to manage Ethereum accounts and interact with the Ethereum network.
    /// </summary>
    public class EthereumWallet
    {
        private string PATHACCOUNTSAVE = "AccountsETH.txt";
        private Account genesisAccount;
        //This property represents the number of accounts in the Ethereum wallet.
        private int numberOfAccount;
        //This property represents a list of Account objects associated with the Ethereum wallet.
        private  List<AccountETH> Accounts = new List<AccountETH>();
        
        //This property represents the Web3 object for connecting to the Ethereum network, infura
        private Web3 network;

        private string privateKey;
        private string publicAdress;
        private Mnemonic mnemo;
        private List<string> categorys = new List<string>();


        /// <summary>
        /// This constructor creates a new Ethereum wallet using a given seed phrase and password. 
        /// The seedPhrase parameter is a Mnemonic object that represents the seed phrase for the wallet, and the password parameter is a string that represents the password for the wallet.
        /// </summary>
        /// <param name="seedPhrase"></param>
        /// <param name="password"></param>
        public EthereumWallet(Mnemonic seedPhrase,  string network)
        {
            Wallet myWalletEth = new Wallet(seedPhrase.WordList, WordCount.TwentyFour);
            this.genesisAccount = myWalletEth.GetAccount(0);
            this.privateKey = this.genesisAccount.PrivateKey;
            this.publicAdress = this.genesisAccount.Address;
            this.numberOfAccount = 0;
            this.network = new Web3(this.genesisAccount,network);
            
        }

        public int GetNumberOfAccount()
        {
            return this.numberOfAccount;
        }

        public string GetPrivateKey()
        {
            return this.privateKey;
        }

        public List<AccountETH> GetAccounts()
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

        public void AddAccount(string name, string category)
        {
            AccountETH newAccount;
            if(this.Accounts.Count == 0)
            {
                newAccount = new AccountETH(this.network, name, category, PATHACCOUNTSAVE, this.privateKey);
                
            }
            else
            {
                newAccount = new AccountETH(this.network, name, category, PATHACCOUNTSAVE, this.Accounts[this.numberOfAccount-1].GetPrivateKey());
            }
            this.numberOfAccount++;
            this.Accounts.Add(newAccount); 
            this.categorys.Add(category);       
        }  
    }
}