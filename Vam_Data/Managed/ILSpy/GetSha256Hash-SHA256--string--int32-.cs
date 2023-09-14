// SuperController
using System.Security.Cryptography;
using System.Text;

protected static string GetSha256Hash(SHA256 shaHash, string input, int length)
{
	byte[] array = shaHash.ComputeHash(Encoding.UTF8.GetBytes(input));
	StringBuilder stringBuilder = new StringBuilder();
	for (int i = 0; i < array.Length && i < length; i++)
	{
		stringBuilder.Append(array[i].ToString("x2"));
	}
	return stringBuilder.ToString();
}
