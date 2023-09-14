// SuperController
using System.Security.Cryptography;

protected KeyType IsValidKey(string key)
{
	KeyType keyType;
	switch (key[0])
	{
	case 'F':
	case 'f':
		keyType = KeyType.Free;
		break;
	case 'T':
	case 't':
		keyType = KeyType.Teaser;
		break;
	case 'E':
	case 'e':
		keyType = KeyType.Entertainer;
		break;
	case 'S':
	case 's':
		keyType = KeyType.Steam;
		break;
	case 'C':
	case 'c':
		keyType = KeyType.Creator;
		break;
	default:
		return KeyType.Invalid;
	}
	SHA256 shaHash = SHA256.Create();
	string sha256Hash = GetSha256Hash(shaHash, keyType.ToString() + key.ToUpper(), 3);
	string text = null;
	string text2 = null;
	switch (keyType)
	{
	case KeyType.Free:
		text = "F";
		text2 = freeKey;
		break;
	case KeyType.Teaser:
		text = "T";
		text2 = teaserKey;
		break;
	case KeyType.Entertainer:
		text = "E";
		text2 = entertainerKey;
		break;
	case KeyType.Steam:
		text = "S";
		text2 = steamKey;
		break;
	case KeyType.Creator:
		text = "C";
		text2 = creatorKey;
		break;
	}
	if (text + sha256Hash == text2)
	{
		return keyType;
	}
	return KeyType.Invalid;
}
