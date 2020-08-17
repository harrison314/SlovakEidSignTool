using iText.Kernel.Pdf;
using iText.Signatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static iText.Signatures.PdfSigner;

namespace SlovakEidSignTool.Pdf
{
    internal class SignatureEvent : ISignatureEvent
    {
        public SignatureEvent()
        {

        }

        public void GetSignatureDictionary(PdfSignature sig)
        {
            // Update signature data

            // TODO
            //sig.GetPdfObject().Remove(PdfName.Reason);
            //sig.GetPdfObject().Remove(PdfName.Location);
        }
    }
}
