using NBitcoin;
using Newtonsoft.Json.Linq;
using System.Text;

namespace NethereumSample
{
    public enum Speed
    {
        LOW = 74,
        MEDIUM = 148,
        HIGH = 256,
        FAST = 300
    }
    public class BitcoinWallet
    {
        //This property represents the seed phrase for the Ethereum wallet with 12Words.

        public static string BlockChainAPI = "1ef88390eda34881bb460ed1ec58c256";
        private Network network;
        private BitcoinAddress publicAddressBtc;
        private BitcoinSecret privateAdress;

        public BitcoinWallet(Mnemonic seedPhrase, Network network)
        {
            this.network = network;
            ExtKey hdroot = seedPhrase.DeriveExtKey();
            ExtKey pkey = hdroot.Derive(new NBitcoin.KeyPath("m/84'/0'/0'/0/0'"));
            this.privateAdress = pkey.PrivateKey.GetBitcoinSecret(network);
            this.publicAddressBtc = pkey.GetPublicKey().GetAddress(ScriptPubKeyType.Segwit, network);
        }
        public BitcoinWallet(string privateKey, Network network)
        {
            this.network = network;
            this.privateAdress = new BitcoinSecret(privateKey, network);
            this.publicAddressBtc = privateAdress.GetAddress(ScriptPubKeyType.Legacy);
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

        public BitcoinSecret GetPrivateAdress()
        {
            return this.privateAdress;
        }

        public async Task<decimal> CheckBalance()
        {
            using (var httpClient = new HttpClient())
            {
                string url = "https://api.blockchair.com/bitcoin/addresses/balances?addresses=" + this.publicAddressBtc.ToString();
                var response = await httpClient.GetAsync(url);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                JObject json = JObject.Parse(jsonResponse);
                Console.WriteLine(json);
                decimal balanceSatoshi;
                try
                {
                    balanceSatoshi = json["data"][this.publicAddressBtc.ToString()].Value<decimal>();
                }
                catch(System.ArgumentException)
                {
                    balanceSatoshi = 0;
                }
                // Convertir le solde en BTC
                decimal balanceBTC = balanceSatoshi / 1_000_000_000m;
                return balanceSatoshi;
            }
        }

        public bool VerifySignature(string transactionSigned, Network network)
        {
            Transaction signedTransaction = Transaction.Parse(transactionSigned, Network.TestNet); // Utilisez Network.Main pour le réseau principal (mainnet)
            bool allInputsSigned = true;

            for (int i = 0; i < signedTransaction.Inputs.Count; i++)
            {
                TxIn input = signedTransaction.Inputs[i];
                OutPoint previousOutput = input.PrevOut;
                Money prevAmount = 1546991; // À remplacer par le montant de l'output précédent

                var txOut = new TxOut(prevAmount, this.publicAddressBtc.ScriptPubKey);
                var coin = new Coin(previousOutput, txOut);
                var checker = new TransactionChecker(signedTransaction, i, coin.TxOut);

                ScriptEvaluationContext scriptContext = new ScriptEvaluationContext();
                bool isValidSignature = scriptContext.VerifyScript(input.ScriptSig, txOut.ScriptPubKey, checker);

                if (!isValidSignature)
                {
                    allInputsSigned = false;
                    Console.WriteLine($"La signature de l'entrée {i} n'est pas valide.");
                    return false;
                }
            }

            if (allInputsSigned)
            {
                Console.WriteLine("Toutes les signatures de la transaction sont valides.");
                return true;
            }
            else
            {
                Console.WriteLine("Certaines signatures de la transaction ne sont pas valides.");
                return false;
            }
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