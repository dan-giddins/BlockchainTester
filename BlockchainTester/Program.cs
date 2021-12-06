using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Geth;
using Nethereum.Hex.HexTypes;

namespace BlockchainTester
{
	class Program
	{
		private const string SenderAddress = "0x1f7Ac99dCb6b2bC8A613dCFD9A0A274bF9755e65";
		private const string Url = "HTTP://127.0.0.1:7545";
		private const string ContractAddress = "0xfC81e766D2c490e82A74f112b9ECd9d4EBbaC8b2";
		private const string ABI = "[{ 'inputs':[{ 'internalType':'string','name':'message','type':'string'}],'stateMutability':'nonpayable','type':'constructor'},{ 'anonymous':false,'inputs':[{ 'indexed':false,'internalType':'string','name':'stateData','type':'string'}],'name':'StateChanged','type':'event'},{ 'inputs':[],'name':'RequestMessage','outputs':[{ 'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function','constant':true},{ 'inputs':[],'name':'Requestor','outputs':[{ 'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function','constant':true},{ 'inputs':[],'name':'Responder','outputs':[{ 'internalType':'address','name':'','type':'address'}],'stateMutability':'view','type':'function','constant':true},{ 'inputs':[],'name':'ResponseMessage','outputs':[{ 'internalType':'string','name':'','type':'string'}],'stateMutability':'view','type':'function','constant':true},{ 'inputs':[],'name':'State','outputs':[{ 'internalType':'enum HelloBlockchain.StateType','name':'','type':'uint8'}],'stateMutability':'view','type':'function','constant':true},{ 'inputs':[{ 'internalType':'string','name':'requestMessage','type':'string'}],'name':'SendRequest','outputs':[],'stateMutability':'nonpayable','type':'function'},{ 'inputs':[{ 'internalType':'string','name':'responseMessage','type':'string'}],'name':'SendResponse','outputs':[],'stateMutability':'nonpayable','type':'function'}]";

		static async Task Main()
		{
			var web3 = new Web3(Url);
			var balance = await web3.Eth.GetBalance.SendRequestAsync(SenderAddress);
			Console.WriteLine($"Balance in Wei: {balance.Value}");
			var etherAmount = Web3.Convert.FromWei(balance.Value);
			Console.WriteLine($"Balance in Ether: {etherAmount}");
			Console.WriteLine();
			var contract = web3.Eth.GetContract(ABI, ContractAddress);
			var sendRequestFunction = contract.GetFunction("SendRequest");
			var requestValueFunction = contract.GetFunction("RequestMessage");
			Console.WriteLine(await requestValueFunction.CallAsync<string>());
			var transactionHash = await sendRequestFunction.SendTransactionAsync(
				SenderAddress,
				new HexBigInteger(100000),
				new HexBigInteger(Web3.Convert.ToWei(5, Nethereum.Util.UnitConversion.EthUnit.Ether)),
				"Test123ABC");
			Console.WriteLine(await requestValueFunction.CallAsync<string>());
			var receipt = await MineAndGetReceiptAsync(transactionHash);
			Console.WriteLine(await requestValueFunction.CallAsync<string>());
		}

		public static async Task<TransactionReceipt> MineAndGetReceiptAsync(string transactionHash)
		{
			var web3Geth = new Web3Geth(Url);
			var miningResult = await web3Geth.Miner.Start.SendRequestAsync(6);
			Assert.IsTrue(miningResult);
			var receipt = await web3Geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
			while (receipt == null)
			{
				Thread.Sleep(1000);
				receipt = await web3Geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
			}
			miningResult = await web3Geth.Miner.Stop.SendRequestAsync();
			Assert.IsTrue(miningResult);
			return receipt;
		}
	}
}
