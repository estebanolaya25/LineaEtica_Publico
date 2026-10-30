using System.Security.Cryptography;
using System.Text;

namespace CapaDatos.util
{
    public class cEncriptacion
    {
        private static string _key;
        private static string _iv;

        public cEncriptacion() { }

        public static void Configurar(string key, string iv)
        {
            _key = key;
            _iv = iv;
        }

        public bool mtdEncrypt()
        {
            bool booToRet = true;
            long i = 0;
            long j = 0;
            Encoding encTexto = Encoding.Default;

            try
            {
                byte[] bytInput = encTexto.GetBytes(this.strM_sInClearText);
                byte[] bytOutput = new byte[bytInput.Length];
                byte[] n_LocBox = new byte[lonM_nLength];
                this.bytM_nBox.CopyTo(n_LocBox, 0);

                for (long offset = 0; offset < bytInput.Length; offset++)
                {
                    i = (i + 1) % lonM_nLength;
                    j = (j + n_LocBox[i]) % lonM_nLength;
                    byte temp = n_LocBox[i];
                    n_LocBox[i] = n_LocBox[j];
                    n_LocBox[j] = temp;
                    byte a = bytInput[offset];
                    byte b = n_LocBox[(n_LocBox[i] + n_LocBox[j]) % lonM_nLength];
                    bytOutput[offset] = (byte)((int)a ^ (int)b);
                }

                char[] outarrchar = new char[encTexto.GetCharCount(bytOutput, 0, bytOutput.Length)];
                encTexto.GetChars(bytOutput, 0, bytOutput.Length, outarrchar, 0);
                this.strM_sCryptedText = new string(outarrchar);
            }
            catch
            {
                booToRet = false;
            }

            return booToRet;
        }

        public bool mtdDecrypt()
        {
            bool booToRet = true;
            try
            {
                this.strM_sInClearText = this.strM_sCryptedText;
                strM_sCryptedText = string.Empty;
                if (booToRet = mtdEncrypt())
                    strM_sInClearText = strM_sCryptedText;
            }
            catch
            {
                booToRet = false;
            }
            return booToRet;
        }

        public string EncryptionKey
        {
            get => this.strM_sEncryptionKey;
            set
            {
                if (this.strM_sEncryptionKey != value)
                {
                    this.strM_sEncryptionKey = value;
                    long index2 = 0;
                    Encoding ascii = Encoding.Default;
                    Encoding unicode = Encoding.Default;
                    byte[] asciiBytes = Encoding.Convert(unicode, ascii, unicode.GetBytes(this.strM_sEncryptionKey));
                    char[] asciiChars = new char[ascii.GetCharCount(asciiBytes, 0, asciiBytes.Length)];
                    ascii.GetChars(asciiBytes, 0, asciiBytes.Length, asciiChars, 0);
                    this.strM_sEncryptionKeyAscii = new string(asciiChars);
                    long KeyLen = strM_sEncryptionKey.Length;

                    for (long count = 0; count < lonM_nLength; count++)
                        this.bytM_nBox[count] = (byte)count;

                    for (long count = 0; count < lonM_nLength; count++)
                    {
                        index2 = (index2 + bytM_nBox[count] + asciiChars[count % KeyLen]) % lonM_nLength;
                        byte temp = bytM_nBox[count];
                        bytM_nBox[count] = bytM_nBox[index2];
                        bytM_nBox[index2] = temp;
                    }
                }
            }
        }

        public string mtdInClearText
        {
            get => this.strM_sInClearText;
            set { if (this.strM_sInClearText != value) this.strM_sInClearText = value; }
        }

        public string CryptedText
        {
            get => this.strM_sCryptedText;
            set { if (this.strM_sCryptedText != value) this.strM_sCryptedText = value; }
        }

        private string strM_sEncryptionKey = string.Empty;
        private string strM_sEncryptionKeyAscii = string.Empty;
        protected byte[] bytM_nBox = new byte[lonM_nLength];
        static public long lonM_nLength = 255;
        private string strM_sInClearText = string.Empty;
        private string strM_sCryptedText = string.Empty;

        public static string Base64_Encode(string str)
        {
            string cambiocaracteres = str.Replace("*", "softrisk1");
            return Convert.ToBase64String(Encoding.Default.GetBytes(cambiocaracteres));
        }

        public static string Base64_Decode(string str)
        {
            try
            {
                string cambiocaracteres = Encoding.Default.GetString(Convert.FromBase64String(str));
                return cambiocaracteres.Replace("softrisk1", "*");
            }
            catch
            {
                return "";
            }
        }

#pragma warning disable SYSLIB0021
        public static string CifradoData(string textToEncrypt)
        {
            try
            {
                string ToReturn = "";
                string publickey = _key ?? throw new InvalidOperationException("Falta configurar Encriptacion:Key.");
                string secretkey = _iv ?? throw new InvalidOperationException("Falta configurar Encriptacion:IV.");
                byte[] secretkeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = System.Text.Encoding.UTF8.GetBytes(textToEncrypt);
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateEncryptor(publickeybyte, secretkeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    ToReturn = Convert.ToBase64String(ms.ToArray());
                }
                return ToReturn;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public static string DesCifradoData(string textToDecrypt)
        {
            try
            {
                string ToReturn = "";
                string publickey = _key ?? throw new InvalidOperationException("Falta configurar Encriptacion:Key.");
                string secretkey = _iv ?? throw new InvalidOperationException("Falta configurar Encriptacion:IV.");
                byte[] privatekeyByte = System.Text.Encoding.UTF8.GetBytes(secretkey);
                byte[] publickeybyte = System.Text.Encoding.UTF8.GetBytes(publickey);
                MemoryStream ms = null;
                CryptoStream cs = null;
                byte[] inputbyteArray = Convert.FromBase64String(textToDecrypt.Replace(" ", "+"));
                using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
                {
                    ms = new MemoryStream();
                    cs = new CryptoStream(ms, des.CreateDecryptor(publickeybyte, privatekeyByte), CryptoStreamMode.Write);
                    cs.Write(inputbyteArray, 0, inputbyteArray.Length);
                    cs.FlushFinalBlock();
                    ToReturn = Encoding.UTF8.GetString(ms.ToArray());
                }
                return ToReturn;
            }
            catch (Exception ae)
            {
                throw new Exception(ae.Message, ae.InnerException);
            }
        }
#pragma warning restore SYSLIB0021
    }
}
