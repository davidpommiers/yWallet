using NBitcoin;
using Newtonsoft.Json.Linq;
using System.Text;

namespace NethereumSample
{
    public class AccountBTC
    {
        //This property represents the seed phrase for the Ethereum wallet with 12Words.

        public static string BlockChainAPI = "1ef88390eda34881bb460ed1ec58c256";
        private Network network;
        private BitcoinAddress publicAddressBtc;
        private BitcoinSecret privateKey;
        private string category;
        private string name;

        public AccountBTC(string name, string category, string path, BitcoinSecret password, Network network)
        {
            this.network = network;
            Key newKey = new Key();
            this.privateKey = newKey.GetBitcoinSecret(network);
            this.publicAddressBtc = privateKey.GetAddress(ScriptPubKeyType.Legacy);
            this.category = category;
            this.name = name;
            string filePath = path;
            (string, string) lineToAdd = AES.Encypt(this.privateKey.ToString(), password.ToString());
            InformationUser.SaveInfo(lineToAdd.Item1 + " " + lineToAdd.Item2, InformationUser.ACCOUNTSBTC);
        }

        public static string GetBlockChainAPI()
        {
            return BlockChainAPI;
        }

        public Network GetNetwork()
        {
            return this.network;
        }

        public BitcoinAddress GetPublicAddressBtc()
        {
            return this.publicAddressBtc;
        }

        public BitcoinSecret GetPrivateKey()
        {
            return this.privateKey;
        }

        public Money EstimateGazFee(decimal number_of_inputs,  decimal number_of_outputs, Speed speed)
        {
            Money fee = new Money(number_of_inputs*148 + number_of_outputs * 34 + (decimal)speed, MoneyUnit.Satoshi);
            return fee;
        }
        public async Task<string> SendBTC(Network network, string privateKeySender, BitcoinAddress receiverBitcoinAddress, decimal BtcToTransfert, Speed speed)
        {
            Network myNetWork = network;
            BitcoinSecret alice = new BitcoinSecret(privateKeySender, myNetWork);
            string alicePublicAddress = alice.PrivateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, myNetWork).ToString();
            BitcoinAddress SenderBitcoinAddress = BitcoinAddress.Create(alicePublicAddress, myNetWork);

            BitcoinAddress receiverBTCAddress = receiverBitcoinAddress;

            // Récupérer les UTXOs du sender
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage utxoResponse = await httpClient.GetAsync("https://api.blockcypher.com/v1/btc/test3/addrs/"+alicePublicAddress);
            string utxoJson = await utxoResponse.Content.ReadAsStringAsync();
            JObject utxos = JObject.Parse(utxoJson);
            string txrefs= string.Empty;
            try
            {
                txrefs = utxos.GetValue("txrefs").ToString();
            }
            catch(System.NullReferenceException)
            {
                Console.WriteLine("You do not have token in your wallet balance = " + utxos.GetValue("txrefs").ToString());
                return "";
            }
            JArray utxosArray = JArray.Parse(txrefs);

            // Créer et signer la transaction
            Transaction transaction = myNetWork.CreateTransaction();
            Money toTransfer = new Money(BtcToTransfert, MoneyUnit.BTC);
            Money totalInputUTXO = new Money(0, MoneyUnit.BTC);
            List<Coin> coinArray = new List<Coin>();

            foreach (JToken utxo in utxosArray)
            {
                if(toTransfer>totalInputUTXO)
                {
                    try
                    {
                        Console.WriteLine(utxo["spent"].ToString());
                        if(utxo["spent"].ToString() == "False")
                        {
                            OutPoint outpoint = new OutPoint(uint256.Parse(utxo["tx_hash"].ToString()), utxo["tx_output_n"].Value<int>());
                            TxIn txIn = new TxIn(outpoint);
                            TxOut txout = new TxOut(new Money(utxo["value"].Value<decimal>(), MoneyUnit.Satoshi), SenderBitcoinAddress);
                            Coin coin = new Coin(outpoint, txout);
                            transaction.Inputs.Add(txIn);
                            coinArray.Add(coin);
                            totalInputUTXO += coin.Amount;
                        }
                    }
                    catch(System.Exception)
                    {
                        continue;
                    }
                }
                else
                {
                    break;
                }
            }

            //Estimate the gaz fee 
            Money fee = transaction.GetFee(coinArray.ToArray());// EstimateGazFee(numberOfInputsUtxo, numberOfInputsUtxo+1, speed);

            transaction.Outputs.Add(new TxOut(toTransfer, receiverBitcoinAddress));
            Money change = totalInputUTXO - toTransfer - fee - new Money((decimal)speed, MoneyUnit.Satoshi);
            Money zero = new Money(0, MoneyUnit.BTC);

            if (change > zero)
            {
                Console.WriteLine("change = " + change.ToString());
                transaction.Outputs.Add(new TxOut(change, SenderBitcoinAddress));
            }
            else
            {
                Console.WriteLine("Not enough token");
            }
            transaction.Sign(alice, coinArray.ToArray());

            string transactionSigned = transaction.ToHex();
            HttpClient httpClient2 = new HttpClient();
            string blockCypherApiUrl = "https://api.blockcypher.com/v1/btc/test3/txs/push?token=" + BlockChainAPI;
            string jsonContent = "{\"tx\": \"" + transactionSigned + "\"}";
            HttpContent httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync(blockCypherApiUrl, httpContent);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Transaction diffusée avec succès !");
                return transaction.GetHash().ToString();
            }
            else
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Erreur lors de la diffusion de la transaction : " + responseBody);
                return "";
            }
        }
    }
}