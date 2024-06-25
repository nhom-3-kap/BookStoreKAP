using RestSharp;
using System.Text;

namespace BookStoreKAP.Services
{
    public class QrCodeService
    {
        private readonly string beneficiaryBankCode;
        private readonly string accountNumber;
        private readonly string transactionCurrency = "704";
        private readonly string countryCode = "VN";

        public QrCodeService(BankName bankName, string accountNumber)
        {
            switch (bankName)
            {
                case BankName.SacomBank:
                    beneficiaryBankCode = "970403";
                    break;
                case BankName.ACB:
                    beneficiaryBankCode = "970416";
                    break;
                case BankName.VPBank:
                    beneficiaryBankCode = "970432";
                    break;
                case BankName.MBBank:
                    beneficiaryBankCode = "970422";
                    break;
            }
            this.accountNumber = accountNumber;
        }

        public string BuildQRString(double amount, string desc)
        {
            string statusQrCodeString = "000201010211";
            return BuildQRData(statusQrCodeString, amount, desc);
        }

        private string BuildQRData(string statusQrCodeString, double amount, string desc)
        {
            StringBuilder qrData = new StringBuilder(statusQrCodeString);
            qrData.Append(AppendAccountInfo());
            qrData.Append(AppendTransactionDetails(amount, desc));
            qrData.Append("6304"); // 63
            string crc = CreateCRC(qrData.ToString());
            return qrData.ToString() + crc.Trim();
        }

        private string AppendAccountInfo()
        {
            int FIXED_LENGTH = 38;
            int lengthInfoAccount = beneficiaryBankCode.Length + accountNumber.Length + FIXED_LENGTH;
            int lengthAccountNumber = 8 + beneficiaryBankCode.Length + accountNumber.Length;
            StringBuilder accountInfo = new StringBuilder();

            accountInfo.AppendFormat("38{0}0010A00000072701{1}0006{2}01{3}{4}0208QRIBFTTA", lengthInfoAccount, lengthAccountNumber, beneficiaryBankCode, FormatNumber(accountNumber.Length), accountNumber);

            return accountInfo.ToString();
        }

        private string AppendTransactionDetails(double amount, string desc)
        {
            StringBuilder transactionDetails = new();
            transactionDetails.AppendFormat("5303{0}", transactionCurrency); // 53

            if (amount > 0)
            {
                transactionDetails.AppendFormat("54{0}{1}", FormatNumber(amount.ToString().Length), amount); // 54
            }
            transactionDetails.AppendFormat("5802{0}", countryCode); // 58
            if (!string.IsNullOrEmpty(desc))
            {
                int lengthRemarkCode = desc.Length;
                string remarkCode2 = "08" + lengthRemarkCode.ToString() + desc;

                transactionDetails.AppendFormat("62{0}{1}", remarkCode2.Length, remarkCode2);
            }
            return transactionDetails.ToString();
        }

        private string CreateCRC(string textRcr)
        {
            byte[] new_data_bytes = Encoding.ASCII.GetBytes(textRcr);
            string rcrCode = CalculateCRC16CCITTFalse(new_data_bytes);
            return rcrCode;
        }

        private string CalculateCRC16CCITTFalse(byte[] data)
        {
            ushort poly = 0x1021;
            ushort crc = 0xFFFF;

            foreach (byte b in data)
            {
                crc ^= (ushort)(b << 8);
                for (int i = 0; i < 8; i++)
                {
                    if ((crc & 0x8000) > 0)
                    {
                        crc = (ushort)((crc << 1) ^ poly);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                }
            }

            crc &= 0xFFFF;
            return crc.ToString("X4");
        }

        private string FormatNumber(int number)
        {
            return number <= 9 ? "0" + number.ToString() : number.ToString();
        }
    }

    public enum BankName
    {
        SacomBank,
        ACB,
        VPBank,
        MBBank,
    }
}