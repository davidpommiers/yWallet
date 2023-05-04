using System.Security.Cryptography;
using System.Text;

public class InformationUser
{
    public static string USERPATH = "User.txt";
    public static string ACCOUNTSETH = "AccountsETH.txt";
    public static string ACCOUNTSBTC = "AccountsBTC.txt";
    public static string[] FindIVAndHashInLines(string path, int account)
    {
        string[] lines = File.ReadAllLines(path);
        string[] result = new string[2];
        int indexSapce = 0;
        foreach (var carac in lines[account])
        {
            if(carac == 32)
            {
                indexSapce++;
                break;
            }
            else
            {
                result[0] += carac;
            }
            indexSapce++;
        }

        for (int i = indexSapce; i < lines[account].Length; i++)
        {
            result[1] += lines[account][i];
        }

        return result;
    }

    public static void SaveInfo(string write, string path)
    {
        using (StreamWriter writer = new StreamWriter(path, true))
        {
            writer.WriteLine(write);
        }
    }

    public static int ReadNumberAccount(string path)
    {
        string[] lines = File.ReadAllLines(path);
        int result = 0;
        foreach (var carac in lines)
        {
            result++;
        }
        return result;
    }
}